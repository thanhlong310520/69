using DG.Tweening;
using Raccoon;
using Raccoon.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PopupUpgrade : PopupCanvas
{
    public List<UpgradeDataSO> upgradeDataSOs;
    public Transform notiNotEnough;
    public ItemPopupUpgrade itemPrefab;
    public List<ItemPopupUpgrade> items;
    public Transform holderItem;
    public override void Show(PopupType p, UnityAction afterPopup, object obj)
    {
        notiNotEnough.gameObject.SetActive(false);
        base.Show(p, afterPopup, obj);
        if(upgradeDataSOs.Count <= 0)
        {
            upgradeDataSOs = GamePlayControl.instance.GetUpgradeDataSOs;
        }
        if (items == null) LoadItem();
        if (items.Count <= 0) LoadItem();
        ShowDataUpgrade((CharactorData)obj);
    }
    void LoadItem()
    {
        items = new List<ItemPopupUpgrade>();
        foreach (var u in upgradeDataSOs)
        {
            foreach (var ud in u.data)
            {

                ItemPopupUpgrade it = Instantiate(itemPrefab, holderItem);
                it.gameObject.SetActive(false);
                it.Init(u,ud.id,ud.name,ud.icon,ud.bgIcon, Buy);
                items.Add(it);
            }
        }
    }
    void ShowDataUpgrade(CharactorData data)
    {
        if (data == null) return;
        foreach (var dt in upgradeDataSOs)
        {
            foreach (var ud in dt.data)
            {
                var listUd  = data.upgrade.GetDicUpgraded(dt.type);
                var item = GetItemPopupUpgrade(dt.type,ud.id);
                if (item == null) continue;

                if (listUd.Contains(ud.id))
                {
                    item.gameObject.SetActive(false);

                }
                else
                {
                    item.SetData(ud.precent, ud.price, data.upgrade.GetNumberAds(dt.type, ud.id), ud.numberAds);
                    item.gameObject.SetActive(true);
                }
            }
        }

    }

    ItemPopupUpgrade GetItemPopupUpgrade(TypeUpgrade type, string id)
    {
        foreach (var item in items)
        {
            if(item.upgradeData.type == type && item.GetId.Equals(id)) return item;
        }
        return null;
    }

    void Buy(ItemPopupUpgrade item, bool useAds)
    {
        PlayerController.instance.CheckUpgrade(item.upgradeData, item.GetId, useAds,ShowDataUpgrade);
        // Reset data 

    }

}
