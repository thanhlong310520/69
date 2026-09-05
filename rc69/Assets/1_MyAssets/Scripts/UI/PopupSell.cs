using Raccoon.Controller;
using UnityEngine;

public class PopupSell : PopupCanvas
{
    public void SellCurrentBrainrot()
    {
        PlayerController.instance.Sell(PlayerSellType.CurrentBrainrot);
        Hide();
    }
    public void SellAllBrainrot()
    {
        PlayerController.instance.Sell(PlayerSellType.AllBrainrot);
        Hide();
    }
}
