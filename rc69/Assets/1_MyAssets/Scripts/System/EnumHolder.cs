using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raccoon.EnumHolder
{
    public enum EventID
    {
        None,
    }
    public enum TypeShop
    {
        None, UpgradeShop, SellShop
    }

    public enum ItemState
    {
        Normal, Loot, Save,
    }

    public enum EnvironmentType
    {
        Normal, Lava, Night, Water, Radioactive, Heaven 
    }
    public enum ColorEffect
    {
        None, Red, Green, Blue, Yellow, Purple 
    }

    public enum TypeVFX
    {
        Blood, BackBase, Steal, SlotEmpty, Coin
    }
}