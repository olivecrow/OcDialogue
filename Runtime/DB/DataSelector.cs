using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    [Serializable]
    public class DataSelector
    {
        public DBType DBType;
        [ValueDropdown("GetFirstList"), HideLabel]public string firstDropDown;
        public string secondDropDown;

        public ScriptableObject FindData()
        {
            switch (DBType)
            {
                case DBType.GameProcess:
                {
                    var targetRow = GameProcessDatabase.Instance.DataRows.Find(x => x.key == firstDropDown);
                    return targetRow;
                }
                case DBType.Item:
                    break;
                case DBType.Quest:
                    break;
                case DBType.NPC:
                {
                    var targetRow = NPCDatabase.Instance.NPCs.Find(x => x.NPCName == firstDropDown);
                    return targetRow;
                }
                case DBType.Enemy:
                    break;
            }

            return null;
        }

        bool IsSecondDropDownValid() => DBType != DBType.GameProcess && DBType != DBType.NPC;

#if UNITY_EDITOR
        ValueDropdownList<string> GetFirstList()
        {
            var list = new ValueDropdownList<string>();
            switch (DBType)
            {
                case DBType.GameProcess:
                    foreach (var row in GameProcessDatabase.Instance.DataRows) list.Add(row.key);
                    break;
                case DBType.Item:
                    break;
                case DBType.Quest:
                    break;
                case DBType.NPC:
                    foreach (var npc in NPCDatabase.Instance.NPCs) list.Add(npc.NPCName);
                    break;
                case DBType.Enemy:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return list;
        }
#endif
    }
}