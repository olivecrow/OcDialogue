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
            Apply(GameProcessDatabase.Instance.DataRowContainer.dataRows);
        }

        [Button, PropertyOrder(-100), HorizontalGroup("Button")]
        void Match()
        {
            if (!EditorUtility.DisplayDialog(
                "Match",
                "오버라이드 된 값을 유지한 채, 기존의 데이터베이스의 변경된 요소들을 적용함!",
                "질러!", "안돼!")) return;
            
            MatchProcess();
        }

        void MatchProcess()
        {
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

        /// <summary> 다른 DataRow목록으로부터 오버라이드를 만들어서 에디터 프리셋에 적용함 </summary>
        public void Apply(IEnumerable<DataRow> data)
        {
            dataRows = new List<OverridenRow>();
            foreach (var row in data)
            {
                var originalRow = GameProcessDatabase.Instance.DataRowContainer.FindData(row.key);
                var overrideRow = new OverridenRow(originalRow, row.TargetValue);
                dataRows.Add(overrideRow);
            }
            EditorUtility.SetDirty(GameProcessDatabase.Instance);
        }

        /// <summary> 에디터 프리셋에서 오버라이드 된 값들로 DataRow 리스트를 만들어 반환함. </summary>
        public List<DataRow> GetAppliedCopies()
        {
            MatchProcess();
            
            var list = new List<DataRow>();
            foreach (var overridenRow in dataRows)
            {
                var copy = overridenRow.GetAppliedCopy();
                list.Add(copy);
            }

            return list;
        }
    }
}
#endif