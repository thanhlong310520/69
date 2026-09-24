using Raccoon.Controller;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ActionChaseStealer : GoapAction
{
    [SerializeField] private AiMovement movement;
    [SerializeField] private BossController controller;
    [SerializeField] private float startDelay = 1f;

    public UnityEvent<Transform> startPerformEvent;

    private Transform target;
    private bool done = false;
    private bool started = false;
    private float startTime;

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

        if (!started)
        {
            started = true;
            startTime = Time.time;
            target.GetComponent<ICharactor>()?.NotifyBossChase();
            controller.colli.SetActive(true);
            StartCoroutine(DelayPlayAnimRun(startDelay));
        }

        if (Time.time - startTime < startDelay)
        {
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

    IEnumerator DelayPlayAnimRun(float time)
    {
        yield return new WaitForSeconds(time);
        controller.SetBoolAnimator("IsRun", true);
    }

    public override bool IsDone() => done;
    public override void Reset()
    {
        done = false;
        target = null;
        started = false;
    }
}
