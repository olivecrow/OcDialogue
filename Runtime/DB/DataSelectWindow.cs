using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class DataSelectWindow : OdinEditorWindow
    {
        public DataChecker.Comparer Target { get; set; }

        public static void Open()
        {
            GetWindow<DataSelectWindow>().ShowModal();
        }
        
        public DBType DBType;
        [ValueDropdown("GetFirstList"), HideLabel, ShowIf("UseFirstDropdown"), HorizontalGroup("Selector")]
        public string firstDropdown;
        [ValueDropdown("GetSecondList"), HideLabel, ShowIf("UseSecondDropdown"), HorizontalGroup("Selector")]
        public string secondDropDown;
        [ValueDropdown("GetDataList"), HideLabel] public ScriptableObject Data;
        bool UseFirstDropdown() => DBType != DBType.GameProcess && DBType != DBType.NPC;
        bool UseSecondDropdown() => UseFirstDropdown() && (DBType == DBType.Item || DBType == DBType.Quest);
        ValueDropdownList<string> GetFirstList()
        {
            var list = new ValueDropdownList<string>();
            switch (DBType)
            {
                case DBType.Item:
                    var itemTypes = Enum.GetNames(typeof(ItemType));
                    foreach (var type in itemTypes) list.Add(type);
                    break;
                case DBType.Quest:
                    break;
                case DBType.Enemy:
                    break;
                default:
                    Debug.LogWarning("해당 타입의 ValueDropDown이 미구현됨.");
                    break;
            }

            return list;
        }
        ValueDropdownList<string> GetSecondList()
        {
            var list = new ValueDropdownList<string>();
            switch (DBType)
            {
                case DBType.Item:
                    var itemType = (ItemType) Enum.Parse(typeof(ItemType), firstDropdown);
                    switch (itemType)
                    {
                        case ItemType.Generic:
                        {
                            var subtypes = Enum.GetNames(typeof(GenericType));
                            foreach (var type in subtypes) list.Add(type);
                            break;
                        }
                        case ItemType.Armor:
                        {
                            var subtypes = Enum.GetNames(typeof(ArmorType));
                            foreach (var type in subtypes) list.Add(type);
                            break;
                        }
                        case ItemType.Weapon:
                        {
                            var subtypes = Enum.GetNames(typeof(WeaponType));
                            foreach (var type in subtypes) list.Add(type);
                            break;
                        }
                        case ItemType.Accessory:
                        {
                            var subtypes = Enum.GetNames(typeof(AccessoryType));
                            foreach (var type in subtypes) list.Add(type);
                            break;
                        }
                        case ItemType.Important:
                        {
                            var subtypes = Enum.GetNames(typeof(ImportantItemType));
                            foreach (var type in subtypes) list.Add(type);
                            break;
                        }
                        default:
                            Debug.LogWarning("해당 아이템 타입의 ValueDropDown이 미구현됨.");
                            break;
                    }
                    break;
                case DBType.Quest:
                    // 아직 없음.
                    break;
                case DBType.Enemy:
                    // 아직 없음.
                    break;
                default:
                    Debug.LogWarning("해당 타입의 ValueDropDown이 미구현됨.");
                    break;
            }

            return list;
        }
        ValueDropdownList<ScriptableObject> GetDataList()
        {
            var list = new ValueDropdownList<ScriptableObject>();
            switch (DBType)
            {
                case DBType.GameProcess:
                    foreach (var row in GameProcessDatabase.Instance.DataRows) list.Add(row);
                    break;
                case DBType.Item:
                    var targetItems = ItemDatabase.Instance.Items.Where(x =>
                        x.type.ToString() == firstDropdown && x.SubTypeString == secondDropDown);
                    foreach (var item in targetItems) list.Add(item);
                    break;
                case DBType.Quest:
                    // 아직 없음.
                    break;
                case DBType.NPC:
                    foreach (var npc in NPCDatabase.Instance.NPCs) list.Add(npc);
                    break;
                case DBType.Enemy:
                    // 아직 없음.
                    break;
                default:
                    Debug.LogWarning("해당 타입의 ValueDropDown이 미구현됨.");
                    break;
            }

            return list;
        }

        [Button]
        void Apply()
        {
            Target.Data = Data;
            Close();
        }
    }
}
