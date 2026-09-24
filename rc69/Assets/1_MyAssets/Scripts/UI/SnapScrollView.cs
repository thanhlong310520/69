using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Gắn vào item (con trực tiếp của Content) nếu muốn nhận callback khi được select / bỏ select.
/// </summary>
public interface ISnapItem
{
    void OnSnapSelected(bool selected);
}

/// <summary>
/// Gắn cùng GameObject với ScrollRect (chỉ bật 1 chiều Horizontal hoặc Vertical).
/// - Snap: thả tay, scroll chậm lại thì căn item gần nhất vào giữa viewport và select nó.
/// - Infinite: lướt vòng tròn vô hạn (item trôi ra khỏi 1 đầu được chuyển sang đầu kia).
/// - Start: mỗi lần OnEnable căn item StartIndex vào giữa.
/// Index = thứ tự trong các item đang active của Content.
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class SnapScrollView : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Serializable] public class IntEvent : UnityEvent<int> { }

    private const float SnapEpsilon = 0.5f;

    [Header("Snap")]
    [Tooltip("Khi vận tốc scroll nhỏ hơn giá trị này thì bắt đầu snap")]
    [SerializeField] private float snapVelocityThreshold = 200f;
    [Tooltip("Thời gian (xấp xỉ) để item chạy về giữa")]
    [SerializeField] private float snapSmoothTime = 0.12f;

    [Header("Start")]
    [Tooltip("Index item được căn giữa mỗi lần OnEnable")]
    [SerializeField] private int startIndex = 0;

    [Header("Infinite")]
    [Tooltip("Lướt vô hạn. Tổng chiều dài các item nên lớn hơn viewport + 1 item để không bị hở")]
    [SerializeField] private bool infinite = true;

    [Header("Not Infinite")]
    [Tooltip("Tự set padding cho LayoutGroup để item đầu/cuối cũng căn được vào giữa")]
    [SerializeField] private bool autoPadding = true;

    [Header("Events")]
    public IntEvent onItemSelected = new IntEvent();

    // Components
    private ScrollRect scrollRect;
    private RectTransform content;
    private RectTransform viewport;
    private HorizontalOrVerticalLayoutGroup layout;
    private ContentSizeFitter sizeFitter;
    private ScrollRect.MovementType originalMovementType;

    // Items (cache lại khi Refresh)
    private readonly List<RectTransform> items = new List<RectTransform>();
    private readonly List<ISnapItem[]> itemCallbacks = new List<ISnapItem[]>();
    private readonly List<float> itemCenterOffsets = new List<float>(); // tâm item so với localPosition, theo trục scroll
    private bool horizontal;

    // Snap
    private bool isDragging;
    private bool isSnapping;
    private bool waitingLayout;
    private int snapIndex = -1;
    private Vector2 snapTarget;
    private Vector2 snapVelocity;

    // Infinite: tổng chiều dài 1 vòng item (theo trục scroll, trong không gian local của Content)
    private float loopLength;

    public int SelectedIndex { get; private set; } = -1;
    public int ItemCount => items.Count;

    /// <summary>
    /// Index item được căn giữa mỗi lần OnEnable. Set trước khi SetActive(true).
    /// </summary>
    public int StartIndex
    {
        get => startIndex;
        set => startIndex = value;
    }

    #region Unity

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;
        viewport = scrollRect.viewport != null ? scrollRect.viewport : (RectTransform)scrollRect.transform;
        layout = content.GetComponent<HorizontalOrVerticalLayoutGroup>();
        sizeFitter = content.GetComponent<ContentSizeFitter>();
        originalMovementType = scrollRect.movementType;
    }

    private void OnEnable()
    {
        scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        isDragging = false;
        Refresh(true);
        StartCoroutine(RefreshAfterLayout());
    }

    private void OnDisable()
    {
        scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        isSnapping = false;
        waitingLayout = false;
    }

    private void Update()
    {
        if (items.Count == 0 || isDragging || waitingLayout) return;

        if (isSnapping)
            UpdateSnapping();
        else
            CheckAutoSnap();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        isSnapping = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    private void OnScrollValueChanged(Vector2 _)
    {
        WrapItems();
    }

    #endregion

    #region Start / Refresh

    /// <summary>
    /// Lúc vừa OnEnable, kích thước Canvas / viewport có thể chưa được tính xong
    /// nên đợi 1 frame rồi căn lại (giữ item đang chọn / đang snap tới). Trong lúc đợi không tự snap.
    /// </summary>
    private IEnumerator RefreshAfterLayout()
    {
        waitingLayout = true;
        yield return null;
        waitingLayout = false;
        Refresh();
    }

    /// <summary>
    /// Gọi lại sau khi thêm / xoá / bật tắt item trong Content.
    /// resetToStart = true: về item StartIndex, false: giữ item đang chọn.
    /// </summary>
    public void Refresh(bool resetToStart = false)
    {
        if (content == null) return;

        // Giữ item đang chọn (hoặc đang snap tới) theo Transform, vì index có thể đổi khi thêm / xoá item
        int oldIndex = isSnapping ? snapIndex : SelectedIndex;
        RectTransform current = IsValidIndex(oldIndex) ? items[oldIndex] : null;
        isSnapping = false;
        horizontal = scrollRect.horizontal;

        CollectItems();
        int currentIndex = current != null ? items.IndexOf(current) : -1;
        RebuildLayout();

        if (infinite) SetupInfinite();
        else SetupFinite();

        int index = !resetToStart && IsValidIndex(currentIndex) ? currentIndex
                  : IsValidIndex(startIndex) ? startIndex
                  : 0;
        SnapTo(index, false);
    }

    private void CollectItems()
    {
        RectTransform selected = IsValidIndex(SelectedIndex) ? items[SelectedIndex] : null;
        ISnapItem[] selectedCallbacks = selected != null ? itemCallbacks[SelectedIndex] : null;

        items.Clear();
        itemCallbacks.Clear();
        foreach (Transform child in content)
        {
            if (!child.gameObject.activeSelf) continue;
            items.Add((RectTransform)child);
            itemCallbacks.Add(child.GetComponents<ISnapItem>());
        }

        // Item đang chọn vẫn còn thì chỉ cập nhật index, không bắn lại event
        SelectedIndex = selected != null ? items.IndexOf(selected) : -1;
        if (selected != null && SelectedIndex < 0)
        {
            foreach (var callback in selectedCallbacks)
                if (callback as Component != null) callback.OnSnapSelected(false);
        }
    }

    private void RebuildLayout()
    {
        if (layout != null)
        {
            if (!infinite && autoPadding) ApplyAutoPadding();
            layout.enabled = true;
        }
        if (sizeFitter != null) sizeFitter.enabled = true;

        // Rebuild để LayoutGroup xếp lại vị trí gốc (xoá các vị trí đã bị wrap ở lần trước)
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        itemCenterOffsets.Clear();
        foreach (var item in items)
            itemCenterOffsets.Add(Axis(Vector3.Scale(item.rect.center, item.localScale)));
    }

    #endregion

    #region Snap

    /// <summary>
    /// Căn item index vào giữa viewport và select nó. Ở chế độ infinite sẽ đi theo đường ngắn nhất.
    /// </summary>
    public void SnapTo(int index, bool animate = true)
    {
        if (!IsValidIndex(index)) return;

        WrapItems();
        scrollRect.StopMovement();
        snapIndex = index;
        snapTarget = content.anchoredPosition + GetOffsetToCenter(index);
        snapVelocity = Vector2.zero;

        if (animate)
        {
            isSnapping = true;
            return;
        }

        isSnapping = false;
        content.anchoredPosition = snapTarget;
        WrapItems();
        Select(index);
    }

    /// <summary>
    /// Căn item (ISnapItem nằm trên item hoặc con của item) vào giữa và select nó.
    /// </summary>
    public void SnapTo(ISnapItem item, bool animate = true) => SnapTo(GetIndex(item), animate);

    public void Next(bool animate = true) => SnapTo(GetStepIndex(1), animate);
    public void Previous(bool animate = true) => SnapTo(GetStepIndex(-1), animate);

    private void UpdateSnapping()
    {
        scrollRect.velocity = Vector2.zero;
        content.anchoredPosition = Vector2.SmoothDamp(content.anchoredPosition, snapTarget, ref snapVelocity, snapSmoothTime);

        if ((content.anchoredPosition - snapTarget).sqrMagnitude < SnapEpsilon * SnapEpsilon)
        {
            content.anchoredPosition = snapTarget;
            isSnapping = false;
            Select(snapIndex);
        }
    }

    private void CheckAutoSnap()
    {
        float speed = Mathf.Abs(Axis(scrollRect.velocity));
        if (speed >= snapVelocityThreshold) return;

        int nearest = FindNearestIndex();
        if (nearest != SelectedIndex || GetOffsetToCenter(nearest).sqrMagnitude > SnapEpsilon * SnapEpsilon)
            SnapTo(nearest, true);
    }

    private int FindNearestIndex()
    {
        float center = GetViewportCenter();
        int nearest = 0;
        float minDistance = float.MaxValue;

        for (int i = 0; i < items.Count; i++)
        {
            float distance = Mathf.Abs(GetItemCenter(i) - center);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = i;
            }
        }
        return nearest;
    }

    /// <summary>
    /// Lượng cần cộng vào content.anchoredPosition để item index nằm giữa viewport.
    /// </summary>
    private Vector2 GetOffsetToCenter(int index)
    {
        float delta = GetViewportCenter() - GetItemCenter(index);
        return horizontal
            ? new Vector2(delta * content.localScale.x, 0f)
            : new Vector2(0f, delta * content.localScale.y);
    }

    private int GetStepIndex(int step)
    {
        int index = SelectedIndex + step;
        if (infinite) return (index % items.Count + items.Count) % items.Count;
        return Mathf.Clamp(index, 0, items.Count - 1);
    }

    #endregion

    #region Select

    private void Select(int index)
    {
        if (index == SelectedIndex) return;

        SetItemSelected(SelectedIndex, false);
        SelectedIndex = index;
        SetItemSelected(SelectedIndex, true);
        onItemSelected?.Invoke(SelectedIndex);
    }

    private void SetItemSelected(int index, bool selected)
    {
        if (!IsValidIndex(index)) return;
        foreach (var callback in itemCallbacks[index])
            callback.OnSnapSelected(selected);
    }

    /// <summary>
    /// Index của item chứa ISnapItem, -1 nếu item không thuộc scroll này.
    /// </summary>
    public int GetIndex(ISnapItem item)
    {
        if (!(item is Component component)) return -1;

        Transform t = component.transform;
        while (t != null && t.parent != content) t = t.parent;
        return t != null ? items.IndexOf((RectTransform)t) : -1;
    }

    /// <summary>
    /// ISnapItem của item đang được select (null nếu không có).
    /// </summary>
    public ISnapItem GetSelectedItem()
    {
        if (!IsValidIndex(SelectedIndex)) return null;
        var callbacks = itemCallbacks[SelectedIndex];
        return callbacks.Length > 0 ? callbacks[0] : null;
    }

    #endregion

    #region Infinite

    private void SetupInfinite()
    {
        // LayoutGroup chỉ dùng để xếp vị trí ban đầu, sau đó tự quản lý vị trí item.
        // Tắt luôn ContentSizeFitter: nếu không, khi LayoutGroup tắt nó sẽ co Content về 0 làm lệch vị trí item.
        if (layout != null) layout.enabled = false;
        if (sizeFitter != null) sizeFitter.enabled = false;
        scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
        loopLength = CalculateLoopLength();
    }

    /// <summary>
    /// Item nào cách tâm viewport quá nửa vòng thì chuyển sang phía bên kia.
    /// </summary>
    private void WrapItems()
    {
        if (!infinite || loopLength <= 0f) return;

        float half = loopLength * 0.5f;
        float center = GetViewportCenter();
        Vector3 axis = horizontal ? Vector3.right : Vector3.up;

        for (int i = 0; i < items.Count; i++)
        {
            float distance = GetItemCenter(i) - center;
            if (distance > half)
                items[i].localPosition -= axis * (Mathf.Floor((distance + half) / loopLength) * loopLength);
            else if (distance < -half)
                items[i].localPosition += axis * (Mathf.Floor((half - distance) / loopLength) * loopLength);
        }
    }

    private float CalculateLoopLength()
    {
        if (items.Count == 0) return 0f;

        float min = float.MaxValue;
        float max = float.MinValue;
        for (int i = 0; i < items.Count; i++)
        {
            float center = GetItemCenter(i);
            float halfSize = GetItemSize(i) * 0.5f;
            min = Mathf.Min(min, center - halfSize);
            max = Mathf.Max(max, center + halfSize);
        }

        float spacing = layout != null ? layout.spacing : 0f;
        return max - min + spacing;
    }

    #endregion

    #region Not Infinite

    private void SetupFinite()
    {
        scrollRect.movementType = originalMovementType;
        loopLength = 0f;
    }

    /// <summary>
    /// Thêm padding 2 đầu = nửa viewport - nửa item, để khi Content chạm mép
    /// thì item đầu / cuối vẫn nằm đúng giữa viewport (ScrollRect không kéo ngược lại).
    /// Content cần ContentSizeFitter (Preferred Size) để kích thước tính cả padding.
    /// </summary>
    private void ApplyAutoPadding()
    {
        if (items.Count == 0) return;

        float halfViewport = (horizontal ? viewport.rect.width : viewport.rect.height) * 0.5f;
        int startPadding = Mathf.Max(0, Mathf.RoundToInt(halfViewport - GetItemSize(0) * 0.5f));
        int endPadding = Mathf.Max(0, Mathf.RoundToInt(halfViewport - GetItemSize(items.Count - 1) * 0.5f));

        if (horizontal)
        {
            layout.padding.left = startPadding;
            layout.padding.right = endPadding;
        }
        else
        {
            layout.padding.top = startPadding;
            layout.padding.bottom = endPadding;
        }
    }

    #endregion

    #region Helpers

    private bool IsValidIndex(int index) => index >= 0 && index < items.Count;

    private float Axis(Vector3 v) => horizontal ? v.x : v.y;

    /// <summary>Tâm viewport theo trục scroll, trong không gian local của Content.</summary>
    private float GetViewportCenter()
    {
        return Axis(content.InverseTransformPoint(viewport.TransformPoint(viewport.rect.center)));
    }

    /// <summary>Tâm item theo trục scroll, trong không gian local của Content.</summary>
    private float GetItemCenter(int index)
    {
        return Axis(items[index].localPosition) + itemCenterOffsets[index];
    }

    private float GetItemSize(int index)
    {
        var item = items[index];
        return horizontal ? item.rect.width * item.localScale.x : item.rect.height * item.localScale.y;
    }

    #endregion
}
