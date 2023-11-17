using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcUtility;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Random = UnityEngine.Random;

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
        public List<DataRow> DataRows = new List<DataRow>();

        [InfoBox("한 데이터가 true가 될때, 상위 인덱스를 전부 true로 만드는 기능\n" +
                 "예를 들어 [0] stage1_clear, [1] stage2_clear 라는 데이터가 있을 경우, \n" +
                 "stage2_clear가 true가 되면 0번의 stage1_clear는 자동으로 true가 됨")]
        public OcDictionary<string, List<DataRow>> HierarchicalData;
        internal event Action<DataRow> OnRuntimeValueChanged;

        internal void InitFromEditor()
        {
            // 런타임에 생성되었던 데이터들은 null이 되기때문에 일단 없애줌.
            DataRows.RemoveAll(x => x == null);
            foreach (var dataRow in DataRows)
            {
                dataRow.InitFromEditor();
            }
        }

        /// <summary>
        /// 런타임 값을 덮어씌움.
        /// </summary>
        internal void Initialize(List<SavedDataRow> saveData)
        {
            // 런타임에 생성되었던 데이터들은 null이 되기때문에 일단 없애줌.
            DataRows.RemoveAll(x => x == null);
            foreach (var kv in HierarchicalData)
            {
                var list = kv.Value;
                foreach (var data in list) data.OnRuntimeValueChanged += UpdateHierarchicalData;
            }
            
            for (int i = 0; i < saveData.Count; i++)
            {
                var data = saveData[i];
                
                var matchedData = DataRows.Find(x => string.CompareOrdinal(x.name, data.name) == 0);
                matchedData.OnRuntimeValueChanged += OnDataRowValueChanged;
                if (matchedData == null)
                {
                    if (data.isCreatedRuntime)
                    {
                        var added = AddDataRuntime(data.name, data.type, data.id);
                        data.CopyTo(added);
                    }
                    else
                    {
                        Debug.LogError($"{Parent.DRT()}|Overwrite) 해당 키를 가진 데이터를 찾을 수 없음 | key : {data.name} ");
                    }
                    continue;
                }

                data.CopyTo(matchedData);
            }
            
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += ReleaseEvents;
#endif
        }

#if UNITY_EDITOR
        void ReleaseEvents(PlayModeStateChange change)
        {
            if(change != PlayModeStateChange.ExitingPlayMode) return;
            OnRuntimeValueChanged = null;

            foreach (var dataRow in DataRows)
            {
                dataRow.ReleaseEvents();
            }
            
            EditorApplication.playModeStateChanged -= ReleaseEvents;
        }
#endif
        void OnDataRowValueChanged(DataRow row)
        {
            OnRuntimeValueChanged?.Invoke(row);
        }
        
        public List<SavedDataRow> GetSaveData()
        {
            var list = new List<SavedDataRow>();
            for (int i = 0; i < DataRows.Count; i++)
            {
                list.Add(new SavedDataRow(DataRows[i]));
            }

            return list;
        }

        public bool HasKey(string key)
        {
            if (DataRows == null) return false;
            return DataRows.Any(x => string.CompareOrdinal(x.Name, key) == 0);
        }

        [Obsolete("FindData를 대신 사용할 것")]
        public DataRow Get(string key)
        {
            if (DataRows == null) return null;
            return DataRows.FirstOrDefault(x => string.CompareOrdinal(x.Name, key) == 0);
        }

        public DataRow FindData(string key)
        {
            if (DataRows == null) return null;
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

        DataRow AddDataRuntime(string key, DataRowType type, int manualID = 0)
        {
            var data = ScriptableObject.CreateInstance<DataRow>();
            data.OnRuntimeValueChanged += OnDataRowValueChanged;
            data.category = Parent.Category;
            data.isCreatedRuntime = true;
            data.Type = type;
            data.name = key;
            data.SetParent(Parent);
            data.id = manualID == 0 ? Random.Range(int.MinValue, int.MaxValue) : manualID;
            
            DataRows.Add(data);

            return data;
        }
        public void AddDataRuntime(string key, bool value)
        {
            var data = AddDataRuntime(key, DataRowType.Bool);
            data.SetValue(value);
        }
        public void AddDataRuntime(string key, float value)
        {
            var data = AddDataRuntime(key, DataRowType.Float);
            data.SetValue(value);
        }
        public void AddDataRuntime(string key, int value)
        {
            var data = AddDataRuntime(key, DataRowType.Int);
            data.SetValue(value);
        }
        public void AddDataRuntime(string key, string value)
        {
            var data = AddDataRuntime(key, DataRowType.String);
            data.SetValue(value);
        }
        public void AddDataRuntime(string key, Vector4 value)
        {
            var data = AddDataRuntime(key, DataRowType.Vector);
            data.SetValue(value);
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
            row.id = DBManager.GenerateID();
            DataRows.Add(row);

            AssetDatabase.AddObjectToAsset(row, Parent);
            EditorUtility.SetDirty(Parent);

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
