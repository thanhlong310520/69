using Raccoon;
using Raccoon.Ads;
using Raccoon.Data;
using Raccoon.Item;
using UnityEngine;

public class ItemFashion : ItemInventory
{
    public GameObject button;
    public GameObject noAds;
    public GameObject ads;
    bool adsActive;
    public override void SetData(ItemDataSO data, ItemConfig config, int number)
    {
        base.SetData(data, config, number);
        //FashionDataSO fso = data as FashionDataSO;

        //if(fso!= null)
        //{
        //    adsActive = fso.numberAds > 0 ? true : false;
        //    noAds.SetActive(!adsActive);
        //    ads.SetActive(adsActive);
        //}
        //SetSelected(false);

    }

    private void OnEnable()
    {
        adsActive = PlayerData.Get.isOnAds && !PlayerData.Get.isVipIAP;
        noAds.SetActive(!adsActive);
        ads.SetActive(adsActive);
    }
    public override void SetSelected(bool selected)
    {
        base.SetSelected(selected);
        button.SetActive(!selected);
    }

    public override void OnClick()
    {
        if(adsActive)
        {
            AdsActionManager.instance.ShowRewardAds(AdsComplete);
        }
        else 
            base.OnClick();
    }

    void AdsComplete(bool success)
    {
        if (success)
        {
            clickAciton?.Invoke(this);
        }
    }
}
