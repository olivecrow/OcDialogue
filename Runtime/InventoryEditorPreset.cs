using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace OcDialogue
{
    [CreateAssetMenu(fileName = "Inventory Editor Preset", menuName = "Oc Dialogue/Inventory Editor Preset")]
    public class InventoryEditorPreset : ScriptableObject
    {
#if UNITY_EDITOR
        /// <summary> Editor Only. </summary>
        public const string AssetPath = "Inventory Editor Preset";
        /// <summary> Editor Only. </summary>
        public static InventoryEditorPreset Instance => _instance;
        static InventoryEditorPreset _instance;

        [InitializeOnLoadMethod]
        static void Init()
        {
            _instance = Resources.Load<InventoryEditorPreset>(AssetPath);
        }

        public bool usePreset;
        [TableList]public List<ItemPreset> ItemPresets;
        [Serializable]
        public class ItemPreset
        {
            [TableColumnWidth(48, false), PreviewField] public UnityEngine.Object icon;
            [VerticalGroup("Type"), TableColumnWidth(200, false)]public ItemType type;
            [HideInInspector] public string subtypeString;
            [VerticalGroup("Type"), OnValueChanged("ApplySubTypeString"), HideLabel][ShowIf("type", ItemType.Generic)]public GenericType genericType;
            [VerticalGroup("Type"), OnValueChanged("ApplySubTypeString"), HideLabel][ShowIf("type", ItemType.Armor)]public ArmorType armorType;
            [VerticalGroup("Type"), OnValueChanged("ApplySubTypeString"), HideLabel][ShowIf("type", ItemType.Weapon)]public WeaponType weaponType;
            [VerticalGroup("Type"), OnValueChanged("ApplySubTypeString"), HideLabel][ShowIf("type", ItemType.Important)]public ImportantItemType importantItemType;
            
            [VerticalGroup("Item")][ValueDropdown("GetItemList"), OnValueChanged("ApplyIcon"), HideLabel]public ItemBase item;
            [VerticalGroup("Item")][ShowIf("@item != null && item.isStackable")] public int count;

            void ApplySubTypeString()
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
        }
#endif
    }
}
