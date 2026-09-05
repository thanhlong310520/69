using UnityEngine;
using UnityEngine.Events;

public class ShowLoadingAds : MonoBehaviour
{
    public float timeLoad = 3f;
    public GameObject goLoading;
    float currentTime;
    bool canLoad = false;
    UnityAction AfterShow;

    private void Start()
    {
        Hide();
    }

    // Update is called once per frame
    void Update()
    {
        if (!canLoad) return;
        currentTime += Time.deltaTime;
        if(currentTime >= timeLoad)
        {
            Hide();
        }
    }

    void Hide() 
    {
        goLoading.SetActive(false);
        canLoad = false;
        currentTime = 0;
        AfterShow?.Invoke();
    }

    public void Show(UnityAction actionAfterShow)
    {
        goLoading?.SetActive(true);
        canLoad = true;
        currentTime = 0;
        //currentTime = timeLoad;
        AfterShow = actionAfterShow;
    }

}
