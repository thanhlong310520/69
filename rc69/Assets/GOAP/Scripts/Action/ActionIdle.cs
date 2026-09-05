using Raccoon.Controller;
using UnityEngine;
using UnityEngine.Events;

public class ActionIdle : GoapAction
{
    [SerializeField] private AiMovement movement;
    public float threshold = 1f;
    bool isMoving = false;
    private bool done = false;
    Vector3 movePos;

    float timeStay;
    float currentTimeStay = 0;
    public float maxTimeStay = 5f;
    public float minTimeStay = 3f;

    void Awake()
    {
        effects[GoapWorldStateEnum.Idle.ToString()] = true;
        isMoving = false;
        Cost = 1f;
    }

    public override bool CheckProceduralPrecondition(WorldStateData ws)
    {
        // Get target
        return true;
    }
    public override bool Perform(WorldStateData ws)
    {
        
        if (!isMoving)
        {
            print("Start idle");
            movePos = movement.GetRandomReachablePoint();
            if (!movement.MoveToTarget(movePos))
            {
                //Debug.Log("target false");
                done = true;
                return false;
            }
            isMoving = true;

        }
        if (Vector3.Distance(transform.position, movePos) < threshold)
        {
            currentTimeStay += Time.deltaTime;
            if(currentTimeStay >= timeStay)
            {
                done = true;
            }
        }
        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() { done = false; isMoving = false; timeStay = UnityEngine.Random.Range(minTimeStay, maxTimeStay); currentTimeStay = 0; }
}