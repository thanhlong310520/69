using Raccoon.Building;
using Raccoon.Controller;
using Raccoon.EnumHolder;
using Raccoon.Item;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TutPushToSlot : TutorialAction
{
    List<SlotController> playerSlot;
    public override void Init(UnityAction<Transform> actionSetup)
    {
        base.Init(actionSetup);
        playerSlot = HomeCtr.instance.GetAllSlots(PlayerController.instance.transform);
    }
    public override bool SetUp()
    {
        if (PlayerController.instance.stateHandle.GetCurrentItem == null) return false;
        if (PlayerController.instance.stateHandle.GetCurrentItem.data.itemType != Raccoon.Data.ItemType.Brainrot) return false;

        if (target != null)
        {
            foreach (var s in playerSlot)
            {
                if(target == s.transform)
                {
                    if (s.GetSlot_State != Slot_state.Free) target = null;
                }
                break;
            }
        }
        if (target == null)
        {
            var slot =  playerSlot.FirstOrDefault(s => s.GetSlot_State == Slot_state.Free);
            if (slot != null) target = slot.transform;
        }
        if(target != null) actionSetup?.Invoke(target);

        return target != null;
    }
}
