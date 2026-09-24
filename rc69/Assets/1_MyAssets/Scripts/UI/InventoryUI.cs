using Raccoon;
using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.Item;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public ItemInventory prefab;
    public Transform constant;
    public int numberSlot = 8;
    public List<ItemInventory> slots = new List<ItemInventory>();

    private ItemInventory currentSelectedSlot;


    private void Start()
    {
        currentSelectedSlot = null;
        if (slots.Count <= 0) CreateSlot();
        foreach (var slot in slots)
        {
            slot.SetEventClick(OnSlotClicked);
            slot.SetSelected(false);
        }
    }

    public void OnSetDataForSlot(List<ItemConfig> data)
    {
        if (slots.Count <= 0) CreateSlot();
        for (int i = 0; i< slots.Count; i++)
        {
            slots[i].gameObject.SetActive(false);
            if(i < data.Count)
            {
                if (data[i] == null) continue;

                ItemDataSO so = GamePlayControl.instance.GetItemDataSO(data[i].id);

                if (so == null) continue;

                slots[i].SetData(so, data[i], 1);
                slots[i].gameObject.SetActive(true);
            }
        }
    }

    void CreateSlot()
    {
        int count = 1;
        while(count <= numberSlot)
        {
            count++;
            var newSlot = Instantiate(prefab, constant);
            slots.Add(newSlot);
        }
    }
    public void ResetItem()
    {
        if(currentSelectedSlot != null)
        {
            currentSelectedSlot.SetSelected(false);
            currentSelectedSlot = null;
        }

    }


    void OnSlotClicked(ItemInventory slot)
    {
        if (!PlayerController.instance.CanClickItemInventory()) return;
        ItemConfig config = null;
        if (currentSelectedSlot == slot)
        {
            ResetItem();
        }
        else
        {
            ResetItem();
            currentSelectedSlot = slot;
        }

        if (currentSelectedSlot != null && currentSelectedSlot.itemConfig != null)
        {
            config = slot.itemConfig;
            currentSelectedSlot?.SetSelected(true);
        }

        PlayerController.instance.stateHandle.ChangeItem(config);
    }


    public void SetSelectItemByConfig(ItemConfig config)
    {
        ResetItem();
        if (config == null) return;
        foreach (var item in slots)
        {
            if(item.itemConfig.id == config.id && item.itemConfig.level == config.level)
            {
                currentSelectedSlot = item;
                currentSelectedSlot?.SetSelected(true);
                break;
            }
        }
    }
}
