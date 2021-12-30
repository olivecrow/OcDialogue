#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using OcDialogue.DB;
using OcUtility;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;

// #if ENABLE_LOCALIZATION
// using UnityEditor.Localization.Plugins.XLIFF.V12;
// #endif
using UnityEngine;

namespace OcDialogue.Editor
{
    public class OcEditorUtility
    {
        public static int DrawCategory(int currentSelected, IEnumerable<string> categories,
            params GUILayoutOption[] options)
        {
            return DrawCategory(currentSelected, categories, -1, options);
        }
        public static int DrawCategory(int currentSelected, IEnumerable<string> categories, 
            int forceLineBrakeIndex = -1, params GUILayoutOption[] options)
        {
            var guiContents = new List<GUIContent>();
            var count = categories.Count();
            for (int i = 0; i < count; i++)
            {
                guiContents.Add(new GUIContent(categories.ElementAt(i)));
            }
            return DrawCategory(currentSelected, guiContents.ToArray(), forceLineBrakeIndex, options);
        }

        public static int DrawCategory(int currentSelected, IEnumerable<GUIContent> categories, 
            params GUILayoutOption[] options)
        {
            return DrawCategory(currentSelected, categories, -1, options);
        }
        public static int DrawCategory(int currentSelected, IEnumerable<GUIContent> categories, 
            int forceLineBrakeIndex = -1, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(options);
            var originalColor = GUI.color;
            var count = categories.Count();
            for (int i = 0; i < count; i++)
            {
                var content = categories.ElementAt(i);
                if (i == forceLineBrakeIndex)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal(options);
                }
                if (i == currentSelected) GUI.color = originalColor.SetA(0.5f);
                else GUI.color = originalColor;
                if (GUILayout.Button(content, options))
                {
                    currentSelected = i;
                }
            }
            GUI.color = originalColor;
            EditorGUILayout.EndHorizontal();

            return currentSelected;
        }
        
        public static int DrawEditableCategory(
            int current, string[] categories, ref bool isEditMode, 
            SerializedProperty property, params GUILayoutOption[] options)
        {
            SirenixEditorGUI.BeginHorizontalToolbar();
            if(categories.Any() && !isEditMode)
            {
                current = GUILayout.Toolbar(current, categories, options);
            }
        
            if (isEditMode)
            {
                GUI.enabled = true;
                EditorGUILayout.PropertyField(property);
                if (SirenixEditorGUI.ToolbarButton("Done")) isEditMode = false;
                GUI.enabled = false;
            }
            else
            {
                if (SirenixEditorGUI.ToolbarButton("Edit")) isEditMode = true;
            }
            GUI.contentColor = Color.white; GUI.backgroundColor = Color.white;
            SirenixEditorGUI.EndHorizontalToolbar();

            return current;
        }
        
        public static Texture2D CreateColorIcon(Color color)
        {
            var i = new Texture2D(1, 1);
            i.SetPixel(0,0, color);
            i.Apply();
            return i;
        }
        
        
        public static void EditCategory(OcDB db, List<CategoryEditWindow.Transition> transitions,
            Action<string> additional, Action<OcData> removed)
        {
            var undo = Undo.GetCurrentGroup();
            Undo.RecordObject(db, "카테고리 편집");
            var newCategory = new List<string>();
            var removedCategory = new List<string>();
            for (int i = 0; i < transitions.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(transitions[i].after)) newCategory.Add(transitions[i].after);
            }

            foreach (var s in db.Categories)
            {
                if (transitions.All(x => x.before != s)) removedCategory.Add(s);
            }

            db.Categories = newCategory.ToArray();
            foreach (var t in transitions)
            {
                if (string.IsNullOrWhiteSpace(t.before))
                {
                    additional?.Invoke(t.after);
                    continue;
                }

                var data = db.AllData.FirstOrDefault(x => x.Category == t.before);
                if(data != null)
                {
                    if (string.IsNullOrWhiteSpace(t.after))
                    {
                        if (!EditorUtility.DisplayDialog(
                                "삭제된 카테고리의 데이터를 소거하시겠습니까?", t.before,
                                "OK", "Cancel")) return;
                        removed?.Invoke(data);
                    }
                    else
                    {
                        Undo.RecordObject(data, "카테고리 편집");
                        data.Category = t.after;
                        Debug.Log($"data category : {data.Category}");
                    }
                }

                Undo.CollapseUndoOperations(undo);
            }

            foreach (var category in removedCategory)
            {
                var data = db.AllData.FirstOrDefault(x => x.Category == category);
                if (data == null) continue;
                if (!EditorUtility.DisplayDialog(
                        "삭제된 카테고리의 데이터를 소거하시겠습니까?", category,
                        "OK", "Cancel")) continue;
                removed?.Invoke(data);
            }
        }
    }
}
#endif
