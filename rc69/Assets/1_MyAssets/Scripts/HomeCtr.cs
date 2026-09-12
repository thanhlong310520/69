using Raccoon;
using Raccoon.Bot;
using Raccoon.Building;
using Raccoon.Controller;
using Raccoon.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HomeCtr : MonoBehaviour
{
    public List<Home> listHome;
    public GameObject rebirthGOPrefab;
    public static HomeCtr instance;
    public long costUnlock = 0;
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
    private void Start()
    {
        foreach (var h in listHome)
        {
            CharactorData data = GameData.Get.GetCharacterData(h.nameHome);
            RebirthDataSO rebirthSO = GameData.Get.GetCurrentRebirth(data.GetIndexRebirth());
            h.Init(data);

            float multi = 1;
            if(rebirthSO != null)
            {
                multi = rebirthSO.moneyMult;
            }

            OnSetMultiIncome(h, multi);
            
            OnSetDataSlot(h, data);


            ICharactor ic = h.user.GetComponent<ICharactor>();
            ic.SetCharactorData(data);
            ic.SetStartPoint(h.startPoint.position);
        }
    }

    void OnSetDataSlot(Home home, CharactorData data)
    {
        foreach (var s in home.AllSlots)
        {
            DataItemSlot dataBrainInSlot = data.GetIdBotInSlot(s.id);
            s.SetStateSlot(Slot_state.Free);
            if(dataBrainInSlot != null)
            {
                Debug.Log("dataBrainInSlot.config.id " + dataBrainInSlot.config.id);

                Debug.Log("list so " + GamePlayControl.instance.GetBotDataSOs().Count);

                ItemDataSO itemData = GamePlayControl.instance.GetBotDataSOs().Find(i => i.GetId == dataBrainInSlot.config.id);
                Debug.Log("so " +  itemData);
                var botGO = GamePlayControl.instance.SpawnItem(itemData.prefab);
                if(botGO  != null)
                {
                    ItemCanStaySlot itemcanstayslot = botGO.GetComponent<ItemCanStaySlot>();
                    itemcanstayslot.Init(itemData, dataBrainInSlot.config, Raccoon.EnumHolder.ItemState.Save);
                    s.PutItem(itemcanstayslot);
                    s.SetActionChangeIncome(dataBrainInSlot.ChangeIncome);
                    s.SetStartIncome(dataBrainInSlot.income);
                    botGO.gameObject.SetActive(true);
                }
            }
        }

        foreach (var s in home.slots2)
        {
            if (!data.listSlotGate2Unlock.Contains(s.id))
            {
                s.SetStateSlot(Slot_state.LockGate);
                var go = Instantiate(rebirthGOPrefab, transform);
                go.transform.position = s.transform.position;
                home.SetLockSlot2(s.id, go);
            }
        }
        
    }


    public void OnSetMultiIncome(Home home, float multi)
    {
        foreach (var s in home.AllSlots)
        {
            s.SetMultiSpeed(multi);
        }
    }

    public void UnlockSlotInGate2(Transform user, string idSlot)
    {
        Home home = GetHomeByUser(user);
        if(home == null) return;
        if (home.SetUnlocSlotGate2ById(idSlot))
        {
            var go = home.GetRebirthGo(idSlot);
            Destroy(go);
        }
    }
    public Home GetHomeByUser(Transform user)
    {
        return listHome.FirstOrDefault(h => h.user == user);
    }

    public Transform GetStartPos(Transform user)
    {
        Transform result = null;
        var home = listHome.FirstOrDefault(h=>h.user == user);
        if(home != null)
        {
            result = home.startPoint;
        }
        return result;
    }

    public List<SlotController> GetAllSlots(Transform user)
    {
        List<SlotController> result = new List<SlotController>();
        var home = listHome.FirstOrDefault(h => h.user == user);
        if (home != null)
        {
            result.AddRange(home.slots1);
            result.AddRange(home.slots2);
        }
        return result;
    }
    public SlotController GetSlotFree(Transform user)
    {
        SlotController result = null;
        var home = listHome.FirstOrDefault(h => h.user == user);
        if (home != null)
        {
            foreach (var s in home.slots1)
            {
                if(s.GetSlot_State == Slot_state.Free)
                {
                    result = s;
                    break;
                }
            }
            if(result != null) return result;

            foreach (var s in home.slots2)
            {
                if (s.GetSlot_State == Slot_state.Free)
                {
                    result = s;
                    break;
                }
            }
        }
        return result;
    }
}
[Serializable]
public class Home
{
    public string nameHome;
    public Transform user;
    public Transform startPoint;
    public List<SlotController> slots1;
    public List<SlotController> slots2;

    List<SlotController> allSlot;

    [SerializeField] private List<LockSlot> lockslots;
    public void Init(CharactorData data)
    {
        allSlot = new List<SlotController>();
        allSlot.AddRange(slots1);
        allSlot.AddRange(slots2);
        for(int i = 0;i  < allSlot.Count; i++)
        {
            allSlot[i].Init(user, i.ToString());
        }
        lockslots = new List<LockSlot>();
    }

    public void SetLockSlot2(string id, GameObject rebirthgo)
    {
        lockslots.Add(new LockSlot(id, rebirthgo));
    }
    public void SetUnlocSlot2()
    {
        //Debug.Log("home Unlock");
        //foreach (var s in slots2)
        //{
        //    s.SetStateSlot(Slot_state.Free);
        //}
    }
    public bool SetUnlocSlotGate2ById(string id)
    {
        var slot = slots2.FirstOrDefault(s => s.id == id);
        if (slot != null)
        {
            slot.SetStateSlot(Slot_state.Free);
            return true;
        }
        return false;
    }
    public List<SlotController> AllSlots => allSlot;

    public SlotController GetSlotLock()
    {
        return slots2.FirstOrDefault(s => s.GetSlot_State == Slot_state.LockGate);
    }

    public GameObject GetRebirthGo(string id)
    {
        var ls = lockslots.FirstOrDefault(s => s.id == id);
        return ls.rebirth;
    }
    public string GetNameHome => nameHome;

}

[System.Serializable] struct LockSlot
{
    public GameObject rebirth;
    public string id;
    public LockSlot(string id, GameObject go)
    {
        this.id = id;
        rebirth = go;
    }
}