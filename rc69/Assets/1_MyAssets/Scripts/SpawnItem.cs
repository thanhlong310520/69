using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.EnumHolder;
using Raccoon.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class SpawnItem : MonoBehaviour
{
    [SerializeField]List<DataSpawnEgg> datas;

    public List<ItemCtr> listItemSpawn;
    
    #region init

    private void Start()
    {
        Init();
    }
    public void Init()
    {
        foreach (var dt in datas)
        {
            Spawn(dt.so, dt.holderEgg);
        }
    }

    #endregion
    #region Spawn 
    void Spawn(ItemDataSO itemData, Transform holder)
    {
        GameObject item = GamePlayControl.instance.SpawnItem(itemData.prefab);
        item.transform.parent = transform;
        item.SetActive(true);
        item.transform.localPosition = holder.position;
        item.transform.localScale = Vector3.one;
        item.transform.localRotation = holder.rotation;
        item.gameObject.SetActive(true);
        var itemCtr = item.GetComponent<ItemCtr>();

        itemCtr.Init(itemData, new ItemConfig(itemData.GetId, itemData.priority), Raccoon.EnumHolder.ItemState.Normal);
        itemCtr.Spawned(RemoveItem,Stealed,StealFail);
        itemCtr.ChangeEnvironment(GamePlayControl.instance.currentEnvironmentType);

        listItemSpawn.Add(itemCtr);
    }
    
    
    #endregion

    #region Remove 

    void RemoveItem(ItemCtr itemRemove)
    {
        listItemSpawn.Remove(itemRemove);
        ReSpawn(itemRemove.data);
    }
    void Stealed(ItemCtr itemSteal)
    {
        listItemSpawn.Remove(itemSteal);
    }
    void StealFail(ItemCtr itemSteal)
    {
        listItemSpawn.Add(itemSteal);
    }

    #endregion

    #region Respawn

    void ReSpawn(ItemDataSO itemData)
    {
        var dtspawn = GetDataSpawnEgg(itemData);
        Spawn(dtspawn.so,dtspawn.holderEgg);
    }

    DataSpawnEgg GetDataSpawnEgg(ItemDataSO so)
    {
        DataSpawnEgg result = datas.FirstOrDefault(d=> d.so == so);
        return result;
    } 
    #endregion


    #region ChangeEnvironmentForItem
    public void ChangeEnvironment(EnvironmentType type)
    {
        foreach (var item in listItemSpawn)
        {
            if(item != null)
            {
                item.ChangeEnvironment(type);   
            }
        }
    }
    #endregion
}


[Serializable]
struct DataSpawnEgg
{
    public Transform holderEgg;
    public ItemDataSO so;
}
