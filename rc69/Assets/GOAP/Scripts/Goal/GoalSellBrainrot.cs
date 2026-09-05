using System.Collections.Generic;

public class GoalSellBrainrot : GoapGoal
{
    public override float Priority(WorldStateData ws)
    {
        return UnityEngine.Random.Range(85, 110);
    }
    public override bool IsValid(WorldStateData worldState)
    {
        //if (worldState.Get(GoapWorldStateEnum.HaveSlotFree.ToString())) return false;
        return worldState.Get(GoapWorldStateEnum.HaveBrainRot.ToString());
    }
    public override Dictionary<string, bool> DesiredState =>
        new Dictionary<string, bool> {
            { GoapWorldStateEnum.SellBrainrot.ToString(), true }
        };
}