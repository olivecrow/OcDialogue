#if UNITY_EDITOR
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
        public CheckFactor Target { get; set; }

        public static DataSelectWindow Open()
        {
            var wnd = GetWindow<DataSelectWindow>();
            wnd.ShowUtility();
            return wnd;
        }
        
        public DBType DBType;
        [ValueDropdown("GetFirstList"), HideLabel, ShowIf("UseFirstDropdown")]
        public string firstDropdown;
        [ValueDropdown("GetSecondList"), HideLabel, ShowIf("UseSecondDropdown")]
        public string secondDropDown;
        [ValueDropdown("GetThirdList"), HideLabel, ShowIf("UseThirdDropdown")]
        public string thirdDropDown;
        [ValueDropdown("GetDataList"), HideLabel, GUIColor(0,1,1)] public ComparableData Data;
        bool UseFirstDropdown() => DBType != DBType.GameProcess && DBType != DBType.NPC;
        bool UseSecondDropdown() => UseFirstDropdown() && (DBType == DBType.Item || DBType == DBType.Quest);
        bool UseThirdDropdown() => UseSecondDropdown() && ((DBType == DBType.Quest && secondDropDown != "QuestState")
                                                           || (DBType == DBType.Item));

        void OnValidate()
        {
            if (DBType == DBType.Quest)
            {
                if (secondDropDown == "QuestState") thirdDropDown = null;
                else if (secondDropDown == "DataRows")
                {
                    if(Data is Quest)
                    {
                        thirdDropDown = Data.Key;
                        Data = null;
                    }
                }
            }
        }

        ValueDropdownList<string> GetFirstList()
        {
            var list = new ValueDropdownList<string>();
            switch (DBType)
            {
                case DBType.GameProcess:
                    break;
                case DBType.Item:
                    var itemTypes = Enum.GetNames(typeof(ItemType));
                    foreach (var type in itemTypes) list.Add(type);
                    break;
                case DBType.Quest:
                    var category = QuestDatabase.Instance.Category;
                    foreach (var c in category) list.Add(c);
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
                case DBType.GameProcess:
                    break;
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
                    list.Add("QuestState");
                    list.Add("DataRows");
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

        ValueDropdownList<string> GetThirdList()
        {
            var list = new ValueDropdownList<string>();
            switch (DBType)
            {
                case DBType.GameProcess:
                    break;
                case DBType.Item:
                    break;
                case DBType.Quest:
                    if (secondDropDown == "DataRows")
                    {
                        foreach (var quest in QuestDatabase.Instance.Quests) list.Add(quest.key);
                    }
                    break;
                case DBType.NPC:
                    break;
                case DBType.Enemy:
                    break;
                default:
                    Debug.LogWarning("해당 타입의 ValueDropDown이 미구현됨.");
                    break;
            }

            return list;
        }

        ValueDropdownList<ComparableData> GetDataList()
        {
            var list = new ValueDropdownList<ComparableData>();
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
                    if(secondDropDown == "QuestState")
                        foreach (var quest in QuestDatabase.Instance.Quests) list.Add(quest);
                    else if (secondDropDown == "DataRows")
                    {
                        var currentQuest = QuestDatabase.Instance.Quests.Find(x => x.key == thirdDropDown);
                        foreach (var dataRow in currentQuest.DataRows) list.Add(dataRow);
                    }
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
            Target.targetRow = Data as DataRow;
            
            Close();
        }
    }
}
#endif
