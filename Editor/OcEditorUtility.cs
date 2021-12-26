#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using OcUtility;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.Localization.Plugins.XLIFF.V12;
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
    }
}
#endif
