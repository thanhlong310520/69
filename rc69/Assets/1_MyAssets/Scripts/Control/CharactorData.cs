using MessagePack;
using Raccoon.Data;
using Raccoon.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[MessagePackObject(keyAsPropertyName: true)]
[System.Serializable]
public class CharactorData
{
    public string id;
    public string name;
    public long coin;
    public string currentFashion;
    public float multybuytreaMill;
    public CharactorUpgrade upgrade;
    public Dictionary<string, DataItemSlot> dicBotInSlots;
    public List<string> listBotUnlock;
    public List<string> listSlotGate2Unlock;
    public int indexRebirth = -1;
    public List<ItemConfig> itemConfigs;


    [NonSerialized]
    [IgnoreMember]
    public Action<long> onChangeCoin;

    [NonSerialized]
    [IgnoreMember]
    public Action<List<ItemConfig>> onChangeItem;

    public CharactorUpgrade GetUpgrade()
    {
        if(upgrade == null) upgrade = new CharactorUpgrade();   
        return upgrade;
    }
    public void CUpdate(TypeUpgrade type,string idUpdate)
    {
        upgrade.Update(type,idUpdate);
    }



    public CharactorData(string id, string name)
    {
        this.id = id;
        this.name = name;
        coin = 0;
        indexRebirth = -1;
        multybuytreaMill = 0;
        upgrade = new CharactorUpgrade();
        itemConfigs = new List<ItemConfig>();
        itemConfigs.Add(new ItemConfig("default", 10));
        currentFashion = "FashionData_Brain";
        dicBotInSlots = new Dictionary<string, DataItemSlot>();
        listBotUnlock = new List<string>();
        listSlotGate2Unlock = new List<string>();
    }
    #region itemInvetory
    public void AddItem(ItemConfig itemAdd,ItemConfig currentItem)
    {
        int indexCurrentItem = -1;
        if (currentItem != null)
        {
            indexCurrentItem = itemConfigs.IndexOf(currentItem);
            if (indexCurrentItem > -1) itemConfigs.RemoveAt(indexCurrentItem);
        }

        InsertByPriority(itemConfigs, itemAdd);

        if(indexCurrentItem > -1) itemConfigs.Insert(indexCurrentItem, currentItem);

        onChangeItem?.Invoke(itemConfigs);
    }

    void InsertByPriority(List<ItemConfig> list, ItemConfig newItem)
    {
        int left = 0;
        int right = list.Count - 1;

        while (left <= right)
        {
            int mid = (left + right) / 2;

            if (list[mid].priority > newItem.priority)
                left = mid + 1;
            else
                right = mid - 1;
        }

        list.Insert(left, newItem);
    }
    public void AddListItemConfig(List<ItemConfig> listAdd)
    {
        foreach (var i in listAdd)
        {
            InsertByPriority(itemConfigs, i);
        }
        onChangeItem?.Invoke(itemConfigs);
    }

    public ItemConfig RemoveItem(string id, int level)
    {
        ItemConfig ItemRemove = itemConfigs.FirstOrDefault(item => (item.id == id && item.level == level)); 

        if (ItemRemove != null)
        {
            itemConfigs.Remove(ItemRemove);
            onChangeItem?.Invoke(itemConfigs);
        }
        return ItemRemove;
    }
    #endregion

    #region itemslot
    public void RemoveBotInSlot(string id)
    {
        if(dicBotInSlots.ContainsKey(id)) dicBotInSlots.Remove(id);
    }
    public DataItemSlot AddBotInSlot(string idSlot, ItemConfig botConfig)
    {
        if (!dicBotInSlots.ContainsKey(idSlot)) dicBotInSlots.Add(idSlot, new DataItemSlot(botConfig));
        else dicBotInSlots[idSlot].config = botConfig;
        return dicBotInSlots[idSlot];
        
    }

    public DataItemSlot GetIdBotInSlot(string idSlot)
    {
        DataItemSlot result = null;
        if (dicBotInSlots.ContainsKey(idSlot)) result = dicBotInSlots[idSlot];
        return result;
    }

    public List<string> GetAllIdItem()
    {
        List<string> result = new List<string>();
        foreach (var cf in itemConfigs)
        {
            result.Add(cf.id);
        }
        return result;
    }
    #endregion

    public bool AddItemUnlock(string id)
    {
        if(listBotUnlock.Contains(id)) return false;
        listBotUnlock.Add(id);
        return true;
    }

    public void ChangeFashion(string id)
    {
        currentFashion = id;
    }
    public void AddCoin(long add)
    {
        coin += add;
        onChangeCoin?.Invoke(coin);
    }

    public void UnlockSlotGate2(string id)
    {
        if(listSlotGate2Unlock.Contains(id)) return;
        listSlotGate2Unlock.Add(id);
    }
    public long GetCoin => coin;


    public int GetIndexRebirth()
    {
        return indexRebirth;
    }
    public void UpdateIndexRebirth()
    {
        indexRebirth++;
    }

}

[MessagePackObject(keyAsPropertyName: true)]
[System.Serializable]
public class CharactorUpgrade
{
    public Dictionary<TypeUpgrade, List<UpgareData>> dic;
    public CharactorUpgrade()
    {
        dic = new Dictionary<TypeUpgrade, List<UpgareData>>();
    }

    public List<string> GetDicUpgraded(TypeUpgrade type)
    {
        List<string> ret = new List<string>();
        if (dic.ContainsKey(type))
        {
            if(dic[type] == null)
            {
                dic[type] = new List<UpgareData>();
                return ret;
            }
            dic[type].ForEach(d => {
                if (d.upgraded) ret.Add(d.id);
            } );
        }
        return ret;
    }
    public void Update(TypeUpgrade type,string idUpgrade)
    {
        UpgareData u = GetUpgareDataByID(type, idUpgrade);
        u.upgraded = true;

    }
    public void AddAds(TypeUpgrade type, string idUpgrade)
    {
        UpgareData u = GetUpgareDataByID(type, idUpgrade);
        u.AddAds();
    }
    public int GetNumberAds(TypeUpgrade type, string idUpgrade)
    {
        UpgareData u = GetUpgareDataByID(type, idUpgrade);
        return u.numberAds;
    }
    UpgareData GetUpgareDataByID(TypeUpgrade type, string id)
    {
        if (!dic.ContainsKey(type))
        {
            dic[type] = new List<UpgareData> { new UpgareData(id) };
        }
        else
        {
            if (dic[type] == null)
            {
                dic[type] = new List<UpgareData> { new UpgareData(id) };
            }
        }
        var result = dic[type].FirstOrDefault(u => u.id == id);
        if (result == null)
        {
            result = new UpgareData(id);
            dic[type].Add(result);
        }
        return result;
    }
}
[MessagePackObject(keyAsPropertyName: true)]
[System.Serializable]
public class UpgareData
{
    public string id;
    public bool upgraded;
    public int numberAds;
    public UpgareData(string id)
    {
        this.id = id;
        this.upgraded = false;
        this.numberAds = 0;
    }
    public void AddAds()
    {
        this.numberAds++;
    }
}

[MessagePackObject(keyAsPropertyName: true)]
[System.Serializable]
public class DataItemSlot
{
    public ItemConfig config;
    public long income;
    public DataItemSlot(ItemConfig config)
    {
        this.config = config;
        income = 0;
    }

    public void ChangeIncome(long income)
    {
        this.income = income;
    }
}
