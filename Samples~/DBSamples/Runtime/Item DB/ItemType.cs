using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyDB
{
    public enum ItemType
    {
        Generic,
        Armor,
        Weapon,
        Accessory,
        Important
    }

    public enum GenericType
    {
        Material,
        Consumable,
        Ammo
    }

    public enum ArmorType
    {
        Head,
        Torso,
        Arm,
        Leg
    }

    public enum WeaponType
    {
        OneHandSword = 1,
        Axe = 2,
        Hammer = 3,
        Staff = 4,
        Dagger = 5,
        Spear = 6,
        Rapier = 7,
        
        TwoHandSword = 100,
        TwoHandAxe = 101,
        TwoHandHammer = 102,
        Scythe = 103,
        Hallberd = 104,
        Bow = 105
    }

    public enum AccessoryType
    {
        Amulet,
        Other
    }

    public enum ImportantItemType
    {
        Key,
        Book,
        Other
    }
}
