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
    [CreateAssetMenu(fileName = "GameProcessData Preset", menuName = "Oc Dialogue/Editor Preset/GameProcessData Preset")]
    public class GameProcessDataPreset : ScriptableObject
    {
#if UNITY_EDITOR
        /// <summary> Editor Only. </summary>
        public const string AssetPath = "GameProcessData Preset.asset";
        /// <summary> Editor Only. </summary>
        public static GameProcessDataPreset Instance => _instance;
        static GameProcessDataPreset _instance;

        [InitializeOnLoadMethod]
        static void EditorInit()
        {
            _instance = EditorGUIUtility.Load(AssetPath) as GameProcessDataPreset;
        }

        public bool usePreset;
        [TableList]public List<OverrideRow> dataRows;

        public DataRow GetCopy(string key)
        {
            var target = dataRows.Find(x => x.key == key);
            
            var copy = target.row.GetCopy();
            copy.boolValue = target.BoolValue;
            copy.intValue = target.IntValue;
            copy.floatValue = target.FloatValue;
            copy.stringValue = target.StringValue;

            return copy;
        }
        public List<DataRow> GetAllCopies()
        {
            var list = new List<DataRow>();
            foreach (var overrideRow in dataRows)
            {
                var copy = overrideRow.row.GetCopy();
                copy.boolValue = overrideRow.BoolValue;
                copy.intValue = overrideRow.IntValue;
                copy.floatValue = overrideRow.FloatValue;
                copy.stringValue = overrideRow.StringValue;
                
                list.Add(copy);
            }

            return list;
        }

        [Button, PropertyOrder(-100)]
        void ToDefault()
        {
            dataRows = new List<OverrideRow>();
            var copies = GameProcessDatabase.Instance.GetAllCopies();
            foreach (var row in copies)
            {
                var overrideRow = new OverrideRow(row);
                dataRows.Add(overrideRow);
            }
        }

        [Serializable]
        public class OverrideRow
        {
            [HideInInspector]public DataRow row;
            [GUIColor("GetColor")][ShowInInspector, ReadOnly, TableColumnWidth(150, false)]
            public string key => row == null ? "" : row.key;
            [GUIColor("GetColor")][ShowInInspector, ReadOnly, TableColumnWidth(100, false)]
            public DataRow.Type type => row == null ? DataRow.Type.Boolean : row.type;
            
            [GUIColor("GetColor")][VerticalGroup("Value"), PropertyOrder(10), HideLabel][ShowIf("UseBoolValue")][ExplicitToggle()]public bool BoolValue;
            [GUIColor("GetColor")][VerticalGroup("Value"), PropertyOrder(10), HideLabel][ShowIf("UseStringValue")]public string StringValue;
            [GUIColor("GetColor")][VerticalGroup("Value"), PropertyOrder(10), HideLabel][ShowIf("UseIntValue")]public int IntValue;
            [GUIColor("GetColor")][VerticalGroup("Value"), PropertyOrder(10), HideLabel][ShowIf("UseFloatValue")]public float FloatValue;
            
            bool UseBoolValue => row != null && row.type == DataRow.Type.Boolean;
            bool UseStringValue => row != null && row.type == DataRow.Type.String;
            bool UseIntValue => row != null && row.type == DataRow.Type.Int;
            bool UseFloatValue => row != null && row.type == DataRow.Type.Float;

            Color GetColor()
            {
                if (row == null) return Color.white;
                bool isDirty = false;
                switch (type)
                {
                    case DataRow.Type.Boolean:
                        isDirty = row.boolValue != BoolValue;
                        break;
                    case DataRow.Type.Int:
                        isDirty = row.intValue != IntValue;
                        break;
                    case DataRow.Type.Float:
                        isDirty = Math.Abs(row.floatValue - FloatValue) > 0.0001f;
                        break;
                    case DataRow.Type.String:
                        isDirty = row.stringValue != StringValue;
                        break;
                }

                return isDirty ? Color.yellow : Color.white;
            }

            public OverrideRow(DataRow row)
            {
                this.row = row;
                BoolValue = row.boolValue;
                StringValue = row.stringValue;
                IntValue = row.intValue;
                FloatValue = row.floatValue;
            }
        }
#endif
    }
}
