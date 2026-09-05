using DG.Tweening;
using Raccoon;
using Raccoon.Controller;
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
        if(items == null) items = new List<ItemPopupUpgrade>();

        ShowDataUpgrade((CharactorData)obj);
    }

    void ShowDataUpgrade(CharactorData data)
    {
        if (data == null) return;
        foreach (var dt in upgradeDataSOs)
        {
            var it = items.FirstOrDefault(i => i.upgradeData == dt);
            if (it == null)
            {
                it = Instantiate(itemPrefab, holderItem);
                it.gameObject.SetActive(true);
                items.Add(it);
            }
            it.Init(dt, Buy);
            int index = data.upgrade.GetDicUpgraded(dt.type) + 1;
            it.SetData(index);
            it.SetCoinEnough(index, data.GetCoin);
        }

    }

    void Buy(ItemPopupUpgrade item, bool useAds)
    {
        bool enoughCoin = false;
        if (PlayerController.instance.GetCharactorData.GetCoin >= item.upgradeData.GetPriceUpgrade(item.GetIndex)) enoughCoin = true;
        if (useAds) enoughCoin = true;
        if (!enoughCoin)
        {
            // show not enough coin 
            ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.BuyFall);

            //notiNotEnough.gameObject.SetActive(true);
            //notiNotEnough.DOKill();
            //notiNotEnough.localScale = Vector3.zero;
            //notiNotEnough.DOScale(Vector3.one, 2).OnComplete(() =>
            //{
            //    notiNotEnough.transform.SetActive(false);
            //});
            return;
        }
        PlayerController.instance.Upgrade(item.upgradeData, item.GetIndex, useAds);
        int index = PlayerController.instance.GetCharactorData.upgrade.GetDicUpgraded(item.upgradeData.type) + 1;
        item.SetData(index);
        item.SetCoinEnough(index, PlayerController.instance.GetCharactorData.GetCoin);

    }

}
