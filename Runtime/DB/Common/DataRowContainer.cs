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
#if UNITY_EDITOR
        [InlineButton(nameof(RuntimeValuesToPreset), "런타임 값을 프리셋으로")]
        [InlineButton(nameof(EditorPresetToDefault), "프리셋 초기화")]
#endif
        public OcData Parent;
        [TableList(AlwaysExpanded = true, NumberOfItemsPerPage = 50, ShowPaging = true, DrawScrollView = false)]
        public List<DataRow> DataRows;

        [InfoBox("한 데이터가 true가 될때, 상위 인덱스를 전부 true로 만드는 기능\n" +
                 "예를 들어 [0] stage1_clear, [1] stage2_clear 라는 데이터가 있을 경우, \n" +
                 "stage2_clear가 true가 되면 0번의 stage1_clear는 자동으로 true가 됨")]
        public OcDictionary<string, List<DataRow>> HierarchicalData;
        public event Action<DataRow> OnRuntimeValueChanged; 
        public void GenerateRuntimeData()
        {
            foreach (var dataRow in DataRows)
            {
                dataRow.GenerateRuntimeData();
                dataRow.OnRuntimeValueChanged += OnRuntimeValueChanged;
            }

            foreach (var kv in HierarchicalData)
            {
                var list = kv.Value;
                foreach (var data in list) data.OnRuntimeValueChanged += UpdateHierarchicalData;
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
                var data = DataRows.Find(x => string.CompareOrdinal(x.name, kv.Key) == 0);
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
                            data.SetValue(v, DataSetter.Operator.Set, true);
                        }
                        else Debug.LogWarning($"[DataRowContainer] 데이터 형식이 일치하지 않음 | type : {data.Type} | value : {v}");

                        break;
                    }
                    case DataRowType.Int:
                    {
                        if (int.TryParse(kv.Value, out var v))
                        {
                            data.SetValue(v, DataSetter.Operator.Set, true);
                        }
                        else Debug.LogWarning($"[DataRowContainer] 데이터 형식이 일치하지 않음 | type : {data.Type} | value : {v}");

                        break;
                    }
                    case DataRowType.Float:
                    {
                        if (float.TryParse(kv.Value, out var v))
                        {
                            data.SetValue(v, DataSetter.Operator.Set, true);
                        }
                        else Debug.LogWarning($"[DataRowContainer] 데이터 형식이 일치하지 않음 | type : {data.Type} | value : {v}");

                        break;
                    }
                    case DataRowType.String:
                        data.SetValue(kv.Value, DataSetter.Operator.Set, true);
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

        public bool HasKey(string key)
        {
            return DataRows.Any(x => string.CompareOrdinal(x.Name, key) == 0);
        }

        public DataRow Get(string key)
        {
            return DataRows.FirstOrDefault(x => string.CompareOrdinal(x.Name, key) == 0);
        }

        void UpdateHierarchicalData(DataRow dataRow)
        {
            var targetList = HierarchicalData.FirstOrDefault(x => x.Value.Contains(dataRow));
            if (targetList == null)
            {
                Debug.LogWarning($"해당 DataRow가 포함된 HierarchicalData를 찾을 수 없음 | DataRow : {dataRow.name}");
                return;
            }

            var index = targetList.Value.IndexOf(dataRow);
            var validData = targetList.Value.Take(index);
            foreach (var data in validData)
            {
                Debug.Log($"[DataRowContainer] Update Hierarchical Data) {data.name} => true");
                data.SetValue(true);
            }
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
            var key = OcDataUtility.CalculateDataName("New DataRow", DataRows.Select(x => x.name));

            return AddData(key);
        }

        public DataRow AddData(string key)
        {
            AssetDatabase.SaveAssets();
            var row = ScriptableObject.CreateInstance<DataRow>();
            row.SetParent(Parent);
            row.name = key;
            DataRows.Add(row);
            // OcDataUtility.Repaint();

            AssetDatabase.AddObjectToAsset(row, Parent);
            EditorUtility.SetDirty(Parent);
            // EditorApplication.delayCall += AssetDatabase.SaveAssets;

            return row;
        }
        [HorizontalGroup("btn"),Button, GUIColor(1,0,0)]
        public void DeleteRow(string key)
        {
            var row = DataRows.FirstOrDefault(x => string.CompareOrdinal(x.name, key) == 0);
            if (row == null)
            {
                Debug.LogWarning($"해당 키값의 DataRow가 없어서 삭제에 실패함 : {key}");
                return;
            }
            DataRows.Remove(row);

            OcDataUtility.Repaint();

            AssetDatabase.RemoveObjectFromAsset(row);
            EditorUtility.SetDirty(Parent);
            // AssetDatabase.SaveAssets();
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

        
        internal void EditorPresetToDefault()
        {
            foreach (var dataRow in DataRows)
            {
                dataRow.EditorPresetToDefault();
            }
        }

        internal void RuntimeValuesToPreset()
        {
            foreach (var dataRow in DataRows)
            {
                dataRow.RuntimeValueToEditorPresetValue();
            }
        }
#endif
    }
}
