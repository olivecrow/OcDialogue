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
        [InlineButton("MatchName", ShowIf = "@key != name"), PropertyOrder(0), HorizontalGroup("Key"), LabelWidth(100)]public string key;
        [HorizontalGroup("Key"), ValueDropdown("GetCategory"), LabelWidth(100)]public string Category;
        [Multiline(5), PropertyOrder(1), LabelWidth(100)]public string description;
        [PropertyOrder(5)]public List<ComparableData> References;
        [TableList(IsReadOnly = true), PropertyOrder(20)] public List<DataRow> DataRows;
        public State QuestState { get; set; }

        public Quest GetCopy()
        {
            var quest = CreateInstance<Quest>();
            quest.key = key;
            quest.Category = Category;
            quest.description = description;

            quest.References = References;
            var rows = new List<DataRow>();
            foreach (var dataRow in DataRows)
            {
                rows.Add(dataRow.GetCopy());
            }

            quest.DataRows = rows;

            return quest;
        }
        
        public override bool IsTrue(CompareFactor factor, Operator op, object value)
        {
            if (factor != CompareFactor.QuestState) return false;

            return op switch
            {
                Operator.Equal => QuestState == (State) value,
                Operator.NotEqual => QuestState != (State) value,
                _ => false
            };
        }
        
#if UNITY_EDITOR

        bool MisMatchDataNames() => DataRows.Any(x => x.name != x.key);

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

        [HorizontalGroup("Buttons"), Button("Add DataRow"), GUIColor(0,1,1), PropertyOrder(10)]
        void AddData()
        {
            var row = CreateInstance<DataRow>();
            row.ownerDB = DBType.Quest;
            row.name = OcDataUtility.CalculateDataName("New DataRow", DataRows.Select(x => x.key));
            row.key = row.name;
            DataRows.Add(row);
            OcDataUtility.Repaint();
            AssetDatabase.AddObjectToAsset(row, this);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        [HorizontalGroup("Buttons"), Button("Delete Row"), GUIColor(1,0,0), PropertyOrder(11)]
        void DeleteRow(string key)
        {
            var row = DataRows.FirstOrDefault(x => x.key == key);
            if (row == null)
            {
                var path = AssetDatabase.GetAssetPath(this);
                var allAssets = AssetDatabase.LoadAllAssetsAtPath(path).Select(x => x as DataRow);
                row = allAssets.FirstOrDefault(x => x.key == key);
                if(row == null)
                {
                    Debug.LogWarning($"해당 키값의 DataRow가 없어서 삭제에 실패함 : {key}");
                    return;
                }
            }

            DataRows.Remove(row);
            
            OcDataUtility.Repaint();
            AssetDatabase.RemoveObjectFromAsset(row);
            DataRows = DataRows.Where(x => x != null).ToList();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        [HorizontalGroup("Buttons"), Button("Match Names"), PropertyOrder(12), ShowIf("MisMatchDataNames")]
        void MatchDataRowNames()
        {
            foreach (var dataRow in DataRows)
            {
                if (dataRow.name != dataRow.key) dataRow.name = dataRow.key;
            }
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif

    }
}
