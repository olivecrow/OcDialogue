using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue;
using OcDialogue.DB;
using OcUtility;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace MyDB
{
    public class DynamicDataUser : MonoBehaviour
    {
        [DisableInPrefabs][PropertyOrder(-10)]public DataRow DataRow;

#if UNITY_EDITOR
        [PropertyOrder(-9)][ShowInInspector][ShowIf("@DataRow != null"), HideLabel, HorizontalGroup("row")][DelayedProperty]public string Key
        {
            get => DataRow == null ? "" : DataRow.Name;
            set => DataRow.Name = value;
        }
        [PropertyOrder(-8)][ShowInInspector][ShowIf("@DataRow != null"), HideLabel, HorizontalGroup("row")]public DataRowType Type
        {
            get => DataRow == null ? DataRowType.Bool : DataRow.Type;
            set => DataRow.Type = value;
        }
        [PropertyOrder(-7)][ShowInInspector][ShowIf("@DataRow != null"), HideLabel, HorizontalGroup("row")]public PrimitiveValue Value
        {
            get => DataRow == null ? new PrimitiveValue() : DataRow.RuntimeValue;
        }
        [PropertyOrder(-6)][ShowInInspector][ShowIf("@DataRow != null"), HideLabel][SuffixLabel("description", true)]public string Description
        {
            get => DataRow == null ? "" : DataRow.description;
            set => DataRow.description = value;
        }
#endif
        [TitleGroup("On Start")] [ShowIf(nameof(Type), DataRowType.Bool)]  public UnityEvent<bool> OnStart_Bool;
        [TitleGroup("On Start")] [ShowIf(nameof(Type), DataRowType.Int)]   public UnityEvent<int> OnStart_Int;
        [TitleGroup("On Start")] [ShowIf(nameof(Type), DataRowType.Float)] public UnityEvent<float> OnStart_Float;
        [TitleGroup("On Start")] [ShowIf(nameof(Type), DataRowType.String)]public UnityEvent<string> OnStart_String;

        [TitleGroup("On Value Changed")][ShowIf(nameof(Type), DataRowType.Bool)]  public UnityEvent<bool> OnValueChanged_Bool;
        [TitleGroup("On Value Changed")][ShowIf(nameof(Type), DataRowType.Int)]   public UnityEvent<int> OnValueChanged_Int;
        [TitleGroup("On Value Changed")][ShowIf(nameof(Type), DataRowType.Float)] public UnityEvent<float> OnValueChanged_Float;
        [TitleGroup("On Value Changed")][ShowIf(nameof(Type), DataRowType.String)]public UnityEvent<string> OnValueChanged_String;


        void Start()
        {
            if (DataRow == null)
            {
                Printer.Print($"[DynamicDataUser] DataRow가 없음 : {name}");
                return;
            }
            
            DataRow.OnRuntimeValueChanged += row =>
            {
                switch (row.Type)
                {
                    case DataRowType.Bool:
                        OnValueChanged_Bool.Invoke(row.RuntimeValue.BoolValue);
                        break;
                    case DataRowType.Int:
                        OnValueChanged_Int.Invoke(row.RuntimeValue.IntValue);
                        break;
                    case DataRowType.Float:
                        OnValueChanged_Float.Invoke(row.RuntimeValue.FloatValue);
                        break;
                    case DataRowType.String:
                        OnValueChanged_String.Invoke(row.RuntimeValue.StringValue);
                        break;
                }
            };
            
            switch (DataRow.Type)
            {
                case DataRowType.Bool:
                    OnStart_Bool.Invoke(DataRow.RuntimeValue.BoolValue);
                    break;
                case DataRowType.Int:
                    OnStart_Int.Invoke(DataRow.RuntimeValue.IntValue);
                    break;
                case DataRowType.Float:
                    OnStart_Float.Invoke(DataRow.RuntimeValue.FloatValue);
                    break;
                case DataRowType.String:
                    OnStart_String.Invoke(DataRow.RuntimeValue.StringValue);
                    break;
            }
        }

        public void SetBool(bool value)
        {
            DataRow.SetValue(value);
        }

        public void SetInt(int value)
        {
            DataRow.SetValue(value);
        }

        public void SetFloat(float value)
        {
            DataRow.SetValue(value);
        }

        public void SetString(string value)
        {
            DataRow.SetValue(value);
        }
    }
}
