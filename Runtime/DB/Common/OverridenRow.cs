using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    [Serializable]
    public class OverridenRow
    {
        [HideInInspector] public DataRow row;

        [GUIColor("GetColor")]
        [ShowInInspector, TableColumnWidth(150, false)]
        public string key => row == null ? "" : row.key;

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

        [GUIColor("GetColor")] [VerticalGroup("Override"), PropertyOrder(10), HideLabel] [ShowIf("type", DataRow.Type.Int)]
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
        
        public OverridenRow(DataRow row)
        {
            this.row = row;
            boolValue = row.boolValue;
            stringValue = row.stringValue;
            intValue = row.intValue;
            floatValue = row.floatValue;
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
    }
}