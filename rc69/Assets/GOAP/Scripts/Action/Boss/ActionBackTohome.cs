using Raccoon.Controller;
using UnityEngine;
using UnityEngine.Events;

public class ActionBackTohome : GoapAction
{
    [SerializeField] private AiMovement movement;
    [SerializeField] private Transform target;
    [SerializeField] private BossController controller;
    public float threshold = 0.5f;
    bool isMoving = false;
    private bool done = false;

    
    public UnityEvent upgradeEvent;

    void Awake()
    {
        effects[GoapWorldStateEnum.Idle.ToString()] = true;
        isMoving = false;
        Cost = 1f;
    }

    public override bool CheckProceduralPrecondition(WorldStateData ws)
    {
        target = controller.bosshandle.stayTransform;
        if (!ws.Get(GoapWorldStateEnum.Stealed.ToString()))
            return true;
        return false;
    }
    public override bool Perform(WorldStateData ws)
    {
        if (target == null) { done = true; return false; }

        if (ws.Get(GoapWorldStateEnum.Stealed.ToString())) { done = true; return false; }

        if (!isMoving)
        {
            if (!movement.MoveToTarget(target.position))
            {
                done = true;
                return false;
            }
            isMoving = true;
        }

        if (Vector3.Distance(transform.position, target.position) < threshold)
        {
            upgradeEvent.Invoke();
            movement.StopMove();
            controller.SetBoolAnimator("IsRun", false);
            controller.colli.SetActive(false);
        }
        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() { done = false; isMoving = false; }
}
