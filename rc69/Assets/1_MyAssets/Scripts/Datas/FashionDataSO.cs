using UnityEngine;

namespace Raccoon.Data
{
    [CreateAssetMenu(fileName = "FashionData_", menuName = "Raccoon/Item/FashionDataSO")]
    public class FashionDataSO : ItemDataSO
    {
        public Sprite avatar;
        public bool male;
        public int numberAds;
    }
}
