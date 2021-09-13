using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcUtility;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.DB
{
    [Serializable]
    public class DataRowContainer
    {
        [InlineButton(nameof(RuntimeValuesToPreset), "런타임 값을 프리셋으로")]
        [InlineButton(nameof(EditorPresetToDefault), "프리셋 초기화")]public OcData Parent;
        [TableList(IsReadOnly = true)]public List<DataRow> DataRows;
        public event Action<DataRow> OnRuntimeValueChanged; 
        public void GenerateRuntimeData()
        {
            foreach (var dataRow in DataRows)
            {
                dataRow.GenerateRuntimeData();
                dataRow.OnRuntimeValueChanged += OnRuntimeValueChanged;
            }

#if UNITY_EDITOR
            Application.quitting += () => OnRuntimeValueChanged = null;      
#endif
        }

        /// <summary>
        /// 런타임 값을 덮어씌움.
        /// </summary>
        /// <param name="dict"></param>
        public void Overwrite(Dictionary<string, string> dict)
        {
            foreach (var kv in dict)
            {
                var data = DataRows.Find(x => x.name == kv.Key);
                if (data == null)
                {
                    Printer.Print($"[DataRowContainer]Overwrite) 해당 키를 가진 데이터를 찾을 수 없음 | key : {kv.Key}");
                    continue;
                }

                switch (data.Type)
                {
                    case DataRowType.Bool:
                    {
                        if (bool.TryParse(kv.Value, out var v))
                        {
                            data.SetValue(v, true);
                        }
                        else Debug.LogWarning($"[DataRowContainer] 데이터 형식이 일치하지 않음 | type : {data.Type} | value : {v}");

                        break;
                    }
                    case DataRowType.Int:
                    {
                        if (int.TryParse(kv.Value, out var v))
                        {
                            data.SetValue(v, true);
                        }
                        else Debug.LogWarning($"[DataRowContainer] 데이터 형식이 일치하지 않음 | type : {data.Type} | value : {v}");

                        break;
                    }
                    case DataRowType.Float:
                    {
                        if (float.TryParse(kv.Value, out var v))
                        {
                            data.SetValue(v, true);
                        }
                        else Debug.LogWarning($"[DataRowContainer] 데이터 형식이 일치하지 않음 | type : {data.Type} | value : {v}");

                        break;
                    }
                    case DataRowType.String:
                        data.SetValue(kv.Value, true);
                        break;
                }
            }
        }

        public Dictionary<string, string> GetSaveData()
        {
            var dict = new Dictionary<string, string>();
            foreach (var dataRow in DataRows)
            {
                dict[dataRow.Name] = dataRow.TargetValue.ToString();
            }

            return dict;
        }


        
        #if UNITY_EDITOR

        public void LoadFromEditorPreset()
        {
            foreach (var dataRow in DataRows)
            {
                dataRow.LoadFromEditorPreset();
            }
        }

        [HorizontalGroup("btn"),Button("Add Row"), GUIColor(0,1,1)]
        void Btn_AddData()
        {
            AddData();
        }

        public DataRow AddData()
        {
            AssetDatabase.SaveAssets();
            var row = ScriptableObject.CreateInstance<DataRow>();
            row.SetParent(Parent);
            row.name = OcDataUtility.CalculateDataName("New DataRow", DataRows.Select(x => x.name));
            DataRows.Add(row);
            OcDataUtility.Repaint();

            AssetDatabase.AddObjectToAsset(row, Parent);
            EditorUtility.SetDirty(Parent);
            EditorApplication.delayCall += AssetDatabase.SaveAssets;

            return row;
        }
        [HorizontalGroup("btn"),Button, GUIColor(1,0,0)]
        public void DeleteRow(string key)
        {
            var row = DataRows.FirstOrDefault(x => x.name == key);
            if (row == null)
            {
                Debug.LogWarning($"해당 키값의 DataRow가 없어서 삭제에 실패함 : {key}");
                return;
            }
            DataRows.Remove(row);

            OcDataUtility.Repaint();

            AssetDatabase.RemoveObjectFromAsset(row);
            EditorUtility.SetDirty(Parent);
            AssetDatabase.SaveAssets();
        }
        [HorizontalGroup("btn"),Button]
        public void MatchParent()
        {
            foreach (var dataRow in DataRows)
            {
                if(dataRow.Parent != Parent)
                {
                    Printer.Print($"[DataRowContainer]MatchParent) 다음 데이터가 잘못된 Parent값을 갖고 있어서 변경 => {dataRow.name}");
                    dataRow.SetParent(Parent);
                    EditorUtility.SetDirty(dataRow);
                }
            }
        }

        
        public void EditorPresetToDefault()
        {
            foreach (var dataRow in DataRows)
            {
                dataRow.EditorPresetToDefault();
            }
        }

        public void RuntimeValuesToPreset()
        {
            foreach (var dataRow in DataRows)
            {
                dataRow.RuntimeValueToEditorPresetValue();
            }
        }
#endif
    }
}
