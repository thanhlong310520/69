
using Raccoon.Controller;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AbstractPopup : PopupCanvas
{
    protected List<ItemInventory> listItem;

    public ItemInventory itemPrefab;
    public Transform content;
    protected ItemInventory currentItem;

    public override void Show(PopupType p, UnityAction afterPopup, object obj)
    {
        base.Show(p, afterPopup, obj);
        ResetItem();
        if (listItem == null) listItem = new List<ItemInventory>();
        ShowData((CharactorData)obj);
    }

    protected virtual void ShowData(CharactorData obj)
    {

    }

    protected ItemInventory Spawn()
    {
        var newItem = Instantiate(itemPrefab, content);
        listItem.Add(newItem);
        return newItem;
    }

    protected virtual void OnClick(ItemInventory item)
    {
        if (currentItem == item) ResetItem();
        else
        {
            ResetItem();
            currentItem = item;
        }
        if (currentItem != null)
        {
            currentItem?.SetSelected(true);
        }
        
    }

    protected void ResetItem()
    {
        if (currentItem != null)
        {
            currentItem.SetSelected(false);
            currentItem = null;
        }

    }
}
