using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcUtility;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue.Editor
{
    [Serializable]
    public class InventorySerializer
    {
        public Inventory TargetInventory => _inventory;
        [ShowInInspector]public List<ItemBase> AllItems => _inventory.Items.ToList();
        [ShowInInspector][InlineEditor()]public List<ItemBase> Items;
        Inventory _inventory;
        public InventorySerializer(Inventory inventory)
        {
            if(inventory == null) return;
            _inventory = inventory;
        }

        public void DrawItems(ItemType type)
        {
            if(_inventory == null) return;
            Items = _inventory.Items.Where(x => x.type == type).ToList();
        }

        [BoxGroup("buttons")]
        [HorizontalGroup("buttons/1")][Button]
        int Count(ItemBase itemBase, string itemName)
        {
            if (itemBase == null && string.IsNullOrWhiteSpace(itemName))
            {
                Printer.Print("ItemBase 혹은 itemName중 하나라도 유효한 값을 입력해야함.", LogType.Warning);
                return 0;
            }
            if(itemBase != null) return _inventory.Count(itemBase);
            if (!string.IsNullOrWhiteSpace(itemName)) return _inventory.Count(FindItemFromName(itemName));
            return 0;
        }

        [HorizontalGroup("buttons/2")][Button]
        void AddItem(ItemBase itemBase, string itemName, int count)
        {
            if (itemBase == null && string.IsNullOrWhiteSpace(itemName))
            {
                Printer.Print("ItemBase 혹은 itemName중 하나라도 유효한 값을 입력해야함.", LogType.Warning);
                return;
            }
            if (itemBase != null) _inventory.AddItem(itemBase, count);
            else if (!string.IsNullOrWhiteSpace(itemName)) _inventory.AddItem(FindItemFromName(itemName), count);
        }

        [HorizontalGroup("buttons/2")][Button]
        void RemoveItem(ItemBase itemBase, string itemName, int count)
        {
            if (itemBase == null && string.IsNullOrWhiteSpace(itemName))
            {
                Printer.Print("ItemBase 혹은 itemName중 하나라도 유효한 값을 입력해야함.", LogType.Warning);
                return;
            }
            if (itemBase != null) _inventory.RemoveItem(itemBase, count);
            else if (!string.IsNullOrWhiteSpace(itemName)) _inventory.RemoveItem(FindItemFromName(itemName), count);
        }

        ItemBase FindItemFromName(string itemName) => ItemDatabase.Instance.Items.Find(x => x.itemName == itemName);
        
    }
}
