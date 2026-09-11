
// --- Ví dụ: Tấn công kẻ địch ---
using Raccoon.Controller;
using Raccoon.EnumHolder;
using Raccoon.Item;
using UnityEngine;
using UnityEngine.Events;

public class ActionGoToLootItem : GoapAction
{
    [SerializeField] private AiMovement movement;
    [SerializeField] private ItemCtr target;
    public float threshold = 1f;
    bool isMoving = false;
    private bool done = false;

    public UnityEvent<Transform> DoneMoveTargetEvent = null;

    void Awake()
    {
        effects[GoapWorldStateEnum.HaveLoot.ToString()] = true;
        isMoving = false;
        Cost = 1f;
    }

    public override bool CheckProceduralPrecondition(WorldStateData ws)
    {
        // Get target
        target = null;
        var itemctr = GamePlayControl.instance.GetRandomItemInSpawnItem();
        if (itemctr != null) target = itemctr;
        return target != null;
    }
    public override bool Perform(WorldStateData ws)
    {
        if (target == null || !target.gameObject.activeSelf) 
        {
            target = null;
            movement.StopMove();
            done = true; 
            return false; 
        }
        if(target.GetState != ItemState.Normal)
        {
            target = null;
            movement.StopMove();
            done = true;
            return false;
        }
        if (!isMoving)
        {
            if (!movement.MoveToTarget(target.transform.position))
            {
                //Debug.Log("target false");
                done = true;
                return false;
            }
            isMoving = true;

        }
        if (Vector3.Distance(transform.position, target.transform.position) < threshold)
        {
            //print("target done ");
            DoneMoveTargetEvent?.Invoke(target.transform);
            target = null;
            done = true;
        }
        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() { done = false; isMoving = false; }
}