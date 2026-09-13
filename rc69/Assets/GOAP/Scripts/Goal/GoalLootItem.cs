using System.Collections.Generic;
using Raccoon.Controller;
using UnityEngine;

public class GoalLootItem : GoapGoal
{
    public override float Priority(WorldStateData ws)
    {
        if (!ws.Get(GoapWorldStateEnum.HaveBrainRot.ToString())) return 125;
        return 80;
    }

    public override bool IsValid(WorldStateData worldState)
    {
        return GamePlayControl.instance.HasAvailableItemInSpawnItem();
    }

    private static readonly Dictionary<string, bool> desiredState = new Dictionary<string, bool> {
        { GoapWorldStateEnum.SaveLoot.ToString(), true }
    };

    public override Dictionary<string, bool> DesiredState => desiredState;
}
