using System.Collections.Generic;

public class GoalUnlockGate : GoapGoal
{
    public override float Priority(WorldStateData ws)
    {
        return UnityEngine.Random.Range(85, 125);
    }
    public override bool IsValid(WorldStateData worldState)
    {
        if (worldState.Get(GoapWorldStateEnum.IsUnlockGate.ToString())) return false;
        if (worldState.Get(GoapWorldStateEnum.HaveSlotFree.ToString())) return false;
        return true;
    }
    public override Dictionary<string, bool> DesiredState =>
        new Dictionary<string, bool> {
            { GoapWorldStateEnum.IsUnlockGate.ToString(), true }
        };
}