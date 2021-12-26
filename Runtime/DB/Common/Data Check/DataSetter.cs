using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcUtility;
using Sirenix.OdinInspector;
using UnityEngine;

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

        [InfoBox("데이터가 비어있음", InfoMessageType.Error, VisibleIf = "@TargetData == null")]
        [HorizontalGroup("1")]
        [GUIColor(1f, 1f, 1.2f)]
        [InlineButton("OpenSelectWindow", " 선택 ")]
        [LabelText("@e_address")]
        [LabelWidth(180)]
        public OcData TargetData
        {
            get => targetData;
            set => targetData = value;
        }

        public OcData targetData;

        /// <summary> TargetData내에서도 판단의 분류가 나뉘는 경우, 여기에 해당하는 값을 입력해서 그걸 기준으로 어떤 변수를 판단할지 정함. </summary>
        [HorizontalGroup("1", MaxWidth = 200)] [HideLabel] [HideIf("@string.IsNullOrWhiteSpace(Detail)")]
        [ValueDropdown("GetDetail")] [GUIColor(1f,1f,1f)]
        public string detail;
        
        [LabelText("@e_Label"), LabelWidth(250)][GUIColor(1,1,1,2f)]
        [HideLabel][ValueDropdown("GetOperator")][HorizontalGroup("2", MinWidth = 400)] 
        public Operator op;

        [GUIColor(1f,1f,1f)]
        [HorizontalGroup("2"), HideLabel] [ShowIf("GetDataType", DataRowType.Bool)] [ExplicitToggle()]
        public bool BoolValue;
        
        [GUIColor(1f,1f,1f)]
        [HorizontalGroup("2"), HideLabel] [ShowIf("GetDataType", DataRowType.Int)]
        public int IntValue;
        
        [GUIColor(1f,1f,1f)]
        [HorizontalGroup("2"), HideLabel] [ShowIf("GetDataType", DataRowType.Float)]
        public float FloatValue;
        
        [GUIColor(1f,1f,1f)]
        [HorizontalGroup("2"), HideLabel] [ShowIf("GetDataType", DataRowType.String)]
        public string StringValue;

        public object TargetValue()
        {
            // TODO : !!
            return BoolValue;}
        
        /// <summary> Setter를 실행함. </summary>
        public void Execute()
        {
            TargetData.SetData(detail, op, TargetValue());
        }


#if UNITY_EDITOR
        string e_Label => TargetData == null ?
            "" :
            string.IsNullOrWhiteSpace(detail) ? $"< {TargetData.name} > 을(를)" : $"< {TargetData.name} >의 {detail} (을)를";
        [HideInInspector] public string e_address;
        public void UpdateAddress()
        {
            e_address = TargetData == null ? "" : TargetData.TotalAddress.Replace(TargetData.Address, "");
        }
        
        void OpenSelectWindow()
        {
            DataSelectWindow.Open(this);
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
                    break;
            }
        }

        /// <summary> Editor Only. </summary>
        public void SetTargetData(OcData data)
        {
            TargetData = data;

            detail = data.GetFieldNames()[0];
            
            op = data.GetSetterOperator()[0].Value;
        }
        

#endif

    }
}
