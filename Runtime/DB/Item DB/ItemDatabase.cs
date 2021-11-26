using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Linq;
using OcDialogue.DB;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Random = UnityEngine.Random;

namespace OcDialogue
{
    [CreateAssetMenu(fileName = "Item Database", menuName = "Oc Dialogue/DB/Item Database")]
    public class ItemDatabase : OcData
    {
        public override string Address => "ItemDatabase";
        public static ItemDatabase Instance => DBManager.Instance.ItemDatabase;
        
        public List<ItemBase> Items;

        public ItemBase FindItem(int guid)
        {
            return Items.Find(x => x.GUID == guid);
        }

        public ItemBase FindItem(string itemName)
        {
            return Items.Find(x => x.itemName == itemName);
        }
        public static Type GetSubType(ItemType type)
        {
            var t = type switch
            {
                ItemType.Generic => typeof(GenericType),
                ItemType.Armor => typeof(ArmorType),
                ItemType.Weapon => typeof(WeaponType),
                ItemType.Important => typeof(ImportantItemType),
                ItemType.Accessory => typeof(AccessoryType),
                _ => throw new ArgumentOutOfRangeException()
            };
            return t;
        }
        
        
        
        
#if UNITY_EDITOR
        [HideInInspector]public InventoryEditorPreset editorPreset;
        
        public ItemBase AddItem(ItemType type, string subType)
        {
            ItemBase asset = type switch
            {
                ItemType.Generic => CreateInstance<GenericItem>(),
                ItemType.Weapon => CreateInstance<WeaponItem>(),
                ItemType.Armor => CreateInstance<ArmorItem>(),
                ItemType.Important => CreateInstance<ImportantItem>(),
                ItemType.Accessory => CreateInstance<AccessoryItem>(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            asset.GUID = OcDataUtility.CalcItemGUID();
            asset.SetSubTypeFromString(subType);

            asset.name = OcDataUtility.CalculateDataName($"New {subType}", Items.Select(x => x.itemName));
            asset.itemName = asset.name;
            
            Items.Add(asset);
            AssetDatabase.AddObjectToAsset(asset, AssetDatabase.GetAssetPath(this));
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return asset;
        }
        
        public void DeleteItem(ItemBase item)
        {
            if(!Items.Contains(item)) return;
            Items.Remove(item);
            AssetDatabase.RemoveObjectFromAsset(item);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif

    }
}
