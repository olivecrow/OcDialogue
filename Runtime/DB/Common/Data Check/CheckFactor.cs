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
using Object = UnityEngine.Object;

namespace OcDialogue.DB
{
    public interface ICheckable
    {
        public int Index { get;}
        public bool IsTrue();
#if UNITY_EDITOR
        public string ToExpression(bool useRichText);  
#endif
    }
    [Serializable]
    public class CheckFactor : IOcDataSelectable, ICheckable
    {
        public enum Operator
        {
            Equal,
            NotEqual,
            Greater,
            GreaterEqual,
            Less,
            LessEqual
        }


        public int Index
        {
            get => index;
            set => index = value;
        }
        [TableColumnWidth(35, false), ReadOnly]
        public int index;

        public OcData TargetData
        {
            get => targetData;
            set
            {
                targetData = value;
                if (targetData != null)
                {
                    var fieldNames = targetData.GetFieldNames();
                    if (fieldNames != null && fieldNames.Length > 0)
                    {
                        detail = fieldNames[0];
                    }
                    else detail = "";
                }
                else detail = "";
            }
        }
        [OnValueChanged(nameof(OnTargetDataChanged))][SuffixLabel("@GetValueType()")]
        [SerializeField][HideLabel]
        OcData targetData;
        [ValueDropdown(nameof(GetDetailFieldName))][TableColumnWidth(120, false)]
        public string detail;
        [TableColumnWidth(100, false)][ValueDropdown(nameof(GetOperatorDropDown))]
        public Operator op;
        [ShowIf(nameof(GetValueType), DataRowType.Bool)][VerticalGroup("value")][HideLabel][ExplicitToggle()]
        public bool BoolValue;
        [ShowIf(nameof(GetValueType), DataRowType.Int)][VerticalGroup("value")][HideLabel]
        public int IntValue;
        [ShowIf(nameof(GetValueType), DataRowType.Float)][VerticalGroup("value")][HideLabel]
        public float FloatValue;
        [ShowIf("@GetValueType()==DataRowType.String && !IsEnumType()")][VerticalGroup("value")][HideLabel]
        public string StringValue;

        [ShowInInspector][ShowIf(nameof(IsEnumType))][ValueDropdown(nameof(GetEnumDropDown))]
        [VerticalGroup("value")][HideLabel]
        public string EnmValue
        {
            get => StringValue;
            set
            {
                IntValue = (targetData as IEnumHandler).GetEnumNames(detail).ToString().IndexOf(value);
                StringValue = value;
            }
        }
        
        [HideInInspector]
        public string address;

        public string Detail
        {
            get => detail;
            set => detail = value;
        }

        public void OnDataApplied()
        {
            address = TargetData == null ? "" : TargetData.TotalAddress;

        }

        public object TargetValue => GetValueType() switch
        {
            DataRowType.Bool => BoolValue,
            DataRowType.Int => IntValue,
            DataRowType.Float => FloatValue,
            DataRowType.String => StringValue,
            _ => null
        };

        public bool IsTrue()
        {
            return TargetData.IsTrue(detail, op, TargetValue);
        }

        void OnTargetDataChanged()
        {
            if (targetData == null || 
                targetData.GetFieldNames() == null || targetData.GetFieldNames().Length == 0)
            {
                detail = "";
                return;
            }

            detail = targetData.GetFieldNames()[0];
        }
        DataRowType GetValueType()
        {
            if (targetData == null) return DataRowType.Bool;
            return targetData.GetValueType(detail);
        }

        ValueDropdownList<Operator> GetOperatorDropDown()
        {
            var list = new ValueDropdownList<Operator>();
            if (targetData == null) return list;
            switch (targetData.GetValueType(detail))
            {
                case DataRowType.Bool:
                case DataRowType.String:
                    list.Add(Operator.Equal);
                    list.Add(Operator.NotEqual);
                    break;
                case DataRowType.Int:
                case DataRowType.Float:
                    list.Add(Operator.Equal);
                    list.Add(Operator.NotEqual);
                    list.Add(Operator.Greater);
                    list.Add(Operator.Less);
                    list.Add(Operator.GreaterEqual);
                    list.Add(Operator.LessEqual);
                    break;
            }

            return list;
        }

        ValueDropdownList<string> GetDetailFieldName()
        {
            var list = new ValueDropdownList<string>();
            if (targetData == null || targetData.GetFieldNames() == null) return list;
            foreach (var fieldName in targetData.GetFieldNames())
            {
                list.Add(fieldName);
            }

            return list;
        }

        void PrintAddress()
        {
            Debug.Log(address);
        }

        bool IsEnumType()
        {
            if (targetData == null) return false;
            return targetData is IEnumHandler && targetData.GetValueType(detail) == DataRowType.String;
        }

        ValueDropdownList<string> GetEnumDropDown()
        {
            var list = new ValueDropdownList<string>();
            if (targetData == null) return list;
#if UNITY_2021_1_OR_NEWER
            if (targetData is not IEnumHandler eHandler) return list;
#else
            if (!(targetData is IEnumHandler eHandler)) return list;
#endif
            foreach (var enumName in eHandler.GetEnumNames(detail))
            {
                list.Add(enumName);
            }

            return list;
        }

#if UNITY_EDITOR
        [Button("선택"), GUIColor(1, 1, 2)][TableColumnWidth(50, false)][VerticalGroup("_")]
        void OpenSelectWindow()
        {
            DataSelectWindow.Open(this);
        }

        
        public string ToExpression(bool useRichText = false)
        {
            if (TargetData == null) return "";
            var addDetail = string.IsNullOrWhiteSpace(detail) ? "" : $".{detail}";
            return useRichText ? 
                $"{TargetData.Address.DRT(TargetData)}{addDetail.Rich(Color.cyan)} " +
                $"{op.ToOperationString()} {TargetValue.ToString().Rich(new Color(1f, 1f, 0.7f))}" :
                $"{TargetData.Address}{addDetail} {op.ToOperationString()} {TargetValue}";
        }

        void PrintResult()
        {
            var result = IsTrue() ? "True".Rich(Color.green) : "False".Rich(Color.red);
            var prefix = Application.isPlaying
                ? "(Runtime)".Rich(Color.cyan) 
                : "(Editor)".Rich(Color.yellow);
            Printer.Print($"[DataChecker] {prefix} {ToExpression()} ? => {result}");
        }
#endif
    }
}
