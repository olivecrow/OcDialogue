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
                overrideQuest.ApplyOverrides(copy);
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
        public class OverridenQuest : IComparableData
        {
            [ReadOnly]public Quest quest;
            [HorizontalGroup("1")]
            [GUIColor("GetColor")]public Quest.State overrideState;
            [TableList][HorizontalGroup("1")]public List<OverridenRow> overrideRows;

            public OverridenQuest(Quest quest)
            {
                this.quest = quest;
                overrideRows = new List<OverridenRow>();

                foreach (var dataRow in quest.DataRowContainer.dataRows)
                {
                    var overrideRow = new OverridenRow(dataRow, dataRow.TargetValue);
                    overrideRows.Add(overrideRow);
                }
            }

            public void ApplyOverrides(Quest target)
            {
                target.QuestState = overrideState;
                for (int i = 0; i < target.DataRowContainer.dataRows.Count; i++)
                {
                    target.DataRowContainer.SetValue(overrideRows[i].Key, overrideRows[i].OverridenValue);
                }
            }

            Color GetColor()
            {
                if(overrideState != Quest.State.None) return Color.yellow;
                
                return Color.white;
            }

            public string Key => quest.key;
            public bool IsTrue(CompareFactor factor, Operator op, object value1)
            {
                var tmp = quest.GetCopy();
                ApplyOverrides(tmp);
                return tmp.IsTrue(factor, op, value1);
            }
        }
    }
}
#endif