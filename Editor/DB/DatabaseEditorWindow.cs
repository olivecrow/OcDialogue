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
            
            if(DBType == DBType.Item)
            {
                new ItemDatabaseEditor(ForceMenuTreeRebuild).Draw(
                    ref ItemDatabase.Instance.itemType,
                    ref ItemDatabase.Instance.itemSubType,
                    MenuTree.Selection.SelectedValue as ItemBase);
            }
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();
            switch (DBType)
            {
                case DBType.GameProcess:
                    tree.Add("GameProcess DB", GameProcessDatabase.Instance);
                    break;
                case DBType.Item:
                    foreach (var itemBase in ItemDatabase.Instance.Items)
                    {
                        if(itemBase.SubTypeString != ItemDatabase.Instance.itemSubType) continue;
                        tree.Add(itemBase.itemName, itemBase, itemBase.editorIcon);
                        tree.Selection.SelectionConfirmed += selection => Selection.activeObject = selection.SelectedValue as UnityEngine.Object; 
                    }
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
