using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
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
        Consumable
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
        OneHandSword,
        TwoHandSword
    }

    public enum AccessoryType
    {
        Amulet
    }

    public enum ImportantItemType
    {
        Key,
        Book
    }
}
