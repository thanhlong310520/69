using Raccoon.Ads;
using Raccoon.Data;
using Raccoon.EnumHolder;
using Raccoon.GameService;
using Raccoon.Item;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Raccoon.Controller
{
    public class GamePlayControl : MonoBehaviour
    {
        public static GamePlayControl instance;
        private void Awake()
        {

            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        [SerializeField] private List<ItemDataSO> itemDataSOs;
        [SerializeField] private List<FashionDataSO> fashionDataSOs;
        [SerializeField] private List<ShowModelInMap> showFashion;
        [SerializeField] private List<ItemDataSO> itemCanStaySlotDataSOs;
        [SerializeField] private List<ItemDataSO> eggDataSOs;
        [SerializeField] public List<ItemDataSO> botDataSOs;
        [SerializeField] public List<IndexRewardData> indexRewardDataSOs;
        [SerializeField] private List<UpgradeDataSO> upgradeDataSOs;
        [SerializeField] private List<TreamillDataSO> treamillDataSOs;
        [SerializeField] private List<DataColorType> dataColorTypes;
        [SerializeField] private List<SpawnItem> spawnItems;
        [SerializeField] private Transform sellShop;
        [SerializeField] private Transform upgradeShop;
        [SerializeField] private ChangeEnvironment changeEnvironment;
        public CountTimeShowPopupVip ctsv;

        public Camera mainCam;
        public Camera rebirthCam;
        public RebirthHandle rebirthHandle;
        public Transform GetSellShop => sellShop;
        public Transform GetUpgradeShop => upgradeShop;

        public Material matColorRainbowUI;

        public long incomeHaveEffect = 200;

        public List<UpgradeDataSO> GetUpgradeDataSOs => upgradeDataSOs;
        public List<TreamillDataSO> GetTreamillDataSOs => treamillDataSOs;

        bool canAction = false;

        public bool CanAction => canAction;

        public EnvironmentType currentEnvironmentType = EnvironmentType.Normal;

        public float timeDelayShowInter = 5f;

        public Transform bossHolder;
        private void Start()
        {
            itemCanStaySlotDataSOs = new List<ItemDataSO>();
            itemCanStaySlotDataSOs.AddRange(eggDataSOs);
            itemCanStaySlotDataSOs.AddRange(botDataSOs);
            if (PlayerController.instance != null)
            {
                PlayerController.instance.eventShowUIPickupItem.AddListener(ShowButtonPickup);
                PlayerController.instance.eventShowUIDropItemLoot.AddListener(ShowButtonDropItemLoot);
                PlayerController.instance.eventShowUIHandleSlot.AddListener(ShowButtonHandleSlot);
            }
            StartCoroutine(DelayChangeMap());
            StartCoroutine(DelaySetDataFashionForShow());
            itemDataSOs.AddRange(itemCanStaySlotDataSOs);
            ResetAfterHidePopupInterBonus();
            SetCanAction(true);
            mainCam.enabled = true;
            rebirthCam.enabled = false;

            GameData.Get.CheckSubscribe();
            if (!PlayerData.Get.isVipIAP)
            {
                ShowPopup(PopupCanvas.PopupType.VipSub);
                GameData.Get.isShowVip = true;
            }
            StartCoroutine(ShowInappReviewDelayed());

            //StartCoroutine(DelayShowInterOpen());
        }
        IEnumerator DelayShowInterOpen()
        {
            while (!GameData.Get.checkLoadingScene)
            {
                yield return null;
            }
            yield return new WaitForSeconds(timeDelayShowInter);
            AdsActionManager.instance.ShowInterOpenAd();
        }
        IEnumerator ShowInappReviewDelayed()
        {
            yield return new WaitForSeconds(20f);

            if (GameInappReview.Instance != null)
                GameInappReview.Instance.TryShowReviewFirstSession();
        }
        IEnumerator DelayChangeMap()
        {
            yield return new WaitForSeconds(0.1f);
            changeEnvironment?.Init(currentEnvironmentType, ChangeEnvironment);
        }
        IEnumerator DelaySetDataFashionForShow()
        {
            yield return new WaitForSeconds(0.1f);
            string idFashionPlayer = PlayerController.instance.GetCharactorData.currentFashion;
            for (int i = 0; i < fashionDataSOs.Count; ++i)
            {
                if (i >= showFashion.Count) break;
                if (fashionDataSOs[i].GetId == idFashionPlayer) showFashion[i].gameObject.SetActive(false);
                else
                {
                    showFashion[i].gameObject.SetActive(true);
                }
                showFashion[i].Init(PlayerController.instance.transform, fashionDataSOs[i]);
            }

        }
        private void Update()
        {
            if (MainUiControl.instance.IsPopupVipActive()) return;
            CheckInterAds();
        }

        float timeCountAdsInter;
        public float timeShowInter = 10;
        bool canCheckAds = false;

        bool canShowPopup = false;

        public void SetCanAction(bool canAction)
        {
            this.canAction = canAction;
        }

        void ChangeEnvironment(EnvironmentType environmentType)
        {
            currentEnvironmentType = environmentType;
            foreach (var s in spawnItems)
            {
                s.ChangeEnvironment(currentEnvironmentType);
            }
        }

        private void CheckInterAds()
        {
            if (!canCheckAds) return;

            timeCountAdsInter += Time.deltaTime;
            if (timeCountAdsInter >= timeShowInter)
            {
                if(!PlayerController.instance.loot.IsHaveItemLoot())
                    ShowInterBonus();
            }
        }

        void ShowInterBonus()
        {
            canCheckAds = false;
            //

            canShowPopup = false;
            ShowPopup(PopupCanvas.PopupType.InterBonus, ResetAfterHidePopupInterBonus);

            
        }

        void ResetAfterHidePopupInterBonus()
        {
            canCheckAds = true;
            timeCountAdsInter = 0;
            canShowPopup = true;
        }

        #region UI
        public void ShowButtonPickup(bool isShow)
        {
            MainUiControl.instance.ShowUiPickupItem(isShow);
        }

        public void ShowButtonDropItemLoot(bool isShow)
        {
            MainUiControl.instance.ShowUiDropItemLoot(isShow);
        }
        public void ShowButtonHandleSlot(TypeHandleSlot type)
        {
            MainUiControl.instance.ShowUiShowHandleSlot(type);
        }
        public void ShowPopup(PopupCanvas.PopupType type,CharactorData data = null)
        {
            if (CheckCanShowPopup())
            {
                MainUiControl.instance.ShowPopup(type, HidePopup, data);
                SetCanAction(false);
            }
            
        }
        public void ShowPopup(PopupCanvas.PopupType type, UnityAction action)
        {
            MainUiControl.instance.ShowPopup(type, action, null);

        }

        public void PlayerBackBase()
        {
            if(canAction)
                PlayerController.instance.BackToHome();
        }
        public void PlayerShield()
        {
            if (canAction)
                AdsActionManager.instance.ShowRewardAds((success) =>
                {
                    if(success) PlayerController.instance.Shield();

                });
        }
        public void OnClickButtonRebirth(bool isAds)
        {
            rebirthHandle.Rebirth(PlayerController.instance.GetCharactorData, isAds);
        }
        public void OnChangeMapByUI(EnvironmentType type)
        {
            changeEnvironment.ChangeByType(type);
        }

        void HidePopup()
        {
            SetCanAction(true);
        }

        bool CheckCanShowPopup()
        {
            bool result = true;
            if (!canShowPopup) result = false;
            if (!canAction) result = false;
            return result;
        }

        public void ShowUIBossChase()
        {
            MainUiControl.instance.ShowUIBotChase();
        }
        #endregion

        #region Data
        public ItemDataSO GetItemDataSO(string id)
        {
            return itemDataSOs.Find(i => i.GetId == id);
        }

        public List<ItemDataSO> GetItemDataStaySlotSOs()
        {
            return itemCanStaySlotDataSOs;
        }
        public List<FashionDataSO> GetFashionDataSOs()
        {
            return fashionDataSOs;
        }
        public FashionDataSO GetFashionDataById(string id)
        {
            return fashionDataSOs.FirstOrDefault(so => so.GetId.Equals(id));
        }
        public UpgradeDataSO GetUpgradeDataByType(TypeUpgrade type)
        {
            return upgradeDataSOs.FirstOrDefault(u => u.type == type);
        }
        public IndexRewardData GetIndexRewardByZone(ZoneType type)
        {
            return indexRewardDataSOs.FirstOrDefault(r => r.zone == type);
        }


        public Sprite GetBgByColorType(ZoneType type)
        {
            DataColorType dt = dataColorTypes.FirstOrDefault(d => d.type == type);
            return dt.bg;
        }
        public List<ItemDataSO> GetBotDataInZone(ZoneType type)
        {
            return botDataSOs.Where(b => b.colorType == type).ToList();
        }
        #endregion
        #region Spawner

        public GameObject SpawnItem(string idItem)
        {
            GameObject result = null;
            var data = GetItemDataSO(idItem);
            if(data!= null)
            {
                if (data.prefab != null) result = PoolByID.Instance.GetPrefab(data.prefab);
            }
            return result;
        }
        public GameObject SpawnItem(GameObject prefab)
        {
            GameObject result = null;
            //if (prefab != null) result = PoolByID.Instance.GetPrefab(prefab);
            if (prefab != null) result = GamePool.Get(prefab);

            return result;
        }

        public void RespawnItem(GameObject item) 
        {
            //PoolByID.Instance.PushToPool(item);
            GamePool.Release(item);
        }
        #endregion

        private readonly List<ItemCtr> _cachedItemBuffer = new List<ItemCtr>(32);

        public ItemCtr GetRandomItemInSpawnItem(int max = 5)
        {
            _cachedItemBuffer.Clear();
            int count = Mathf.Min(max, spawnItems.Count);
            for (int i = 0; i < count; i++)
            {
                var items = spawnItems[i].listItemSpawn;
                for (int j = 0; j < items.Count; j++)
                {
                    if (items[j] != null && items[j].GetState == ItemState.Normal)
                        _cachedItemBuffer.Add(items[j]);
                }
            }
            if (_cachedItemBuffer.Count == 0) return null;
            return _cachedItemBuffer[UnityEngine.Random.Range(0, _cachedItemBuffer.Count)];
        }

        public bool HasAvailableItemInSpawnItem(int max = 5)
        {
            int count = Mathf.Min(max, spawnItems.Count);
            for (int i = 0; i < count; i++)
            {
                var items = spawnItems[i].listItemSpawn;
                for (int j = 0; j < items.Count; j++)
                {
                    if (items[j] != null && items[j].GetState == ItemState.Normal)
                        return true;
                }
            }
            return false;
        }

        public void LookatTarget(Transform origin, Transform target, Vector3 offset)
        {
            Vector3 dir = target.position - origin.transform.position + offset;
            Quaternion lookRot = Quaternion.LookRotation(dir);

            //origin.DORotateQuaternion(lookRot, 0.1f).SetEase(Ease.Linear).OnComplete(() =>
            //{
            //    //
            //});

            origin.rotation = lookRot;
        }


        public void ChangeModelPlayer(FashionDataSO so)
        {
            string id = PlayerController.instance.GetCharactorData.currentFashion;

            foreach (var s in showFashion)
            {
                if(s.data.GetId == id)
                {
                    s.gameObject.SetActive(true);
                    break;
                }
            }

            PlayerController.instance.ChangeModelWhenClickItemFashion(so.GetId);

        }
    }
    [System.Serializable] struct DataColorType
    {
        public ZoneType type;
        public Sprite bg;
    }
}