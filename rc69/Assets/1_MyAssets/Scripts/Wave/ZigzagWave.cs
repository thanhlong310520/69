using UnityEngine;

public class ZigzagWave : Wave
{
    public float speedMoveModel = 5;

    public float maxX;

    float direction = 1;

    Vector3 target;
    public override void Init(Transform startPos, Transform endPos)
    {
        base.Init(startPos, endPos);
        float ran = UnityEngine.Random.Range(-1f, 1f);
        if (ran > 0) direction = 1;
        else direction = -1;
        ChangeTarget();
    }

    void ChangeTarget()
    {
        direction *= (-1);
        target = new Vector3(maxX * direction, model.localPosition.y, model.localPosition.z);
    }
    protected override void MoveModel()
    {
        model.localPosition = Vector3.MoveTowards(model.localPosition, target, speed * Time.deltaTime);
        if (Vector3.Distance(model.localPosition, target) <= threshold) ChangeTarget();
    }
}
