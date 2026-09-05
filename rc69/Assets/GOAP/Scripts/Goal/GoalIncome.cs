using System.Collections.Generic;

public class GoalIncome : GoapGoal
{
    public override float Priority(WorldStateData ws)
    {
        return UnityEngine.Random.Range(40, 120);
    }
    public override bool IsValid(WorldStateData worldState)
    {
        if (!worldState.Get(GoapWorldStateEnum.HaveSlotIncome.ToString())) return false;
        return true;
    }
    public override Dictionary<string, bool> DesiredState =>
        new Dictionary<string, bool> {
            { GoapWorldStateEnum.CollectCoin.ToString(), true }
        };
}