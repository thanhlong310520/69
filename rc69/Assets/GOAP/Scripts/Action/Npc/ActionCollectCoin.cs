
// --- Ví dụ: Tấn công kẻ địch ---
using Raccoon.Building;
using Raccoon.Controller;
using Raccoon.EnumHolder;
using Raccoon.Item;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActionCollectCoin : GoapAction
{
    [SerializeField] private AiMovement movement;
    [SerializeField] private NpcController controller;
    private List<SlotController> slots;

    public float threshold = 0.25f;
    bool isMoving = false;
    private bool done = false;

    public UnityEvent<SlotController> DoneMoveToSlotEvent = null;

    int index = 0;
    void Awake()
    {
        effects[GoapWorldStateEnum.CollectCoin.ToString()] = true;
        isMoving = false;
        Cost = 1f;
    }

    public override bool CheckProceduralPrecondition(WorldStateData ws)
    {
        // Get target
        slots = new List<SlotController>();
        slots.AddRange(controller.GetSlotIncomes);
        return slots.Count > 0;
    }
    public override bool Perform(WorldStateData ws)
    {
        if (index >= slots.Count)
        {
            movement.StopMove();
            done = true;
        }
        else
        {
            if (!isMoving)
            {
                if (!movement.MoveToTarget(slots[index].transform.position))
                {
                    done = true;
                    return false;
                }
                isMoving = true;

            }
            if (slots[index].GetSlot_State != Slot_state.HaveItem)
            {
                index++;
                isMoving = false;
            }
            else
            {
                if (Vector3.Distance(transform.position, slots[index].transform.position) < threshold && isMoving)
                {
                    DoneMoveToSlotEvent?.Invoke(slots[index]);
                    index++;
                    isMoving = false;
                }
            }
        }

        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() { done = false; isMoving = false; index = 0; }
}