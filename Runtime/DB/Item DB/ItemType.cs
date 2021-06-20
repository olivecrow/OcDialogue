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
        Leg,
        Accessory
    }

    public enum WeaponType
    {
        OneHandSword,
        TwoHandSword
    }

    public enum ImportantItemType
    {
        Key,
        Book
    }
}
