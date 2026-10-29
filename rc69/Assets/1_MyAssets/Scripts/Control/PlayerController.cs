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
        public PlayerTreamill playerTreamill;

        public PlayerSoundFootStep sfs;

        public Rigidbody rb;
        public CapsuleCollider capsule;
        public PlayerContact contact;

        public Camera mainCam;
        public CharactorData data;
        public CharactorData GetCharactorData => data;
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
            //foreach (TypeUpgrade type in Enum.GetValues(typeof(TypeUpgrade)))
            //{
            //    UpgradeDataSO upgradeDataSO = GamePlayControl.instance.GetUpgradeDataByType(type);
            //    SetDataUpgrade(upgradeDataSO);
            //}
            foreach (var item in GamePlayControl.instance.GetUpgradeDataSOs)
            {
                SetDataUpgrade(item);
            }
            this.data.onChangeItem += UpdateUIWhenChangeItem;
            this.data.onChangeCoin += UpdateUIWhenChangeCoin;
            this.data.onAddBotUnlock += UpdateItemIndexUnlock;

            ChangeModelWhenClickItemFashion(data.currentFashion);
            UpdateUIWhenChangeItem(data.itemConfigs);
            UpdateUIWhenChangeCoin(data.coin);
            foreach (var cf in data.itemConfigs)
            {
                var so = GamePlayControl.instance.GetItemDataSO(cf.id);
                UpdateItemBackPack(so, cf, true);
            }
            foreach (var botunlock in data.listBotUnlock)
            {
                foreach (var id in botunlock.Value.listUnlock)
                {
                    UpdateItemIndexUnlock(id);
                }
            }

            HandleEventWhenChangeData();
        }
        public void SetStartPoint(Vector3 startPoint)
        {
            rb.position = startPoint;
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
            AddItemUnlock(item.data);
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


        public void NotifyBossChase()
        {
            GamePlayControl.instance.ShowUIBossChase();
        }


        Treamill tm;
        bool inTreamill;
        public void ContactTreamill(Treamill treamill)
        {
            tm = treamill;
            movement.SetCanMove(false);
            if(tm != null)
            {
                rb.position = treamill.holderPos.position;
                rb.rotation = treamill.holderPos.rotation;
                rb.useGravity = false;
            }
            inTreamill = true;
            stateHandle.playerAnim.PlayAnimatorTreamill();
            if (playerTreamill == null) playerTreamill = gameObject.AddComponent<PlayerTreamill>();
            playerTreamill.StartRun(tm, data, RefreshSpeed);
        }

        void RefreshSpeed()
        {
            var speedUpgrade = GamePlayControl.instance.GetUpgradeDataByType(TypeUpgrade.Speed);
            if (speedUpgrade != null) SetDataUpgrade(speedUpgrade);
        }

        void EndTreamill()
        {
            if(tm != null) rb.position = tm.endPos.position;
            rb.useGravity = true;
            movement.SetCanMove(true);
            inTreamill = false;
            tm = null;
            stateHandle.playerAnim.EndAnimTreamill();
            if (playerTreamill != null) playerTreamill.StopRun();
        }

        public bool CanUpdateTreamill()
        {
            var listTreamill = GamePlayControl.instance.GetTreamillDataSOs;
            if (listTreamill == null) return false;
            return data.GetIndexTreamill() + 1 < listTreamill.Count;
        }

        public bool EnoughCoinUpdateTreamill()
        {
            if (!CanUpdateTreamill()) return false;
            TreamillDataSO nextTreamill = GamePlayControl.instance.GetTreamillDataSOs[data.GetIndexTreamill() + 1];
            return nextTreamill.price <= data.GetCoin;
        }

        public void UpdateTreamill(UnityAction<CharactorData> action = null)
        {
            if (!CanUpdateTreamill()) return;

            TreamillDataSO nextTreamill = GamePlayControl.instance.GetTreamillDataSOs[data.GetIndexTreamill() + 1];
            if (nextTreamill.price > data.GetCoin)
            {
                ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.BuyFall);
                return;
            }

            PayForUpgrade(nextTreamill.price);
            data.UpdateIndexTreamill();
            ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.UpgradeSuccess);

            Home home = HomeCtr.instance.GetHomeByUser(transform);
            home?.SetDataTreamill(data);

            action?.Invoke(data);
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
            if (value.isJump)
            {
                if (inTreamill)
                {
                    EndTreamill();
                }
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
                    if (curentSlot.GetSlot_State == Slot_state.HaveItem) typeHandleSlot = TypeHandleSlot.PickupBot;
                    else if(curentSlot.GetSlot_State == Slot_state.Free)
                    {
                        if (stateHandle.GetCurrentItem != null && (stateHandle.GetCurrentItem.data.itemType == ItemType.Brainrot || stateHandle.GetCurrentItem.data.itemType == ItemType.Egg)) typeHandleSlot = TypeHandleSlot.PutBot;
                    }
                }
                else
                {
                    if (curentSlot.GetSlot_State == Slot_state.HaveItem) typeHandleSlot = TypeHandleSlot.Steal;
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
                if(itemContact != null) itemContact.StartConttact();
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
            if (stateHandle.GetCurrentItem.data.itemType != ItemType.Brainrot && stateHandle.GetCurrentItem.data.itemType != ItemType.Egg) return;

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
            loot.DropItemLoot(transform);
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
            item.Stealed(stateHandle.holderItem,transform);

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
                       
            }
            var listSave = loot.SaveLoot(transform);
            data.AddListItemConfig(listSave);
            var lastItem = listSave[listSave.Count - 1];
            stateHandle.ChangeItem(lastItem);
            UpdateSelectAfterSaveLoot(lastItem);
        }

        #endregion


        public void CheckUpgrade(UpgradeDataSO upgradeData, string id, bool useAds,UnityAction<CharactorData> action)
        {
            if (useAds)
            {

                if(upgradeData.GetData(id).numberAds <= 0)
                {
                    Upgrade(upgradeData, id);
                }


                //Watch Ads
                AdsActionManager.instance.ShowRewardAds((success) =>
                {
                    if (success)
                    {
                        data.upgrade.AddAds(upgradeData.type, id);
                        if (upgradeData.EnoughAds(id, data.upgrade.GetNumberAds(upgradeData.type, id)))
                        {
                            Upgrade(upgradeData, id);
                        }
                    }
                });
            }

            else
            {
                if(upgradeData.GetPriceUpgrade(id) <= data.GetCoin)
                {
                    PayForUpgrade(upgradeData.GetPriceUpgrade(id));
                    Upgrade(upgradeData, id);
                }
                else
                {
                    ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.BuyFall);
                }


            }
            action?.Invoke(data);
        }

        void Upgrade(UpgradeDataSO upgradeData, string id)
        {
            ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.UpgradeSuccess); 
            data.CUpdate(upgradeData.type,id);
            eventTutorial?.Invoke(TypeActionTutorial.Upgrade);
            SetDataUpgrade(upgradeData);
        }

        public void SetDataUpgrade(UpgradeDataSO upgradeData)
        {
            float set = 0;
            if (upgradeData != null)
            {
                List<string> listUpdated = data.upgrade.GetDicUpgraded(upgradeData.type);
                foreach (var u in listUpdated)
                {
                    set += upgradeData.GetPrecent(u);
                }
            }
            switch (upgradeData.type)
            {
                case TypeUpgrade.Speed:
                    {
                        set += GetMultiSpeedBuyTreamill();
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


        float GetMultiSpeedBuyTreamill()
        {
            return data.multybuytreaMill;
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
        void UpdateItemIndexUnlock(string id)
        {
            var so = GamePlayControl.instance.GetItemDataStaySlotSOs().Find(i => i.GetId == id);
            if (so == null) return;

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
            loot.DropItemLoot(transform);
            stateHandle.Die();
        }

        void SetNormal(bool haveResetPos = true)
        {
            if (haveResetPos)
            {
                Transform startPos = HomeCtr.instance.GetStartPos(transform);
                SetStartPoint(startPos.position);
                VfxCtrl.instance.SpawnRandomVfx(TypeVFX.BackBase, startPos.position);
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
            SetStartPoint(startPos.position);

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
                if (stateHandle.GetCurrentItem.data.itemType == ItemType.Brainrot || stateHandle.GetCurrentItem.data.itemType == ItemType.Egg)
                {
                    stateHandle.ResetItem();
                }
            }

            long cost = 0;
            List<ItemConfig> removes = new List<ItemConfig>();
            foreach (var cf in data.itemConfigs)
            {
                var so = GamePlayControl.instance.GetItemDataSO(cf.id);
                if(so != null && (so.itemType == ItemType.Brainrot || so.itemType == ItemType.Egg))
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

        public void SellItemByListItemConfig(List<ItemConfig> removeList)
        {
            if(stateHandle.GetCurrentItem != null)
            {
                if(removeList.Contains(stateHandle.GetCurrentItem.GetItemConfig)) stateHandle.ResetItem();
            }

            long cost = 0;
            foreach (var rm in removeList)
            {
                if (!data.itemConfigs.Contains(rm)) continue;
                var so = GamePlayControl.instance.GetItemDataSO(rm.id);
                if (so != null)
                {
                    UpdateItemBackPack(so, rm, false);
                    cost += so.cost;
                }
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

        public void AddItemUnlock(ItemDataSO so)
        {

            if (data == null) return;

            if (so.itemType != ItemType.Brainrot) return;

            data.AddItemUnlock(so.colorType, so.GetId);
        }

        public CharactorData Getdata()
        {
            return data;
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
