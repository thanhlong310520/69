using Raccoon.Building;
using UnityEngine;
using UnityEngine.Events;

public class ActionBrainrotPushToSlot: GoapAction
{
    [SerializeField] private AiMovement movement;
    [SerializeField] private SlotController target;
    [SerializeField] private NpcController controller;
    public float threshold = 1f;
    bool isMoving = false;
    private bool done = false;

    public UnityEvent<SlotController> pushEvent;

    public UnityEvent startPerformEvent;

    void Awake()
    {
        preconditions[GoapWorldStateEnum.HaveSlotFree.ToString()] = true;
        preconditions[GoapWorldStateEnum.HaveBrainRot.ToString()] = true;

        effects[GoapWorldStateEnum.PushToSlot.ToString()] = true;
        isMoving = false;
        Cost = 1f;
    }

    public override bool CheckProceduralPrecondition(WorldStateData ws)
    {
        // Get target
        target = controller.GetSlotFree();
        return target != null;
    }
    public override bool Perform(WorldStateData ws)
    {
        if (target == null) { done = true; return false; }

        if (!ws.Get(GoapWorldStateEnum.HaveSlotFree.ToString())) { done = true; return false; }
        if (!ws.Get(GoapWorldStateEnum.HaveBrainRot.ToString())) { done = true; return false; }

        if (!isMoving)
        {
            startPerformEvent.Invoke();
            if (!movement.MoveToTarget(target.transform.position))
            {
                done = true;
                Debug.Log("comming false");
                return false;
            }
            isMoving = true;
        }

        if (Vector3.Distance(transform.position, target.transform.position) < threshold)
        {
            if (target == null) return false;

            pushEvent?.Invoke(target);
            done = true;
        }
        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() { done = false; isMoving = false; }
}
