using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossInSteal
{
    public Transform stayTransform;
    public List<Transform> listStealer;

    [SerializeField] BossState state;
    public BossState GetState => state;

    public GoapAgent goapAgent;

    UnityAction<GoapWorldStateEnum,bool> actionSetWorldState;

    public BossInSteal(Transform stayTransform, GoapAgent goapAgent, UnityAction<GoapWorldStateEnum, bool> actionSetWorldState)
    {
        this.goapAgent = goapAgent;
        this.actionSetWorldState = actionSetWorldState;
        state = BossState.Normal;
        this.stayTransform = stayTransform;
        listStealer = new List<Transform>();
    }

    #region Stealed
    public void ChangeState(BossState state)
    {
        if (this.state == state) return;

        this.state = state;
    }

    public void AddStealer(Transform stealer)
    {
        if (listStealer.Contains(stealer)) return;

        listStealer.Add(stealer);
    }

    public void RemoveStealer(Transform stealer)
    {
        listStealer.Remove(stealer);
    }

    public void Update()
    {
        if (listStealer.Count > 0)
        {
            actionSetWorldState?.Invoke(GoapWorldStateEnum.Stealed, true);
            if (state == BossState.Normal)
            {
                goapAgent.ForceReplan();
                state = BossState.EggStealed;
            }
        }
        else
        {
            actionSetWorldState?.Invoke(GoapWorldStateEnum.Stealed, false);
            state = BossState.Normal;
        }
    }
    #endregion
}
