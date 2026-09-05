using Raccoon.Controller;
using System.Collections.Generic;
using UnityEngine;

public class GoalAttackPlayer : GoapGoal
{
    public NpcController npcController;
    public override float Priority(WorldStateData ws)
    {
        return 200;
    }
    public override bool IsValid(WorldStateData worldState)
    {
        if (!worldState.Get(GoapWorldStateEnum.Stealed.ToString())) return false;

        if (PlayerController.instance == null) return false;
        if (PlayerController.instance.stateHandle.GetPlayerState == EPlayerState.KnockBack) return false;
        if (PlayerController.instance.stateHandle.GetPlayerState == EPlayerState.Dead) return false;
        return true;
    }
    public override Dictionary<string, bool> DesiredState =>
        new Dictionary<string, bool> {
            { GoapWorldStateEnum.AttackPlayer.ToString(), true }
        };
}
