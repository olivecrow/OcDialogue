using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OcDialogue.DB
{
    public class DBDebugUI : MonoBehaviour
    {
        public RectTransform dbToggleArea;
        public ToggleGroup dbToggleGroup;
        public Toggle dbToggleTemplate;
        
        public RectTransform treeViewToggleArea;
        public ToggleGroup treeViewToggleGroup;
        public Toggle treeViewToggleTemplate;

        public RectTransform categoryArea;
        public ToggleGroup categoryToggleGroup;
        public Toggle categoryToggleTemplate;
        
        public RectTransform inspectorArea;
        public DBDebugStandardField StandardFieldTemplate;
        public DBDebugObjectField ObjectFieldTemplate;

        public RectTransform dataRowContainerView;
        public RectTransform dataRowContentsArea;
        public DBDebugDataRow dataRowTemplate;

        OcDB _currentDB;
        ToggleData _currentCategory;
        ToggleData _currentTreeView;
        List<ToggleData> _instancedDBToggle;
        List<ToggleData> _instancedCategory;
        List<ToggleData> _instancedTreeView;
        List<DBDebugBlock> _inspectorBlocks;
        List<DBDebugDataRow> _dataRows;
        List<OcDB> DBs => DBManager.Instance.DBs;
        Dictionary<OcDB, string> _lastViewCategory = new Dictionary<OcDB, string>();

        void Start()
        {
            UpdateDBButtons();
            UpdateCategory();
            
            dbToggleTemplate.gameObject.SetActive(false);
            treeViewToggleTemplate.gameObject.SetActive(false);
            categoryToggleTemplate.gameObject.SetActive(false);
            StandardFieldTemplate.gameObject.SetActive(false);
            ObjectFieldTemplate.gameObject.SetActive(false);
            dataRowTemplate.gameObject.SetActive(false);
        }

        void Update()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;   
        }

        void UpdateDBButtons()
        {
            if(DBManager.Instance == null) return;

            if (_instancedDBToggle == null) _instancedDBToggle = new List<ToggleData>();

            for (int i = 0; i < DBs.Count; i++)
            {
                var db = DBs[i];
                var toggle = Instantiate(dbToggleTemplate, dbToggleArea);
                toggle.group = categoryToggleGroup;
                toggle.GetComponentInChildren<TextMeshProUGUI>().text = db.name;
                toggle.onValueChanged.AddListener(isOn =>
                {
                    if(isOn) Set(db);
                });
                toggle.gameObject.SetActive(true);

                var data = new ToggleData(toggle, dbToggleGroup, db);
                _instancedDBToggle.Add(data);
            }

            _instancedDBToggle[0].toggle.isOn = true;
        }

        void UpdateCategory()
        {
            if (_instancedCategory == null) _instancedCategory = new List<ToggleData>();
            else
            {
                categoryToggleGroup.allowSwitchOff = true;
                var count = _instancedCategory.Count;
                for (int i = 0; i < count; i++)
                {
                    Destroy(_instancedCategory[i].gameObject);
                }
                _instancedCategory.Clear();
            }

            for (int i = 0; i < _currentDB.Categories.Length; i++)
            {
                var category = _currentDB.Categories[i];
                var toggle = Instantiate(categoryToggleTemplate, categoryArea);
                var data = new ToggleData(toggle, categoryToggleGroup, null);
                data.text.text = category;
                
                toggle.onValueChanged.AddListener(isOn =>
                {
                    if(isOn) OnCategorySelected(data);
                });
                _instancedCategory.Add(data);
                toggle.gameObject.SetActive(true);
            }
            categoryToggleGroup.allowSwitchOff = false;
        }

        void UpdateLayout()
        {
            wait.frame(1, () =>
            {
                foreach (var t in GetComponentsInChildren<RectTransform>().Reverse())
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(t);
                }
            });
        }

        void Set(OcDB db)
        {
            var category = _lastViewCategory.ContainsKey(db) ? _lastViewCategory[db] : db.Categories[0];
            
            Set(db, category);
        }

        void Set(OcDB db, string category)
        {
            var isNewDB = _currentDB != db;
            _currentDB = db;
            if (isNewDB)
            {
                UpdateCategory();
            }
            
            SelectCategoryWithoutNotify(category);
        }

        void UpdateTreeView()
        {
            var allData = _currentDB.AllData.ToList();
            if (_instancedTreeView == null) _instancedTreeView = new List<ToggleData>();
            else
            {
                var count = _instancedTreeView.Count;
                for (int i = 0; i < count; i++)
                {
                    Destroy(_instancedTreeView[i].gameObject);
                }
                _instancedTreeView.Clear();
            }

            foreach (var data in allData.Where(x => x.Category == _currentCategory.data.name))
            {
                var toggle = Instantiate(treeViewToggleTemplate, treeViewToggleArea);
                var toggleData = new ToggleData(toggle, treeViewToggleGroup, data);
                toggleData.text.text = data.name;
                toggle.onValueChanged.AddListener(isOn =>
                {
                    if(isOn) OnTreeViewSelected(toggleData);
                });
                toggle.gameObject.SetActive(true);
                _instancedTreeView.Add(toggleData);
            }
            

            _currentCategory = _instancedTreeView.FirstOrDefault();
        }
        void UpdateInspector()
        {
            if (_inspectorBlocks == null) _inspectorBlocks = new List<DBDebugBlock>();
            else
            {
                var count = _inspectorBlocks.Count;
                for (int i = 0; i < count; i++)
                {
                    Destroy(_inspectorBlocks[i].gameObject);
                }
                _inspectorBlocks.Clear();
            }

            var fieldsInfo = _currentTreeView.data
                .GetType()
                .GetFields();

            var dataRowContainerInit = false;
            for (int i = 0; i < fieldsInfo.Length; i++)
            {
                var field = fieldsInfo[i];
                var type = field.FieldType;

                var isReadOnly = field.Name is "id" or "parent" or "dataRowContainer" or "category" ||
                                 field.GetCustomAttributes().Any(x => x.GetType().Name.Contains("ReadOnly"));

                if (type == typeof(DataRowContainer))
                {
                    UpdateDataRowContainer(field.GetValue(_currentTreeView.data) as DataRowContainer);
                    dataRowContainerInit = true;
                }
                if (type.IsValueType || type == typeof(string))
                {
                    var block = Instantiate(StandardFieldTemplate, inspectorArea);
                    block.Set(_currentTreeView.data, field);
                    block.gameObject.SetActive(true);
                    if (isReadOnly) block.interactable = false;
                    _inspectorBlocks.Add(block);
                }
                else
                {
                    var block = Instantiate(ObjectFieldTemplate, inspectorArea);
                    block.Set(_currentTreeView.data, field);
                    block.gameObject.SetActive(true);
                    if (isReadOnly) block.interactable = false;
                    _inspectorBlocks.Add(block);
                }
            }
            
            dataRowContainerView.gameObject.SetActive(dataRowContainerInit);
            if(dataRowContainerView.gameObject.activeInHierarchy) dataRowContainerView.transform.SetAsLastSibling();
            UpdateLayout();
        }

        void UpdateDataRowContainer(DataRowContainer container)
        {
            if (_dataRows == null) _dataRows = new List<DBDebugDataRow>();
            else
            {
                var count = _dataRows.Count;
                for (int i = 0; i < count; i++)
                {
                    Destroy(_dataRows[i].gameObject);
                }
                _dataRows.Clear();
            }

            for (int i = 0; i < container.DataRows.Count; i++)
            {
                var data = container.DataRows[i];
                var block = Instantiate(dataRowTemplate, dataRowContentsArea);
                block.Set(data);
                block.gameObject.SetActive(true);
                _dataRows.Add(block);
            }
        }

        public void SelectCategory(string category)
        {
            var toggle = _instancedCategory.First(x => x.text.text == category);
            toggle.toggle.isOn = true;
        }

        void SelectCategoryWithoutNotify(string category)
        {
            var toggle = _instancedCategory.First(x => x.text.text == category);
            toggle.toggle.SetIsOnWithoutNotify(true);
        }

        void OnCategorySelected(ToggleData data)
        {
            _currentCategory = data;
            UpdateTreeView();
        }
        void OnTreeViewSelected(ToggleData data)
        {
            _currentTreeView = data;
            UpdateInspector();
        }
    }

    public class ToggleData
    {
        public GameObject gameObject => toggle.gameObject;
        public readonly Toggle toggle;
        public TextMeshProUGUI text;
        public OcData data;

        public ToggleData(Toggle t, ToggleGroup group, OcData data)
        {
            this.toggle = t;
            text = t.GetComponentInChildren<TextMeshProUGUI>();
            t.group = group;
            this.data = data;
        }
    }
}