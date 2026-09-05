using Raccoon.Controller;
using Raccoon.UI;
//using RCKit.Ads;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Raccoon.Ads
{
    public class AdsActionManager : MonoBehaviour
    {
        public static AdsActionManager instance;

        [SerializeField] private bool initialized = false;

        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

#if USE_ADS


        public void ShowInterstitialAd(System.Action onDone, System.Action onFail = null)
        {
            if (!GameAds.IsAdsEnabled)
            {
                onDone?.Invoke();
                return;
            }

            var ads = GameAds.Get;
            if (ads == null)
            {
                onDone?.Invoke();
                return;
            }

            ads.ShowInter(success =>
            {
                if (success)
                    onDone?.Invoke();
                else
                    onFail?.Invoke();
            });
        }

        public void ShowInterOpenAd(System.Action onDone = null, System.Action onFail = null)
        {
            if (!GameAds.IsAdsEnabled)
            {
                onDone?.Invoke();
                return;
            }

            var ads = GameAds.Get;
            if (ads == null)
            {
                onDone?.Invoke();
                return;
            }

            ads.ShowInterOpen(success =>
            {
                if (success)
                    onDone?.Invoke();
                else
                    onFail?.Invoke();
            });
        }


        public void ShowRewardAds(Action<bool> callback)
        {

            var ads = GameAds.Get;
            if (ads == null)
            {
                callback?.Invoke(false);
                return;
            }
            ads.ShowReward(callback);
        }
#else 
        public void ShowRewardAds(Action<bool> callback)
        {
            callback?.Invoke(true);
        }

        public void CheckShowInterstitialAdWithCallback(System.Action onDone)
        {
            onDone?.Invoke();
        }

        public void OnResetTimeCount()
        {
        }

        public void OnStartCountNative()
        {
        }
        
        public void ShowInterstitialAd(Action action)
        {
        }
#endif
    }
}
