using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue.DB;
using OcUtility;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace OcDialogue
{
    public enum QuestState
    {
        None,
        WorkingOn,
        Done
    }
    public class QuestV2 : AddressableData, IDataRowUser
    {
        public override string Address => $"{Category}/{name}";
        public DataRowContainerV2 DataRowContainer => dataRowContainer;
        [ValueDropdown("GetCategory")][PropertyOrder(-2)]public string Category;
        [ShowInInspector, PropertyOrder(-1)]
        public string Name
        {
            get => name;
            set
            {
                name = value;
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }
        [TextArea]
        public string Description;

        public AddressableData[] References;

        public DataRowContainerV2 dataRowContainer;
        [LabelText("클리어 조건")][InlineButton("QueryMyDataRow", "데이터 가져오기")]
        public DataCheckerV2 Checker;
      
#if UNITY_EDITOR
        
        public RuntimeValue EditorPreset => _editorPreset;
        [SerializeField]
        [DisableIf("@UnityEditor.EditorApplication.isPlaying")]
        [BoxGroup("Debug")]
        [HorizontalGroup("Debug/1")]
        RuntimeValue _editorPreset;
#endif
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
            DataRowContainer.GenerateRuntimeData();
        }


        public bool IsTrue(Operator op, QuestState state)
        {
            switch (op)
            {
                case Operator.Equal:
#if UNITY_EDITOR
                    if (!EditorApplication.isPlaying) return _editorPreset.QuestState == state;     
#endif
                    return _runtime.QuestState == state;
                case Operator.NotEqual:
#if UNITY_EDITOR
                    if (!EditorApplication.isPlaying) return _editorPreset.QuestState != state;     
#endif
                    return _runtime.QuestState != state;
            }

            return false;
        }

        public bool IsTrue(Operator op, bool isAbleToClear)
        {
            switch (op)
            {
                case Operator.Equal:
#if UNITY_EDITOR
                    if (!EditorApplication.isPlaying) return _editorPreset.IsAbleToClear == isAbleToClear;     
#endif
                    return _runtime.IsAbleToClear == isAbleToClear;
                case Operator.NotEqual:
#if UNITY_EDITOR
                    if (!EditorApplication.isPlaying) return _editorPreset.IsAbleToClear != isAbleToClear;     
#endif
                    return _runtime.IsAbleToClear != isAbleToClear;
            }

            return false;
        }
        
        
#if UNITY_EDITOR
        void Reset()
        {
            if (DataRowContainer == null) dataRowContainer = new DataRowContainerV2();
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
            var factorList = new List<CheckFactorV2>();
            foreach (var dataRow in DataRowContainer.DataRows)
            {
                var factor = new CheckFactorV2();
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
            [LabelWidth(110)]public bool IsAbleToClear;
        }
    }
}
