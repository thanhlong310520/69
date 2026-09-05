
// --- Ví dụ: Tấn công kẻ địch ---
using Raccoon.Controller;
using System;
using UnityEngine;
using UnityEngine.Events;

public class ActionSellBrainrot : GoapAction
{
    [SerializeField] private AiMovement movement;
    [SerializeField] private Transform target;
    [SerializeField] private NpcController controller;
    public float threshold = 0.25f;
    bool isMoving = false;
    private bool done = false;


    float timeStay;
    float currentTimeStay = 0;
    public float maxTimeStay = 5f;
    public float minTimeStay = 3f;

    public UnityEvent sellEvent;

    void Awake()
    {
        effects[GoapWorldStateEnum.SellBrainrot.ToString()] = true;
        isMoving = false;
        Cost = 1f;
    }

    public override bool CheckProceduralPrecondition(WorldStateData ws)
    {
        // Get target
        if (target == null) target = GamePlayControl.instance.GetSellShop;
        return target != null;
    }
    public override bool Perform(WorldStateData ws)
    {
        if (target == null) { done = true; return false; }

        if (!ws.Get(GoapWorldStateEnum.HaveBrainRot.ToString())) { done = true; return false; }

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

        if (Vector3.Distance(transform.position, target.position) < threshold)
        {
            currentTimeStay += Time.deltaTime;
            if (currentTimeStay >= timeStay)
            {
                sellEvent.Invoke();
                done = true;
            }
        }
        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() { done = false; isMoving = false; timeStay = UnityEngine.Random.Range(minTimeStay, maxTimeStay); currentTimeStay = 0; }
}