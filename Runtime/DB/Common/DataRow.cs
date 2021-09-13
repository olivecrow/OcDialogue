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
#if UNITY_EDITOR
        [ShowInInspector, TableColumnWidth(150, false), PropertyOrder(-1)]
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
                    };              
                }
#endif
                return Type switch
                {
                    DataRowType.Bool => _runtimeValue.BoolValue,
                    DataRowType.Int => _runtimeValue.IntValue,
                    DataRowType.Float => _runtimeValue.FloatValue,
                    DataRowType.String => _runtimeValue.StringValue,
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
        public void SetValue(bool value, bool withoutNotify = false)
        {
            var isNew = _runtimeValue.BoolValue != value;
            _runtimeValue.BoolValue = value;
            if(!withoutNotify && isNew) OnRuntimeValueChanged?.Invoke(this);
        }

        /// <summary> 런타임 int 값을 변경함. </summary>
        public void SetValue(int value, bool withoutNotify = false)
        {
            var isNew = _runtimeValue.IntValue != value;
            _runtimeValue.IntValue = value;
            if(!withoutNotify && isNew) OnRuntimeValueChanged?.Invoke(this);
        }

        /// <summary> 런타임 float 값을 변경함. </summary>
        public void SetValue(float value, bool withoutNotify = false)
        {
            var isNew = Math.Abs(_runtimeValue.FloatValue - value) > 0.0005f;
            _runtimeValue.FloatValue = value;
            if(!withoutNotify && isNew) OnRuntimeValueChanged?.Invoke(this);
        }

        /// <summary> 런타임 string 값을 변경함. </summary>
        public void SetValue(string value, bool withoutNotify = false)
        {
            var isNew = _runtimeValue.StringValue != value;
            _runtimeValue.StringValue = value;
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

#if UNITY_EDITOR
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
