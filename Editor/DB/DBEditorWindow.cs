﻿using System;
using System.Collections.Generic;
using System.Linq;
using OcDialogue.DB;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class DBEditorWindow : OdinMenuEditorWindow
    {
        List<string> _DBNames;
        OcDB _currentDB;
        IDBEditor _CurrentDBEditor;
        static string _searchText;
        static DialogueEditorWindow.SortMethod _sortMethod;

        int CurrentSelectedDBIndex
        {
            get => __currentSelectedDBIndex;
            set
            {
                var isNew = value != __currentSelectedDBIndex;
                __currentSelectedDBIndex = value;
                if(isNew)
                {
                    UpdateDBEditor();
                    ForceMenuTreeRebuild();
                }
            }
        }
        int __currentSelectedDBIndex;
        
        [MenuItem("OcDialogue/DB 에디터")]
        static void Open()
        {
            var window = GetWindow<DBEditorWindow>("DB 에디터", true, typeof(SceneView));
            window.minSize = new Vector2(720, 480);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateDBEditor();
        }
        
        protected override void OnBeginDrawEditors()
        {
            GUI.color = new Color(1f, 1f, 2f);
            CurrentSelectedDBIndex = 
                OcEditorUtility.DrawCategory(CurrentSelectedDBIndex, _DBNames, GUILayout.Height(40));
            GUI.color = Color.white;
            
            _CurrentDBEditor?.DrawToolbar();
            _CurrentDBEditor?.OnInspectorGUI();

        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();

            _CurrentDBEditor?.CreateTree(tree);
            var list = tree.MenuItems;
            if(!string.IsNullOrWhiteSpace(_searchText))
            {
                tree.MenuItems.RemoveAll(x => !x.Name.ToLower().Contains(_searchText.ToLower()));
            }
            switch (_sortMethod)
            {
                case DialogueEditorWindow.SortMethod.Index:
                    break;
                case DialogueEditorWindow.SortMethod.Name:
                    list = list.OrderBy(x => x.Name).ToList();
                    break;
                case DialogueEditorWindow.SortMethod.NameDescending:
                    list = list.OrderByDescending(x => x.Name).ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            var newTree = new OdinMenuTree();
            foreach (var item in list)
            {
                newTree.Add(item.Name, item.Value, item.Icon);
            }
            return newTree;
        }

        protected override void DrawMenu()
        {
            EditorGUI.BeginChangeCheck();
            _searchText = EditorGUILayout.TextField("",_searchText);
            _sortMethod = (DialogueEditorWindow.SortMethod)EditorGUILayout.EnumPopup("", _sortMethod);
            if (EditorGUI.EndChangeCheck())
            {
                ForceMenuTreeRebuild();
            }
            base.DrawMenu();
        }

        void UpdateDBEditor()
        {
            if (_DBNames == null) _DBNames = new List<string>();
            if(DBManager.Instance.DBs.Count == 0) return;
            _DBNames.Clear();
            foreach (var db in DBManager.Instance.DBs) _DBNames.Add(db.Address);
            if(_CurrentDBEditor != null)
            {
                DestroyImmediate(_CurrentDBEditor as UnityEditor.Editor);
                _CurrentDBEditor = null;
            }

            _currentDB = DBManager.Instance.DBs[__currentSelectedDBIndex];
            _CurrentDBEditor =
                UnityEditor.Editor.CreateEditor(DBManager.Instance.DBs[__currentSelectedDBIndex]) as IDBEditor;
            _CurrentDBEditor.Window = this;
        }

    }
}