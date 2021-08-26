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
            get => _type;
            set
            {
                var isNew = _type != value;
                _type = value;
                if (isNew)
                {
                    _initialValue.Type = Type;
#if UNITY_EDITOR
                    _editorPresetValue.Type = Type;              
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

        [HideInInspector][SerializeField]
        DataRowType _type;

        [SerializeField] [DisableIf("@UnityEditor.EditorApplication.isPlaying")] [TableColumnWidth(110, false)]
        [PropertyOrder(2)]
        PrimitiveValue _initialValue;

        PrimitiveValue _runtimeValue;

        public void GenerateRuntimeData()
        {
            RuntimeValue = new PrimitiveValue()
            {
                Type = Type,
                BoolValue = InitialValue.BoolValue,
                IntValue = InitialValue.IntValue,
                FloatValue = InitialValue.FloatValue,
                StringValue = InitialValue.StringValue,
            };
        }

        /// <summary> 런타임 bool 값을 변경함. </summary>
        public void SetValue(bool value)
        {
            _runtimeValue.BoolValue = value;
        }

        /// <summary> 런타임 int 값을 변경함. </summary>
        public void SetValue(int value)
        {
            _runtimeValue.IntValue = value;
        }

        /// <summary> 런타임 float 값을 변경함. </summary>
        public void SetValue(float value)
        {
            _runtimeValue.FloatValue = value;
        }

        /// <summary> 런타임 string 값을 변경함. </summary>
        public void SetValue(string value)
        {
            _runtimeValue.StringValue = value;
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
        public void EditorPresetToDefault()
        {
            _editorPresetValue.BoolValue = _initialValue.BoolValue;
            _editorPresetValue.IntValue = _initialValue.IntValue;
            _editorPresetValue.FloatValue = _initialValue.FloatValue;
            _editorPresetValue.StringValue = _initialValue.StringValue;
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
