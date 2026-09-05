using System.Collections.Generic;
using UnityEngine;

public class GoalLootItem : GoapGoal
{
    public override float Priority(WorldStateData ws)
    {
        if (!ws.Get(GoapWorldStateEnum.HaveBrainRot.ToString())) return 125;
        return 80;
    }

    public override Dictionary<string, bool> DesiredState => 
        new Dictionary<string, bool> { 
            { GoapWorldStateEnum.SaveLoot.ToString(), true }
        };
}
