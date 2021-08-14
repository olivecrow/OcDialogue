using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

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
        
        public bool IsTrue(Operator op, object value)
        {
            return IsTrue(type.ToCompareFactor(), op, value);
        }
        
        public override bool IsTrue(CompareFactor factor, Operator op, object value)
        {
            return factor switch
            {
                CompareFactor.Float => floatValue.IsTrue(op, (float)value),
                CompareFactor.Int => intValue.IsTrue(op, (int)value),
                CompareFactor.String => stringValue.IsTrue(op, (string)value),
                CompareFactor.Boolean => boolValue.IsTrue(op, (bool)value),
                _ => false
            };
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
