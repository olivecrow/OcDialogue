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
        [GUIColor("e_keyColor")][TableColumnWidth(150, Resizable = false)]public string key;
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
                    Type.Boolean => boolValue,
                    Type.Int => intValue,
                    Type.Float => floatValue,
                    Type.String => stringValue,
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

#if UNITY_EDITOR
        Color e_keyColor = Color.white;
        /// <summary> Editor Only. 이름과 키값을 매칭해서 맞지 않으면 guiColor를 바꿈. </summary>
        public void CheckName()
        {
            if (name != key) e_keyColor = Color.magenta;
            else e_keyColor = Color.white;
        }
#endif
    }
}
