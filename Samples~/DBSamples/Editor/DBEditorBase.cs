using System;
using System.Collections.Generic;
using System.Linq;
using OcDialogue;
using OcDialogue.DB;
using OcDialogue.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace MyDB.Editor
{
    public abstract class DBEditorBase : OdinEditor, IDBEditor
    {
        protected OcData SelectedData
        {
            get
            {
                if(_selectedData == null)
                {
                    if (Window == null || Window.MenuTree == null) return null;
                    _selectedData = Window.MenuTree.Selection.SelectedValue as OcData;
                }
                return _selectedData;
            }
            set => _selectedData = value;
        }

        protected int CurrentCategoryIndex
        {
            get => _currentCategoryIndex;
            set
            {
                if (_db.Categories.Length == 0)
                {
                    _currentCategoryIndex = 0;
                    return;
                }
                if (value < 0 || value > _db.Categories.Length) value = 0;
                var isNew = _currentCategoryIndex != value;
                _currentCategoryIndex = value;
                if(isNew)
                {
                    CurrentCategory = _db.Categories[value];
                }
            }
        }
        int _currentCategoryIndex;
        
        protected string CurrentCategory
        {
            get => _currentCategory;
            set
            {
                if (_db.Categories.Length == 0)
                {
                    _currentCategory = "";
                    return;
                }
                var isNew = _currentCategory != value;
                _currentCategory = value;
                if(_db != null)_currentCategoryIndex = _db.Categories.ToList().IndexOf(_currentCategory);
                if(isNew)
                {
                    if(Window != null) Window.ForceMenuTreeRebuild();
                    OnCategoryChanged(_currentCategoryIndex, _currentCategory);
                }
            }
        }
        string _currentCategory;
        OcData _selectedData;
        OcDB _db;
        public OdinMenuEditorWindow Window { get; set; }
        protected override void OnEnable()
        {
            base.OnEnable();
            _db = target as OcDB;
            if (_db.Categories == null || _db.Categories.Length == 0)
            {
                serializedObject.Update();
                Debug.Log($"[{_db.Address}]Category : Main 추가됨");
                _db.Categories = new[] { "Main" };
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            if(_db.Categories.Length > 0) CurrentCategory = _db.Categories[_currentCategoryIndex];
        }

        public virtual void CreateTree(OdinMenuTree tree)
        {
            foreach (var data in _db.AllData)
            {
                if(data.Category == CurrentCategory)tree.Add(data.name, data);
            }
            
            tree.Selection.SelectionChanged += type =>
            {
                switch (type)
                {
                    case SelectionChangedType.ItemRemoved:
                        SelectedData = null;
                        break;
                    case SelectionChangedType.ItemAdded:
                        try
                        {
                            SelectedData = tree.Selection.SelectedValue as OcData;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            // throw;
                        }
                        
                        break;
                    case SelectionChangedType.SelectionCleared:
                        SelectedData = null;
                        break;
                }
            };
            tree.Selection.SelectionConfirmed += s =>
                EditorGUIUtility.PingObject(s.SelectedValue as Object);
        }
        
        public abstract void DrawToolbar();
        public virtual void AddDialogueContextualMenu(ContextualMenuPopulateEvent evt, DialogueGraphView graphView){}
        public abstract string[] GetCSVFields();
        public abstract IEnumerable<string[]> GetCSVData();
        public abstract IEnumerable<LocalizationCSVRow> GetLocalizationData();

        protected virtual void OnCategoryChanged(int index, string category)
        {
            var data = _db.AllData.FirstOrDefault(x => x.Category == category);
            SelectTreeMenu(data);
        }

        protected void SelectTreeMenu(OcData menuItem)
        {
            if(Window == null) return;
            if(menuItem == null) return;
            var item = Window.MenuTree.MenuItems.Find(x => (x.Value as OcData) == menuItem);
            if (item == null) return;
            
            Window.MenuTree.Selection.Add(item);
        }

        protected void SelectCategoryLastItem()
        {
            var categoryItems = Window.MenuTree.MenuItems
                .Where(x => (x.Value as OcData).Category == _currentCategory);
            if(!categoryItems.Any())
            {
                Window.MenuTree.Selection.Clear();
                return;
            }
            Window.MenuTree.Selection.Add(Window.MenuTree.MenuItems
                .FindLast(x => (x.Value as OcData).Category == CurrentCategory));
        }
    }
}