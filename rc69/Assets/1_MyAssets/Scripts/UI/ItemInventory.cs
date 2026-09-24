using Raccoon;
using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.Item;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class ItemInventory : MonoBehaviour
{
    public Image icon;
    public Image bg;
    public GameObject selected;
    public ItemDataSO data;
    public ItemConfig itemConfig;
    public TMP_Text numberTxt;
    protected UnityAction<ItemInventory> clickAciton;

    public void SetEventClick(UnityAction<ItemInventory> aciton)
    {
        clickAciton = aciton;
    }
    public virtual void SetData(ItemDataSO data,ItemConfig config, int number)
    {
        this.data = data;
        this.itemConfig = config;
        SetIcon(data);
        if (number > 1) numberTxt.text = number.ToString();
        else numberTxt.text = ""; 
    }
    void SetIcon(ItemDataSO data)
    {
        icon.sprite = data.spriteIcon;
    }
    public virtual void SetSelected(bool selected)
    {
        print("Set select " +  selected);
        this.selected.SetActive(selected);
    }
    public virtual void OnClick()
    {
        clickAciton?.Invoke(this);
    }
    public void ChangeBg(Sprite bg)
    {
        //this.bg.sprite = bg;    
    }

    public void ShowNumber(string value)
    {
        numberTxt.text = value;
    }
}
