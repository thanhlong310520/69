using System.Linq;
using UnityEngine;

public class ActionChaseStealer : GoapAction
{
    [SerializeField] private AiMovement movement;
    [SerializeField] private BossController controller;

    private Transform target;
    private bool done = false;

    void Awake()
    {
        preconditions[GoapWorldStateEnum.Stealed.ToString()] = true;
        effects[GoapWorldStateEnum.Stealed.ToString()] = false;
        Cost = 1f;
    }

    public override bool CheckProceduralPrecondition(WorldStateData ws)
    {
        return controller.GetListChase()!= null && controller.GetListChase().Count > 0;
    }

    public override bool Perform(WorldStateData ws)
    {
        if (controller.GetListChase().Count == 0)
        {
            Debug.Log("no chase");
            movement.StopMove();
            done = true;
            return true;
        }

        target = GetStealer();
        if (target == null)
        {
            Debug.Log("no chase 1");

            movement.StopMove();
            done = true;
            return true;
        }

        movement.MoveToTarget(target.position);
        return true;
    }

    Transform GetStealer()
    {
        if (controller.GetListChase() == null) return null;
        if (controller.GetListChase().Count <= 0) return null;
        return controller.GetListChase()[0];
    }

    public override bool IsDone() => done;
    public override void Reset()
    {
        done = false;
        target = null;
    }
}
