using Raccoon.Data;
using UnityEngine;

[CreateAssetMenu(fileName = "EggData_", menuName = "Raccoon/Egg/EggDataSO")]
public class EggDataSO : ItemDataSO
{
    public ItemDataSO monsterSO;
    public GrowTime growTime;
}

[System.Serializable]
public struct GrowTime
{
    public int hours;
    public int minutes;
    public int seconds;

    public float ToSeconds()
    {
        return hours * 3600 + minutes * 60 + seconds;
    }
}
