using Raccoon.Building;
using Raccoon.Controller;
using UnityEngine;
using UnityEngine.Events;

public class ActionSteal : GoapAction
{
    [SerializeField] private AiMovement movement;
    [SerializeField] private Transform target;
    [SerializeField] private NpcController controller;

    SlotController slot;
    public float threshold = 1f;
    bool isMoving = false;
    private bool done = false;

    bool gotoSlot = false;

    public UnityEvent saveEvent;

    void Awake()
    {
        effects[GoapWorldStateEnum.Steal.ToString()] = true;
        isMoving = false;
        Cost = 1f;
    }

    public override bool CheckProceduralPrecondition(WorldStateData ws)
    {
        // Get target
        if (target == null) target = controller.GetStartPos();
        CheckSlot();
        return target != null && slot != null;
    }

    void CheckSlot()
    {
        slot = null;
        Home home = HomeCtr.instance.GetHomeByUser(PlayerController.instance.transform);
        foreach (var s in home.AllSlots)
        {
            if (s.GetSlot_State == Raccoon.Building.Slot_state.HaveItem)
            {
                slot = s; break;
            }
        }
    }
    public override bool Perform(WorldStateData ws)
    {
        
        if (gotoSlot)
        {
            if (slot == null) { done = true; return false; }
            if (slot.GetSlot_State != Slot_state.HaveItem) { done = true; return false; }
            if (ShieldHomePlayer.instance.IsShield) { done = true; return false; }

            if (!isMoving)
            {
                if (!movement.MoveToTarget(slot.transform.position))
                {
                    done = true;
                    return false;
                }
                MainUiControl.instance.WarringSteal();
                isMoving = true;
            }
            if (Vector3.Distance(transform.position, slot.transform.position) < threshold)
            {
                if (slot == null) { done = true; return false; }
                if (slot.GetSlot_State != Slot_state.HaveItem) { done = true; return false; }
                controller.StealBrainrotInSlot(slot);
                gotoSlot = false;
                isMoving = false;
            }
        }
        else
        {
            if (target == null) { done = true; return false; }
            if (!isMoving)
            {
                if (!movement.MoveToTarget(target.position))
                {
                    //Debug.Log("target false");
                    done = true;
                    return false;
                }
                isMoving = true;

            }
            if (Vector3.Distance(transform.position, target.position) < threshold)
            {
                controller.StealDone();
                done = true;
            }
        }

        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() { 
        done = false; 
        isMoving = false;
        gotoSlot = true;
    }
}
