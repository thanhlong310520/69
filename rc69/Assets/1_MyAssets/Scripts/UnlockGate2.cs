using Raccoon.Controller;
using UnityEngine;
using UnityEngine.Events;

public class UnlockGate2 : MonoBehaviour, InteractObjectByTrigger
{
    Transform user;
    long costUnlock;
    public void Init(Transform user, long costUnlock)
    {
        this.user = user;
        this.costUnlock = costUnlock;
    }
    public float timeDelayOpen = 1f;
    protected float currentTime = 0f;

    protected bool waitOpen = false;


    protected UnityAction<bool> ActionShowWaitOpen;
    protected UnityAction<float> ActionChangeAmountWait;
    protected UnityAction<Home> AcitonUnLock;


    protected virtual void Start()
    {
        waitOpen = false;
    }
    public virtual void HandleStartInteract(Transform contactor)
    {
        if(contactor != user) { return; }

        Debug.Log("start contact " + contactor);
        waitOpen = true;
        currentTime = 0;
        ShowUIWait(true);
    }

    public virtual void HandleStopInteract(Transform contactor)
    {

        if (contactor != user) { return; }
        Debug.Log("end contact " + contactor);
        if (!waitOpen) return;
        waitOpen = false;
        ShowUIWait(false);
    }

    public virtual void Raise()
    {
        ChangeAmountWait(currentTime, timeDelayOpen);
        currentTime += Time.deltaTime;
        if (currentTime > timeDelayOpen)
        {
            waitOpen = false;
            ShowUIWait(false);
            ShowUIOpen();
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

    void ShowUIWait(bool isShow)
    {
        ActionShowWaitOpen?.Invoke(isShow);
    }
    void ChangeAmountWait(float curent, float max)
    {
        float amount = Mathf.Clamp01(curent / max);
        ActionChangeAmountWait?.Invoke(amount);
    }

    protected virtual void ShowUIOpen()
    {
        user.GetComponent<ICharactor>()?.ContactUnlockShop(costUnlock);
    }

    private void OnDisable()
    {
        waitOpen = false;
        ActionShowWaitOpen = null;
        ActionChangeAmountWait = null;
        AcitonUnLock = null;
    }

}
