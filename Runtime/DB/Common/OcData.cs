using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OcDialogue.DB;
using Sirenix.OdinInspector;
using UnityEngine;

[assembly:InternalsVisibleTo("OcDialogue.Editor.Tests")]
namespace OcDialogue
{
    public abstract class OcData : ScriptableObject
    {
        public virtual OcData Parent => parent;
        public abstract string Address { get; }
        public abstract string Category { get; set; }
        
        [ReadOnly][HideInTables]
        public int id;
        public string TotalAddress => $"{(Parent == null ? "" : Parent.TotalAddress + "/")}{Address}";
        [HideInTables]
        public OcData parent;
        
        public abstract bool IsTrue(string fieldName, CheckFactor.Operator op, object checkValue);
        public abstract object GetValue(string fieldName);
        public abstract string[] GetFieldNames();
        public abstract void SetValue(string fieldName, DataSetter.Operator op, object value);
        public abstract DataRowType? GetValueType(string fieldName);
        public abstract event Action changed;
        
        public static bool IsTrue(bool a, CheckFactor.Operator op, bool b)
        {
            var ia = a == true ? 1 : 0;
            var ib = b == true ? 1 : 0;
            return op switch
            {
                CheckFactor.Operator.Equal => a == b,
                CheckFactor.Operator.NotEqual => a != b,
                CheckFactor.Operator.Greater => ia > ib,
                CheckFactor.Operator.GreaterEqual => ia >= ib,
                CheckFactor.Operator.Less => ia < ib,
                CheckFactor.Operator.LessEqual => ia <= ib,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        public static bool IsTrue(int a, CheckFactor.Operator op, int b)
        {
            return op switch
            {
                CheckFactor.Operator.Equal => a == b,
                CheckFactor.Operator.NotEqual => a != b,
                CheckFactor.Operator.Greater => a > b,
                CheckFactor.Operator.GreaterEqual => a >= b,
                CheckFactor.Operator.Less => a < b,
                CheckFactor.Operator.LessEqual => a <= b,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        public static bool IsTrue(float a, CheckFactor.Operator op, float b)
        {
            return op switch
            {
                CheckFactor.Operator.Equal => Math.Abs(a - b) < 0.0005f,
                CheckFactor.Operator.NotEqual => Math.Abs(a - b) > 0.0005f,
                CheckFactor.Operator.Greater => a > b,
                CheckFactor.Operator.GreaterEqual => a >= b,
                CheckFactor.Operator.Less => a < b,
                CheckFactor.Operator.LessEqual => a <= b,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        

        public void SetParent(OcData parent)
        {
            this.parent = parent;
        }
    }
}
