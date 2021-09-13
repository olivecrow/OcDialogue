using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue.DB;
using OcUtility;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcDialogue
{
    public enum QuestState
    {
        None,
        WorkingOn,
        Done
    }
    public class Quest : OcData, IDataRowUser
    {
        public override string Address => $"{Category}/{name}";
        public DataRowContainer DataRowContainer => dataRowContainer;
        [ValueDropdown("GetCategory")][PropertyOrder(-2)]public string Category;
        [ShowInInspector, PropertyOrder(-1)][DelayedProperty]
        public string Name
        {
            get => name;
            set
            {
                name = value;
#if UNITY_EDITOR
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(this), name);
#endif
            }
        }
        [TextArea]
        public string Description;

        public Quest ParentQuest;
        public OcData[] References;

        public DataRowContainer dataRowContainer;
        [LabelText("클리어 조건")][InlineButton("QueryMyDataRow", "데이터 가져오기")]
        public DataChecker Checker;
      
#if UNITY_EDITOR
        [PropertyOrder(-3)]public int e_order;
        public RuntimeValue EditorPreset => _editorPreset;
        [SerializeField]
        [DisableIf("@UnityEditor.EditorApplication.isPlaying")]
        [BoxGroup("Debug")]
        [HorizontalGroup("Debug/1")]
        RuntimeValue _editorPreset;
#endif
        public event Action<Quest> OnRuntimeValueChanged;

        public const string saveDataKey_questState = "QuestState";
        [ShowInInspector]
        [HorizontalGroup("Debug/1")]
        [EnableIf("@UnityEngine.Application.isPlaying")]
        public RuntimeValue Runtime
        {
            get => _runtime;
            set => _runtime = value;
        }
        RuntimeValue _runtime;

        public void GenerateRuntimeData()
        {
            _runtime = new RuntimeValue();
            dataRowContainer.GenerateRuntimeData();
            dataRowContainer.OnRuntimeValueChanged += row => OnRuntimeValueChanged?.Invoke(this);
        }


        public bool IsAbleToClear(CheckFactor.Operator op, QuestState state)
        {
            switch (op)
            {
                case CheckFactor.Operator.Equal:
#if UNITY_EDITOR
                    if (!EditorApplication.isPlaying) return _editorPreset.QuestState == state;     
#endif
                    return _runtime.QuestState == state;
                case CheckFactor.Operator.NotEqual:
#if UNITY_EDITOR
                    if (!EditorApplication.isPlaying) return _editorPreset.QuestState != state;     
#endif
                    return _runtime.QuestState != state;
            }

            return false;
        }

        public bool IsAbleToClear(CheckFactor.Operator op, bool isAbleToClear)
        {
            switch (op)
            {
                case CheckFactor.Operator.Equal: return Checker.IsTrue() == isAbleToClear;
                case CheckFactor.Operator.NotEqual: return Checker.IsTrue() != isAbleToClear;
            }

            return false;
        }

        public void SetState(QuestState targetState, bool withoutNotify = false)
        {
            var isNew = _runtime.QuestState != targetState;
            _runtime.QuestState = targetState;
            if(!withoutNotify && isNew) OnRuntimeValueChanged?.Invoke(this);
        }

        public CommonSaveData GetSaveData()
        {
            var data = new CommonSaveData
            {
                Key = Name,
                DataRowContainerDict = dataRowContainer.GetSaveData(),
                Data = new Dictionary<string, string>
                {
                    [saveDataKey_questState] = _runtime.QuestState.ToString()
                }
            };
            return data;
        }

        public void Load(CommonSaveData data)
        {
            dataRowContainer.Overwrite(data.DataRowContainerDict);
            if (data.Data.ContainsKey(saveDataKey_questState) &&
                Enum.TryParse(data.Data[saveDataKey_questState], out QuestState state))
            {
                SetState(state, true);   
            }
        }
        
#if UNITY_EDITOR
        void Reset()
        {
            if (DataRowContainer == null) dataRowContainer = new DataRowContainer();
            DataRowContainer.Parent = this;
        }

        public void Resolve()
        {
            if(DataRowContainer.Parent != this) Printer.Print($"[Quest]{name}) DataRowContainer의 Parent를 재설정");
            DataRowContainer.Parent = this;
            DataRowContainer.MatchParent();
        }

        ValueDropdownList<string> GetCategory()
        {
            var list = new ValueDropdownList<string>();
            foreach (var category in QuestDB.Instance.Category)
            {
                list.Add(category);
            }

            return list;
        }
        
        void QueryMyDataRow()
        {
            if(!EditorUtility.DisplayDialog("데이터 가져오기", "이 작업은 현재의 Checker를 덮어씌웁니다. 계속 진행하시겠습니까?"
                , "진행", "취소"))
                return;
            Undo.RecordObject(this, "Query My DataRow");
            var factorList = new List<CheckFactor>();
            foreach (var dataRow in DataRowContainer.DataRows)
            {
                var factor = new CheckFactor();
                factor.Data = dataRow;
                factor.UpdateAddress();
                factorList.Add(factor);
            }

            Checker.factors = factorList.ToArray();
            Checker.UpdateExpression();
        }
#endif

        [Serializable]
        public struct RuntimeValue
        {
            [LabelWidth(110)]public QuestState QuestState;
        }
    }
}
