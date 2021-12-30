using System;
using System.Collections;
using System.Collections.Generic;
using OcUtility;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcDialogue.DB
{
    public class DataRow : OcData
    {
        public override string Address => name;

        public override string Category
        {
            get => category;
            set => category = value;
        }
        
        [HideInTables]
        public string category;
        [ShowInInspector]
        [DisableIf("@UnityEditor.EditorApplication.isPlaying")]
        [PropertyOrder(1)]
        [TableColumnWidth(80, false)]
        public DataRowType Type
        {
            get => _initialValue.Type;
            set
            {
#if !UNITY_EDITOR
                if(Application.isPlaying) return;            
#endif
                var isNew = _initialValue.Type != value;
                _initialValue.Type = value;
                if (isNew)
                {
#if UNITY_EDITOR
                    _editorPresetValue.Type = Type;
                    _runtimeValue.Type = Type;
#endif
                }
            }
        }
        public PrimitiveValue InitialValue => _initialValue;
        [ShowInInspector]
        [ShowIf("@UnityEngine.Application.isPlaying")]
        [TableColumnWidth(110, false)]
        [PropertyOrder(9)]
        public PrimitiveValue RuntimeValue
        {
            get => _runtimeValue;
            set => _runtimeValue = value;
        }
        public event Action<DataRow> OnRuntimeValueChanged;
        [ShowInInspector, TableColumnWidth(150, false), PropertyOrder(-1)][DelayedProperty]
        public string Name
        {
            get => name;
            set
            {
                name = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();          
#endif
            }
        }
#if UNITY_EDITOR
        public PrimitiveValue EditorPresetValue => _editorPresetValue;

        [PropertyOrder(8)]
        [SerializeField]
        [DisableIf("@UnityEditor.EditorApplication.isPlaying")]
        [TableColumnWidth(110, false)]
        PrimitiveValue _editorPresetValue;

        [PropertyOrder(10)] public string description;
#endif
        public object TargetValue
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    return Type switch
                    {
                        DataRowType.Bool => _initialValue.BoolValue,
                        DataRowType.Int => _initialValue.IntValue,
                        DataRowType.Float => _initialValue.FloatValue,
                        DataRowType.String => _initialValue.StringValue,
                        _ => throw new ArgumentOutOfRangeException()
                    };              
                }
#endif
                return Type switch
                {
                    DataRowType.Bool => _runtimeValue.BoolValue,
                    DataRowType.Int => _runtimeValue.IntValue,
                    DataRowType.Float => _runtimeValue.FloatValue,
                    DataRowType.String => _runtimeValue.StringValue,
                    _ => _runtimeValue.BoolValue
                };
            }
        }


        [SerializeField] [DisableIf("@UnityEditor.EditorApplication.isPlaying")] [TableColumnWidth(110, false)]
        [PropertyOrder(2)]
        PrimitiveValue _initialValue;

        PrimitiveValue _runtimeValue;

        public void GenerateRuntimeData()
        {
            OnRuntimeValueChanged = null;
            RuntimeValue = new PrimitiveValue()
            {
                Type = Type,
                BoolValue = InitialValue.BoolValue,
                IntValue = InitialValue.IntValue,
                FloatValue = InitialValue.FloatValue,
                StringValue = InitialValue.StringValue,
            };
        }

        public void SetTypeAndValue(DataRowType type, object value)
        {
            Type = type;
            switch (type)
            {
                case DataRowType.Bool:
                    if(!(value is bool b)) return;
                    SetValue(b);
                    break;
                case DataRowType.Int:
                    if(!(value is int i)) return;
                    SetValue(i);
                    break;
                case DataRowType.Float:
                    if(!(value is float f)) return;
                    SetValue(f);
                    break;
                case DataRowType.String:
                    if(!(value is string s)) return;
                    SetValue(s);
                    break;
            }
        }

        /// <summary> 런타임 bool 값을 변경함. </summary>
        public void SetValue(bool value, DataSetter.Operator op = DataSetter.Operator.Set, bool withoutNotify = false)
        {
            if(op != DataSetter.Operator.Set) 
                Debug.LogWarning($"[DataRow][{name}] bool 타입엔 Operator.Set만 사용할 수 있음 | SetValue ==> {value}");
            var isNew = _runtimeValue.BoolValue != value;
            _runtimeValue.BoolValue = value;
            if(!withoutNotify && isNew) OnRuntimeValueChanged?.Invoke(this);
        }

        /// <summary> 런타임 int 값을 변경함. </summary>
        public void SetValue(int value, DataSetter.Operator op = DataSetter.Operator.Set, bool withoutNotify = false)
        {
            var result = op switch
            {
                DataSetter.Operator.Set => value,
                DataSetter.Operator.Add => _runtimeValue.IntValue + value,
                DataSetter.Operator.Multiply => _runtimeValue.IntValue * value,
                DataSetter.Operator.Divide => _runtimeValue.IntValue / value,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
            var isNew = _runtimeValue.IntValue != result;
            _runtimeValue.IntValue = result;
            if(!withoutNotify && isNew) OnRuntimeValueChanged?.Invoke(this);
        }

        /// <summary> 런타임 float 값을 변경함. </summary>
        public void SetValue(float value, DataSetter.Operator op = DataSetter.Operator.Set, bool withoutNotify = false)
        {
            var result = op switch
            {
                DataSetter.Operator.Set => value,
                DataSetter.Operator.Add => _runtimeValue.FloatValue + value,
                DataSetter.Operator.Multiply => _runtimeValue.FloatValue * value,
                DataSetter.Operator.Divide => _runtimeValue.FloatValue / value,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
            
            var isNew = Math.Abs(_runtimeValue.FloatValue - result) > 0.0005f;
            _runtimeValue.FloatValue = value;
            if(!withoutNotify && isNew) OnRuntimeValueChanged?.Invoke(this);
        }

        /// <summary> 런타임 string 값을 변경함. </summary>
        public void SetValue(string value, DataSetter.Operator op = DataSetter.Operator.Set, bool withoutNotify = false)
        {
            string result;
            switch(op)
            {
                case DataSetter.Operator.Set :
                    result = value;
                    break;
                case DataSetter.Operator.Add :
                    result = _runtimeValue.StringValue + value;
                    break;
                default:
                    result = value;
                    Debug.LogWarning($"[DataRow][{name}] bool 타입엔 Operator.Set만 사용할 수 있음 | SetValue ==> {value}");
                    break;
            };
            var isNew = _runtimeValue.StringValue != result;
            _runtimeValue.StringValue = result;
            if(!withoutNotify && isNew) OnRuntimeValueChanged?.Invoke(this);
        }

        public bool IsTrue(CheckFactor.Operator op, object value)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                return Type switch
                {
                    DataRowType.Bool => _editorPresetValue.BoolValue.IsTrue(op, (bool) value),
                    DataRowType.Int => _editorPresetValue.IntValue.IsTrue(op, (int) value),
                    DataRowType.Float => _editorPresetValue.FloatValue.IsTrue(op, (float) value),
                    DataRowType.String => _editorPresetValue.StringValue.IsTrue(op, (string) value),
                    _ => false
                };
            }
#endif
            return Type switch
            {
                DataRowType.Bool => _runtimeValue.BoolValue.IsTrue(op, (bool) value),
                DataRowType.Int => _runtimeValue.IntValue.IsTrue(op, (int) value),
                DataRowType.Float => _runtimeValue.FloatValue.IsTrue(op, (float) value),
                DataRowType.String => _runtimeValue.StringValue.IsTrue(op, (string) value),
                _ => false
            };
        }

        
        public override bool IsTrue(string fieldName, CheckFactor.Operator op, object checkValue)
        {
            return IsTrue(op, checkValue);
        }

        public override object GetValue(string fieldName)
        {
            return TargetValue;
        }

        public override string[] GetFieldNames()
        {
            return null;
        }

        public override void SetValue(string fieldName, DataSetter.Operator op, object value)
        {
            switch (Type)
            {
                case DataRowType.Bool:
                    SetValue((bool)value, op);
                    break;
                case DataRowType.Int:
                    SetValue((int)value, op);
                    break;
                case DataRowType.Float:
                    SetValue((float)value, op);
                    break;
                case DataRowType.String:
                    SetValue(value.ToString(), op);
                    break;
            }
        }
        public override DataRowType GetValueType(string fieldName) => Type;
        


        public override string ToString()
        {
            return $"{TotalAddress} | {Type} | {TargetValue}";
        }

