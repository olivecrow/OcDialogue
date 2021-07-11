using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    public class DataRow : ComparableData
    {
        public enum Type
        {
            Boolean,
            Int,
            Float,
            String,
        }

        [HideInInspector]public DBType ownerDB;
        public override string Key => key;
        [TableColumnWidth(150, Resizable = false)]public string key;
        [TableColumnWidth(100, Resizable = false)]public Type type;
        [ShowIf("type", Type.Boolean), VerticalGroup("Value"), HideLabel, TableColumnWidth(100, Resizable = false)][ExplicitToggle]public bool boolValue;
        [ShowIf("type", Type.Int),     VerticalGroup("Value"), HideLabel, TableColumnWidth(100, Resizable = false)]public int intValue;
        [ShowIf("type", Type.Float),   VerticalGroup("Value"), HideLabel, TableColumnWidth(100, Resizable = false)]public float floatValue;
        [ShowIf("type", Type.String),  VerticalGroup("Value"), HideLabel, TableColumnWidth(100, Resizable = false)]public string stringValue;

        public object TargetValue
        {
            get
            {
                return type switch
                {
                    DataRow.Type.Boolean => boolValue,
                    DataRow.Type.Int => intValue,
                    DataRow.Type.Float => floatValue,
                    DataRow.Type.String => stringValue,
                    _ => 0
                };
            }
        }
        
        /// <summary> Editor Only. </summary>
        public string description;

        public DataRow GetCopy()
        {
            var row = CreateInstance<DataRow>();
            row.ownerDB = ownerDB;
            row.key = key;
            row.type = type;
            row.boolValue = boolValue;
            row.intValue = intValue;
            row.floatValue = floatValue;
            row.stringValue = stringValue;
            row.name = key;
            
            return row;
        }
        
        public override bool IsTrue(CompareFactor factor, Operator op, object value)
        {
            switch (factor)
            {
                case CompareFactor.Float:
                    return op switch
                    {
                        Operator.Equal        => Math.Abs((float) value - floatValue) < 0.0001f,
                        Operator.NotEqual     => Math.Abs((float) value - floatValue) > 0.0001f,
                        Operator.Greater      => floatValue > (float) value,
                        Operator.GreaterEqual => floatValue >= (float) value,
                        Operator.Less         => floatValue < (float) value,
                        Operator.LessEqual    => floatValue <= (float) value,
                        _ => false
                    };
                case CompareFactor.Int:
                    return op switch
                    {
                        Operator.Equal        => intValue == (int) value,
                        Operator.NotEqual     => intValue != (int) value,
                        Operator.Greater      => intValue >  (int) value,
                        Operator.GreaterEqual => intValue >= (int) value,
                        Operator.Less         => intValue <  (int) value,
                        Operator.LessEqual    => intValue <= (int) value,
                        _ => false
                    };
                case CompareFactor.String:
                    return op switch
                    {
                        Operator.Equal    => stringValue == (string) value,
                        Operator.NotEqual => stringValue != (string) value,
                        _ => false
                    };
                case CompareFactor.Boolean:
                    return op switch
                    {
                        Operator.Equal    => boolValue == (bool) value,
                        Operator.NotEqual => boolValue != (bool) value,
                        _ => false
                    };
            }

            return false;
        }

    }
}
