#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.DB
{
    public class DataSelectWindow : OdinEditorWindow
    {
        public IOcDataSelectable DataSelectable;
        public Action<OcData> OnDataSelected;
        public StandardDataSelector Selector { get; private set; }
        string[] _DBNames;

        OcDB CurrentSelectedDB
        {
            get => _currentSelectedDB;
            set
            {
                var isNew = _currentSelectedDB != value;
                _currentSelectedDB = value;
                if (isNew) Selector = new StandardDataSelector(value);
            }
        }
        OcDB _currentSelectedDB;
        int _currentSelectedDBIndex;
        bool _useDataSelector;
        public static DataSelectWindow Open(IOcDataSelectable dataSelectable)
        {
            var wnd = GetWindow<DataSelectWindow>(true);
            wnd.minSize = new Vector2(700, 300);
            wnd.maxSize = new Vector2(700, 300);
            wnd.DataSelectable = dataSelectable;
            return wnd;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _DBNames = DBManager.Instance.DBs.Select(x => x.name).ToArray();
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            _currentSelectedDBIndex = EditorGUILayout.Popup(_currentSelectedDBIndex, _DBNames);
            CurrentSelectedDB = DBManager.Instance.DBs[_currentSelectedDBIndex];
        }

        [Button]
        void Apply()
        {
            if(Selector != null && Selector.Data != null)
            {
                DataSelectable.TargetData = Selector.Data;
                DataSelectable.UpdateAddress();
                OnDataSelected?.Invoke(Selector.Data);
                if(Selection.activeObject != null) EditorUtility.SetDirty(Selection.activeObject);
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
        public OcData Data;
        [ShowIf(nameof(dataSelectionType), DataSelectionType.Target_DataRow)]
        public DataRowSelector DataRowSelector;
        
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
            Category = db.Category[0];
        }

        ValueDropdownList<string> GetCategory()
        {
            if (_DB == null) return null;
            var list = new ValueDropdownList<string>();
            foreach (var category in _DB.Category)
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
            var user = Data as IDataRowUser;
            DataRowSelector = new DataRowSelector(user.DataRowContainer.DataRows);
        }
    }
}
#endif