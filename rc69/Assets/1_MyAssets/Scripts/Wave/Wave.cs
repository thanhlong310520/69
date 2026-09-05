using Raccoon.Controller;
using UnityEngine;

public class Wave : MonoBehaviour
{
    public float speed;
    public float threshold = 0.5f;
    Vector3 endPos;
    bool isMove = false;

    public Transform model;
    public virtual void Init(Transform startPos, Transform endPos)
    {
        transform.position = startPos.position;
        this.endPos = endPos.position;
        isMove = true;
        model.localPosition = Vector3.zero;
    }

    private void Update()
    {
        if (!isMove) return;
        if (CheckFinishMove())
        {
            EndMove();
        }
        Move();
        MoveModel();
    }
    protected void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position, endPos, speed * Time.deltaTime);
    }

    bool CheckFinishMove()
    {
        return Vector3.Distance(transform.position, endPos) <= threshold;
    }

    void EndMove()
    {
        isMove = false;
        PoolByID.Instance.PushToPool(gameObject);
    }

    protected virtual void MoveModel()
    {

    }



}
