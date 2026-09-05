using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupBackPack : AbstractPopup
{
    public List<TabColorUI> listButtonTab;
    public ScrollRect scrollRect;

    TabColorUI currentTab;
    ColorType colorType = ColorType.Normal;

    Dictionary<ColorType, List<DataItemShow>> dicDataItem;

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
        currentTab?.SetSelect(true);
        if (dicDataItem == null) InitDic();
        if (listItem == null) listItem = new List<ItemInventory>();
        ShowItem();
    }
    void InitDic()
    {
        dicDataItem = new Dictionary<ColorType, List<DataItemShow>>();
        foreach (ColorType type in Enum.GetValues(typeof(ColorType)))
        {
            dicDataItem[type] = new List<DataItemShow>();
        }
    }
    void ShowItem()
    {
        for (int i = 0; i < listItem.Count; i++)
        {
            ItemInventory item = listItem[i];
            if(i < dicDataItem[colorType].Count)
            {
                item.gameObject.SetActive(true);
                SetDataItem(item, dicDataItem[colorType][i]);
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
        int index = listItem.Count;
        for(int i = index; i < dicDataItem[colorType].Count; i++)
        {
            ItemInventory item = Spawn();
            item.SetEventClick(OnClick);
            item.gameObject.SetActive(true);
            SetDataItem(item, dicDataItem[colorType][i]);
        }
        
    }

    void SetDataItem(ItemInventory item, DataItemShow data)
    {
        item.SetData(data.data, data.config, 1);
        Sprite bg = GamePlayControl.instance.GetBgByColorType(colorType);
        item.ChangeBg(bg);
    }

    protected override void OnClick(ItemInventory item)
    {
        if (!PlayerController.instance.CanClickItemInventory()) return;

        base.OnClick(item);
        ItemConfig config = null;
        if (currentItem != null) config = currentItem.itemConfig;
        PlayerController.instance.ChangeItemWhenClickItemBackpack(config);
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
        ResetItem();
        ShowItem();
    }



    public void UpdateItem(ItemDataSO data, ItemConfig config, bool isAdd)
    {
        if(dicDataItem == null) InitDic();
        if(data == null) return;
        if (isAdd)
        {
            DataItemShow dtAdd = new DataItemShow(data,config);
            dicDataItem[data.colorType].Add(dtAdd);
        }
        else
        {
            DataItemShow dtRemove = dicDataItem[data.colorType].FirstOrDefault(dis => dis.data == data && dis.config == config);
            dicDataItem[data.colorType].Remove(dtRemove);
        }

    }

    class DataItemShow
    {
        public ItemDataSO data;
        public ItemConfig config;
        public DataItemShow(ItemDataSO data, ItemConfig config)
        {
            this.data = data;
            this.config = config;
        }
    }

}
