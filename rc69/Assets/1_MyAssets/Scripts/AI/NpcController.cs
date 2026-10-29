using Raccoon;
using Raccoon.Building;
using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.EnumHolder;
using Raccoon.Item;
using Raccoon.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class NpcController : MonoBehaviour, ICharactor, IReceive
{
    public PlayerContact contact;
    public WorldState worldState;
    public GoapAgent goapAgent;
    public NpcMove move;
    public PlayerStateHandle stateHandle;
    public PlayerLoot loot;
    public CapsuleCollider capsule;
    public Rigidbody rb;
    public CharactorData data;
    List<BotData> dataSOs;
    List<UpgradeDataSO> upgradeDataSOs;

    public List<SlotController> slots;
    List<SlotController> slotFree;
    List<SlotController> slotIncome;
    float multiSpeed;
    float multiJump;
    DataNpcUpgrade dataUpgrade;
    public List<SlotController> GetSlotIncomes => slotIncome;
    public bool InSaveZone => contact != null && contact.InSaveZone;
    bool inited = false;
    public bool attacking;

    public UnityEvent eventDoneAttack;


    void SetWorldStateData(GoapWorldStateEnum key, bool value)
    {
        worldState.Data.Set(key.ToString(), value);
    }

    public void SetSpeed()
    {
        float set = 0;
        if (upgradeDataSOs != null)
        {
            List<string> listUpdated = data.upgrade.GetDicUpgraded(TypeUpgrade.Speed);
            var us = upgradeDataSOs.FirstOrDefault(u => u.type == TypeUpgrade.Speed);
            foreach (var u in listUpdated)
            {
                set += us.GetPrecent(u);
            }
        }
        move.SetMultiSpeed(set);
    }
    public void ChangeModel()
    {
        var listSo = GamePlayControl.instance.GetFashionDataSOs();
        int index = UnityEngine.Random.Range(0, listSo.Count);
        var so = listSo[index];
        data.ChangeFashion(so.GetId);
        stateHandle.ChangeModel(so.prefab);

        foreach (var item in loot.GetItemLoots)
        {
            item.SetTarget(stateHandle.holderItem);
        }
    }
    private void Update()
    {
        if (!inited) return;
        UpdateWorldStateData();
    }

    
    void AddBotData(ItemDataSO so, ItemConfig cf)
    {
        if(so == null) return;
        if (so.itemType == ItemType.Brainrot || so.itemType == ItemType.Egg)
        {
            BotData bd = new BotData(cf, so);
            dataSOs.Add(bd);
        }
    }
    void RemoveBotData(ItemDataSO so, ItemConfig cf)
    {
        if (so == null) return;
        if (so.itemType == ItemType.Brainrot || so.itemType == ItemType.Egg)
        {
            var bd = dataSOs.FirstOrDefault(b => b.so == so && b.config == cf);
            dataSOs.Remove(bd);
        }
    }
    #region AI
    void UpdateWorldStateData()
    {
        SetWorldStateData(GoapWorldStateEnum.HaveBrainRot, dataSOs.Count > 0);
        SetWorldStateData(GoapWorldStateEnum.HaveLoot, loot.IsHaveItemLoot());
        SetWorldStateData(GoapWorldStateEnum.HaveSlotFree, slotFree != null && slotFree.Count > 0);
        SetWorldStateData(GoapWorldStateEnum.HaveSlotIncome, slotIncome != null && slotIncome.Count > 0);

    }

    public Transform GetStartPos()
    {
        return HomeCtr.instance.GetStartPos(this.transform);
    }
    public SlotController GetSlotFree()
    { 
        SlotController slot = null;
        if (slotFree.Count > 0) slot = slotFree[0];
        return slot;
    }

    void SetSlot()
    {
        slots = HomeCtr.instance.GetAllSlots(transform);
        slotFree = slots.Where(s => s.GetSlot_State == Slot_state.Free).ToList();
        slotIncome = slots.Where(s => s.GetSlot_State == Slot_state.HaveItem).ToList();
    }
    public void GetBrainrot()
    {
        if(dataSOs.Count <= 0) return;
        stateHandle.ChangeItem(dataSOs[0].config);
    }

    public void PushToSlot(SlotController slot)
    {
        if (stateHandle.GetCurrentItem == null) return;
        if(stateHandle.GetCurrentItem.data.itemType != ItemType.Brainrot && stateHandle.GetCurrentItem.data.itemType != ItemType.Egg) return;
        stateHandle.HandleSlot(slot, TypeHandleSlot.PutBot);
    }

    public void Upgrade()
    {
        if (!worldState.Data.Get(GoapWorldStateEnum.HaveUpgrade.ToString())) return;

        PayForUpgrade(dataUpgrade.upgradeSO.data[dataUpgrade.index].price);
        data.CUpdate(dataUpgrade.upgradeSO.type, dataUpgrade.upgradeSO.data[dataUpgrade.index].id);
        SetDataUpgrade(dataUpgrade.upgradeSO);
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
                    multiSpeed = set;
                    break;
                }
            case TypeUpgrade.JumpForce:
                {
                    multiJump = set;
                    break;
                }
            case TypeUpgrade.SlotLoot:
                {
                    loot.SetNumberLoot(set);
                    set += 1;
                    break;
                }
        }
    }

    public void SellAllBrainrot()
    {
        if (stateHandle.GetCurrentItem != null)
        {
            if (stateHandle.GetCurrentItem.data.itemType == ItemType.Brainrot)
            {
                stateHandle.ResetItem();
            }
        }
        long cost = 0;
        foreach (var dt in dataSOs)
        {
            cost += dt.so.cost;
            data.RemoveItem(dt.config.id, dt.config.level);
        }
        dataSOs.Clear();
        if (cost > 0)
            Income(cost);
    }
    public void Income(long income)
    {
        data.AddCoin(income);
        GetHaveUpgrade();
    }
    public void PayForUpgrade(long pay)
    {
        data.AddCoin(-pay);
        GetHaveUpgrade();
    }
    public void GetHaveUpgrade()
    {
        bool have = false;
        foreach (var u in upgradeDataSOs)
        {
            for (int i = 0; i < u.data.Count; i++)
            {
                if (u.data[i].price <= data.GetCoin)
                {
                    dataUpgrade = new DataNpcUpgrade(u, i);
                    have = true;
                    break;
                }
            }
        }
        SetWorldStateData(GoapWorldStateEnum.HaveUpgrade, have);

    }
    public void IncomeBySlot(SlotController slot)
    {
        slot.HandleCollectIncome(transform);
        int pickup = UnityEngine.Random.Range(0, 2);
        if(pickup < 1)
            stateHandle.HandleSlot(slot, TypeHandleSlot.PickupBot);
    }


    public bool ChangeItemWeapon()
    {
        if (stateHandle.GetCurrentItem != null && stateHandle.GetCurrentItem.data != null && stateHandle.GetCurrentItem.data.itemType == ItemType.Weapon) return true;

        ItemConfig icWeapon = null;
        foreach (var ic in data.itemConfigs)
        {
            var so = GamePlayControl.instance.GetItemDataSO(ic.id);
            if(so != null && so.itemType  == ItemType.Weapon)
            {
                icWeapon = ic;
                break;
            }
        }
        if (icWeapon == null) return false;
        stateHandle.ChangeItem(icWeapon);
        return true;
    }

    public bool CheckAttack()
    {
        if(attacking) return false;
        if (loot.IsLooting) return false;
        if (stateHandle.GetPlayerState == EPlayerState.KnockBack) return false;
        stateHandle.Attack();
        attacking = true;
        return true;
    }

    public void DoneAttack()
    {
        attacking = false;
        eventDoneAttack?.Invoke();
    }
    #endregion
    #region Loot

    public void OnLoot(Transform itemGO)
    {
        ItemCtr item = itemGO.GetComponent<ItemCtr>();
        HandlePickUp(item);
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
    void RemoveDataItem(ItemCtr item)
    {
        var config = item.GetItemConfig;
        data.RemoveItem(config.id, config.level);
    }
    void AddDataItem(ItemCtr item)
    {
        GamePlayControl.instance.RespawnItem(item.gameObject);
        var config = item.GetItemConfig;
        ItemConfig currentconfig = null;
        if (stateHandle.GetCurrentItem != null) currentconfig = stateHandle.GetCurrentItem.GetItemConfig;
        data.AddItem(config, currentconfig);
        stateHandle.ChangeItem(config);

    }
    public void OnDropItemLoot()
    {
        loot.DropItemLoot(transform);
        stateHandle.ChangeLayerItem(ItemType.None);
    }
    void OnStartLoot()
    {

    }
    void LootFail()
    {

    }
    void LootSuccess(ItemCtr item)
    {
        item.Stealed(stateHandle.holderItem,transform);
        stateHandle.LootHandle();
    }
    void OnWhileLoot(float progess)
    {

    }
    public void SaveLoot()
    {
        if (!loot.IsHaveItemLoot()) return;

        var listItem = loot.GetItemLoots;
        foreach (var item in listItem)
        {
            AddBotData(item.data,item.GetItemConfig);
        }

        var listSave = loot.SaveLoot(transform);
        data.AddListItemConfig(listSave);
        stateHandle.ChangeItem(data.itemConfigs[0]);
    }

    #endregion
    #region StateHandle
    void AddItemToDataWhenPickup(SlotController slot, ItemCtr item)
    {
        var config = item.GetItemConfig;
        if (config == null) return;
        data.RemoveBotInSlot(slot.id);
        ItemConfig currentconfig = null;
        if (stateHandle.GetCurrentItem != null) currentconfig = stateHandle.GetCurrentItem.GetItemConfig;
        data.AddItem(config, currentconfig);
        slotFree.Add(slot);
        slotIncome.Remove(slot);
        AddBotData(item.data,item.GetItemConfig);
    }
    void RemoveItemFromDataWhenPutInSlot(SlotController slot, ItemCtr item)
    {
        var config = item.GetItemConfig;
        if (config == null) return;
        var dtbotinslot = data.AddBotInSlot(slot.id, config);
        slot.SetActionChangeIncome(dtbotinslot.ChangeIncome);
        data.RemoveItem(config.id, config.level);
        slotFree.Remove(slot);
        slotIncome.Add(slot);
        RemoveBotData(item.data,item.GetItemConfig);
    }
    void Steal(SlotController slot, ItemCtr item)
    {
        var config = item.GetItemConfig;
        if (config == null) return;
        data.AddItem(config, null);
        stateHandle.ChangeItem(config);
    }

    public void StealBrainrotInSlot(SlotController slot)
    {
        if (slot == null) return;
        var bot = slot.GetBotAtSlot();
        if (bot == null) return;
        Steal(slot, bot);
        slot.Stealed();
    }
    void ActionWhenChangeItem(ItemType type)
    {
    }
    #endregion
    #region SetCharactor
    public void SetCharactorData(CharactorData data)
    {
        inited = true;
        attacking = false;

        if (data == null)
        {
            Home home = HomeCtr.instance.GetHomeByUser(transform);
            data = GameData.Get.GetCharacterData(home.nameHome);
        }
        this.data = data;
        dataSOs = new List<BotData>();
        slotFree = new List<SlotController>();
        slotIncome = new List<SlotController>();

        loot.Init(OnStartLoot, LootFail, OnWhileLoot, LootSuccess);
        stateHandle.Init(rb, AddItemToDataWhenPickup, RemoveItemFromDataWhenPutInSlot,Steal , ActionWhenChangeItem);
        SetSlot();

        upgradeDataSOs = GamePlayControl.instance.GetUpgradeDataSOs;
        var botso = GamePlayControl.instance.GetItemDataStaySlotSOs();
        foreach (var cf in data.itemConfigs)
        {
            ItemDataSO so = botso.Find(i => i.GetId == cf.id);
            AddBotData(so,cf);
        }
        ChangeModel();


        SetWorldStateData(GoapWorldStateEnum.SaveLoot, false);
        SetWorldStateData(GoapWorldStateEnum.CollectCoin, false);
        GetHaveUpgrade();
        UpdateWorldStateData();
        foreach (var u in upgradeDataSOs)
        {
            SetDataUpgrade(u);
        }

        StartCoroutine(DelayStartGoap());
    }
    public void AddItemUnlock(ItemDataSO so)
    {

    }
IEnumerator DelayStartGoap()
    {
        yield return new WaitForSeconds(1f);
        if(data.itemConfigs.Count > 0) stateHandle.ChangeItem(data.itemConfigs[0]);
        StealDone();
        goapAgent.SetStart(true);
    }

    public void SetStartPoint(Vector3 startPoint)
    {
        transform.position = startPoint;
    }

    public void ContactShop(TypeShop type)
    {
    }

    public void ContactUnlockShop(long costUnlock)
    {

    }

    public void ContactWave()
    {
        VfxCtrl.instance.SpawnRandomVfx(TypeVFX.Blood, transform.position + Vector3.up * 1.5f);
        Dead();
        StartCoroutine(ResetAfterDead(true));
    }

    public void Stealed(SlotController slot)
    {
        data.RemoveBotInSlot(slot.id);
        slotFree.Add(slot);
        slotIncome.Remove(slot);
        loot.DropItemLoot(transform);
        SetWorldStateData(GoapWorldStateEnum.Stealed, true);
        goapAgent.ForceReplan();
        
    }


    public bool canSteal = false;
    public CountTime ct;
    public void StealDone()
    {
        canSteal = false;
        stateHandle.ChangeItem(null);
        float timeDelay = UnityEngine.Random.Range(180,300);
        ct.Init(timeDelay, null, setCanSteal);
    }

    public void setCanSteal()
    {
        canSteal = true;
    }

    #endregion

    #region IRecieve
    public void KnockBack(int value, Transform sender)
    {
        VfxCtrl.instance.SpawnRandomVfx(TypeVFX.Blood, transform.position + Vector3.up * 1.5f);
        stateHandle.DropItem(false, RemoveDataItem);
        Dead();
        StartCoroutine(ResetAfterDead(false));
    }
    IEnumerator ResetAfterDead(bool haveResetPos)
    {
        yield return new WaitForSeconds(2f);
        stateHandle.playerAnim.DoneKnockBack();
        SetNormal(haveResetPos);
        goapAgent.ResetAgent();

    }

    public void BackBase()
    {
        goapAgent.ForceReplan();
        loot.DropItemLoot(transform);
        SetNormal(true);

    }

    void Dead()
    {
        capsule.enabled = false;
        stateHandle.Die();
        loot.DropItemLoot(transform);
        goapAgent.StopAgent();
        move.StopMove();
    }

    void SetNormal(bool haveResetPos = true)
    {
        if (haveResetPos)
        {
            Transform startPos = HomeCtr.instance.GetStartPos(transform);
            transform.position = startPos.position;
        }
        move.ResetMove();
        capsule.enabled = true;
        stateHandle.ChangeState(EPlayerState.Normal);
    }

    public void NotifyBossChase()
    {
        
    }

    public CharactorData Getdata()
    {
        return data;
    }

    public void ContactTreamill(Treamill treamill)
    {

    }

    #endregion
}

[System.Serializable]
class BotData
{
    public ItemConfig config;
    public ItemDataSO so;

    public BotData(ItemConfig cf,  ItemDataSO so)
    {
        this.config = cf;
        this.so = so;
    }
}

[System.Serializable]
struct DataNpcUpgrade
{
    public UpgradeDataSO upgradeSO;
    public int index;

    public DataNpcUpgrade(UpgradeDataSO type, int index)
    {
        this.upgradeSO = type;
        this.index = index;
    }
}