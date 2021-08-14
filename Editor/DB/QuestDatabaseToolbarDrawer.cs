using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class QuestDatabaseToolbarDrawer : ICategoryUser
    {
        public string[] CategoryRef
        {
            get => QuestDatabase.Instance.Category;
            set => QuestDatabase.Instance.Category = value;
        }

        public string CurrentCategory => _categoryDrawer.CurrentCategory;
        DatabaseEditorWindow EditorWindow;
        UnityEditor.Editor _originalEditor;
        DataCategoryDrawer _categoryDrawer;
        bool _rebuildRequest;
        public QuestDatabaseToolbarDrawer(DatabaseEditorWindow editorWindow)
        {
            EditorWindow = editorWindow;
            _originalEditor = UnityEditor.Editor.CreateEditor(QuestDatabase.Instance);
            _categoryDrawer = new DataCategoryDrawer(this);
        }

        public void Draw()
        {
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                _categoryDrawer.Draw(new Color(1f, 2f, 1f), new Color(1f, 2f, 1f), _originalEditor.serializedObject, out var isSelectionChanged);
                if (isSelectionChanged) _rebuildRequest = true;
            }
            SirenixEditorGUI.EndHorizontalToolbar();
                
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                GUI.enabled = !_categoryDrawer.IsEditMode;
                if (SirenixEditorGUI.ToolbarButton("Create"))
                {
                    QuestDatabase.Instance.AddQuest(CurrentCategory);
                    _rebuildRequest = true;
                    EditorWindow.TrySelectMenuItemWithObject(QuestDatabase.Instance.Quests[QuestDatabase.Instance.Quests.Count - 1]);
                }
                if (SirenixEditorGUI.ToolbarButton("Delete"))
                {
                    if(EditorWindow.MenuTree.Selection == null) return;
                    var q = EditorWindow.MenuTree.Selection.SelectedValue as Quest;
                    QuestDatabase.Instance.DeleteQuest(q.key);
                }
                if (!_categoryDrawer.IsEditMode && SirenixEditorGUI.ToolbarButton("Edit Category"))
                {
                    _categoryDrawer.IsEditMode = true;
                }

                GUI.enabled = _categoryDrawer.IsEditMode;
                if (_categoryDrawer.IsEditMode)
                {
                    GUI.color = new Color(1f, 2f, 1f);
                    if (SirenixEditorGUI.ToolbarButton("Done")) _categoryDrawer.IsEditMode = false;
                    GUI.color = Color.white;
                }
                GUI.enabled = !_categoryDrawer.IsEditMode;
                
                var resolveContents = new GUIContent();
                resolveContents.text = "Resolve";
                resolveContents.tooltip = "다음의 문제들을 해결함.\n" +
                                          "모든 퀘스트의 DataRow중, ownerDB가 퀘스트가 아닌 것을 고침";
                if (SirenixEditorGUI.ToolbarButton(resolveContents))
                {
                    QuestDatabase.Instance.Resolve();
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();

            if (_rebuildRequest)
            {
                EditorWindow.ForceMenuTreeRebuild();
                _rebuildRequest = false;
            }
        }
    }
}
