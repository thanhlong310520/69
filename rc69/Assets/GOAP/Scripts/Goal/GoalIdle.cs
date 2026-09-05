using System.Collections.Generic;

public class GoalIdle : GoapGoal
{
    public override float Priority(WorldStateData ws)
    {
        if (!ws.Get(GoapWorldStateEnum.HaveBrainRot.ToString())) return UnityEngine.Random.Range(1, 110);
        return UnityEngine.Random.Range(40, 125);
    }
    public override bool IsValid(WorldStateData worldState)
    {
        return true;
    }
    public override Dictionary<string, bool> DesiredState =>
        new Dictionary<string, bool> {
            { GoapWorldStateEnum.Idle.ToString(), true }
        };
}