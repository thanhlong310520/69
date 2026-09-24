using UnityEngine;
namespace Raccoon.Data
{
    public abstract class ItemID : ScriptableObject
    {
        public string id;

        public string GetId
        {
            get
            {
                if (string.IsNullOrEmpty(id)) return name;
                return id;
            }
        }
    }

    public class ItemDataSO : ItemID
    {
        public string itemName;
        public int priority;
        public ItemType itemType;
        public Sprite spriteIcon;
        public GameObject prefab;
        public RarityType rarityType;
        public ZoneType colorType;
        public Sprite spriteIconLock;
        public long cost;
    }

    public enum ItemType
    {
        None = 0,
        Weapon,
        Brainrot,
        Fashion,
        OtherWeapon,
        OtherItem,
        Egg,
    }

    public enum RarityType
    {
        Common = 0,
        Rare,
        Epic,
        Legendary,
        Mythic,
        Godly,
        Secret,
        Uncommon,
        Cosmic,
        Eternal,

        Limited = 20,
    }

    public enum ZoneType
    {
        Forest,
        Lake,
        Desert,
        Jungle,
        Snow,
        Volcano,
        AbyssOcean,
        Prehistoric,
        //Cosmic,
        //CherryBlossom,
        //TitanTemple
    }
}