using Raccoon.Ads;
using Raccoon.Controller;
using Raccoon.Data;
using UnityEngine;
using UnityEngine.Events;

public class ShowModelInMap : MonoBehaviour, InteractObjectByTrigger
{

    public FashionDataSO data;
    public Transform holder;
    Transform user;
    Transform model;
    public void Init(Transform user, FashionDataSO data)
    {
        this.user = user;
        Setup(data);

    }
    public float timeDelayOpen = 1f;
    protected float currentTime = 0f;

    protected bool waitOpen = false;


    protected UnityAction<bool> ActionShowWaitOpen;
    protected UnityAction<float> ActionChangeAmountWait;


    protected virtual void Start()
    {
        waitOpen = false;
        ActionChangeAmountWait = MainUiControl.instance.ShowTextWaitAds;
        ActionShowWaitOpen = MainUiControl.instance.EnableTextWaitAds;
    }
    public virtual void HandleStartInteract(Transform contactor)
    {
        if (contactor != user) { return; }

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
            WatchAds();
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

    void WatchAds()
    {
        AdsActionManager.instance.ShowRewardAds((success) =>
        {
            if (success)
            {
                Handle();
            }
        });
    }

    protected virtual void Handle()
    {

        //string id = PlayerController.instance.GetCharactorData.currentFashion;
        //var so = GamePlayControl.instance.GetFashionDataById(id);
        if(data != null)
        {
            GamePlayControl.instance.ChangeModelPlayer(data);
            gameObject.SetActive(false);
        }

        //if (so == null)
        //{
        //    Destroy(gameObject);
        //    return;
        //}
        //if (so.prefab == null)
        //{
        //    Destroy(gameObject);
        //    return;
        //}

        //Setup(so);

    }

    void Setup(FashionDataSO data)
    {
        this.data = data;
        if (model != null) Destroy(model);
        if (data == null)
        {
            Destroy(gameObject);
            return;
        }
        var go = Instantiate(data.prefab, holder);
        if(go == null)
        {
            Destroy(gameObject);
            return;
        }
        model = go.transform;
        model.localPosition = Vector3.zero;
        model.localRotation = Quaternion.identity;

    }

    

    private void OnDisable()
    {
        waitOpen = false;
    }
    private void OnEnable()
    {
        
    }
}