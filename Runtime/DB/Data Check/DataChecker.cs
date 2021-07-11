using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if UNITY_EDITOR
using OcDialogue.Editor;
#endif
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    [Serializable]
    public class DataChecker
    {
        public DataRow targetRow;
        public Operator op;
        [ShowIf("UseBoolValue")]public bool BoolValue;
        [ShowIf("UseStringValue")]public string StringValue;
        [ShowIf("UseIntValue")]public int IntValue;
        [ShowIf("UseFloatValue")]public float FloatValue;

        public object TargetValue
        {
            get
            {
                return targetRow.type switch
                {
                    DataRow.Type.Boolean => BoolValue,
                    DataRow.Type.Int => IntValue,
                    DataRow.Type.Float => FloatValue,
                    DataRow.Type.String => StringValue,
                    _ => 0
                };
            }
        }
        
#if UNITY_EDITOR
        bool UseBoolValue => targetRow != null && targetRow.type == DataRow.Type.Boolean;
        bool UseStringValue => targetRow != null && targetRow.type == DataRow.Type.String;
        bool UseIntValue => targetRow != null && targetRow.type == DataRow.Type.Int;
        bool UseFloatValue => targetRow != null && targetRow.type == DataRow.Type.Float;

        [Button("결과 출력")]
        void Check()
        {
            Debug.Log($"{targetRow.key} {op.ToOperationString()} {TargetValue} ? " +
                      $"=> {targetRow.IsTrue(targetRow.type.ToCompareFactor(), op, TargetValue)}");
        }
#endif
    }
}
