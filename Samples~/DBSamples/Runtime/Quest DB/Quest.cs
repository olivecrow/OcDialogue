using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue;
using OcDialogue.DB;
using OcUtility;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MyDB
{
    public enum QuestState
    {
        None,
        WorkingOn,
        Done
    }
    public class Quest : OcData, IDataRowUser, IEnumHandler
    {
        public override string Address => $"{category}/{name}";

        public override string Category
        {
            get => category;
            set => category = value;
        }
        public DataRowContainer DataRowContainer => dataRowContainer;
        [PropertyOrder(-2)]public string category;
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

        public const string fieldName_QuestState = "QuestState";
        public const string fieldName_IsAbleToClear = "IsAbleToClear";
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
                    [fieldName_QuestState] = _runtime.QuestState.ToString()
                }
            };
            return data;
        }

        public void Overwrite(CommonSaveData data)
        {
            dataRowContainer.Overwrite(data.DataRowContainerDict);
            if (data.Data.ContainsKey(fieldName_QuestState))
                Enum.TryParse(data.Data[fieldName_QuestState], out _runtime.QuestState);
        }
        
        
        
        
        public override bool IsTrue(string fieldName, CheckFactor.Operator op, object checkValue)
        {
            if (fieldName == fieldName_QuestState)
            {
                if (checkValue is string sName)
                {
                    Debug.Log($"sName : {sName}");
                    return ((int)_runtime.QuestState).IsTrue(op, (int)Enum.Parse(typeof(QuestState), sName));
                }
                if(checkValue is int index) return ((int)_runtime.QuestState).IsTrue(op, index);
            }
            if (fieldName == fieldName_IsAbleToClear) return IsAbleToClear(op, (bool)checkValue);

            Debug.LogWarning($"[Quest][{category}][{name}]유효하지 않은 fieldName : {fieldName} => IsTrue == false");
            return false;
        }

        public override object GetValue(string fieldName)
        {
            if (fieldName == fieldName_QuestState) return _runtime.QuestState;
            if (fieldName == fieldName_IsAbleToClear) return IsAbleToClear(CheckFactor.Operator.Equal, true);

            Debug.LogWarning($"[Quest][{category}][{name}]유효하지 않은 fieldName : {fieldName} => GetValue == null");
            return null;
        }

        public override string[] GetFieldNames()
        {
            return new[] { fieldName_QuestState, fieldName_IsAbleToClear };
        }

        public override void SetValue(string fieldName, DataSetter.Operator op, object value)
        {
            if (fieldName == fieldName_QuestState)
            {
                _runtime.QuestState = op switch
                {
                    DataSetter.Operator.Set => (QuestState)Enum.Parse(typeof(QuestState), value.ToString()),
                    _ => _runtime.QuestState
                };
                return;
            }

            Debug.LogWarning($"[Quest][{category}][{name}]유효하지 않은 fieldName : {fieldName} => SetData ==> x");
        }

        public override DataRowType GetValueType(string fieldName)
        {
            if (fieldName == fieldName_QuestState) return DataRowType.String;
            if (fieldName == fieldName_IsAbleToClear) return DataRowType.Bool;
            
            return DataRowType.String;
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
                factor.TargetData = dataRow;
                factor.UpdateExpression();
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

        public Type GetEnumType(string fieldName)
        {
            return typeof(QuestState);
        }

        public string[] GetEnumNames(string fieldName)
        {
            return Enum.GetNames(typeof(QuestState));
        }
    }
}
