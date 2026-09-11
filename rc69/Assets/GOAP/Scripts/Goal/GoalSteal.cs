using Raccoon.Building;
using Raccoon.Controller;
using System.Collections.Generic;
using UnityEngine;

public class GoalSteal : GoapGoal
{
    public NpcController npcController;
    public override float Priority(WorldStateData ws)
    {
        return UnityEngine.Random.Range(40, 125);
    }
    public override bool IsValid(WorldStateData worldState)
    {
        if (!npcController.canSteal) return false;
        if (ShieldHomePlayer.instance.IsShield) return false;
        Home home = HomeCtr.instance.GetHomeByUser(PlayerController.instance.transform);
        foreach (var s in home.AllSlots)
        {
            if (s.GetSlot_State == Raccoon.Building.Slot_state.HaveItem)
            {
                return true;
            }
        }
        return false;
    }
    public override Dictionary<string, bool> DesiredState =>
        new Dictionary<string, bool> {
            { GoapWorldStateEnum.Steal.ToString(), true }
        };
}

