using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    [Serializable]
    public class OverridenRow : IComparableData
    {
        [HideInInspector] public DataRow row;

        [GUIColor("GetColor")]
        [ShowInInspector, TableColumnWidth(150, false)]
        public string Key => row == null ? "" : row.key;

        [GUIColor("GetColor")]
        [ShowInInspector, TableColumnWidth(100, false)]
        public DataRow.Type type => row == null ? DataRow.Type.Boolean : row.type;

        [GUIColor("GetColor")]
        [VerticalGroup("Value"), PropertyOrder(10), HideLabel]
        [ShowIf("type", DataRow.Type.Boolean)]
        [ExplicitToggle()]
        public bool boolValue;

        [GUIColor("GetColor")]
        [VerticalGroup("Value"), PropertyOrder(10), HideLabel]
        [ShowIf("type", DataRow.Type.String)]
        public string stringValue;

        [GUIColor("GetColor")] [VerticalGroup("Value"), PropertyOrder(10), HideLabel] [ShowIf("type", DataRow.Type.Int)]
        public int intValue;

        [GUIColor("GetColor")]
        [VerticalGroup("Value"), PropertyOrder(10), HideLabel]
        [ShowIf("type", DataRow.Type.Float)]
        public float floatValue;

        public object OverridenValue
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
        
        public OverridenRow(DataRow row, object value)
        {
            this.row = row;

            switch (row.type)
            {
                case DataRow.Type.Boolean:
                    boolValue = (bool)value;
                    break;
                case DataRow.Type.Int:
                    intValue = (int) value;
                    break;
                case DataRow.Type.Float:
                    floatValue = (float) value;
                    break;
                case DataRow.Type.String:
                    stringValue = (string) value;
                    break;
            }
        }

        public bool IsOverriden()
        {
            bool isOverriden = false;
            switch (type)
            {
                case DataRow.Type.Boolean:
                    isOverriden = row.boolValue != boolValue;
                    break;
                case DataRow.Type.Int:
                    isOverriden = row.intValue != intValue;
                    break;
                case DataRow.Type.Float:
                    isOverriden = Math.Abs(row.floatValue - floatValue) > 0.0001f;
                    break;
                case DataRow.Type.String:
                    isOverriden = row.stringValue != stringValue;
                    break;
            }

            return isOverriden;
        }

        Color GetColor()
        {
            if (row == null) return Color.white;


            return IsOverriden() ? Color.yellow : Color.white;
        }

        public bool IsTrue(Operator op, object value)
        {
            return IsTrue(type.ToCompareFactor(), op, value);
        }
        public bool IsTrue(CompareFactor factor, Operator op, object value)
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

        public DataRow GetAppliedCopy()
        {
            var copy = row.GetCopy();
            copy.boolValue = boolValue;
            copy.intValue = intValue;
            copy.floatValue = floatValue;
            copy.stringValue = stringValue;

            return copy;
        }
    }
}