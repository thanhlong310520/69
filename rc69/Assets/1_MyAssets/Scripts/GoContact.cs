using Raccoon.Controller;
using UnityEngine;
using UnityEngine.Events;

public class GoContact : MonoBehaviour, InteractObjectByTrigger
{
    protected Transform user;
    public void Init(Transform user)
    {
        this.user = user;
    }
    public float timeDelayOpen = 1f;
    protected float currentTime = 0f;

    protected bool waitOpen = false;


    protected UnityAction<bool> ActionShowWaitOpen;
    protected UnityAction<float> ActionChangeAmountWait;

    protected virtual void Start()
    {
        waitOpen = false;
    }
    public virtual void HandleStartInteract(Transform contactor)
    {
        if (contactor != user) { return; }

        Debug.Log("start contact " + contactor);
        waitOpen = true;
        currentTime = 0;
        ShowProgessWait(true);
    }

    public virtual void HandleStopInteract(Transform contactor)
    {

        if (contactor != user) { return; }
        Debug.Log("end contact " + contactor);
        if (!waitOpen) return;
        waitOpen = false;
        ShowProgessWait(false);
    }

    public virtual void Raise()
    {
        ChangeAmountWait(currentTime, timeDelayOpen);
        currentTime += Time.deltaTime;
        if (currentTime > timeDelayOpen)
        {
            waitOpen = false;
            ShowProgessWait(false);
            DoneWait();
        }

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (waitOpen)
        {
            Raise();
        }
    }

    void ShowProgessWait(bool isShow)
    {
        ActionShowWaitOpen?.Invoke(isShow);
    }
    void ChangeAmountWait(float curent, float max)
    {
        float amount = Mathf.Clamp01(curent / max);
        ActionChangeAmountWait?.Invoke(amount);
    }

    protected virtual void DoneWait()
    {

    }

}
