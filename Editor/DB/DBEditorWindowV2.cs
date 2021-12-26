﻿using System.Collections.Generic;
using OcDialogue.DB;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class DBEditorWindowV2 : OdinMenuEditorWindow
    {
        List<string> _DBNames;
        OcDB _currentDB;
        IDBEditor _CurrentDBEditor;

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
        
        [MenuItem("OcDialogue/DB 에디터 V2")]
        static void Open()
        {
            var window = GetWindow<DBEditorWindowV2>("DB 에디터 V2", true, typeof(SceneView));
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
            _CurrentDBEditor?.AddTreeMenu(tree);

            return tree;
        }

        void UpdateDBEditor()
        {
            if (_DBNames == null) _DBNames = new List<string>();
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