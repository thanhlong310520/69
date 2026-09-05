using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.Item;
using UnityEngine;
using UnityEngine.AI;

public class TestNavmesh : MonoBehaviour
{
    public ItemDataSO itemData;
    public Transform holder;
    public void spawn()
    {
        //GameObject item = GamePlayControl.instance.SpawnItem(itemData.prefab);
        //item.transform.parent = transform;
        //item.SetActive(true);
        //item.transform.localPosition = Vector3.zero;
        //item.transform.localScale = Vector3.one;
        //item.transform.localRotation = Quaternion.Euler(0, 180, 0);
        //var itemCtr = item.GetComponent<ItemCtr>();

        //string id = item.GetInstanceID().ToString();
        //itemCtr.Init(itemData, new ItemConfig(itemData.GetId, itemData.priority));
        //itemCtr.Spawned(RemoveItem);
    }
    void RemoveItem(ItemCtr ctr)
    {

    }
}
