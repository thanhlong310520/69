using Raccoon;
using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemIndex : MonoBehaviour
{
    ItemDataSO data;
    public Image icon;
    public Image iconLock;
    public TMP_Text textIncome;
    public Image bg;
    public GameObject goLock;
    public void Init(ItemDataSO so)
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
        
    }
    public void SetUnlock()
    {
        goLock.SetActive(false);
        //textIncome.text = data.income.ConvertCurrencyToString() + "/s";
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
    public ItemDataSO GetData => data;


}
