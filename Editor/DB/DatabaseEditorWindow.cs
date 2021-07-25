using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
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
        ItemDatabaseEditor _itemDatabaseEditor;
        QuestDatabaseEditor _questDatabaseEditor;
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

        protected override void OnEnable()
        {
            base.OnEnable();
            _itemDatabaseEditor ??= new ItemDatabaseEditor(this);
        }

        protected override void OnBeginDrawEditors()
        {
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                GUI.backgroundColor = new Color(2f, 1f, 0f);
                GUI.contentColor = Color.yellow;
                DBType = (DBType) GUILayout.Toolbar(
                    (int) DBType, Enum.GetNames(typeof(DBType)), 
                    GUILayoutOptions.Height(40));
                GUI.backgroundColor = Color.white;
                GUI.contentColor = Color.white;
            }
            SirenixEditorGUI.EndHorizontalToolbar();

            switch (DBType)
            {
                case DBType.Quest:
                {
                    if (QuestDatabase.Instance == null)
                    {
                        GUI.Label(EditorGUILayout.GetControlRect(), "퀘스트 데이터베이스가 없는듯?");
                        GUI.Label(EditorGUILayout.GetControlRect(), "DB Manager에 등록이 안 되었는지 확인해보고, 없으면 새로 만드셈");
                        return;
                    }

                    if (_questDatabaseEditor == null)
                    {
                        _questDatabaseEditor = UnityEditor.Editor.CreateEditor(QuestDatabase.Instance) as QuestDatabaseEditor;
                    }
                    
                    if (MenuTree.Selection.SelectedValue is QuestEditorPreset)
                    {
                        var preset = QuestDatabase.Instance.editorPreset;
                        preset.tmpList = 
                            preset.Quests.Where(x => x.quest.Category == _questDatabaseEditor.CurrentCategory).ToList();
                    }
                    _questDatabaseEditor.Draw();
                    break;
                }
                case DBType.Item:
                {
                    if (ItemDatabase.Instance == null)
                    {
                        GUI.Label(EditorGUILayout.GetControlRect(), "아이템 데이터베이스가 없는듯?");
                        GUI.Label(EditorGUILayout.GetControlRect(), "DB Manager에 등록이 안 되었는지 확인해보고, 없으면 새로 만드셈");
                        return;
                    }
                    
                    if(!(MenuTree.Selection.SelectedValue is InventoryEditorPreset))
                    {
                        _itemDatabaseEditor.Draw(MenuTree.Selection?.SelectedValue as ItemBase);
                    }

                    break;
                }
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
            var yellowTex = new Texture2D(1, 1);
            yellowTex.SetPixel(0,0, Color.yellow);
            yellowTex.Apply();
            var greenTex = new Texture2D(1, 1);
            greenTex.SetPixel(0,0, Color.green);
            greenTex.Apply();
            switch (DBType)
            {
                case DBType.GameProcess:
                    tree.Add("Game Process DB", GameProcessDatabase.Instance, greenTex);
                    tree.Add("Game Process Editor Preset", GameProcessDatabase.Instance.editorPreset, yellowTex);
                    break;
                case DBType.Item:
                    if (ItemDatabase.Instance == null)
                    {
                        break;
                    }
                    
                    tree.Add("Item Database", ItemDatabase.Instance, greenTex);
                    tree.Add("Inventory Editor Preset", ItemDatabase.Instance.editorPreset, yellowTex);
                    foreach (var itemBase in ItemDatabase.Instance.Items)
                    {
                        if(itemBase.SubTypeString != _itemDatabaseEditor.subTypeName) continue;
                        tree.Add(itemBase.itemName, itemBase, itemBase.editorIcon); 
                    }
                    break;
                case DBType.Quest:
                    tree.Add("Quest Database", QuestDatabase.Instance, greenTex);
                    tree.Add("Quest Database Editor Preset", QuestDatabase.Instance.editorPreset, yellowTex);
                    foreach (var quest in QuestDatabase.Instance.Quests)
                    {
                        if(quest.Category != _questDatabaseEditor.CurrentCategory) continue;
                        tree.Add(quest.key, quest);
                    }
                    break;
                case DBType.NPC:
                    tree.Add("NPC DB", NPCDatabase.Instance);
                    break;
                case DBType.Enemy:
                    break;
            }

            tree.Selection.SelectionChanged += type =>
            {
                _questDatabaseEditor.IsEditMode = false;
            };
            return tree;
        }
    }
}
