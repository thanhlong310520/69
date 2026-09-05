using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.EnumHolder;
using Raccoon.Item;
using Raccoon.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class SpawnItem : MonoBehaviour
{
    //public GameObject boxPrefab;
    public int numberItem = 8;
    [Header("Grid")]
    public int gridX = 50;
    public int gridZ = 50;
    NoiseGenerator noise;

    public int minIncome;
    public int maxIncome;

    [SerializeField]List<ItemDataSO> datas;

    [Header("Spawn")]
    float offsetX;
    float offsetZ;
    public float scaleGridX = 3;
    public float scaleGridZ = 5;
    [SerializeField] List<CellSpawnItem> listCell = new List<CellSpawnItem>();
    CellSpawnItem[,] arrayCell;

    [Header("Respawn")]
    Queue<string> queueRemoveBox = new Queue<string>();
    public float tiemeRespawnBox = 2f;
    [SerializeField] List<DataRespawnItem> listRespawnItem = new List<DataRespawnItem>();
    [SerializeField] List<DataRespawnItem> listCanSpawn = new List<DataRespawnItem>();

    public List<ItemCtr> listItemSpawn;
    private void Update()
    {
        HandleQueueRemove();
        CheckRespawn();
    }
    #region init

    private void Start()
    {
        listCanSpawn = new List<DataRespawnItem>();
        Init();
    }
    public void Init()
    {
        offsetX = gridX % 2 == 0 ? 0.5f : 0;
        offsetZ = gridZ % 2 == 0 ? 0.5f : 0;

        //noise = new NoiseGenerator(123, 1.5f, 0.55f);
        //noise.persistence = 0.75f;

        listCell = new List<CellSpawnItem>();
        listRespawnItem = new List<DataRespawnItem>();
        listCanSpawn = new List<DataRespawnItem>();
        arrayCell = new CellSpawnItem[gridX, gridZ];

        datas = new List<ItemDataSO>();
        datas.AddRange(GamePlayControl.instance.GetBotDataSOsIncomeRange(minIncome, maxIncome));

        InitListCell();
        //foreach (var cell in listCell)
        //{
        //    if (noise.IsSolid2D(cell.x, cell.z))
        //    {
        //        var data = GetBoxDataSpawn();
        //        Spawn(data, cell.x, cell.z);
        //    }
        //}

        
    }

    void InitListCell()
    {
        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                CellSpawnItem c = new CellSpawnItem(x, z);
                
                listCell.Add(c);
                arrayCell[x, z] = c;
                //GameObject box = Instantiate(boxPrefab, transform);
                //box.SetActive(true);
                //box.transform.localPosition = new Vector3(x - gridX / 2 + offsetX, 0, z - gridZ / 2 + offsetZ) * scaleGrid;
                //box.transform.localScale = Vector3.one;
            }
        }

        for (int x = 0; x < numberItem; x++)
        {
            HandleSpawn();
        }

    }
    #endregion
    #region Spawn 

    void HandleSpawn()
    {
        ItemDataSO it = GetItemDataSpawn();
        var cell = GetCellHaventItem();
        if (cell != null)
        {
            Spawn(it, cell.x, cell.z);
        }
    }
    ItemDataSO GetItemDataSpawn()
    {
        int index = UnityEngine.Random.Range(0, datas.Count);
        ItemDataSO boxData = datas[index];   
        return boxData;
    }
    void Spawn(ItemDataSO itemData, int x, int z)
    {
        GameObject item = GamePlayControl.instance.SpawnItem(itemData.prefab);
        item.transform.parent = transform;
        item.SetActive(true);
        item.transform.localPosition = new Vector3((x - gridX / 2 + offsetX) * scaleGridX, 0, (z - gridZ / 2 + offsetZ) * scaleGridZ);
        item.transform.localScale = Vector3.one;
        item.transform.localRotation = Quaternion.Euler(0, 180, 0);
        item.gameObject.SetActive(true);
        var itemCtr = item.GetComponent<ItemCtr>();

        itemCtr.Init(itemData, new ItemConfig(itemData.GetId, itemData.priority), Raccoon.EnumHolder.ItemState.Normal);
        itemCtr.Spawned(RemoveItem);
        itemCtr.ChangeEnvironment(GamePlayControl.instance.currentEnvironmentType);

        SetHaveBoxForCell(itemCtr, x, z);

        listItemSpawn.Add(itemCtr);
    }
    CellSpawnItem GetCellHaventItem()
    {
        var l = listCell.Where(c => c.itemInCell == null).ToList();
        if(l.Count <= 0) return null;
        return l[UnityEngine.Random.Range(0, l.Count)];
    }
    void SetHaveBoxForCell(ItemCtr item, int x, int z)
    {
        arrayCell[x, z].SetItemInCell(item.transform);
    }
    #endregion

    #region Remove 

    void RemoveItem(ItemCtr itemRemove)
    {
        listItemSpawn.Remove(itemRemove);
        var cell = listCell.FirstOrDefault(c => c.itemInCell == itemRemove.transform);
        if (cell != null) cell.SetItemInCell(null);
        queueRemoveBox.Enqueue(itemRemove.GetItemConfig.id);
    }

    void HandleQueueRemove()
    {
        while (queueRemoveBox.Count > 0)
        {
            var idItemRemove = queueRemoveBox.Dequeue();
            HandleRemoveBox(idItemRemove);
        }
    }

    void HandleRemoveBox(string id)
    {
        listRespawnItem.Add(new DataRespawnItem(id, tiemeRespawnBox));

        // event khacs
        //EventBrokenBox?.Invoke(dataBrokenBox.boxdata, dataBrokenBox.pos);

    }
    #endregion

    #region Respawn

    void CheckRespawn()
    {
        if (listRespawnItem.Count <= 0) return;
        listCanSpawn.Clear();
        listRespawnItem.ForEach(r =>
        {
            r.UpdateTime(Time.deltaTime);
            if (r.timeRespawn <= 0)
            {
                listCanSpawn.Add(r);
            }
        });
        foreach (var r in listCanSpawn)
        {
            listRespawnItem.Remove(r);
            ReSpawn(r.idData);
        }
    }
    void ReSpawn(string id)
    {
        HandleSpawn();

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
[Serializable] public class CellSpawnItem
{
    public int x;
    public int z;
    public Transform itemInCell;
    public CellSpawnItem(int x, int z)
    {
        this.x = x;
        this.z = z;
    }
    public void SetItemInCell(Transform item)
    {
        this.itemInCell = item; 
    }
}

[Serializable]
public class DataRespawnItem
{
    public string idData;
    public float timeRespawn;
    public DataRespawnItem(string idData, float timeRespawn)
    {
        this.idData = idData;
        this.timeRespawn = timeRespawn;
    }

    public void UpdateTime(float deltaTime)
    {
        timeRespawn -= deltaTime;
    }
}
