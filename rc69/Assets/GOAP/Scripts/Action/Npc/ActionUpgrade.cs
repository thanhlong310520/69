using Raccoon.Controller;
using UnityEngine;
using UnityEngine.Events;

public class ActionUpgrade : GoapAction
{
    [SerializeField] private AiMovement movement;
    [SerializeField] private Transform target;
    [SerializeField] private NpcController controller;
    public float threshold = 0.5f;
    bool isMoving = false;
    private bool done = false;

    float timeStay;
    float currentTimeStay = 0;
    public float maxTimeStay = 3f;
    public float minTimeStay = 2f;

    public UnityEvent upgradeEvent;

    void Awake()
    {
        effects[GoapWorldStateEnum.GotoUpgrade.ToString()] = true;
        isMoving = false;
        Cost = 1f;
    }

    public override bool CheckProceduralPrecondition(WorldStateData ws)
    {
        // Get target
        if (target == null) target = GamePlayControl.instance.GetUpgradeShop;
        return target != null;
    }
    public override bool Perform(WorldStateData ws)
    {
        if (target == null) { done = true; return false; }

        if (!ws.Get(GoapWorldStateEnum.HaveUpgrade.ToString())) { done = true; return false; }

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
            currentTimeStay += Time.deltaTime;
            if (currentTimeStay >= timeStay)
            {
                upgradeEvent.Invoke();
                done = true;
            }
        }
        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() { done = false; isMoving = false; timeStay = UnityEngine.Random.Range(minTimeStay, maxTimeStay); currentTimeStay = 0; }
}