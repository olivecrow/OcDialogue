using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcUtility;
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
        [BoxGroup("Data"), HideLabel]public DataRowContainer DataRowContainer;
        [GUIColor(1f, 2f, 1f)]
        [InfoBox("클리어 여부 체크로 자기 자신을 넣을 수 없음", InfoMessageType.Error, "IsClearConditionWarningOn")]
        [InlineButton("QueryMyDataRow", "DataRow 반영")][BoxGroup("클리어 조건"), HideLabel]
        public DataChecker checker;
        public State QuestState
        {
            get => _questState;
            set
            {
                var isNew = _questState != value;
                _questState = value;
                if(isNew)
                {
                    OnStateChanged?.Invoke(value);
                    Printer.Print($"[Quest] {key} 퀘스트 State 변경 => {value}");
                }
            }
        }

        Quest __original;
        State _questState;
        public event Action<State> OnStateChanged; 

        public Quest GetCopy()
        {
            var quest = CreateInstance<Quest>();
            quest.__original = this;
            quest.name = key;
            quest.key = key;
            quest.Category = Category;
            quest.description = description;

            quest.References = References;
            var rows = DataRowContainer.GetAllCopies();
            quest.DataRowContainer = new DataRowContainer(quest, rows);

            return quest;
        }
        /// <summary>현재 퀘스트의 State가 전달된 값과 비교했을때 맞는지 판단함. QuestState와 QuestClearAvailability 외에는 제대로 작동하지 않음</summary>
        public override bool IsTrue(CompareFactor factor, Operator op, object value1)
        {
            switch (factor)
            {
                case CompareFactor.QuestState:
                    return op switch
                    {
                        Operator.Equal => QuestState == (State) value1,
                        Operator.NotEqual => QuestState != (State) value1,
                        _ => false
                    };
                case CompareFactor.QuestClearAvailability:
                    return op switch
                    {
                        Operator.Equal => IsAbleToClear() == (bool) value1,
                        Operator.NotEqual => IsAbleToClear() != (bool) value1,
                        _ => false
                    };
                default: return false;
            }
        }

        /// <summary> 현재 퀘스트의 클리어 조건들이 전부 가능한 상태인지 확인함. 클리어가 가능하면 true를 반환함.
        /// 퀘스트 상태가 WorkingOn이 아니면 항상 false를 반환함. 오리지날 데이터는 항상 false를 반환함.</summary>
        public bool IsAbleToClear()
        {
            if (__original == null) return false;
            return QuestState == State.WorkingOn && __original.checker.IsTrue();
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

        [Button, HorizontalGroup("Data/btn"), PropertyOrder(9), GUIColor(0,1,1)]
        void AddData()
        {
            DataRowContainer.owner = this;
            DataRowContainer.AddData(DBType.Quest, DataStorageType.Embeded);
        }
        
        [Button, HorizontalGroup("Data/btn"), PropertyOrder(9), GUIColor(1,0,0)]
        void DeleteData(string k)
        {
            DataRowContainer.DeleteRow(k, DataStorageType.Embeded);
        }

        [Button, HorizontalGroup("Data/btn"), PropertyOrder(9)]
        void MatchNames()
        {
            DataRowContainer.MatchDataRowNames();
        }

        void QueryMyDataRow()
        {
            Undo.RecordObject(this, "Query My DataRow");
            var factorList = new List<CheckFactor>();
            foreach (var dataRow in DataRowContainer.dataRows)
            {
                var factor = new CheckFactor();
                var selector = factor.DataSelector = new DataSelector();
                selector.targetData = dataRow;
                selector.DBType = DBType.Quest;
                selector.path = $"QuestDatabase/{Category}/{key}";

                factor.Index = DataRowContainer.dataRows.IndexOf(dataRow);
                factorList.Add(factor);
            }

            checker.factors = factorList.ToArray();
        }

        bool IsClearConditionWarningOn()
        {
            if (checker == null) return false;
            if (checker.factors == null) return false;
            foreach (var checkerFactor in checker.factors)
            {
                if (checkerFactor.DataSelector.targetData == this)
                {
                    return true;
                }
            }

            return false;
        }
#endif

    }
}
