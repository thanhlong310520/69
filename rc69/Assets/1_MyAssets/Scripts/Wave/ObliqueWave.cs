using UnityEngine;

public class ObliqueWave : Wave
{
    public float speedMoveModel = 5;

    float direction = 1;

    public override void Init(Transform startPos, Transform endPos)
    {
        base.Init(startPos, endPos);
        float ran = UnityEngine.Random.Range(-1f, 1f);
        if (ran > 0) direction = 1;
        else direction = -1;
    }
    protected override void MoveModel()
    {
        model.localPosition = model.localPosition + new Vector3(speedMoveModel * direction * Time.deltaTime, 0f, 0f);
    }
}
