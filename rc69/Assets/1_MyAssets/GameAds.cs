using System.Collections;
using System.Collections.Generic;
using System.Threading;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using Raccoon;
using Raccoon.UI;
using RCKit.Ads.Raccoon;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_RACCOON_JNATIVE
using RCKit.Ads.JNative;
using DG.Tweening;
using Nami;
using UnityEngine.Events;
using Raccoon.Controller;
using RCKit.Ads.Main;


#endif

#if ENABLE_RACCOON_ADMOB
using RCKit.Ads.AdMob;
#endif

namespace RCKit.Ads
{
    public class GameAds : MonoBehaviour
    {
        public bool IsInterstitialForceNativeAvailable =>
            IsAdsEnabled && RaccoonAdsManager.Instance.AdsIsReady(AD_TYPE.INTER_FORCE_NATIVE);

        public static GameAds Get { get; private set; }
        public static bool IsAdsEnabled => PlayerData.Get == null || (PlayerData.Get.isOnAds && !PlayerData.Get.isVipIAP);

        bool canShowAppOpen;
        private void Awake()
        {
            if (Get != null && Get != this)
            {
                Destroy(gameObject);
                return;
            }
            Get = this;
        }
        public void Start()
        {
            RaccoonAdsManager.Instance.Initialize(() =>
            {
                Debug.Log("Init Ads");
                RaccoonAdsManager.Instance.PreloadAdsWaitShow(AD_TYPE.OPEN_AD_NATIVE);
                if (!IsAdsEnabled)
                    RemoveAds();
            });
        }
        public void RemoveAds()
        {
            RaccoonAdsManager.Instance.SetAdsRemoved();
        }
        public void HideBanner()
        {
            RaccoonAdsManager.Instance.HideBanner(AD_TYPE.OPEN_AD_NATIVE);
        }
        public void ShowBanner()
        {
            RaccoonAdsManager.Instance.ShowBanner(AD_TYPE.OPEN_AD_NATIVE);
        }
        public void ShowInter(System.Action<bool> onComplete = null)
        {
            RaccoonAdsManager.Instance.ShowAd(AD_TYPE.OPEN_AD_NATIVE, onComplete);
        }
        public void ShowInterOpen(System.Action<bool> onComplete = null)
        {
            RaccoonAdsManager.Instance.ShowAd(AD_TYPE.OPEN_AD_NATIVE, onComplete);
        }
        public void ShowReward(System.Action<bool> onComplete)
        {
            RaccoonAdsManager.Instance.ShowAd(AD_TYPE.OPEN_AD_NATIVE, onComplete);
        }

    }
}
