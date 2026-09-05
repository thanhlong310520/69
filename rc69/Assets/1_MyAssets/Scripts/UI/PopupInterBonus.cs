using Raccoon;
using Raccoon.Controller;
using Raccoon.Ads;
using Raccoon.Utils;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupInterBonus : PopupCanvas
{
    [SerializeField] List<InforItemInterBonus> listInfor;
    private int currentBonus;

    public ShowLoadingAds showLoadingAds;
    public Button getButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentBonus = 0;
        foreach (var item in listInfor)
        {
            item.toggle.isOn = false;
            item.text.text = item.bonus.ConvertCurrencyToString();
            item.icon.sprite = item.sprite;
        }
    }
    public override void Show(PopupType p, UnityAction afterPopup, object obj)
    {
        base.Show(p, afterPopup, obj);
        listInfor[currentBonus].toggle.isOn = true;
        getButton.interactable = false;
    }
    public override void DoneShow()
    {
        base.DoneShow();
        if (!PlayerData.Get.isOnAds || PlayerData.Get.isVipIAP)
        {
            getButton.interactable = true;
        }
        else
        {
            showLoadingAds.Show(() =>
            {
                getButton.interactable = true;
                AdsActionManager.instance.ShowInterstitialAd(() =>
                {

                });
            });
        }
    }
    public void OnClaimBonus()
    {
        var bonus = listInfor[currentBonus].bonus;
        //var startAnim = togglesBonus[currentBonus];
        PlayerController.instance.Income(bonus);
        currentBonus++;
        currentBonus = currentBonus % listInfor.Count;
        //FlyAnim.Instance.PlayAnimationUI(btnClaim.transform);
        Hide();
    }

    [System.Serializable] struct InforItemInterBonus
    {
        public long bonus;
        public Sprite sprite;
        public Image icon;
        public Toggle toggle;
        public TMP_Text text;
    }
}
