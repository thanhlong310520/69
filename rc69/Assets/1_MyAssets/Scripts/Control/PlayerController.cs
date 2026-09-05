using Raccoon.Ads;
using Raccoon.Building;
using Raccoon.Data;
using Raccoon.EnumHolder;
using Raccoon.InputCtr;
using Raccoon.Item;
using Raccoon.Player;
using Raccoon.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Raccoon.Controller
{
    public class PlayerController : MonoBehaviour, IIteractSlot, ICharactor, IReceive
    {
        #region singleton
        public static PlayerController instance;

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
        #endregion
        public PlayerStateHandle stateHandle;
        public PlayerMovement movement;
        public PlayerLoot loot;
        public PlayerShield shield;

        public PlayerSoundFootStep sfs;

        public Rigidbody rb;
        public CapsuleCollider capsule;
        public PlayerContact contact;

        public Camera mainCam;
        public CharactorData data;
        public CharactorData GetCharactorData => data;
        //public UnityEvent<EPlayerState> EventChangeStatePlayer;
        public UnityEvent<TypeHandleSlot> eventShowUIHandleSlot;
        public UnityEvent<bool> eventShowUIPickupItem;
        public UnityEvent<bool> eventShowUIDropItemLoot;

        public UnityEvent<TypeActionTutorial> eventTutorial;

        public UnityEvent<CharactorData> eventWhenChangeData;

        bool inProtect;

        bool inited = false;

        private void Start()
        {
            inProtect = false;

            EventBus.Subscribe<PlayerInput>(UpdateValueInput);
            loot.Init(OnStartLoot,LootFail,OnWhileLoot,LootSuccess);
            stateHandle.Init(rb,AddItemToDataWhenPickup,RemoveItemFromDataWhenPutInSlot,Steal ,ActionWhenChangeItem);
            stateHandle.EventResetItem.AddListener(OnResetItem);
            movement.Init(rb, capsule);
            ActionWhenChangeItem(ItemType.None);
        }

        #region ICharactor

        public void Jump()
        {
            ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.Jump);

        }
        public void SetCharactorData(CharactorData data)
        {
            inited = true;
            if(data == null)
            {
                Home home = HomeCtr.instance.GetHomeByUser(transform);
                data = GameData.Get.GetCharacterData(home.nameHome);
            }
            this.data = data;
            foreach (TypeUpgrade type in Enum.GetValues(typeof(TypeUpgrade)))
            {
                UpgradeDataSO upgradeDataSO = GamePlayControl.instance.GetUpgradeDataByType(type);
                SetDataUpgrade(upgradeDataSO, data.upgrade.GetDicUpgraded(type));
            }
            this.data.onChangeItem += UpdateUIWhenChangeItem;
            this.data.onChangeCoin += UpdateUIWhenChangeCoin;

            ChangeModelWhenClickItemFashion(data.currentFashion);
            UpdateUIWhenChangeItem(data.itemConfigs);
            UpdateUIWhenChangeCoin(data.coin);
            foreach (var cf in data.itemConfigs)
            {
                var so = GamePlayControl.instance.GetItemDataSO(cf.id);
                UpdateItemBackPack(so, cf, true);
            }
            foreach (var id in data.listBotUnlock)
            {
                var so = GamePlayControl.instance.GetBotDataSOs().Find(i => i.GetId == id);
                UpdateItemIndexUnlock(so);
            }

            HandleEventWhenChangeData();
        }
        public void SetStartPoint(Vector3 startPoint)
        {
            transform.position = startPoint;
        }

        public void ContactUnlockShop(long costUnlock)
        {
            data.SetUnlockGate(true);
            HomeCtr.instance.UnlockGate2FormUser(transform);
        }
        void AddItemToDataWhenPickup(SlotController slot, ItemCtr item)
        {
            var config = item.GetItemConfig;
            if (config == null) return;
            data.RemoveBotInSlot(slot.id);
            ItemConfig currentconfig = null;
            if (stateHandle.GetCurrentItem != null) currentconfig = stateHandle.GetCurrentItem.GetItemConfig;
            data.AddItem(config, currentconfig);
            UpdateItemBackPack(item, true);
        }
        void RemoveItemFromDataWhenPutInSlot(SlotController slot, ItemCtr item)
        {
            var config = item.GetItemConfig;
            if (config == null) return;
            var dtbotinslot = data.AddBotInSlot(slot.id, config);
            slot.SetActionChangeIncome(dtbotinslot.ChangeIncome);
            data.RemoveItem(config.id, config.level);
            UpdateItemBackPack(item, false);
        }

        void RemoveDataItem(ItemCtr item)
        {
            var config = item.GetItemConfig;
            data.RemoveItem(config.id, config.level);
            UpdateItemBackPack(item, false);
        }
        void AddDataItem(ItemCtr item)
        {
            GamePlayControl.instance.RespawnItem(item.gameObject);
            var config = item.GetItemConfig;
            ItemConfig currentconfig = null;
            if (stateHandle.GetCurrentItem != null) currentconfig = stateHandle.GetCurrentItem.GetItemConfig;
            data.AddItem(config, currentconfig);
            UpdateItemBackPack(item, true);

            stateHandle.ChangeItem(config);
            UpdateSelectAfterSaveLoot(config);
        }
        void Steal(SlotController slot, ItemCtr item)
        {
            ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.Steal);

            var config = item.GetItemConfig;
            if (config == null) return;
            data.AddItem(config, null);
            UpdateItemBackPack(item, true);
            stateHandle.ChangeItem(config);
            UpdateSelectAfterSaveLoot(config);

        }
        public void ContactShop(TypeShop type)
        {
            GamePlayControl.instance.ShowPopup(ConvertToPopupType(type), data);
        }

        PopupCanvas.PopupType ConvertToPopupType(TypeShop type)
        {
            switch (type)
            {
                case TypeShop.UpgradeShop:
                    {
                        return PopupCanvas.PopupType.Upgrade;
                    }
                case TypeShop.SellShop:
                    {
                        return PopupCanvas.PopupType.Sell;
                    }
            }
            return PopupCanvas.PopupType.None;
        }

        public void ContactWave()
        {
            if (inProtect || shield.IsShield) return;

            ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.ContactWave);

            VfxCtrl.instance.SpawnRandomVfx(TypeVFX.Blood, transform.position + Vector3.up * 1.5f);
            Dead();
            StartCoroutine(ResetAfterDead(true));
        }

        public void Stealed(SlotController slot)
        {
            data.RemoveBotInSlot(slot.id);
        }
        #endregion
        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerInput>(UpdateValueInput);
        }

        private void Update()
        {
            if (!inited) return;
            //SaveItemBrainrot();
            //ShowUIPickUp();
            UpdateSlot();
            UpdateItem();
            ShowButtonDropItemLoot();
            SetDirForMovement();

            SaveLoot();
            CheckInProtectZone();
            AutoFillBrainrotInSlot();
        }

        void CheckInProtectZone()
        {
            if(inProtect != contact.InProtect)
            {
                inProtect = contact.InProtect;
                if (inProtect) AdsActionManager.instance.ShowRewardAds(SetInProtect);
            }
        }

        void SetInProtect(bool adsSuccess)
        {
            //inProtect = adsSuccess;
        }

        void SetDirForMovement()
        {
            if (mainCam == null) mainCam = Camera.main;

            movement.SetDirFoward(mainCam.transform.forward);
            movement.SetDirRight(mainCam.transform.right);
        }


        public void UpdateValueInput(PlayerInput value)
        {
            if (value == null) return;
            if (stateHandle.GetPlayerState == EPlayerState.Dead) value.Reset();
            movement.SetInput(value);

            if (value.horizontalAxis != 0 || value.verticalAxis != 0)
            {
                if(movement.isGrounded) sfs.SetPlayAudio(true);
                else sfs.SetPlayAudio(false);
            }
            else sfs.SetPlayAudio(false);
            if (value.isRunning) sfs.SetScaleTime((movement.MultiSpeedRun-1)/50 + 1);
            else sfs.SetScaleTime(1);

            if (value.isAttack)
            {
                if (CheckAttack()) stateHandle.Attack();
            }
            if(value.isDropBox)
            {
                if (stateHandle.GetCurrentItem != null && stateHandle.GetCurrentItem.data.itemType == ItemType.OtherItem)
                {
                    stateHandle.DropItem(true, RemoveDataItem);
                }
                value.isDropBox = false;
            }


        }
        bool CheckAttack()
        {
            if(loot.IsLooting) return false;
            if(stateHandle.GetPlayerState == EPlayerState.KnockBack) return false;
            return true;
        }

        #region HandleSlotctr
        public void Income(long income)
        {
            ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.CollectCoin);
            VfxCtrl.instance.SpawnRandomVfx(TypeVFX.Coin, transform.position);
            data.AddCoin(income);
        }
        public void PayForUpgrade(long pay)
        {
            data.AddCoin(-pay);
        }

        ItemCtr itemContact;

        SlotController curentSlot;
        TypeHandleSlot typeHandleSlot;

        void UpdateSlot()
        {
            if (curentSlot != contact.SlotController)
            {
                ChangeSlot(contact.SlotController);
            }
            typeHandleSlot = TypeHandleSlot.None;
            if (curentSlot != null)
            {
                if(curentSlot.user == transform)
                {
                    if (curentSlot.GetSlot_State == Slot_state.HaveBot) typeHandleSlot = TypeHandleSlot.PickupBot;
                    else if(curentSlot.GetSlot_State == Slot_state.Free)
                    {
                        if (stateHandle.GetCurrentItem != null && stateHandle.GetCurrentItem.data.itemType == ItemType.Brainrot) typeHandleSlot = TypeHandleSlot.PutBot;
                    }
                }
                else
                {
                    if (curentSlot.GetSlot_State == Slot_state.HaveBot) typeHandleSlot = TypeHandleSlot.Steal;
                }
            }
            eventShowUIHandleSlot?.Invoke(typeHandleSlot);

        }
        void ChangeSlot(SlotController newSlot)
        {
            curentSlot?.SetHighlightObject(false);
            curentSlot = newSlot;
        }

        void UpdateItem()
        {
            if (itemContact != contact.ItemContact)
            {
                itemContact = contact.ItemContact;
            }

            bool onUIPickup = false;
            if(itemContact != null) { onUIPickup = true; }
            if(stateHandle.GetPlayerState == EPlayerState.KnockBack) { onUIPickup = false; }
            if(stateHandle.GetPlayerState == EPlayerState.Dead) { onUIPickup = false; }
            if(loot.IsMaxLoot) { onUIPickup = false; }
            eventShowUIPickupItem?.Invoke(onUIPickup);
        }

        public void HandleSlot()
        {
            stateHandle.HandleSlot(curentSlot, typeHandleSlot);
            if(typeHandleSlot == TypeHandleSlot.PutBot)
            {
                eventTutorial?.Invoke(TypeActionTutorial.PushBrainrot);
            }
            curentSlot = null;
        }

        void AutoFillBrainrotInSlot()
        {
            if (ShieldHomePlayer.instance == null) return;
            if (ShieldHomePlayer.instance.homePlayerContact == null) return;
            if (!ShieldHomePlayer.instance.homePlayerContact.isPlayerInHome) return;
            if (stateHandle.GetCurrentItem == null) return;
            if (stateHandle.GetCurrentItem.data == null) return;
            if (stateHandle.GetCurrentItem.data.itemType != ItemType.Brainrot) return;
            var slot = GetFreeSlot();
            if(slot == null) return;
            stateHandle.HandleSlot(slot, TypeHandleSlot.PutBot);
            eventTutorial?.Invoke(TypeActionTutorial.PushBrainrot);
            curentSlot = null;

        }

        public SlotController GetFreeSlot()
        {
            return HomeCtr.instance.GetSlotFree(transform);
        }
        #endregion
        #region Loot

        public void OnPickUp(bool state)
        {
            if (state)
            {
                HandlePickUp(itemContact);
            }
            else
            {
                //loot.CancelLoot();
            }
        }

        public void HandlePickUp(ItemCtr itemContact)
        {
            if (itemContact.GetIsSave)
            {
                AddDataItem(itemContact);
                HolderItemDropped.instance.RemoveItem(itemContact);
            }
            else
            {
                loot.Loot(itemContact);
            }
        }


        public void OnDropItemLoot()
        {
            loot.DropItemLoot();
            stateHandle.ChangeLayerItem(ItemType.None);
        }
        void ShowButtonDropItemLoot()
        {
            bool isShow = loot.IsHaveItemLoot();
            if(stateHandle.GetPlayerState == EPlayerState.Dead) isShow = false;
            eventShowUIDropItemLoot?.Invoke(isShow);
        }
        void OnStartLoot()
        {

        }
        void LootFail()
        {

        }
        void LootSuccess(ItemCtr item)
        {

            ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.LootItem);
            item.Pick(stateHandle.holderItem);
            stateHandle.LootHandle();
        }
        void OnWhileLoot(float progess)
        {

        }
        void SaveLoot()
        {
            if (!contact.InSaveZone) return;
            if (!loot.IsHaveItemLoot()) return;

            eventTutorial?.Invoke(TypeActionTutorial.LootItem);
            ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.SaveLoot);
            var listItem = loot.GetItemLoots;
            foreach (var item in listItem)
            {
                UpdateItemBackPack(item, true);
                BotDataSO bd = item.data as BotDataSO;
                if(bd != null)
                {
                    if (data.AddItemUnlock(bd.GetId)) UpdateItemIndexUnlock(bd);
                }        
            }
            var listSave = loot.SaveLoot();
            data.AddListItemConfig(listSave);
            var lastItem = listSave[listSave.Count - 1];
            stateHandle.ChangeItem(lastItem);
            UpdateSelectAfterSaveLoot(lastItem);
        }

        #endregion

        public void Upgrade(UpgradeDataSO upgradeData, int index, bool useAds)
        {
            // tru tien 
            // update data
            if(index < 0) return;
            if(!upgradeData.HaveData(index)) return;

            if(!useAds) PayForUpgrade(upgradeData.GetPriceUpgrade(index));
            ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.UpgradeSuccess);
            eventTutorial?.Invoke(TypeActionTutorial.Upgrade);

            data.CUpdate(upgradeData.type,index);
            SetDataUpgrade(upgradeData,index);
        }

        public void SetDataUpgrade(UpgradeDataSO upgradeData, int index)
        {
            float set = 0;
            if (index >= 0 && upgradeData.HaveData(index)) set = upgradeData.GetPrecent(index);

            switch (upgradeData.type)
            {
                case TypeUpgrade.Speed:
                    {
                        movement.SetMultiSpeedRun(set);
                        break;
                    }
                case TypeUpgrade.JumpForce:
                    {
                        movement.SetMultiJumpForce(set);
                        break;
                    }
                case TypeUpgrade.SlotLoot:
                    {
                        loot.SetNumberLoot(set);
                        set += 1;
                        break;
                    }
            }
            MainUiControl.instance.UpdateInforPlayerUpgrade(upgradeData.type, set);
        }

        #region HandleUI

        public void OnResetItem()
        {
            MainUiControl.instance.ResetItemInventoy();
        }

        void UpdateUIWhenChangeItem(List<ItemConfig> items)
        {
            MainUiControl.instance.OnSetDataInventory(data.itemConfigs);
            HandleEventWhenChangeData();
        }

        void UpdateSelectAfterSaveLoot(ItemConfig config)
        {
            MainUiControl.instance.SelectItemInventoryByConfig(config);
        }

        void UpdateUIWhenChangeCoin(long coin)
        {
            string s = coin.ConvertCurrencyToString();
            MainUiControl.instance.ShowCoin(s);
            HandleEventWhenChangeData();
        }


        public bool CanClickItemInventory()
        {
            bool canClick = true;
            if(loot.IsHaveItemLoot()) canClick = false;
            if (stateHandle.GetPlayerState == EPlayerState.Dead) canClick = false;
            return canClick;
        }

        void UpdateItemBackPack(ItemCtr itemCtr, bool isAdd)
        {
            if (itemCtr == null) return;
            UpdateItemBackPack(itemCtr.data, itemCtr.GetItemConfig, isAdd);
        }
        void UpdateItemBackPack(ItemDataSO so, ItemConfig config, bool isAdd)
        {
            MainUiControl.instance.UpdateItemForPopupBackPack(so, config, isAdd);
        }
        void UpdateItemIndexUnlock(BotDataSO so)
        {
            if(so == null) return;
            MainUiControl.instance.UpdateItemIndexUnlock(so);
        }

        public void ChangeItemWhenClickItemBackpack(ItemConfig config)
        {
            if(config != null)
            {
                data.RemoveItem(config.id, config.level);
                data.AddItem(config, null);

            }
            UpdateSelectAfterSaveLoot(config);
            stateHandle.ChangeItem(config);
        }

        public void ChangeModelWhenClickItemFashion(string id)
        {
            var so = GamePlayControl.instance.GetFashionDataById(id);
            if(so == null) return;
            if (so.prefab == null) return;

            MainUiControl.instance.ChangeIconAvatar(so.avatar);
            data.ChangeFashion(id); 
            stateHandle.ChangeModel(so.prefab);

            foreach (var item in loot.GetItemLoots)
            {
                item.SetTarget(stateHandle.holderItem);
            }
        }


        void ActionWhenChangeItem(ItemType type)
        {
            MainUiControl.instance.TurnOnButtonAttack(type == ItemType.Weapon || type == ItemType.OtherWeapon);
        }
        #endregion


        IEnumerator ResetAfterDead(bool haveResetPos)
        {
            yield return new WaitForSeconds(2f);
            stateHandle.playerAnim.DoneKnockBack();
            SetNormal(haveResetPos);
        }

        void Dead()
        {
            rb.isKinematic = true;
            capsule.enabled = false;
            loot.DropItemLoot();
            stateHandle.Die();
        }

        void SetNormal(bool haveResetPos = true)
        {
            if (haveResetPos)
            {
                Transform startPos = HomeCtr.instance.GetStartPos(transform);
                transform.position = startPos.position;
            }
            rb.isKinematic = false;
            capsule.enabled = true;
            stateHandle.ChangeState(EPlayerState.Normal);
        }

        public void BackToHome()
        {
            if (loot.IsHaveItemLoot()) return;
            if (stateHandle.GetPlayerState == EPlayerState.Dead || stateHandle.GetPlayerState == EPlayerState.KnockBack) return;

            Transform startPos = HomeCtr.instance.GetStartPos(transform);
            transform.position = startPos.position;

            VfxCtrl.instance.SpawnRandomVfx(TypeVFX.BackBase, startPos.position);
        }
        public void Shield()
        {
            ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.Shield);

            shield.StartShield();
        }
        #region Sell
        public void Sell(PlayerSellType typeSell)
        {
            switch (typeSell) {
                case PlayerSellType.CurrentBrainrot :
                {
                        SellCurrentBrainrot();
                        break;
                }
                case PlayerSellType.AllBrainrot:
                {
                        SellAllBrainrot();
                        break;
                }
            }
        }

        void SellCurrentBrainrot()
        {
            long cost = 0;
            if (stateHandle.GetCurrentItem == null) return;
            if (stateHandle.GetCurrentItem.data.itemType != ItemType.Brainrot) return;
            var item = stateHandle.GetCurrentItem;
            data.RemoveItem(item.GetItemConfig.id, item.GetItemConfig.level);
            UpdateItemBackPack(item, false);
            stateHandle.ResetItem();

            cost = item.data.cost;
            Income(cost);


        }
        void SellAllBrainrot()
        {
            if (stateHandle.GetCurrentItem != null)
            {
                if (stateHandle.GetCurrentItem.data.itemType == ItemType.Brainrot)
                {
                    stateHandle.ResetItem();
                }
            }

            long cost = 0;
            List<ItemConfig> removes = new List<ItemConfig>();
            foreach (var cf in data.itemConfigs)
            {
                var so = GamePlayControl.instance.GetItemDataSO(cf.id);
                if(so != null && so.itemType == ItemType.Brainrot)
                {
                    removes.Add(cf);
                    UpdateItemBackPack(so, cf, false);
                    cost += so.cost;
                }
            }

            foreach (var rm in removes)
            {
                data.RemoveItem(rm.id, rm.level);
            }

            if (cost > 0)
                Income(cost);
        }

        #endregion

        void HandleEventWhenChangeData()
        {
            eventWhenChangeData.Invoke(data);
        }

        public void Rebirth(long price, List<string> listBot ,bool isAds)
        {
            data.UpdateIndexRebirth();
            if (!isAds)
            {
                PayForUpgrade(price);

                ItemConfig cf = null;
                foreach (var id in listBot)
                {
                    cf = data.itemConfigs.FirstOrDefault(it => it.id == id);
                    if(cf!= null)
                    {
                        var so = GamePlayControl.instance.GetItemDataSO(cf.id);
                        if (so != null && so.itemType == ItemType.Brainrot)
                        {
                            UpdateItemBackPack(so, cf, false);
                        }

                        if(stateHandle.GetCurrentItem != null)
                        {
                            if (stateHandle.GetCurrentItem.data == so && stateHandle.GetCurrentItem.GetItemConfig == cf) stateHandle.ResetItem();
                        }
                        data.RemoveItem(cf.id, cf.level);
                    }
                }
            }
        }

        public void KnockBack(int value, Transform sender)
        {

            VfxCtrl.instance.SpawnRandomVfx(TypeVFX.Blood, transform.position + Vector3.up * 1.5f);
            stateHandle.DropItem(false, RemoveDataItem);
            Dead();
            StartCoroutine(ResetAfterDead(false));
        }
    }

    public enum EPlayerState
    {
        None,
        Normal,
        KnockBack,
        HoldItemBot,
        Dead,
    }
    public enum PlayerSellType { CurrentBrainrot, AllBrainrot}
    public enum TypeHandleSlot
    {
        None,PickupBot,PutBot,Steal
    }
}
