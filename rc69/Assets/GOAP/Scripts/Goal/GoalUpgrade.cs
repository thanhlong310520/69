using System.Collections.Generic;

public class GoalUpgrade : GoapGoal
{
    public override float Priority(WorldStateData ws)
    {
        return UnityEngine.Random.Range(85, 125);
    }
    public override bool IsValid(WorldStateData worldState)
    {
        return worldState.Get(GoapWorldStateEnum.HaveUpgrade.ToString());
    }
    public override Dictionary<string, bool> DesiredState =>
        new Dictionary<string, bool> {
            { GoapWorldStateEnum.GotoUpgrade.ToString(), true }
        };
}