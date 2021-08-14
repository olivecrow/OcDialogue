#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;

namespace OcDialogue
{
    [Serializable]
    public class InventoryEditorPreset
    {
        public bool usePreset;
        [TableList]public List<ItemPreset> ItemPresets;
        
        [PreviewField(200f, ObjectFieldAlignment.Left), OnValueChanged("OnItemDropped"), LabelWidth(80)] public UnityEngine.Object dropDesk;
        [OnCollectionChanged("OnArrayDropped"), LabelWidth(80)] public List<UnityEngine.Object> arrayDropDesk;
        
        void OnItemDropped(UnityEngine.Object target)
        {
            var item = target as ItemBase;
            if (item == null) return;
            var preset = new ItemPreset()
            {
                item = item,
                icon = item.editorIcon,
                type = item.type,
                
            };
            switch (item.type)
            {
                case ItemType.Generic:
                    preset.genericType = (GenericType) Enum.Parse(typeof(GenericType), item.SubTypeString);
                    break;
                case ItemType.Armor:
                    preset.armorType = (ArmorType) Enum.Parse(typeof(ArmorType), item.SubTypeString);
                    break;
                case ItemType.Weapon:
                    preset.weaponType = (WeaponType) Enum.Parse(typeof(WeaponType), item.SubTypeString);
                    break;
                case ItemType.Accessory:
                    preset.accessoryType = (AccessoryType) Enum.Parse(typeof(AccessoryType), item.SubTypeString);
                    break;
                case ItemType.Important:
                    preset.importantItemType = (ImportantItemType) Enum.Parse(typeof(ImportantItemType), item.SubTypeString);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            ItemPresets.Add(preset);
            dropDesk = null;
        }

        void OnArrayDropped()
        {
            OnItemDropped(arrayDropDesk[0]);
            arrayDropDesk.Clear();
        }


        [Serializable]
        public class ItemPreset : ComparableData
        {
            [TableColumnWidth(48, false), PreviewField, OnValueChanged("OnItemDropped")] public UnityEngine.Object icon;
            [VerticalGroup("Type"), TableColumnWidth(200, false), OnValueChanged("OnTypeChanged")]public ItemType type;
            [HideInInspector] public string subtypeString;
            [VerticalGroup("Type"), OnValueChanged("OnTypeChanged"), HideLabel][ShowIf("type", ItemType.Generic)]public GenericType genericType;
            [VerticalGroup("Type"), OnValueChanged("OnTypeChanged"), HideLabel][ShowIf("type", ItemType.Armor)]public ArmorType armorType;
            [VerticalGroup("Type"), OnValueChanged("OnTypeChanged"), HideLabel][ShowIf("type", ItemType.Weapon)]public WeaponType weaponType;
            [VerticalGroup("Type"), OnValueChanged("OnTypeChanged"), HideLabel][ShowIf("type", ItemType.Important)]public ImportantItemType importantItemType;
            [VerticalGroup("Type"), OnValueChanged("OnTypeChanged"), HideLabel][ShowIf("type", ItemType.Accessory)]public AccessoryType accessoryType;
            
            [VerticalGroup("Item")][ValueDropdown("GetItemList"), OnValueChanged("ApplyIcon"), HideLabel]public ItemBase item;
            [VerticalGroup("Item")][ShowIf("@item != null && item.isStackable")] public int count;
            [VerticalGroup("Item")][ShowIf("@item != null && (type == ItemType.Weapon || type == ItemType.Armor || type == ItemType.Accessory)")]
            public bool equip;

            [HideInInspector] public UnityEngine.Object _cachedIcon;
            void OnTypeChanged()
            {
                switch (type)
                {
                    case ItemType.Generic:
                        subtypeString = genericType.ToString();
                        break;
                    case ItemType.Armor:
                        subtypeString = armorType.ToString();
                        break;
                    case ItemType.Weapon:
                        subtypeString = weaponType.ToString();
                        break;
                    case ItemType.Important:
                        subtypeString = importantItemType.ToString();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            void ApplyIcon()
            {
                icon = item.editorIcon;
                _cachedIcon = icon;
            }

            void OnItemDropped()
            {
                if (_cachedIcon != icon)
                {
                    item = icon as ItemBase;
                }
                
                
                icon = item.editorIcon;

                type = item.type;
                switch (type)
                {
                    case ItemType.Generic:
                        genericType = (GenericType) Enum.Parse(typeof(GenericType), item.SubTypeString);
                        break;
                    case ItemType.Armor:
                        armorType = (ArmorType) Enum.Parse(typeof(ArmorType), item.SubTypeString);
                        break;
                    case ItemType.Weapon:
                        weaponType = (WeaponType) Enum.Parse(typeof(WeaponType), item.SubTypeString);
                        break;
                    case ItemType.Accessory:
                        accessoryType = (AccessoryType) Enum.Parse(typeof(AccessoryType), item.SubTypeString);
                        break;
                    case ItemType.Important:
                        importantItemType = (ImportantItemType) Enum.Parse(typeof(ImportantItemType), item.SubTypeString);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            ValueDropdownList<ItemBase> GetItemList()
            {
                var list = ItemDatabase.Instance.Items.Where(x => x.type == type && x.SubTypeString == subtypeString);
                var vdList = new ValueDropdownList<ItemBase>();
                foreach (var itemBase in list)
                {
                    vdList.Add(itemBase);
                }

                return vdList;
            }

            public ItemBase GetCopy()
            {
                var copy = item.GetCopy();
                if (copy.isStackable) copy.AddStack(count);

                return copy;
            }

            public override string Key => item.itemName;
            public override bool IsTrue(CompareFactor factor, Operator op, object value)
            {
                return factor switch
                {
                    CompareFactor.ItemCount => count.IsTrue(op, (int)value),
                    _ => false
                };
            }
        }
    }
}
#endif