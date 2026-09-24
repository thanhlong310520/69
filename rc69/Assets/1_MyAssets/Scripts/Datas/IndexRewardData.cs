using Raccoon.Data;
using UnityEngine;

[CreateAssetMenu(fileName = "IndexReward", menuName = "Raccoon/Data/IndexReward")]
public class IndexRewardData : ScriptableObject
{
    public ZoneType zone;
    public long coin;
    public float speed;
}
