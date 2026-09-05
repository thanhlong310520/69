
// --- Ví dụ: Tấn công kẻ địch ---
using UnityEngine;
using UnityEngine.Events;

public class ActionSaveLoot : GoapAction
{
    [SerializeField] private AiMovement movement;
    [SerializeField] private Transform target;
    [SerializeField] private NpcController controller;
    public float threshold = 1f;
    bool isMoving = false;
    private bool done = false;

    public UnityEvent saveEvent;

    void Awake()
    {
        preconditions[GoapWorldStateEnum.HaveLoot.ToString()] = true;

        effects[GoapWorldStateEnum.SaveLoot.ToString()] = true;
        isMoving = false;
        Cost = 1f;
    }

    public override bool CheckProceduralPrecondition(WorldStateData ws)
    {
        // Get target
        if (target == null) target = controller.GetStartPos();
        return target != null;
    }
    public override bool Perform(WorldStateData ws)
    {
        if (target == null) { done = true; return false; }

        if (!ws.Get(GoapWorldStateEnum.HaveLoot.ToString())) { done = true; return false; }

        if (!isMoving)
        {
            if (!movement.MoveToTarget(target.position))
            {
                done = true;
                Debug.Log("comming false");
                return false;
            }
            isMoving = true;
        }

        if (controller.InSaveZone)
        {
            if (target == null) { done = true; return false; }

            ws.Set(GoapWorldStateEnum.SaveLoot.ToString(), false);
            saveEvent?.Invoke();
            print("Save loot");
            done = true;
        }
        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() { done = false; isMoving = false; }
}