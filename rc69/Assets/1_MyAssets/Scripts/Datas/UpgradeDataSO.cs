using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeDataSO", menuName = "Raccoon/Item/UpgradeDataSO")]
public class UpgradeDataSO : ScriptableObject
{
    public TypeUpgrade type;
    public Sprite icon;
    public Sprite bgIcon;
    public List<DataUpgrade> data;

    public bool isDynamicData = false;
    public int basePrice = 283;
    public float r = 1.2f;
    public int maxData;
    public DataUpgrade GetdataByIndex(int index)
    {
        if (index < 0 || index >= data.Count) return new DataUpgrade();
        return data[index]; 
    }

    public long GetPriceUpgrade(int index)
    {
        if(!isDynamicData)
        {
            if (index < 0 || index >= data.Count) return 0;
            return data[index].price;
        }
        long price = 0;
        price = (long)Mathf.Pow(r, index) * basePrice;
        return price;
    }

    public float GetPrecent(int index)
    {
        if (!isDynamicData)
        {
            if (index < 0 || index >= data.Count) return 0;
            return data[index].precent;
        }
        return index + 1;
    }

    public bool HaveData(int index)
    {
        if (!isDynamicData)
        {
            if(index >= data.Count) return false;
            return true;
        }
        else
        {
            if(index > maxData) return false;
            return true;
        }
    }
}

public enum TypeUpgrade
{
    Speed, JumpForce, SlotLoot
}

[System.Serializable] public struct DataUpgrade
{
    public long price;
    public float precent;
}
