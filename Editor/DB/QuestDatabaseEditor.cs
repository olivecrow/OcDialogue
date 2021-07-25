using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OcDialogue.Editor
{
    [CustomEditor(typeof(QuestDatabase))]
    public class QuestDatabaseEditor : OdinEditor
    {
        public bool IsEditMode;
        public string CurrentCategory;
        DatabaseEditorWindow EditorWindow;
        bool _rebuildRequest;
        public QuestDatabaseEditor(DatabaseEditorWindow editorWindow)
        {
            EditorWindow = editorWindow;
        }

        public void Draw()
        {
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                GUI.contentColor = new Color(1f, 2f, 1f);
                GUI.backgroundColor = new Color(1f, 2f, 1f);;
                if(QuestDatabase.Instance.Category.Length > 0 && !IsEditMode)
                {
                    var categoryList = QuestDatabase.Instance.Category.ToList();
                    var currentIdx = categoryList.Contains(CurrentCategory) ? categoryList.IndexOf(CurrentCategory) : 0;

                    var idx = GUILayout.Toolbar(currentIdx, QuestDatabase.Instance.Category, GUILayoutOptions.Height(25));

                    if (currentIdx != idx) _rebuildRequest = true;

                    CurrentCategory = QuestDatabase.Instance.Category[idx];
                }

                if (IsEditMode)
                {
                    // _user.EditCategory();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Category"));
                    serializedObject.ApplyModifiedProperties();
                }
                
                GUI.contentColor = Color.white;
                GUI.backgroundColor = Color.white;
            }
            SirenixEditorGUI.EndHorizontalToolbar();
                
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                GUI.enabled = !IsEditMode;
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
                if (!IsEditMode && SirenixEditorGUI.ToolbarButton("Edit Category"))
                {
                    IsEditMode = true;
                }

                GUI.enabled = IsEditMode;
                if (IsEditMode)
                {
                    GUI.color = new Color(1f, 2f, 1f);
                    if (SirenixEditorGUI.ToolbarButton("Done")) IsEditMode = false;
                    GUI.color = Color.white;
                }
                GUI.enabled = !IsEditMode;
                
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
