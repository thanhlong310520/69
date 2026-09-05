using Raccoon.Controller;
using Raccoon.Item;
using System.Collections.Generic;
using UnityEngine;

public class HolderItemDropped : MonoBehaviour
{
    #region singleton
    public static HolderItemDropped instance;

    public List<ItemCtr> listItem;

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

    private void Start()
    {
        listItem = new List<ItemCtr>();
    }
    public void AddItem(ItemCtr item)
    {
        listItem.Add(item);
    }

    public void RemoveItem(ItemCtr item)
    {
        listItem.Remove(item);
    }

    public List<ItemCtr> CheckItemNearTransform(Transform checker, float radius = 3)
    {
        List<ItemCtr> result = new List<ItemCtr>();
        if (checker == null) return result;
        foreach (var item in listItem)
        {
            if(item != null)
            {
                if (Vector3.Distance(item.transform.position, checker.position) <= radius) result.Add(item);
            }
        }
        return result;
    }

}
