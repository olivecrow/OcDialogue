#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcDialogue
{
    [Serializable]
    public class GameProcessDataEditorPreset
    {
        public bool usePreset;
        [TableList] public List<OverridenRow> dataRows;

        [Button, PropertyOrder(-100), HorizontalGroup("Button")]
        void ToDefault()
        {
            dataRows = new List<OverridenRow>();
            foreach (var row in GameProcessDatabase.Instance.DataRowContainer.dataRows)
            {
                var overrideRow = new OverridenRow(row);
                dataRows.Add(overrideRow);
            }
        }

        [Button, PropertyOrder(-100), HorizontalGroup("Button")]
        void Match()
        {
            if (!EditorUtility.DisplayDialog(
                "Match",
                "오버라이드 된 값을 유지한 채, 기존의 데이터베이스의 변경된 요소들을 적용함!",
                "질러!", "안돼!")) return;
            var overrideValues = new Dictionary<DataRow, object>();
            foreach (var overrideRow in dataRows)
            {
                if (overrideRow.row == null) continue;
                if (!overrideRow.IsOverriden()) continue;
                overrideValues[overrideRow.row] = overrideRow.OverridenValue;
            }

            ToDefault();
            foreach (var overrideRow in dataRows)
            {
                if (!overrideValues.ContainsKey(overrideRow.row)) continue;
                switch (overrideRow.type)
                {
                    case DataRow.Type.Boolean:
                        overrideRow.boolValue = (bool) overrideValues[overrideRow.row];
                        break;
                    case DataRow.Type.Int:
                        overrideRow.intValue = (int) overrideValues[overrideRow.row];
                        break;
                    case DataRow.Type.Float:
                        overrideRow.floatValue = (float) overrideValues[overrideRow.row];
                        break;
                    case DataRow.Type.String:
                        overrideRow.stringValue = (string) overrideValues[overrideRow.row];
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
#endif