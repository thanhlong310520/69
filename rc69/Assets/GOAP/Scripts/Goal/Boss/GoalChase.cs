using System.Collections.Generic;
using UnityEngine;

public class GoalChase : GoapGoal
{
    public override float Priority(WorldStateData ws)
    {
        return 100;
    }
    public override bool IsValid(WorldStateData worldState)
    {
        return worldState.Get(GoapWorldStateEnum.Stealed.ToString());
    }
    public override Dictionary<string, bool> DesiredState =>
        new Dictionary<string, bool> {
            { GoapWorldStateEnum.Stealed.ToString(), false }
        };
}
