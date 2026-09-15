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
    public TMP_Text txtNumberAds;
    public Image icon;
    public Image bgicon;
    public TMP_Text txtPrice;

    public UpgradeDataSO upgradeData;

    string id;
    public string GetId => id;

    UnityAction<ItemPopupUpgrade,bool> actionClick;
    public void Init(UpgradeDataSO dataSO, string id,string nameUpgrade,Sprite icon ,Sprite bg, UnityAction<ItemPopupUpgrade, bool> actionClick)
    {
        this.id = id;
        upgradeData = dataSO;
        //bgicon.sprite = bg;
        txtName.text = nameUpgrade;
        this.icon.sprite = icon;
        this.actionClick = actionClick; 
    } 

    public void ClickButtonBuy()
    {
        Buy(false);
    }
    public void ClickButtonAds()
    {
        Buy(true);
    }

    public void Buy(bool useAds)
    {
        actionClick?.Invoke(this,useAds);
    }

    public void SetData(float precent, long price, int playerNumberads, int totalAds)
    {
        txtPrecent.text = "x" + precent.ToString();
        txtPrice.text = price.ConvertCurrencyToString();
        if(totalAds > 0)
        {
            txtNumberAds.text = $" {playerNumberads}/{totalAds}";
        }
        else
        {
            txtNumberAds.text = $"Free";

        }
    }


}
