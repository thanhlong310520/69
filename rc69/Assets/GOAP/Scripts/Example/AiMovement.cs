
using UnityEngine;
using UnityEngine.AI;

public class AiMovement : MonoBehaviour
{
    public NavMeshAgent agent;

    public bool MoveToTarget(Vector3 pos)
    {
        Vector3 newpos = pos;
        if (!agent.isOnNavMesh) return false;
        if (!SamplePosition(transform.position, out newpos)) return false;

        NavMeshPath path = new NavMeshPath();

        bool foundPath = NavMesh.CalculatePath(agent.transform.position, newpos, NavMesh.AllAreas, path);

        if (foundPath && path.status == NavMeshPathStatus.PathComplete)
        {
            agent.isStopped = false;
            agent.SetDestination(pos);
            return true;
        }

        return false;

    }

    public void StopMove()
    {
        agent.isStopped = true;
    }

    private void Update()
    {
        if (!agent.isOnNavMesh) SetPos();


    }
    void SetPos()
    {
        Vector3 newpos;
        if(SamplePosition(transform.position, out newpos, 1))
        {
            agent.Warp(newpos);
        }
    }
    bool SamplePosition(Vector3 pos, out Vector3 newPos, float dis = 0.25f)
    {
        NavMeshHit hit;
        newPos = pos;
        bool result = false;
        if (NavMesh.SamplePosition(transform.position, out hit, dis, NavMesh.AllAreas))
        {
            newPos =  hit.position;
            result = true;
        }
        return result;
    }

    public Vector3 GetRandomReachablePoint(float randomRadius = 5)
    {
        for (int i = 0; i < 10; i++)
        {
            // random điểm xung quanh agent
            Vector3 randomDirection = Random.insideUnitSphere * randomRadius;
            randomDirection += agent.transform.position;

            // tìm điểm gần nhất trên NavMesh
            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();

                // kiểm tra có đường đi không
                bool hasPath = agent.CalculatePath(hit.position, path);

                if (hasPath && path.status == NavMeshPathStatus.PathComplete)
                {
                    return hit.position;
                }
            }
        }

        // nếu không tìm được
        return agent.transform.position;
    }
}
