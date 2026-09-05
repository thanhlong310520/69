using Raccoon.Ads;
using Raccoon.Bot;
using Raccoon.Building;
using Raccoon.Controller;
using Raccoon.Item;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHandleSlot : MonoBehaviour
{
    UnityAction<SlotController,ItemCtr> actionPickup;
    UnityAction<SlotController,ItemCtr> actionSteal;
    UnityAction<SlotController, ItemCtr> actionPut;

    public void Init(UnityAction<SlotController, ItemCtr> actionPickup, UnityAction<SlotController, ItemCtr> actionPut, UnityAction<SlotController, ItemCtr> actionSteal)
    {
        this.actionPickup = actionPickup;
        this.actionPut = actionPut;
        this.actionSteal = actionSteal;
    }
    public void Handle(SlotController slot,ItemCtr item, TypeHandleSlot typeHandle)
    {
        if (slot == null) return;
        switch (typeHandle)
        {
            case TypeHandleSlot.PickupBot:
                {
                    PickupBrainrotInSlot(slot);
                    break;
                }
            case TypeHandleSlot.PutBot:
                {
                    PutBrainrotInSlot(slot,item);
                    break;
                }
            case TypeHandleSlot.Steal:
                {
                    AdsActionManager.instance.ShowRewardAds((bool success) =>
                    {
                        if (success) StealBrainrotInSlot(slot);
                    });
                    break;
                }
        }
        slot.SetHighlightObject(false);
    }
    void PickupBrainrotInSlot(SlotController slot)
    {
        if (slot == null) return;
        var bot = slot.GetBotAtSlot();
        if (bot == null) return;
        actionPickup?.Invoke(slot,bot); 
        slot.SetEmpty();
    }
    void PutBrainrotInSlot(SlotController slot, ItemCtr item)
    {
        if (slot == null) return;
        if (item == null) return;
        if(item.GetComponent<BotController>() == null) return;
        actionPut?.Invoke(slot,item);
        slot.PutBrainrot(item.GetComponent<BotController>());
    }

    void StealBrainrotInSlot(SlotController slot)
    {
        if (slot == null) return;
        var bot = slot.GetBotAtSlot();
        if (bot == null) return;
        actionSteal?.Invoke(slot, bot);
        slot.Stealed();
    }

}
