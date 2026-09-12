using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class BossHandle
{
    public Transform stayTransform;
    public List<Transform> listTarget;

    [SerializeField] BossState state;
    public BossState GetState => state;

    public GoapAgent goapAgent;

    UnityAction<GoapWorldStateEnum,bool> actionSetWorldState;

    public BossHandle(Transform stayTransform, GoapAgent goapAgent, UnityAction<GoapWorldStateEnum, bool> actionSetWorldState)
    {
        this.goapAgent = goapAgent;
        this.actionSetWorldState = actionSetWorldState;
        state = BossState.Normal;
        this.stayTransform = stayTransform;
        listTarget = new List<Transform>();
    }

    #region Stealed
    public void ChangeState(BossState state)
    {
        if (this.state == state) return;

        this.state = state;
    }

    public void AddTarget(Transform target)
    {
        if (listTarget.Contains(target)) return;

        listTarget.Add(target);
    }

    public void RemoveTarget(Transform target)
    {
        listTarget.Remove(target);
    }

    public void Update()
    {
        if (listTarget.Count > 0)
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
