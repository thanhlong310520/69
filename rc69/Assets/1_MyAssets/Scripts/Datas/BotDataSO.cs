using UnityEngine;

namespace Raccoon.Data
{
    [CreateAssetMenu(fileName = "BotData_", menuName = "Raccoon/Item/BotDataSO")]
    public class BotDataSO : ItemDataSO
    {
        public long income;
        public AudioClip audioClip;
        // public Material matColorType;
        public float health;
    }

}