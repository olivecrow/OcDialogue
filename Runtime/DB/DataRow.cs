using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    public class DataRow : ScriptableObject
    {
        public enum Type
        {
            Boolean,
            Int,
            Float,
            String,
        }
        [TableColumnWidth(150, Resizable = false)]public string key;
        [TableColumnWidth(100, Resizable = false)]public Type type;
        [ShowIf("type", Type.Boolean), VerticalGroup("Value"), HideLabel, TableColumnWidth(100, Resizable = false)][ExplicitToggle]public bool boolValue;
        [ShowIf("type", Type.Int),     VerticalGroup("Value"), HideLabel, TableColumnWidth(100, Resizable = false)]public int intValue;
        [ShowIf("type", Type.Float),   VerticalGroup("Value"), HideLabel, TableColumnWidth(100, Resizable = false)]public float floatValue;
        [ShowIf("type", Type.String),  VerticalGroup("Value"), HideLabel, TableColumnWidth(100, Resizable = false)]public string stringValue;

        /// <summary> Editor Only. </summary>
        public string description;

        public DataRow GetCopy()
        {
            var row = CreateInstance<DataRow>();
            row.key = key;
            row.type = type;
            row.boolValue = boolValue;
            row.intValue = intValue;
            row.floatValue = floatValue;
            row.stringValue = stringValue;
            row.name = key;
            
            return row;
        }
    }
}
