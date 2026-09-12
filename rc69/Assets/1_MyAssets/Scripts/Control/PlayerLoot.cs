using Raccoon;
using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.Item;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerLoot : MonoBehaviour
{

    public List<Transform> posToLoots;

    public List<ItemCtr> itemLoots;

    UnityAction eventStartLoot;
    UnityAction eventLootFail;
    UnityAction<float> eventWhileLoot;
    UnityAction<ItemCtr> eventLootSuccess;

    ItemCtr itemLoot;

    bool isLooting;
    public bool IsLooting => isLooting;
    public float timeLoot = 1f;
    float currentTimeLoot;

    int numberLoot;

    public void Init(UnityAction eventStartLoot, UnityAction eventLootFail, UnityAction<float> evenWhileLoot, UnityAction<ItemCtr> eventLootSuccess)
    {
        this.eventLootFail = eventLootFail;
        this.eventWhileLoot = evenWhileLoot;
        this.eventLootSuccess = eventLootSuccess;
        this.eventStartLoot = eventStartLoot;
        isLooting = false;
        currentTimeLoot = 0f;
        itemLoots = new List<ItemCtr>();
    }
    public void SetNumberLoot(float set)
    {
        numberLoot = 1 + (int)set;
    }
    public void Loot(ItemCtr itemCtr)
    {

        eventStartLoot?.Invoke();
        itemLoot = itemCtr;
        currentTimeLoot = 0;
        isLooting = true;


        Done();
    }

    private void Update()
    {
        //if (!isLooting) return;
        //currentTimeLoot += Time.deltaTime;
        //LootProgess();
    }

    void LootProgess()
    {
        float percent = Mathf.Clamp01(currentTimeLoot / timeLoot);
        eventWhileLoot?.Invoke(percent);
        if (itemLoot == null)
        {
            LootFail();
            return;
        }
        if(currentTimeLoot > timeLoot)
        {
            Done();
        }
    }

    void Done()
    {
        itemLoots.Add(itemLoot);
        eventLootSuccess?.Invoke(itemLoot);
        ResetLoot();
    }

    public void DropItemLoot(Transform control)
    {
        //drop
        foreach (var item in itemLoots)
        {
            if (item != null)
            {
                item.StealFail(control);
                item.transform.position = transform.position;
            }
        }
        itemLoots.Clear();
    }
    void LootFail()
    {
        eventLootFail?.Invoke();
        ResetLoot();
    }
    void ResetLoot()
    {
        currentTimeLoot = 0;
        itemLoot = null;
        isLooting = false;
    }
    public void CancelLoot()
    {
        if (!isLooting) return;
        eventLootFail?.Invoke();
        ResetLoot();
    }
    public List<ItemCtr> GetItemLoots => itemLoots;
    public List<ItemConfig> SaveLoot(Transform control)
    {
        List<ItemConfig> listItemConfig = new List<ItemConfig>();
        foreach (var item in itemLoots)
        {
            listItemConfig.Add(item.GetItemConfig);
            item.Save(control);
            GamePlayControl.instance.RespawnItem(item.gameObject);
        }

        itemLoots.Clear();
        return listItemConfig;
    }

    public bool IsHaveItemLoot() => itemLoots.Count > 0;

    public bool IsMaxLoot => itemLoots.Count >= numberLoot;

}
