using Raccoon;
using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemIndex : MonoBehaviour
{
    BotDataSO data;
    public Image icon;
    public Image iconLock;
    public TMP_Text textIncome;
    public Image bg;
    public GameObject goLock;
    Image iconRainbow;
    public void Init(BotDataSO so)
    {
        data = so;
        goLock.SetActive(true);
        SetIcon(data);
        textIncome.text = "?????";
        iconLock.sprite = data.spriteIconLock;
    }

    void SetIcon(ItemDataSO data)
    {
        icon.sprite = data.spriteIcon;
        if (data.colorType != ColorType.Rainbow)
        {
            if (iconRainbow != null)
                iconRainbow.gameObject.SetActive(false);
            return;
        }
        if (iconRainbow == null)
            iconRainbow = Instantiate(icon, icon.transform.parent);
        iconRainbow.sprite = data.spriteIconLock;
        iconRainbow.material = GamePlayControl.instance.matColorRainbowUI;
        iconRainbow.gameObject.SetActive(true);
    }
    public void SetUnlock()
    {
        goLock.SetActive(false);
        textIncome.text = data.income.ConvertCurrencyToString() + "/s";
    }

    private void OnEnable()
    {
        if (PlayerData.Get.isVipIAP)
        {
            SetUnlock();
        }
    }
    public void ChangeBg(Sprite bg)
    {
        this.bg.sprite = bg;
    }
    public BotDataSO GetData => data;


}
