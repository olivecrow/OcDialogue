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
        public int index;
        public OcData TargetData { get; set; }
        
        public string detail;
         
        public Operator op;

        public bool BoolValue;

        public int IntValue;
        
        public float FloatValue;

        public string StringValue;
        

        public string Detail
        {
            get => detail;
            set => detail = value;
        }

        public void UpdateAddress()
        {
            throw new NotImplementedException();
        }

        public object TargetValue => TargetData.GetValue(detail);

        public bool IsTrue()
        {
            return TargetData.IsTrue(detail, op, TargetValue);
        }

#if UNITY_EDITOR
        
        void OpenSelectWindow()
        {
            DataSelectWindow.Open(this);
        }

        
        public string ToExpression(bool useRichText = false)
        {
            if (TargetData == null) return "";
            var addDetail = string.IsNullOrWhiteSpace(detail) ? "" : $".{detail}";
            return useRichText ? 
                $"{TargetData.Address.ToRichText(ColorExtension.Random(TargetData.GetHashCode()))}{addDetail.ToRichText(Color.cyan)} " +
                $"{op.ToOperationString()} {TargetValue.ToString().ToRichText(new Color(1f, 1f, 0.7f))}" :
                $"{TargetData.Address}{addDetail} {op.ToOperationString()} {TargetValue}";
        }

        void PrintResult()
        {
            var result = IsTrue() ? "True".ToRichText(Color.green) : "False".ToRichText(Color.red);
            var prefix = Application.isPlaying
                ? "(Runtime)".ToRichText(Color.cyan) 
                : "(Editor)".ToRichText(Color.yellow);
            Printer.Print($"[DataChecker] {prefix} {ToExpression()} ? => {result}");
        }
#endif
    }
}
