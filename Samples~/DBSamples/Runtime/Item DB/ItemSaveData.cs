using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyDB
{
    public class ItemSaveData
    {
        public int GUID;
        public string itemName;
        public int count;
        public int upgrade;
        public float durability;
        public bool isEquipped;
        public ItemSaveData(){}
        public ItemSaveData(ItemBase item)
        {
            GUID = item.GUID;
            itemName = item.itemName;
            count = item.isStackable ? item.CurrentStack : 0;

            if (item is IEquipment equipment)
            {
                upgrade = equipment.CurrentUpgrade;
                durability = equipment.CurrentDurability;
                isEquipped = equipment.IsEquipped;
            }
        }

        public override string ToString()
        {
            return $"GUID : {GUID} \n" +
                   $"itemName : {itemName} \n" +
                   $"count : {count} \n" +
                   $"upgrade : {upgrade} \n" +
                   $"durability : {durability} \n" +
                   $"isEquipped : {isEquipped}";
        }
    }
}
