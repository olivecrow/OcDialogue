using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcDialogue
{
    public class Quest : ComparableData
    {
        public enum State
        {
            None,
            WorkingOn,
            Finished
        }

        public override string Key => key;
        [InlineButton("MatchName", ShowIf = "@key != name"), HorizontalGroup("Key"), LabelWidth(100)]public string key;
        [HorizontalGroup("Key"), ValueDropdown("GetCategory"), LabelWidth(100)]public string Category;
        [Multiline(5), LabelWidth(100)]public string description;
        public List<ComparableData> References;
        [BoxGroup("Data")]
        [HorizontalGroup("Data/1", Width = 500, Order = 10)]public DataRowContainer DataRowContainer;
        [HorizontalGroup("Data/1", Order = 11), LabelText("클리어 조건")]public DataChecker checker;
        public State QuestState { get; set; }

        public Quest GetCopy()
        {
            var quest = CreateInstance<Quest>();
            quest.name = key;
            quest.key = key;
            quest.Category = Category;
            quest.description = description;

            quest.References = References;
            var rows = DataRowContainer.GetAllCopies();
            quest.DataRowContainer = new DataRowContainer(quest, rows);

            return quest;
        }

        public bool IsClearAvailable()
        {
            return checker.IsTrue();
        }

        public bool IsTrue(Operator op, Quest.State questState)
        {
            return IsTrue(CompareFactor.QuestState, op, questState);
        }
        
        public override bool IsTrue(CompareFactor factor, Operator op, object value1)
        {
            if (factor != CompareFactor.QuestState) return false;

            return op switch
            {
                Operator.Equal => QuestState == (State) value1,
                Operator.NotEqual => QuestState != (State) value1,
                _ => false
            };
        }
        
#if UNITY_EDITOR
        void Reset()
        {
            if(DataRowContainer == null) return;
            DataRowContainer.owner = this;
        }

        void OnValidate()
        {
            DataRowContainer.CheckNames();
        }

        ValueDropdownList<string> GetCategory()
        {
            var list = new ValueDropdownList<string>();
            foreach (var s in QuestDatabase.Instance.Category)
            {
                list.Add(s);
            }

            return list;
        }
        void MatchName()
        {
            var path = AssetDatabase.GetAssetPath(this);
            AssetDatabase.RenameAsset(path, key);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        [Button, HorizontalGroup("Row"), PropertyOrder(9), GUIColor(0,1,1)]
        void AddData()
        {
            DataRowContainer.owner = this;
            DataRowContainer.AddData(DBType.Quest, DataStorageType.Embeded);
        }
        
        [Button, HorizontalGroup("Row"), PropertyOrder(9), GUIColor(1,0,0)]
        void DeleteData(string k)
        {
            DataRowContainer.DeleteRow(k, DataStorageType.Embeded);
        }

        [Button, HorizontalGroup("Row"), PropertyOrder(9)]
        void MatchNames()
        {
            DataRowContainer.MatchDataRowNames();
        }
        
#endif

    }
}
