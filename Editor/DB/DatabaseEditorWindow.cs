using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public enum DBType
    {
        GameProcess,
        Item,
        Quest,
        NPC,
        Enemy
    }
    public class DatabaseEditorWindow : OdinMenuEditorWindow
    {
        

        public DBType DBType
        {
            get => _dbType;
            set
            {
                var isDirty = value != _dbType;
                _dbType = value;
                if(isDirty)ForceMenuTreeRebuild();
            }
        }

        DBType _dbType;
        
        [MenuItem("Tools/데이터베이스 에디터")]
        static void Open()
        {
            var window = GetWindow<DatabaseEditorWindow>();
            window.Show();
        }

        void Awake()
        {
            ResizableMenuWidth = false;
        }

        protected override void OnBeginDrawEditors()
        {
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                DBType = (DBType) GUILayout.Toolbar((int) DBType, Enum.GetNames(typeof(DBType)));
                switch (DBType)
                {
                    case DBType.Item:
                        MenuWidth = 256;
                        break;
                    default:
                        MenuWidth = 1;
                        break;
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            Debug.Log("Build menu Tree");
            var tree = new OdinMenuTree();
            switch (DBType)
            {
                case DBType.GameProcess:
                    break;
                case DBType.Item:
                    break;
                case DBType.Quest:
                    break;
                case DBType.NPC:
                    tree.Add("NPC DB", NPCDatabase.Instance);
                    break;
                case DBType.Enemy:
                    break;
            }
            return tree;
        }
    }
}
