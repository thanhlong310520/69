using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeDataSO", menuName = "Raccoon/Item/UpgradeDataSO")]
public class UpgradeDataSO : ScriptableObject
{
    public TypeUpgrade type;
    public List<DataUpgrade> data;
    public int maxData;

    public long GetPriceUpgrade(string id)
    {
        var d = GetData(id);
        return d.price;
    }
    public DataUpgrade GetData(string id)
    {
        return data.FirstOrDefault(x => x.id == id);
    }

    public float GetPrecent(string id)
    {
        var d = GetData(id);
        return d.precent;
    }

    public bool EnoughAds(string id, int ads)
    {
        var d = GetData(id);
        return ads >= d.numberAds;
    }

}

public enum TypeUpgrade
{
    Speed, JumpForce, SlotLoot
}

[System.Serializable] public struct DataUpgrade
{
    public string id;
    public string name;
    public Sprite icon;
    public Sprite bgIcon;
    public int numberAds;
    public long price;
    public float precent;

}
