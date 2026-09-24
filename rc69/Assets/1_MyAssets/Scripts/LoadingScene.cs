using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Raccoon;
using Raccoon.Ads;
using Raccoon.Controller;
using Raccoon.Purchase;
using RCKit.Ads.Main;

public class LoadingScene : MonoBehaviour
{
    [SerializeField] private Slider sliderLoading; // drag Image (fill bar) vào đây
    [SerializeField] private GameObject loadingUI; // drag UI loading panel vào đây
    private float multi_speed = 1f;
    [SerializeField] private List<IAPProductData> lstSubCheck;
    private void Start()
    {
        LoadScene();
    }

    private void LoadScene()
    {
        loadingUI.SetActive(true); // bật UI loading
        sliderLoading.value = 0;
        StartCoroutine(LoadSceneAsync());
        StartCoroutine(WaitInterOpenAd());
    }

    private IEnumerator LoadSceneAsync()
    {
        var delayLoad = 15f;
#if UNITY_EDITOR
        delayLoad = 2f;
#endif
        var timeLoad = 0f;
        var wait = new WaitForEndOfFrame();
        yield return wait;
        multi_speed = 1f;
        var operation = SceneManager.LoadSceneAsync("Movement", LoadSceneMode.Additive);
        operation.allowSceneActivation = false;
        while (timeLoad < delayLoad)
        {
            timeLoad += Time.deltaTime * multi_speed;
            var progress = timeLoad / delayLoad;
            sliderLoading.value = progress;
            yield return wait;
        }

        operation.allowSceneActivation = true;
        while (operation != null && !operation.isDone)
        {
            yield return wait;
        }

        StopCoroutine(WaitInterOpenAd());
        RaccoonAdsManager.Instance.StopLoadingAd(AD_TYPE.OPEN_AD_FIRST_NATIVE);
        sliderLoading.value = 1f;
        Debug.Log("Load Complete");
        yield return wait;

        SceneManager.UnloadSceneAsync("Loading");
        RaccoonAdsManager.Instance.OnLoadingSceneLoaded();
        GameData.Get.checkLoadingScene = true;  
    }

    private IEnumerator WaitInterOpenAd()
    {
        var wait = new WaitForSeconds(1f);
        yield return wait;
        while (true)
        {
            yield return wait;

            if (!(RaccoonAdsManager.Instance?.AdsIsReady(AD_TYPE.OPEN_AD_FIRST_NATIVE) ?? false)) continue;
            yield return wait;
            RaccoonAdsManager.Instance.ShowAdUntilLoaded(AD_TYPE.OPEN_AD_FIRST_NATIVE, false);
            multi_speed = 10f;
            yield return wait;
            break;
        }
    }
}
