using Raccoon.Controller;
using Raccoon.Data;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InforIndex : MonoBehaviour, ISnapItem
{
    public Image bgZone;
    public TMP_Text nameZone;
    public ItemIndex prefab;
    public Transform content;
    public List<ItemIndex> listItemIndex;
    public TMP_Text textProgess;
    public Slider amountProgess;

    UnityAction<InforIndex> actionSnap;
    UnityAction<ItemIndex> actionClickItem;
    public ZoneType ZoneType { get; private set; }

    public void Init(ZoneType zoneType, UnityAction<InforIndex> actionSnap, UnityAction<ItemIndex> actionClickItem = null)
    {
        this.actionSnap = actionSnap;
        this.actionClickItem = actionClickItem;
        ZoneType = zoneType;
        Sprite bg = GamePlayControl.instance.GetBgByColorType(zoneType);
        bgZone.sprite = bg;
        if (nameZone != null) nameZone.text = zoneType.ToString();
        SpawnItemIndex();
        UpdateProgress();
    }

    void SpawnItemIndex()
    {
        var listData = GamePlayControl.instance.GetBotDataInZone(ZoneType);
        listItemIndex = new List<ItemIndex>();
        foreach (var dt in listData)
        {
            SpawnItem(dt);
        }
    }
    protected ItemIndex SpawnItem(ItemDataSO data)
    {
        var newItem = Instantiate(prefab, content);
        newItem.transform.localScale = Vector3.one;
        newItem.Init(data, actionClickItem);
        listItemIndex.Add(newItem);
        return newItem;
    }

    public void UpdateProgress()
    {
        int total = listItemIndex.Count;
        int unlocked = listItemIndex.Count(i => i.IsUnlock);
        if (textProgess != null) textProgess.text = $"{unlocked}/{total}";
        if (amountProgess != null) amountProgess.value = total > 0 ? (float)unlocked / total : 0;
    }

    public void OnSnapSelected(bool selected)
    {
        if(selected) actionSnap?.Invoke(this);
    }

    public void OnUnlockItem(ItemDataSO so)
    {
        foreach (var item in listItemIndex)
        {
            if (item.GetData.GetId == so.GetId) item.SetUnlock();
        }
        UpdateProgress();
    }
}
