using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue.DB;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    public abstract class OcData : ScriptableObject
    {
        public virtual OcData Parent => parent;
        public abstract string Address { get; }
        public string TotalAddress => $"{(Parent == null ? "" : Parent.TotalAddress + "/")}{Address}";
        public OcData parent;

        public abstract bool IsTrue(string fieldName, CheckFactor.Operator op, object checkValue);
        public abstract object GetValue(string fieldName);
        public abstract string[] GetFieldNames();
        public abstract void SetData(string fieldName, DataSetter.Operator op, object value);
        public abstract ValueDropdownList<CheckFactor.Operator> GetCheckerOperator();
        public abstract ValueDropdownList<DataSetter.Operator> GetSetterOperator();

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

        protected static class Operator
        {
            public static ValueDropdownList<CheckFactor.Operator> CheckerAll
            {
                get
                {
                    var list = new ValueDropdownList<CheckFactor.Operator>();
                    var enumNames = Enum.GetNames(typeof(CheckFactor.Operator));
                    for (int i = 0; i < enumNames.Length; i++)
                    {
                        list.Add((CheckFactor.Operator)Enum.Parse(typeof(CheckFactor.Operator), enumNames[i]));
                    }

                    return list;
                }
            }
            public static ValueDropdownList<CheckFactor.Operator> CheckerBool
            {
                get
                {
                    var list = new ValueDropdownList<CheckFactor.Operator>();
                    list.Add(CheckFactor.Operator.Equal);
                    list.Add(CheckFactor.Operator.NotEqual);

                    return list;
                }
            }
            public static ValueDropdownList<DataSetter.Operator> SetterAll
            {
                get
                {
                    var list = new ValueDropdownList<DataSetter.Operator>();
                    var enumNames = Enum.GetNames(typeof(DataSetter.Operator));
                    for (int i = 0; i < enumNames.Length; i++)
                    {
                        list.Add((DataSetter.Operator)Enum.Parse(typeof(DataSetter.Operator), enumNames[i]));
                    }

                    return list;
                }
            }
            public static ValueDropdownList<DataSetter.Operator> SetterBool
            {
                get
                {
                    var list = new ValueDropdownList<DataSetter.Operator>();
                    list.Add(DataSetter.Operator.Set);

                    return list;
                }
            }
        } 

#if UNITY_EDITOR
        /// <summary> Editor Only. </summary>
        public void SetParent(OcData parent)
        {
            this.parent = parent;
        }
#endif
    }
}
