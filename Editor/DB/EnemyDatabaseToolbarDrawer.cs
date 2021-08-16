using System.Collections;
using System.Collections.Generic;
using OcUtility;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class EnemyDatabaseToolbarDrawer : ICategoryUser
    {
        public string[] CategoryRef
        {
            get => EnemyDatabase.Instance.Category;
            set => EnemyDatabase.Instance.Category = value;
        }
        public string CurrentCategory => _categoryDrawer.CurrentCategory;
        DataCategoryDrawer _categoryDrawer;
        DatabaseEditorWindow EditorWindow;
        UnityEditor.Editor _originalEditor;
        bool _rebuildRequest;
        public EnemyDatabaseToolbarDrawer(DatabaseEditorWindow editorWindow)
        {
            EditorWindow = editorWindow;
            _categoryDrawer = new DataCategoryDrawer(this);
            _originalEditor = UnityEditor.Editor.CreateEditor(EnemyDatabase.Instance);
        }

        public void Draw()
        {
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                _categoryDrawer.Draw(new Color(2f, 2f, 1f), new Color(2f, 2f, 1f), _originalEditor.serializedObject,
                    out bool isSelectionChanged);
                if (isSelectionChanged) _rebuildRequest = true;
            }
            SirenixEditorGUI.EndHorizontalToolbar();
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                GUI.enabled = !_categoryDrawer.IsEditMode;
                if (SirenixEditorGUI.ToolbarButton("Create"))
                {
                    EnemyDatabase.Instance.AddEnemy(CurrentCategory);
                    _rebuildRequest = true;
                    EditorWindow.TrySelectMenuItemWithObject(
                        EnemyDatabase.Instance.Enemies[EnemyDatabase.Instance.Enemies.Count - 1]);
                }

                if (SirenixEditorGUI.ToolbarButton("Delete"))
                {
                    if (EditorWindow.MenuTree.Selection == null) return;
                    var npc = EditorWindow.MenuTree.Selection.SelectedValue as NPC;
                    if (npc == null)
                    {
                        Printer.Print("NPC가 선택되어있지 않음.");
                        return;
                    }

                    EnemyDatabase.Instance.DeleteEnemy(npc.NPCName);
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
                    EnemyDatabase.Instance.Resolve();
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