#if UNITY_EDITOR
        [Button("X"), GUIColor(2,0,0)][PropertyOrder(100)][TableColumnWidth(50, false)]
        void DeleteSelf()
        {
            if(parent == null)
            {
                Debug.LogWarning($"parent가 비어있음");
                return;
            }

            (parent as IDataRowUser).DataRowContainer.DeleteRow(name);
        }
        public void LoadFromEditorPreset()
        {
            RuntimeValue = new PrimitiveValue()
            {
                Type = Type,
                BoolValue = _editorPresetValue.BoolValue,
                IntValue = _editorPresetValue.IntValue,
                FloatValue = _editorPresetValue.FloatValue,
                StringValue = _editorPresetValue.StringValue,
            };
        }
        public void EditorPresetToDefault()
        {
            _editorPresetValue.BoolValue   = _initialValue.BoolValue;
            _editorPresetValue.IntValue    = _initialValue.IntValue;
            _editorPresetValue.FloatValue  = _initialValue.FloatValue;
            _editorPresetValue.StringValue = _initialValue.StringValue;
        }

        public void RuntimeValueToEditorPresetValue()
        {
            _runtimeValue.BoolValue   = _editorPresetValue.BoolValue;
            _runtimeValue.IntValue    = _editorPresetValue.IntValue;
            _runtimeValue.FloatValue  = _editorPresetValue.FloatValue;
            _runtimeValue.StringValue = _editorPresetValue.StringValue;
        }
#endif
    }

    public enum DataRowType
    {
        Bool,
        Int,
        Float,
        String
    }
}
