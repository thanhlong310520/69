using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class NpcMove : MonoBehaviour
{
    float turnAmount;
    float forwardAmount;
    public float baseSpeed = 3.5f;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] UnityEvent<float, float, bool> actionAnimMove;
    [SerializeField] UnityEvent<float> actionSetMultiSpeed;


    public void SetMultiSpeed(float multi)
    {
        actionSetMultiSpeed.Invoke(Mathf.Clamp(multi / 20, 0.75f, multi));
        agent.speed = baseSpeed * (multi/10 + 1);
    }
    public void Init(NavMeshAgent agent)
    {
        this.agent = agent;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (agent == null) return;
        Vector3 move = agent.velocity;
        Move(move);
    }

    public void StopMove()
    {
        agent.isStopped = true;
    }
    public void ResetMove()
    {
        agent.isStopped = false;
    }

    public void Move(Vector3 move)
    {
        move = transform.InverseTransformDirection(move);

        move = Vector3.ProjectOnPlane(move, Vector3.up);
        turnAmount = Mathf.Atan2(move.x, move.z);
        forwardAmount = Mathf.Abs(move.z);
        //print("turnAmount " + forwardAmount);

        actionAnimMove?.Invoke(forwardAmount, turnAmount, true);

    }
}
