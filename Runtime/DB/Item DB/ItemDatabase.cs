using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcDialogue
{
    [CreateAssetMenu(fileName = "Item Database", menuName = "Oc Dialogue/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
#if UNITY_EDITOR
        /// <summary> Editor Only. </summary>
        public const string AssetPath = "Item DB/Item Database";
        /// <summary> Editor Only. </summary>
        public static ItemDatabase Instance => _instance;
        static ItemDatabase _instance;

        [EnumToggleButtons] public ItemType itemType;
        [ValueDropdown("GetSubTypeDropdownList")]public string itemSubType;

        [InitializeOnLoadMethod]
        static void Init()
        {
            _instance = Resources.Load<ItemDatabase>(AssetPath);
        }

        public ItemBase AddItem(ItemType type, string subType)
        {
            ItemBase asset = type switch
            {
                ItemType.Generic => CreateInstance<GenericItem>(),
                ItemType.Weapon => CreateInstance<WeaponItem>(),
                ItemType.Armor => CreateInstance<ArmorItem>(),
                ItemType.Important => CreateInstance<ImportantItem>(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            
            asset.type = type;
            asset.SetSubTypeFromString(subType);

            asset.name = OcDataUtility.CalculateDataName($"New {itemSubType}", Items.Select(x => x.itemName));
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
        
        
        ValueDropdownList<string> GetSubTypeDropdownList()
        {
            var list = new ValueDropdownList<string>();
            switch (itemType)
            {
                case ItemType.Generic:
                    Enum.GetNames(typeof(GenericType)).ForEach(x => list.Add(x));
                    break;
                case ItemType.Armor:
                    Enum.GetNames(typeof(ArmorType)).ForEach(x => list.Add(x));
                    break;
                case ItemType.Weapon:
                    Enum.GetNames(typeof(WeaponType)).ForEach(x => list.Add(x));
                    break;
                case ItemType.Important:
                    Enum.GetNames(typeof(ImportantItemType)).ForEach(x => list.Add(x));
                    break;
            }

            return list;
        }
#endif
        public List<ItemBase> Items;

        public static Type GetSubType(ItemType type)
        {
            var t = type switch
            {
                ItemType.Generic => typeof(GenericType),
                ItemType.Armor => typeof(ArmorType),
                ItemType.Weapon => typeof(WeaponType),
                ItemType.Important => typeof(ImportantItemType),
                _ => throw new ArgumentOutOfRangeException()
            };
            return t;
        }
        
    }
}
