using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.Item;
using Raccoon.Purchase;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainUiControl : MonoBehaviour
{

    public static MainUiControl instance;
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
    public GameObject buttonPickup;
    public GameObject buttonDropItemLoot;
    public GameObject buttonHandleSlot;
    public GameObject pickupGO;
    public GameObject putGO;
    public GameObject stealGO;

    public TextMeshProUGUI textCoin;
    public InventoryUI inventory; 
    public ShowLoadingAds showLoadingAds;
    public List<PopupCanvas> popups;

    public Image iconAvatar;
    public GameObject buttonAttack;
    public UIWarning uiWarning;
    public UIWarning uiWarningBossChase;
    public TMP_Text textWaitAds;
    [SerializeField] List<InforUiPlayerUpgrade> inforUiPlayerUpgrade;
    PopupBackPack backPack;
    PopupIndex pIndex;
    VipSubPopup vip;

    private void Start()
    {
        textWaitAds.gameObject.SetActive(false);
    }

    public void OnButtonPickup(bool state)
    {
        PlayerController.instance.OnPickUp(state);
    }
    public void OnButtonDropItemLoot(bool state)
    {
        if(state) PlayerController.instance.OnDropItemLoot();
    }
    public void OnButtonHandleSlot(bool state)
    {
        if (state) PlayerController.instance.HandleSlot();
    }

    public void ShowUiPickupItem(bool isShow)
    {
        buttonPickup.SetActive(isShow); 
    }
    public void ShowUiDropItemLoot(bool isShow)
    {
        buttonDropItemLoot.SetActive(isShow);
    }
    public void ShowUiShowHandleSlot(TypeHandleSlot type)
    {
        if(type == TypeHandleSlot.None) buttonHandleSlot.SetActive(false);
        else
        {
            buttonHandleSlot.SetActive(true);
            switch (type)
            {
                case TypeHandleSlot.None:
                    {
                        buttonHandleSlot.SetActive(false);
                        break;
                    }
                case TypeHandleSlot.PutBot:
                    {
                        putGO.SetActive(true);
                        pickupGO.SetActive(false);
                        stealGO.SetActive(false);
                        break;
                    }
                case TypeHandleSlot.PickupBot:
                    {
                        putGO.SetActive(false);
                        pickupGO.SetActive(true);
                        stealGO.SetActive(false);
                        break;
                    }
                case TypeHandleSlot.Steal:
                    {
                        putGO.SetActive(false);
                        pickupGO.SetActive(false);
                        stealGO.SetActive(true);
                        break;
                    }
            }
        }
    }

    public void ShowPopup(PopupCanvas.PopupType type,UnityAction actionHide, object obj)
    {
        var popup = popups.First(p => p.popup == type);
        if(popup != null) popup.Show(type,actionHide,obj);
    }

    public void ResetItemInventoy()
    {
        inventory.ResetItem();
    }

    public void OnSetDataInventory(List<ItemConfig> data)
    {
        inventory.OnSetDataForSlot(data);
    }
    public void SelectItemInventoryByConfig(ItemConfig config)
    {
        inventory.SetSelectItemByConfig(config);
    }


    public void ShowCoin(string text)
    {
        textCoin.text = text;
    }

    public void UpdateItemForPopupBackPack(ItemDataSO data, ItemConfig config, bool isAdd)
    {
        if (backPack == null) backPack = LoadPopup(PopupCanvas.PopupType.BackPack).GetComponent<PopupBackPack>();
        if (backPack == null) return;
        backPack.UpdateItem(data, config, isAdd);
    }
    public void UpdateItemIndexUnlock(ItemDataSO data)
    {
        if (pIndex == null) pIndex = LoadPopup(PopupCanvas.PopupType.Index).GetComponent<PopupIndex>();
        if (pIndex == null) return;
        pIndex.UpdateItem(data);
    }

    public PopupCanvas LoadPopup(PopupCanvas.PopupType type)
    {
        var popup = popups.FirstOrDefault(p => p.popup == type);
        if(popup == null) return null;
        return popup;
    }

    public void ChangeIconAvatar(Sprite icon)
    {
        iconAvatar.sprite = icon;
    }

    public void TurnOnButtonAttack(bool isON)
    {
        buttonAttack.SetActive(isON);
    }


    public void UpdateInforPlayerUpgrade(TypeUpgrade type, float number)
    {
        var infor = inforUiPlayerUpgrade.FirstOrDefault(i => i.type == type);
        if(infor.text != null)
        {
            infor.text.text = number.ToString();
        }
    }

    public void OnClickBackBase()
    {
        GamePlayControl.instance.PlayerBackBase();
    }
    public void OnClickShield()
    {
        GamePlayControl.instance.PlayerShield();
    }

    public bool IsPopupVipActive()
    {
        if (vip == null) vip = LoadPopup(PopupCanvas.PopupType.VipSub).GetComponent<VipSubPopup>();
        if (vip == null) return false;
        if (vip.IsActive) return true;
        return false;
    }

    public void WarringSteal()
    {
        uiWarning.StartWarning();
    }

    public void ShowTextWaitAds(float amountWait)
    {
        textWaitAds.text = "Loading ads " + (int)(amountWait * 100) + "%";
    }

    public void EnableTextWaitAds(bool isEnable)
    {
        textWaitAds.gameObject.SetActive(isEnable);
    }

    public void ShowUIBotChase()
    {
        uiWarningBossChase?.StartWarning();
    }

    [System.Serializable]
    struct InforUiPlayerUpgrade
    {
        public TypeUpgrade type;
        public TMP_Text text;
    }
}
