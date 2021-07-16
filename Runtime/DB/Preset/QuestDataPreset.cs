using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcDialogue
{
    [CreateAssetMenu(fileName = "Quest Data Preset", menuName = "Oc Dialogue/Editor Preset/Quest Data Preset")]
    public class QuestDataPreset : ScriptableObject
    {
#if UNITY_EDITOR
        /// <summary> Editor Only. </summary>
        public const string AssetPath = "Quest Data Preset.asset";
        /// <summary> Editor Only. </summary>
        public static QuestDataPreset Instance => _instance;
        static QuestDataPreset _instance;

        [InitializeOnLoadMethod]
        static void EditorInit()
        {
            _instance = EditorGUIUtility.Load(AssetPath) as QuestDataPreset;
        }

        public bool usePreset;
        [TableList]public List<OverrideQuest> Quests;

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
            var copies = new List<OverrideQuest>();
            foreach (var quest in QuestDatabase.Instance.Quests)
            {
                var copy = quest.GetCopy();
                var overrideQuest = new OverrideQuest(copy);
                copies.Add(overrideQuest);
            }

            Quests = copies;
        }

        [Serializable]
        public class OverrideQuest
        {
            public Quest quest;
            public Quest.State overrideState;
            public List<GameProcessDataPreset.OverrideRow> overrideRows;

            public OverrideQuest(Quest quest)
            {
                this.quest = quest;
                overrideRows = new List<GameProcessDataPreset.OverrideRow>();

                foreach (var dataRow in quest.DataRows)
                {
                    var overrideRow = new GameProcessDataPreset.OverrideRow(dataRow);
                    overrideRows.Add(overrideRow);
                }
            }
        }
#endif
    }
}
