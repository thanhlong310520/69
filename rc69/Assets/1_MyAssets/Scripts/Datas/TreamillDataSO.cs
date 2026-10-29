using UnityEngine;

namespace Raccoon.Data
{
    [CreateAssetMenu(fileName = "treamill_", menuName = "Raccoon/Treamill/TreamillDataSO")]
    public class TreamillDataSO : ScriptableObject
    {
        public long price;
        public float speedPerSecond;
        public GameObject prefabModel;
    }
}
