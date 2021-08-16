#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class DataSelectWindow : OdinEditorWindow
    {
        public DataSelector Target { get; set; }

        public static DataSelectWindow Open()
        {
            var wnd = GetWindow<DataSelectWindow>(true);
            return wnd;
        }
        
        public DBType DBType;
        [ValueDropdown("GetFirstList"), HideLabel, ShowIf("UseFirstDropdown"), GUIColor(2f, 0.8f, 1.2f)]
        public string firstDropdown;
        [ValueDropdown("GetSecondList"), HideLabel, ShowIf("UseSecondDropdown"), GUIColor(2.2f, 1.2f, 1.6f)]
        public string secondDropDown;
        [ValueDropdown("GetThirdList"), HideLabel, ShowIf("UseThirdDropdown"), GUIColor(2.5f, 1.8f, 2f)]
        public string thirdDropDown;

        [ValueDropdown("GetDataList"), HideLabel, GUIColor(0,1,1)] public ComparableData Data;
        bool UseFirstDropdown() => DBType != DBType.GameProcess;
        bool UseSecondDropdown() => UseFirstDropdown() && (DBType == DBType.Item || DBType == DBType.Quest || DBType == DBType.NPC || DBType == DBType.Enemy);
        bool UseThirdDropdown() => UseSecondDropdown() && (secondDropDown == "DataRows");

        void OnValidate()
        {
            switch (DBType)
            {
                case DBType.Quest:
                {
                    if (secondDropDown == "Quest.DataRows")
                    {
                        if(Data is Quest)
                        {
                            thirdDropDown = Data.Key;
                            Data = null;
                        }
                    }

                    break;
                }
                case DBType.NPC:
                    if (secondDropDown == "NPC.DataRows")
                    {
                        if(Data is NPC)
                        {
                            thirdDropDown = Data.Key;
                            Data = null;
                        }
                    } 
                    break;
                case DBType.Enemy:
                    if (secondDropDown == "Enemy.DataRows")
                    {
                        if(Data is Enemy)
                        {
                            thirdDropDown = Data.Key;
                            Data = null;
                        }
                    } 
                    break;
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
                {
                    var itemTypes = Enum.GetNames(typeof(ItemType));
                    foreach (var type in itemTypes) list.Add(type);
                    break;
                }
                case DBType.Quest:
                {
                    var category = QuestDatabase.Instance.Category;
                    foreach (var c in category) list.Add(c);
                    break;
                }
                case DBType.NPC:
                {
                    var category = NPCDatabase.Instance.Category;
                    foreach(var c in category) list.Add(c);
                    break;
                }
                case DBType.Enemy:
                {
                    var category = EnemyDatabase.Instance.Category;
                    foreach(var c in category) list.Add(c);
                    break;
                }
                default:
                    Debug.LogWarning("해당 타입의 ValueDropDown이 미구현됨.");
                    break;
            }

            return list;
        }
        ValueDropdownList<string> GetSecondList()
        {
            var list = new ValueDropdownList<string>();
            if (string.IsNullOrWhiteSpace(firstDropdown)) return list;
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
                    list.Add("Quest");
                    list.Add("DataRows");
                    break;
                case DBType.NPC:
                    list.Add("NPC");
                    list.Add("DataRows");
                    break;    
                case DBType.Enemy:
                    list.Add("Enemy");
                    list.Add("DataRows");
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
            if (string.IsNullOrWhiteSpace(secondDropDown)) return list;
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
                    if (secondDropDown == "DataRows")
                    {
                        foreach (var npc in NPCDatabase.Instance.NPCs) list.Add(npc.NPCName);
                    }
                    break;
                case DBType.Enemy:
                    if (secondDropDown == "DataRows")
                    {
                        foreach (var enemy in EnemyDatabase.Instance.Enemies) list.Add(enemy.key);
                    }
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
                    foreach (var row in GameProcessDatabase.Instance.DataRowContainer.dataRows) list.Add(row);
                    break;
                // case DBType.Item:
                //     var targetItems = ItemDatabase.Instance.Items.Where(x =>
                //         x.type.ToString() == firstDropdown && x.SubTypeString == secondDropDown);
                //     foreach (var item in targetItems) list.Add(item);
                //     break;
                case DBType.Quest:
                    if(secondDropDown == "Quest")
                        foreach (var quest in QuestDatabase.Instance.Quests) list.Add(quest);
                    else if (secondDropDown == "DataRows")
                    {
                        var currentQuest = QuestDatabase.Instance.Quests.Find(x => x.key == thirdDropDown);
                        foreach (var dataRow in currentQuest.DataRowContainer.dataRows) list.Add(dataRow);
                    }
                    break;
                case DBType.NPC:
                    if(secondDropDown == "NPC")
                        foreach (var npc in NPCDatabase.Instance.NPCs) list.Add(npc);
                    else if (secondDropDown == "DataRows")
                    {
                        var currentNPC = NPCDatabase.Instance.NPCs.Find(x => x.Key == thirdDropDown);
                        foreach (var dataRow in currentNPC.DataRowContainer.dataRows) list.Add(dataRow);
                    }
                    break;
                case DBType.Enemy:
                    if(secondDropDown == "Enemy")
                        foreach (var enemy in EnemyDatabase.Instance.Enemies) list.Add(enemy);
                    else if (secondDropDown == "DataRows")
                    {
                        var currentEnemy = EnemyDatabase.Instance.Enemies.Find(x => x.key == thirdDropDown);
                        foreach (var dataRow in currentEnemy.DataRowContainer.dataRows) list.Add(dataRow);
                    }
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
            Target.DBType = DBType;
            Target.targetData = Data;
            Target.path = $"{DBType}";
            if (UseFirstDropdown()) Target.path += $"/{firstDropdown}";
            if (UseSecondDropdown()) Target.path += $"/{secondDropDown}";
            if (UseThirdDropdown()) Target.path += $"/{thirdDropDown}";

            Close();
            EditorUtility.SetDirty(Selection.activeObject);
        }
    }
}
#endif
