using Raccoon.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ItemPopupUpgrade : MonoBehaviour
{

    public Button buy;
    public Button buyAds;
    public TMP_Text txtName;
    public TMP_Text txtPrecent;
    public Image icon;
    public Image bgicon;
    public TMP_Text txtPrice;

    public UpgradeDataSO upgradeData;

    int index;
    public int GetIndex => index;

    UnityAction<ItemPopupUpgrade,bool> actionClick;
    int numberAds;
    int currentAds;
    private void Start()
    {
        currentAds = 0;
    }
    public void Init(UpgradeDataSO dataSO, UnityAction<ItemPopupUpgrade, bool> actionClick)
    {
        upgradeData = dataSO;
        bgicon.sprite = dataSO.bgIcon;
        txtName.text = dataSO.type.ToString();
        icon.sprite = dataSO.icon;
        this.actionClick = actionClick; 
    } 

    public void ClickButtonBuy()
    {
        Buy(false);
    }
    public void ClickButtonAds()
    {
        currentAds++;   
        if(currentAds == numberAds)
        {
            currentAds = 0;
            Buy(true);
        }
    }

    public void Buy(bool useAds)
    {
        actionClick?.Invoke(this,useAds);
    }

    public void SetData(int index)
    {
        if (upgradeData == null)
        {
            TurnOffButtonBuy();
            return;
        }
        this.index = index;
        if(!upgradeData.HaveData(index))
        {
            TurnOffButtonBuy();
            return;
        }
        
        UpdateUI(index);
    }

    public void SetCoinEnough(int index,  long coin)
    {
        if(upgradeData.GetPriceUpgrade(index) > coin)
        {
            txtPrice.color = Color.red;
        }
        else
        {
            txtPrice.color = Color.white;
        }
    }

    void TurnOffButtonBuy()
    {
        buy.gameObject.SetActive(false);
        buyAds.gameObject.SetActive(false);
        txtPrecent.gameObject.SetActive(false);
    }

    void UpdateUI(int index)
    {
        buy.gameObject.SetActive(true);
        //buyAds.gameObject.SetActive(true);
        txtPrecent.gameObject.SetActive(true);
        txtPrecent.text = "+" + upgradeData.GetPrecent(index).ToString();
        txtPrice.text = upgradeData.GetPriceUpgrade(index).ConvertCurrencyToString();
    }
}
