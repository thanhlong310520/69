using Raccoon;
using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ItemIndex : MonoBehaviour
{
    ItemDataSO data;
    public Image icon;
    public TMP_Text nametxt;
    public Image bg;
    public Button btn;
    public GameObject selectedObj;
    UnityAction<ItemIndex> onClick;
    public bool IsUnlock { get; private set; }
    public void Init(ItemDataSO so, UnityAction<ItemIndex> onClick = null)
    {
        data = so;
        this.onClick = onClick;
        if (btn != null)
        {
            btn.onClick.RemoveListener(OnClick);
            btn.onClick.AddListener(OnClick);
        }
        SetIcon(data);
        nametxt.text = "?????";
        icon.color = Color.black;
        IsUnlock = false;
        SetSelected(false);
    }

    void SetIcon(ItemDataSO data)
    {
        icon.sprite = data.spriteIcon;

    }
    public void SetUnlock()
    {
        icon.color = Color.white;
        IsUnlock = true;
        nametxt.text = data.itemName;
    }

    //private void OnEnable()
    //{
    //    if (PlayerData.Get.isVipIAP)
    //    {
    //        SetUnlock();
    //    }
    //}
    public void ChangeBg(Sprite bg)
    {
        this.bg.sprite = bg;
    }
    public ItemDataSO GetData => data;

    public void SetSelected(bool selected)
    {
        if (selectedObj != null) selectedObj.SetActive(selected);
    }

    public void OnClick()
    {
        onClick?.Invoke(this);
    }


}
