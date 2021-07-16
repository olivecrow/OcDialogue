using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                        MenuWidth = 256;
                        break;
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();

            if (DBType == DBType.Quest)
            {
                SirenixEditorGUI.BeginHorizontalToolbar();
                {
                    var toList = QuestDatabase.Instance.Category.ToList();
                    var currentIdx = toList.IndexOf(QuestDatabase.Instance.CurrentCategory);
                    var idx = GUILayout.Toolbar(currentIdx, QuestDatabase.Instance.Category);
                    
                    var rebuildRequest = currentIdx != idx;
                    
                    QuestDatabase.Instance.CurrentCategory = QuestDatabase.Instance.Category[idx];
                    switch (DBType)
                    {
                        case DBType.Item:
                            MenuWidth = 256;
                            break;
                        default:
                            MenuWidth = 256;
                            break;
                    }
                    if(rebuildRequest) ForceMenuTreeRebuild();
                }
                SirenixEditorGUI.EndHorizontalToolbar();
                
                SirenixEditorGUI.BeginHorizontalToolbar();
                {
                    if (SirenixEditorGUI.ToolbarButton("Create"))
                    {
                        QuestDatabase.Instance.AddQuest();
                        ForceMenuTreeRebuild();
                        TrySelectMenuItemWithObject(QuestDatabase.Instance.Quests[QuestDatabase.Instance.Quests.Count - 1]);
                    }
                    if (SirenixEditorGUI.ToolbarButton("Delete"))
                    {
                        if(MenuTree.Selection == null) return;
                        var q = MenuTree.Selection.SelectedValue as Quest;
                        QuestDatabase.Instance.DeleteQuest(q.key);
                    }
                    if (SirenixEditorGUI.ToolbarButton("Resolve"))
                    {
                        QuestDatabase.Instance.Resolve();
                    }
                }
                SirenixEditorGUI.EndHorizontalToolbar();
            }
            
            if(DBType == DBType.Item)
            {
                new ItemDatabaseEditor(ForceMenuTreeRebuild).Draw(
                    ref ItemDatabase.Instance.itemType,
                    ref ItemDatabase.Instance.itemSubType, MenuTree.Selection?.SelectedValue as ItemBase);
            }
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();
            tree.Selection.SelectionConfirmed += selection =>
            {
                Selection.activeObject = selection.SelectedValue as UnityEngine.Object;
                EditorGUIUtility.PingObject(Selection.activeObject);
            };
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
                    }
                    break;
                case DBType.Quest:
                    foreach (var quest in QuestDatabase.Instance.Quests)
                    {
                        if(quest.Category != QuestDatabase.Instance.CurrentCategory) continue;
                        tree.Add(quest.key, quest);
                    }
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
