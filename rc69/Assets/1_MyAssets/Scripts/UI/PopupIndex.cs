using Raccoon.Controller;
using Raccoon.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupIndex : PopupCanvas
{
    public List<TabColorUI> listButtonTab;
    public ItemIndex itemPrefab;
    public Transform content;
    public ScrollRect scrollRect;
    TabColorUI currentTab;
    ZoneType colorType = ZoneType.Forest;

    Dictionary<ZoneType, List<ItemIndex>> dicDataItem;

    private void Start()
    {
        foreach (var t in listButtonTab)
        {
            t.ActionClick = ClickTap;
        }
    }



    public override void Show(PopupType p, UnityAction afterPopup, object obj)
    {
        base.Show(p, afterPopup, obj);
        foreach (var t in listButtonTab)
        {
            t.SetSelect(false);
            if (colorType == t.type) currentTab = t;
        }
        if (dicDataItem == null) InitDic();
        currentTab?.SetSelect(true);
        ShowItem();
    }
    void InitDic()
    {
        dicDataItem = new Dictionary<ZoneType, List<ItemIndex>>();
        foreach (ZoneType type in Enum.GetValues(typeof(ZoneType)))
        {
            dicDataItem[type] = new List<ItemIndex>();
        }
        foreach (var so in GamePlayControl.instance.GetBotDataSOs())
        {
            var itemIndex = Spawn();
            Sprite bg = GamePlayControl.instance.GetBgByColorType(colorType);
            itemIndex.ChangeBg(bg);
            itemIndex.Init(so);
            dicDataItem[so.colorType].Add(itemIndex);
        }
    }
    protected ItemIndex Spawn()
    {
        var newItem = Instantiate(itemPrefab, content);
        return newItem;
    }
    void ShowItem()
    {
        bool isActive = false;
        foreach (var lst in dicDataItem)
        {
            if(lst.Key == colorType) isActive = true;
            else isActive = false;
            foreach (var item in lst.Value)
            {
                item.gameObject.SetActive(isActive);
            }
        }
    }
    private void ClickTap(TabColorUI uI)
    {
        if (uI == currentTab) return;
        currentTab?.SetSelect(false);
        currentTab = uI;
        currentTab?.SetSelect(true);
        colorType = currentTab.type;
        content.localPosition = Vector3.zero;
        scrollRect.velocity = Vector3.zero; 
        ShowItem();
    }



    public void UpdateItem(ItemDataSO data)
    {
        print("update");
        if (dicDataItem == null) InitDic();
        var itemindex = dicDataItem[data.colorType].FirstOrDefault(ii => ii.GetData.GetId == data.GetId);
        itemindex?.SetUnlock();

    }
}
