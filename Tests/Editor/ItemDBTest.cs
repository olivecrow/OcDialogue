using System.Collections;
using System.Linq;
using NUnit.Framework;
using OcDialogue;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;


public class ItemDBTest
{
    [Test]
    public void OverlapTest()
    {
        foreach (var item in ItemDatabase.Instance.Items)
        {
            var count = ItemDatabase.Instance.Items.Count(x => x.itemName == item.itemName);
            if (count > 1) Debug.LogError($"[{item.Address}] 중복된 이름이 존재함");
        }
    }

    [Test]
    public void GenericItemTests()
    {
        foreach (var item in ItemDatabase.Instance.Items.Where(x => x.type == ItemType.Generic))
        {
            CommonTest(item);
        }
    }

    [Test]
    public void ArmorItemTests()
    {
        foreach (var item in ItemDatabase.Instance.Items.Where(x => x.type == ItemType.Armor))
        {
            CommonTest(item);
            EquipmentCommonTest(item as ArmorItem);
            IsArmorStatValid(item as ArmorItem);
            IsArmorStatOverlap(item as ArmorItem);
        }
    }

    [Test]
    public void WeaponItemTests()
    {
        foreach (var item in ItemDatabase.Instance.Items.Where(x => x.type == ItemType.Weapon))
        {
            CommonTest(item);
            EquipmentCommonTest(item as WeaponItem);
            IsWeaponStatValid(item as WeaponItem);
        }
    }

    [Test]
    public void AccessoryItemTest()
    {
        foreach (var item in ItemDatabase.Instance.Items.Where(x => x.type == ItemType.Accessory))
        {
            CommonTest(item);
            EquipmentCommonTest(item as AccessoryItem);
        }
    }

    [Test]
    public void ImportantItemTest()
    {
        foreach (var item in ItemDatabase.Instance.Items.Where(x => x.type == ItemType.Important))
        {
            CommonTest(item);
        }
    }

    static void CommonTest(ItemBase item)
    {
        IsStackAmountValidate(item);
        IsDescriptionValidate(item);
        IsIconValidate(item);
    }

    static void EquipmentCommonTest(IEquipment equipment)
    {
        IsMaxDurabilityValidate(equipment);
        IsWeightValidate(equipment);
        IsAvatarValidate(equipment);
        IsEquipmentStackableValid(equipment);
    }

    static bool IsStackAmountValidate(ItemBase item)
    {
        if (!item.isStackable || item.maxStackCount >= 1) return true;
        Debug.LogError($"[{item.Address}] MaxStackCount가 0 이하임");
        return false;
    }

    static bool IsDescriptionValidate(ItemBase item)
    {
        if (item.referOtherDescription && item.descriptionReference != null)
        {
            Debug.LogError($"[{item.Address}] Description Reference 가 없음");
            return false;
        }

        if (!item.referOtherDescription && string.IsNullOrWhiteSpace(item.description))
        {
            Debug.LogError($"[{item.Address}] Description 이 비어있음");
            return false;
        }

        return false;
    }

    static bool IsIconValidate(ItemBase item)
    {
        if (item.IconReference.RuntimeKeyIsValid()) return true;
        Debug.LogError($"[{item.Address}] Icon 이 없음.");
        return false;
    }

    static bool IsMaxDurabilityValidate(IEquipment equipment)
    {
        if (equipment.MaxDurability >= 1) return true;
        var item = equipment.ItemBase;
        Debug.LogError($"[{item.Address}] MaxDurability가 0 이하임.");
        return false;
    }

    static bool IsWeightValidate(IEquipment equipment)
    {
        if (equipment.Weight > 0) return true;
        var item = equipment.ItemBase;
        Debug.LogError($"[{item.Address}] Weight가 0 이하임.");
        return false;
    }

    static bool IsAvatarValidate(IEquipment equipment)
    {
        if (equipment.ItemBase.type == ItemType.Accessory) return true;
        if (equipment.Avatar.RuntimeKeyIsValid()) return true;
        var item = equipment.ItemBase;
        Debug.LogError($"[{item.Address}] Avatar가 비어있음.");
        return false;
    }

    static bool IsEquipmentStackableValid(IEquipment equipment)
    {
        if (!equipment.ItemBase.isStackable) return true;
        var item = equipment.ItemBase;
        Debug.LogError($"[{item.Address}] 장비 아이템은 isStackable일 수 없음.");
        return false;
    }

    static bool IsArmorStatValid(ArmorItem armor)
    {
        var isPhysicalValid =
            armor.DefenseStat.Strike > 0 && armor.DefenseStat.Slice > 0 && armor.DefenseStat.Thrust > 0;
        if (isPhysicalValid) return true;
        var item = armor.ItemBase;
        Debug.LogWarning($"[{item.Address}] 물리 방어력 중 0인 요소가 있음");
        return false;
    }

    static bool IsWeaponStatValid(WeaponItem weapon)
    {
        var isPhysicalValid =
            weapon.AttackStat.Strike > 0 && weapon.AttackStat.Slice > 0 && weapon.AttackStat.Thrust > 0;
        if (isPhysicalValid) return true;
        var item = weapon.ItemBase;
        Debug.LogWarning($"[{item.Address}] 물리 공격력이 중 0인 요소가 있음");
        return false;
    }

    static bool IsArmorStatOverlap(ArmorItem armor)
    {
        foreach (var item in ItemDatabase.Instance.Items.Where(x => x.type == ItemType.Armor))
        {
            var target = item as ArmorItem;
            if (target != armor && armor.DefenseStat == target.DefenseStat)
                Debug.LogWarning($"[{item.Address}] 같은 스텟의 장비가 있음.");
        }

        return true;
    }
}