
using Raccoon.Controller;
using System.Collections.Generic;

public class FashionPopup : AbstractPopup
{
    protected override void ShowData(CharactorData obj)
    {
        if(listItem.Count <= 0)
        {
            var data = GamePlayControl.instance.GetFashionDataSOs();
            foreach (var dt in data)
            {
                ItemInventory item = Spawn();
                item.SetEventClick(OnClick);
                item.SetData(dt, null, 1);
            }
        }

        ShowCurrentItem(obj.currentFashion);
    }
    void ShowCurrentItem(string id)
    {
        foreach (var item in listItem)
        {
            if (item.data.GetId.Equals(id))
            {
                currentItem = item;
                break;
            }
        }
        currentItem.SetSelected(true);
    }
    protected override void OnClick(ItemInventory item)
    {
        if (!PlayerController.instance.CanClickItemInventory()) return;
        if (currentItem == item) return;
        ResetItem();
        currentItem = item;
        if (currentItem != null)
        {
            currentItem?.SetSelected(true);
        }
        PlayerController.instance.ChangeModelWhenClickItemFashion(currentItem.data.GetId);

    }
}
