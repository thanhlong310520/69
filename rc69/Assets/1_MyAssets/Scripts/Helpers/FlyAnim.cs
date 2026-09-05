using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

namespace Raccoon.UI.Other
{
    public class FlyAnim : MonoBehaviour
    {
        private static FlyAnim instance;

        public static FlyAnim Instance => instance;

        [Header("Settings")] [SerializeField] private int poolSizePerType = 10;
        [SerializeField] private Sprite coinSprite;
        [SerializeField] private Sprite gemSprite;
        public AudioClip claimCoinSound;
        public AudioClip claimGemSound;
        
        public RectTransform targetCoin, targetGem;

        private RectTransform coinContainer;
        private RectTransform gemContainer;
        private Queue<RectTransform> coinPool;
        private Queue<RectTransform> gemPool;

        [SerializeField] private RectTransform canvasRectTransform;
        [SerializeField] private RectTransform tempTransfomToConvert;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                InitializeContainers();
                InitializePools();
            }
        }

        private void InitializeContainers()
        {
            GameObject coinObj = new GameObject("CoinContainer", typeof(RectTransform));
            GameObject gemObj = new GameObject("GemContainer", typeof(RectTransform));

            coinContainer = coinObj.GetComponent<RectTransform>();
            gemContainer = gemObj.GetComponent<RectTransform>();

            coinContainer.SetParent(transform);
            gemContainer.SetParent(transform);

            SetRectTransformDefaults(coinContainer);
            SetRectTransformDefaults(gemContainer);
        }

        private void SetRectTransformDefaults(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private void InitializePools()
        {
            coinPool = new Queue<RectTransform>();
            gemPool = new Queue<RectTransform>();

            for (int i = 0; i < poolSizePerType; i++)
            {
                CreatePoolItem(true);
                CreatePoolItem(false);
            }
        }

        private RectTransform CreatePoolItem(bool isCoin)
        {
            GameObject obj = new GameObject(isCoin ? "Coin" : "Gem", typeof(RectTransform));
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.SetParent(isCoin ? coinContainer : gemContainer);

            Image img = obj.AddComponent<Image>();
            img.sprite = isCoin ? coinSprite : gemSprite;
            img.raycastTarget = false;
            
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            rect.sizeDelta = new Vector2(125, 125);
            rect.localScale = Vector3.one;

            rect.anchoredPosition = Vector2.zero;

            obj.SetActive(false);

            if (isCoin)
                coinPool.Enqueue(rect);
            else
                gemPool.Enqueue(rect);

            return rect;
        }

        public void PlayAnimationUI(Transform start, bool isCoin = true, int amount = 10)
        {
            if (start == null) return;
            
            Queue<RectTransform> pool = isCoin ? coinPool : gemPool;
            int itemCount = Mathf.Min(amount, 3);

            var target = targetCoin;
            if (itemCount <= 0) return;
            //if (isCoin)
            //{
            //    GameAudio.SFX("claimCoin", claimCoinSound);
            //}
            //else
            //{
            //    target = targetGem;
            //    GameAudio.SFX("claimGem", claimGemSound);
            //}
            
            Vector3 startPos = start.position;
            Vector3 targetPos = target.position;

            for (int i = 0; i < itemCount; i++)
            {
                RectTransform item = GetPooledItem(pool, isCoin);
                if (item == null) continue;
                var offset = Random.insideUnitSphere * 100;
                offset.z = 0;
                item.position = startPos + offset;
                item.localScale = Vector3.one * 0.5f;
                item.gameObject.SetActive(true);

                Vector3 randomOffset = new Vector3(
                    Random.Range(-150f, 150f),
                    Random.Range(-150f, 150f),
                    0
                );
                Vector3 midPoint = Vector3.Lerp(startPos, targetPos, 0.5f) + randomOffset;

                Sequence sequence = DOTween.Sequence();

                sequence.Append(item.DOPath(
                    new Vector3[] { startPos, midPoint, targetPos },
                    0.5f,
                    PathType.CatmullRom
                ).SetEase(Ease.OutQuad)).SetUpdate(true);

                sequence.Join(item.DOScale(Vector3.one * 0.7f, 0.5f)).SetUpdate(true);
                
                bool isLastItem = (i == itemCount - 1);

                sequence.OnComplete(() =>
                {
                    item.gameObject.SetActive(false);
                    pool.Enqueue(item);
                }).SetUpdate(true);

                sequence.SetDelay(i * 0.05f).SetUpdate(true);
            }

            target.DOScale(1.2f, 0.1f).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
        }
        
        public void PlayAnimation(Transform start, Transform target, bool isCoin, int amount = 10)
        {
            if (start == null) return;
            
            Queue<RectTransform> pool = isCoin ? coinPool : gemPool;
            int itemCount = Mathf.Min(amount, 3);

            Vector3 startPos = start.position;
            Vector3 targetPos = target.position;
            
            if (itemCount <= 0) return;
            //if (isCoin)
            //    GameAudio.SFX("claimCoin", claimCoinSound);
            //else
            //    GameAudio.SFX("claimGem", claimGemSound);
            

            for (int i = 0; i < itemCount; i++)
            {
                RectTransform item = GetPooledItem(pool, isCoin);
                if (item == null) continue;
                var offset = Random.insideUnitSphere * 100;
                offset.z = 0;
                item.position = startPos + offset;
                item.localScale = Vector3.one * 0.5f;
                item.gameObject.SetActive(true);

                Vector3 randomOffset = new Vector3(
                    Random.Range(-150f, 150f),
                    Random.Range(-150f, 150f),
                    0
                );
                Vector3 midPoint = Vector3.Lerp(startPos, targetPos, 0.5f) + randomOffset;

                Sequence sequence = DOTween.Sequence();

                sequence.Append(item.DOPath(
                    new Vector3[] { startPos, midPoint, targetPos },
                    0.5f,
                    PathType.CatmullRom
                ).SetEase(Ease.OutQuad)).SetUpdate(true);

                sequence.Join(item.DOScale(Vector3.one * 0.7f, 0.5f)).SetUpdate(true);
                
                bool isLastItem = (i == itemCount - 1);

                sequence.OnComplete(() =>
                {
                    item.gameObject.SetActive(false);
                    pool.Enqueue(item);
                }).SetUpdate(true);

                sequence.SetDelay(i * 0.05f).SetUpdate(true);
            }

            target.DOScale(1.2f, 0.1f).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
        }

        public void PlayAnimationUIFromWorld(Vector3 worldPos, bool isCoin = true, int amount = 3)
        {
            // Convert world → canvas position
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,  // <-- assign your main canvas RectTransform
                screenPos,
                null,
                out Vector2 localPos);

            if (tempTransfomToConvert == null)
            {// Create a temporary RectTransform to simulate "start"
                var tempStart = new GameObject("TempStart");
                tempTransfomToConvert = tempStart.AddComponent<RectTransform>();
            }
            tempTransfomToConvert.SetParent(canvasRectTransform, false);
            tempTransfomToConvert.anchoredPosition = localPos;

            // Call your original method
            PlayAnimationUI(tempTransfomToConvert, isCoin, amount);
        }
        
        public void PlayAnimationFromWorld(Vector3 worldPos, Transform target, bool isCoin, int amount = 5)
        {
            if (target == null) return;

            // Convert world → canvas position
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,  // <-- assign your main canvas RectTransform
                screenPos,
                null,
                out Vector2 localPos);

            if (tempTransfomToConvert == null)
            {// Create a temporary RectTransform to simulate "start"
                var tempStart = new GameObject("TempStart");
                tempTransfomToConvert = tempStart.AddComponent<RectTransform>();
            }
            tempTransfomToConvert.SetParent(canvasRectTransform, false);
            tempTransfomToConvert.anchoredPosition = localPos;

            // Call your original method
            PlayAnimation(tempTransfomToConvert, target, isCoin, amount);
        }

        private RectTransform GetPooledItem(Queue<RectTransform> pool, bool isCoin)
        {
            if (pool.Count == 0)
                return CreatePoolItem(isCoin);

            return pool.Dequeue();
        }

        public void SetSprites(Sprite coin, Sprite gem)
        {
            coinSprite = coin;
            gemSprite = gem;
        }
    }
}
