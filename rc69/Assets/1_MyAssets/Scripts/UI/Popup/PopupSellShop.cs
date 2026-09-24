using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.Item;
using Raccoon.Utils;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class PopupSellShop : AbstractPopup
{
    public Button buttonEgg;
    public Button buttonBrainrot;
    public TMP_Text txtTotalSelected;

    CharactorData data;
    ItemType currentType = ItemType.Egg;
    List<ItemInventory> selectedItems = new List<ItemInventory>();

    public bool descending = true;

    private void Start()
    {
        buttonEgg.onClick.AddListener(() => ClickTab(ItemType.Egg));
        buttonBrainrot.onClick.AddListener(() => ClickTab(ItemType.Brainrot));
    }

    protected override void ShowData(CharactorData obj)
    {
        data = obj;
        UpdateTabSelected();
        ShowItem();
    }

    void ClickTab(ItemType type)
    {
        if (currentType == type) return;
        currentType = type;
        UpdateTabSelected();
        ResetItem();
        ShowItem();
        
    }

    void UpdateTabSelected()
    {
        buttonEgg.interactable = currentType != ItemType.Egg;
        buttonBrainrot.interactable = currentType != ItemType.Brainrot;
    }

    void ShowItem()
    {
        if (data == null) return;

        selectedItems.Clear();
        UpdateTotalSelected();

        List<ItemConfig> listConfig = data.itemConfigs
            .Where(cf => GamePlayControl.instance.GetItemDataSO(cf.id)?.itemType == currentType)
            .ToList();

        for (int i = 0; i < listItem.Count; i++)
        {
            if (i < listConfig.Count)
            {
                ItemDataSO so = GamePlayControl.instance.GetItemDataSO(listConfig[i].id);
                listItem[i].SetData(so, listConfig[i], 1);
                listItem[i].SetSelected(false);
                listItem[i].ShowNumber(so.cost.ConvertCurrencyToString());
                listItem[i].gameObject.SetActive(true);
            }
            else
            {
                listItem[i].gameObject.SetActive(false);
            }
        }

        for (int i = listItem.Count; i < listConfig.Count; i++)
        {
            ItemDataSO so = GamePlayControl.instance.GetItemDataSO(listConfig[i].id);
            ItemInventory item = Spawn();
            item.SetEventClick(OnClick);
            item.SetData(so, listConfig[i], 1);
            item.SetSelected(false);
            item.ShowNumber(so.cost.ConvertCurrencyToString());
            item.gameObject.SetActive(true);
        }
    }

    public void SortByScaleSize()
    {
        listItem = descending
            ? listItem.OrderByDescending(item => GameUtils.ScaleSize(item.data.rarityType)).ToList()
            : listItem.OrderBy(item => GameUtils.ScaleSize(item.data.rarityType)).ToList();
        ApplySiblingOrder();
    }

    public void SortByCost()
    {
        listItem = descending
            ? listItem.OrderByDescending(item => item.data.cost).ToList()
            : listItem.OrderBy(item => item.data.cost).ToList();
        ApplySiblingOrder();
    }

    public void SelectAll()
    {
        selectedItems.Clear();
        foreach (var item in listItem)
        {
            if (!item.gameObject.activeSelf) continue;
            item.SetSelected(true);
            selectedItems.Add(item);
        }
        UpdateTotalSelected();
    }

    public long GetTotalSelectedCost()
    {
        return selectedItems.Sum(item => item.data.cost);
    }

    void UpdateTotalSelected()
    {
        if (txtTotalSelected == null) return;
        txtTotalSelected.text = GetTotalSelectedCost().ConvertCurrencyToString();
    }

    void ApplySiblingOrder()
    {
        for (int i = 0; i < listItem.Count; i++)
        {
            listItem[i].transform.SetSiblingIndex(i);
        }
    }

    protected override void OnClick(ItemInventory item)
    {
        if (selectedItems.Contains(item))
        {
            selectedItems.Remove(item);
            item.SetSelected(false);
        }
        else
        {
            selectedItems.Add(item);
            item.SetSelected(true);
        }
        UpdateTotalSelected();
    }

    public void OnClickSell()
    {
        if (selectedItems.Count <= 0) return;
        List<ItemConfig> itemConfigs  = new List<ItemConfig>();
        foreach (var item in selectedItems)
        {
            itemConfigs.Add(item.itemConfig);
        }

        PlayerController.instance.SellItemByListItemConfig(itemConfigs);
        ShowData(data);
    }
}
