using DG.Tweening;
using Raccoon;
using Raccoon.Ads;
using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.Utils;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class RebirthPopup : PopupCanvas
{
    [Header("Info Display")]
    public TextMeshProUGUI cashMultText;
    public TextMeshProUGUI slotText;


    [Header("Progress")]
    public Slider progressSlider;
    public TextMeshProUGUI progressText;



    [Header("Buttons")]
    public Button rebirthButton;
    public Button watchRebirthButton;
    public Image rebirthButtonImage;
    public Sprite rebirthSpriteNormal;
    public Sprite rebirthSpriteReady;



    public List<ItemRebirthPopup> listItem;

    public override void Show(PopupType p, UnityAction afterPopup, object obj)
    {
        base.Show(p, afterPopup, obj);
        Refresh((CharactorData)obj);
    }

    private void Refresh(CharactorData data)
    {
        var rbSO = GameData.Get.GetNextRebirth(data.GetIndexRebirth());
        if (rbSO == null)
        {
            SetCommingSoon();
            return;
        }

        long require = rbSO.GetRequireMoney();
        long currentCoin = data.GetCoin;
        bool canAfford = currentCoin >= require;
        bool hasWorkers = rbSO.HasRequiredWorkers(data.GetAllIdItem());
        bool canRebirth = (canAfford && hasWorkers);

        if (cashMultText != null)
            cashMultText.text = $"×{rbSO.moneyMult:F1}";

        if (slotText != null)
            slotText.text = $"+{rbSO.slotUnlock} slot";

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = require;
            progressSlider.value = Mathf.Clamp(currentCoin, 0f, require);
            progressSlider.interactable = false;
        }

        if (progressText != null)
            progressText.text = $"{currentCoin.ConvertCurrencyToString()} / {((long)require).ConvertCurrencyToString()}";

        if (listItem != null)
        {
            var workers = rbSO.requireWorkers;
            bool hasWorkersList = workers != null && workers.Count > 0;
            int count = hasWorkersList ? workers.Count : rbSO.requireWorkerCount;

            for (int i = 0; i < listItem.Count; i++)
            {
                if (listItem[i] == null) continue;
                bool active = i < count;
                listItem[i].gameObject.SetActive(active);
                if (active && hasWorkersList && i < workers.Count)
                {
                    bool isCollected = data.GetAllIdItem().Contains(workers[i].GetId);
                    listItem[i].Setup(workers[i], isCollected);
                }
            }

        }


        if (rebirthButton != null)
            rebirthButton.interactable = canRebirth;

        if (rebirthButtonImage != null && rebirthSpriteNormal != null && rebirthSpriteReady != null)
            rebirthButtonImage.sprite = canRebirth ? rebirthSpriteReady : rebirthSpriteNormal;

    }

    void SetCommingSoon()
    {

    }
    public void ClickRebirth()
    {
        print("ClickRebirth");
        if (!GamePlayControl.instance.rebirthHandle.GetHasRebirth) return;
        CompleteRebirth(false);
    }

    public void WatchRebirth()
    {
        // watch ads
        AdsActionManager.instance.ShowRewardAds((bool success) =>
        {
            if(success) CompleteRebirth(true);
        });
        
    }

    private void CompleteRebirth(bool isAds)
    {
        Hide();
        GamePlayControl.instance.OnClickButtonRebirth(isAds);
    }
}
