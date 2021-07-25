#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcDialogue
{
    [Serializable]
    public class QuestEditorPreset
    {
        public bool usePreset;
        [HideInInspector]public List<OverridenQuest> Quests;
        public List<OverridenQuest> tmpList;
        public List<Quest> GetAllCopies()
        {
            var list = new List<Quest>();

            foreach (var overrideQuest in Quests)
            {
                var copy = overrideQuest.quest.GetCopy();
                copy.QuestState = overrideQuest.overrideState;
                list.Add(copy);
            }

            return list;
        }

        [Button]
        void ToDefault()
        {
            var list = new List<OverridenQuest>();
            foreach (var quest in QuestDatabase.Instance.Quests)
            {
                var overrideQuest = new OverridenQuest(quest);
                list.Add(overrideQuest);
            }

            Quests = list;
        }

        [Serializable]
        public class OverridenQuest
        {
            [ReadOnly]public Quest quest;
            [GUIColor("GetColor")]public Quest.State overrideState;
            [TableList]public List<OverridenRow> overrideRows;

            public OverridenQuest(Quest quest)
            {
                this.quest = quest;
                overrideRows = new List<OverridenRow>();

                foreach (var dataRow in quest.DataRowContainer.dataRows)
                {
                    var overrideRow = new OverridenRow(dataRow);
                    overrideRows.Add(overrideRow);
                }
            }

            Color GetColor()
            {
                if(overrideState != Quest.State.None) return Color.yellow;
                
                return Color.white;
            }
        }
    }
}
#endif