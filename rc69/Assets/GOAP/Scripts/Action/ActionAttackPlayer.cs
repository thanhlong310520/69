using Raccoon.Building;
using Raccoon.Controller;
using Raccoon.Item;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActionAttackPlayer : GoapAction
{
    [SerializeField] private AiMovement movement;
    [SerializeField] private Transform target;
    [SerializeField] private NpcController controller;

    public float threshold = 1f;
    bool isMoving = false;
    private bool done = false;

    bool attacking = false;
    public float timeDelay = 1.25f;
    float currentTime = 0;

    public float timeFollowAttack = 10;
    float currentTimeFollow = 0;

    public UnityEvent saveEvent;

    void Awake()
    {
        effects[GoapWorldStateEnum.AttackPlayer.ToString()] = true;
        isMoving = false;
        Cost = 1f;
    }

    public override bool CheckProceduralPrecondition(WorldStateData ws)
    {
        // Get target
        if (target == null) target = PlayerController.instance.transform;
        return target != null;
    }

    public override bool Perform(WorldStateData ws)
    {
        if (target == null) 
        {
            DoneAttack(ws);
            return false; 
        }

        if(ShieldHomePlayer.instance.homePlayerContact.isPlayerInHome)
        {
            DoneAttack(ws);
            return false;
        }

        if (attacking)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= timeDelay)
            {
                DoneAttack(ws);
                List<ItemCtr> itemDrops = HolderItemDropped.instance.CheckItemNearTransform(controller.transform);
                if(itemDrops.Count > 0)
                {
                    controller.OnLoot(itemDrops[0].transform);
                }
            }
            return true;
        }

        currentTimeFollow -= Time.deltaTime;

        if(currentTimeFollow < 0)
        {
            DoneAttack(ws);
            return false;
        }

        if (!isMoving)
        {
            
            if (!controller.ChangeItemWeapon())
            {

                DoneAttack(ws);
                return false;
            }
            isMoving = true;

        }
        if (Vector3.Distance(transform.position, target.position) < threshold)
        {
            if (!attacking)
            {
                if(controller.CheckAttack())
                {
                    movement.StopMove();
                    attacking = true;
                }
            }
        }
        else
        {
            if (!movement.MoveToTarget(target.position))
            {
                DoneAttack(ws);
                return false;
            }
        }

        return true;
    }
    public void DoneAttack(WorldStateData ws)
    {
        done = true;
        ws.Set(GoapWorldStateEnum.Stealed.ToString(), false);
    }

    public override bool IsDone() => done;
    public override void Reset()
    {
        done = false;
        isMoving = false;
        attacking = false;
        currentTime = 0;
        currentTimeFollow = UnityEngine.Random.Range(timeFollowAttack - 2, timeFollowAttack + 2);
    }
}

