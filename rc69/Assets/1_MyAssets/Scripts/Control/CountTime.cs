using UnityEngine;
using UnityEngine.Events;

public class CountTime : MonoBehaviour
{
    float timeCount = 1;
    bool canCount = false;
    UnityAction actionCountDone;
    UnityAction<float> actionCount;
    public void Init(float timeCount,UnityAction<float> actionCount, UnityAction actionCountDone)
    {
        this.timeCount = timeCount;
        this.actionCount = actionCount;
        this.actionCountDone = actionCountDone;
        this.SetCanCount(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (canCount)
        {
            timeCount -= Time.deltaTime;
            actionCount?.Invoke(timeCount);
            if (timeCount < 0)
            {
                timeCount = 0;
                canCount = false;
                actionCountDone?.Invoke();
            }
        }
    }


    public void SetCanCount(bool cancount)
    {
        canCount = cancount;
    }
}
