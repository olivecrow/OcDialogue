using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OcUtility;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Search;

namespace OcDialogue.DB
{
    [Serializable]
    public class DataSetter : IOcDataSelectable
    {
        public enum Operator
        {
            Set,
            Add,
            Multiply,
            Divide
        }

        public string Detail => detail;
        
        public OcData TargetData
        {
            get => targetData;
            set
            {
                targetData = value;
                OnDataChanged();
            }
        }

        [InfoBox("데이터가 비어있음", InfoMessageType.Error, VisibleIf = "@TargetData == null")]
        [HorizontalGroup("1")]
        [GUIColor(1f, 1f, 1.2f)]
        [InlineButton("OpenSelectWindow", " 선택 ")]
        [HideLabel]
        [OnValueChanged("OnDataChanged")]
        [SerializeField] OcData targetData;

        /// <summary> TargetData내에서도 판단의 분류가 나뉘는 경우, 여기에 해당하는 값을 입력해서 그걸 기준으로 어떤 변수를 판단할지 정함. </summary>
        [HorizontalGroup("1", MaxWidth = 200)] [HideLabel] [HideIf("@string.IsNullOrWhiteSpace(Detail)")]
        [ValueDropdown(nameof(GetDetailFieldName))] [GUIColor(1f,1f,1f)]
        public string detail;
        
        [LabelText("@e_Label"), LabelWidth(250)][GUIColor(1,1,1,2f)]
        [HideLabel][ValueDropdown(nameof(GetOperatorDropDown))][HorizontalGroup("2")] [HideIf("isMethod")]
        public Operator op;

        [GUIColor(1f,1f,1f)]
        [HorizontalGroup("2"), HideLabel] [ShowIf("@GetValueType()==DataRowType.Bool&&!isMethod")] [ExplicitToggle()]
        public bool BoolValue;
        
        [GUIColor(1f,1f,1f)]
        [HorizontalGroup("2"), HideLabel] [ShowIf("@GetValueType()==DataRowType.Int&&!isMethod")]
        public int IntValue;
        
        [GUIColor(1f,1f,1f)]
        [HorizontalGroup("2"), HideLabel] [ShowIf("@GetValueType()==DataRowType.Float&&!isMethod")]
        public float FloatValue;
        
        [GUIColor(1f,1f,1f)]
        [HorizontalGroup("2"), HideLabel] 
        [ShowIf("@GetValueType()==DataRowType.String && !IsEnumType()")]
        public string StringValue;
        
        [ShowInInspector][ShowIf(nameof(IsEnumType))][ValueDropdown(nameof(GetEnumDropDown))]
        [HorizontalGroup("2"), HideLabel][GUIColor(1f,1f,1f)]
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
        public OcData parent;

        public bool isMethod
        {
            get
            {
                if (string.IsNullOrWhiteSpace(detail) || targetData.GetFieldNames().Contains(detail)) return false;
                var methods = targetData.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance| BindingFlags.DeclaredOnly | BindingFlags.InvokeMethod);


                var method = methods.FirstOrDefault(x => x.Name == detail);
                if (method == null) return false;
                return true;
            }
        }
        
        public object TargetValue => GetValueType() switch
        {
            DataRowType.Bool => BoolValue,
            DataRowType.Int => IntValue,
            DataRowType.Float => FloatValue,
            DataRowType.String => StringValue,
            _ => null
        };
        
        /// <summary> Setter를 실행함. </summary>
        public void Execute()
        {
            if (TargetData == null)
            {
                Debug.LogWarning($"TargetData가 비어있어서 DataSetter.Execute를 실행할 수 없음");
                return;
            }
            if(string.IsNullOrWhiteSpace(detail) || targetData.GetFieldNames().Contains(detail))
                TargetData.SetValue(detail, op, TargetValue);
            else
            {
                
                var methods = targetData.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance| BindingFlags.DeclaredOnly | BindingFlags.InvokeMethod);


                var method = methods.FirstOrDefault(x => x.Name == detail);
                if (method == null)
                {
                    Debug.LogError($"[DataSetter] {targetData.name}에 유효한 메서드 이름, 혹은 fieldName이 발견되지 않음 | detail : {detail}");
                }
                else
                {
                    if (method.IsStatic) method.Invoke(null, null);
                    else method.Invoke(targetData, null);
                }
            }
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
                    list.Add(Operator.Set);
                    break;
                case DataRowType.Int:
                case DataRowType.Float:
                    list.Add(Operator.Set);
                    list.Add(Operator.Add);
                    list.Add(Operator.Multiply);
                    list.Add(Operator.Divide);
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

            var methods = targetData.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.InvokeMethod);

            foreach (var method in methods)
            {
                if(method.IsAbstract || method.IsVirtual) continue;
                list.Add($"{method.Name}( )", method.Name);
            }
            return list;
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

        void OnDataChanged()
        {
            if(targetData != null)
            {
                var fieldNames = targetData.GetFieldNames();
                detail = fieldNames == null || fieldNames.Length == 0 ? "" : fieldNames[0];

                op = Operator.Set;
            }
            else
            {
                detail = null;
            }
        }

#if UNITY_EDITOR
        string e_Label => TargetData == null ?
            "" :
            string.IsNullOrWhiteSpace(detail) ? $"< {TargetData.name} > 을(를)" : $"< {TargetData.name} >의 {detail} (을)를";
        [HideInInspector] public string e_address;
        public void OnDataApplied()
        {
            e_address = TargetData == null ? "" : TargetData.TotalAddress.Replace(TargetData.Address, "");
        }
        
        void OpenSelectWindow()
        {
            DataSelectWindow.Open(this, parent);
        }

        public void SetTargetValue(object value)
        {
            switch (value)
            {
                case bool b:
                    BoolValue = b;
                    break;
                case int i:
                    IntValue = i;
                    break;
                case float f:
                    FloatValue = f;
                    break;
                case string s:
                    StringValue = s;
                    break;
                case Enum e:
                    var names = Enum.GetNames(e.GetType()).ToList();
                    IntValue = names.FindIndex(x => x == e.ToString());
                    StringValue = names[IntValue];
                    break;
            }
        }

#endif

    }
}
