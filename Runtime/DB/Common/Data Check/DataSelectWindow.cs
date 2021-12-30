#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using OcUtility;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.DB
{
    public class DataSelectWindow : OdinEditorWindow
    {
        [HideInInspector]
        public IOcDataSelectable DataSelectable;
        [HideInInspector]
        public Action<OcData> OnDataSelected;

        [HideLabel]
        public StandardDataSelector Selector;

        string[] _DBNames;

        public OcDB CurrentDB
        {
            get => _currentDB;
            set
            {
                var isNew = _currentDB != value;
                _currentDB = value;
                if (isNew)
                {
                    Selector = new StandardDataSelector(value);
                    _currentDBIndex = _DBNames.ToList().IndexOf(value.name);
                }
            }
        }
        public bool DBRestriction { get; set; }
        OcDB _currentDB;
        OcData _caller;
        int _currentDBIndex;
        bool _useDataSelector;
        public static DataSelectWindow Open(IOcDataSelectable dataSelectable, OcData caller)
        {
            var wnd = GetWindow<DataSelectWindow>(true);
            wnd.minSize = new Vector2(700, 300);
            wnd.maxSize = new Vector2(700, 300);
            wnd.DataSelectable = dataSelectable;
            wnd._caller = caller;
            return wnd;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _DBNames = DBManager.Instance.DBs.Select(x => x.name).ToArray();
        }

        protected override void OnGUI()
        {
            if (DBRestriction) GUI.enabled = false;

            GUI.color = new Color(0.8f, 1.2f, 2f);
            EditorGUILayout.BeginHorizontal();
            var originalColor = GUI.color;
            for (int i = 0; i < _DBNames.Length; i++)
            {
                var content = _DBNames[i];
                if (i == _currentDBIndex) GUI.color = originalColor.SetA(0.5f);
                else GUI.color = originalColor;
                if (GUILayout.Button(content, GUILayout.Height(30)))
                {
                    _currentDBIndex = i;
                }
            }
            GUI.color = originalColor;
            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;
            
            CurrentDB = DBManager.Instance.DBs[_currentDBIndex];
            GUI.enabled = true;
            base.OnGUI();
        }

        [Button]
        void Apply()
        {
            if(Selector != null && Selector.ValidData != null)
            {
                if(_caller != null) Undo.RecordObject(_caller, "DataSelectWindow Apply");
                if(DataSelectable != null)
                {
                    DataSelectable.TargetData = Selector.ValidData;
                    DataSelectable.UpdateExpression();
                }
                OnDataSelected?.Invoke(Selector.ValidData);
                Close();
            }
        }
    }

    [Serializable]
    public class DataRowSelector
    {
        [ValueDropdown(nameof(GetDataRowList))]public DataRow DataRow;

        IEnumerable<DataRow> _allRow;
        public DataRowSelector(IEnumerable<DataRow> dataRows)
        {
            _allRow = dataRows;
        }

        ValueDropdownList<DataRow> GetDataRowList()
        {
            if (_allRow == null) return null;
            var list = new ValueDropdownList<DataRow>();
            foreach (var data in _allRow)
            {
                list.Add(data.name, data);
            }

            return list;
        }
    }

    [Serializable]
    public class StandardDataSelector
    {
        public enum DataSelectionType
        {
            Target,
            Target_DataRow
        }
        [ValueDropdown(nameof(GetCategory))]public string Category;
        [EnumToggleButtons]public DataSelectionType dataSelectionType;
        [ValueDropdown(nameof(GetData))][OnValueChanged(nameof(UpdateDataContainer))]
        [SerializeField]
        OcData Data;
        [ShowIf(nameof(dataSelectionType), DataSelectionType.Target_DataRow)]
        [SerializeField]
        DataRowSelector DataRowSelector;
        
        public OcData ValidData => dataSelectionType switch
        {
            DataSelectionType.Target => Data,
            DataSelectionType.Target_DataRow => DataRowSelector.DataRow,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        OcDB _DB;
        public StandardDataSelector(OcDB db)
        {
            _DB = db;
            Category = db.CategoryOverride[0];
        }

        ValueDropdownList<string> GetCategory()
        {
            if (_DB == null) return null;
            var list = new ValueDropdownList<string>();
            foreach (var category in _DB.CategoryOverride)
            {
                list.Add(category);
            }

            return list;
        }
        ValueDropdownList<OcData> GetData()
        {
            if (_DB == null) return null;
            var list = new ValueDropdownList<OcData>();
            foreach (var data in _DB.AllData)
            {
                if(data.Address.Contains(Category)) list.Add(data);
            }

            return list;
        }

        void UpdateDataContainer()
        {
#if UNITY_2021_1_OR_NEWER
            if(Data is not IDataRowUser user) return;
#else
            if(!(Data is IDataRowUser user)) return;
#endif
            
            DataRowSelector = new DataRowSelector(user.DataRowContainer.DataRows);
        }
    }
}
#endif