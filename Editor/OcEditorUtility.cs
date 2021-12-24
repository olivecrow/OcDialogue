using System;
using System.Collections.Generic;
using System.Linq;
using OcUtility;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class OcEditorUtility
    {
        public static int DrawCategory(int currentSelected, IEnumerable<string> categories, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(options);
            var originalColor = GUI.color;
            var count = categories.Count();
            for (int i = 0; i < count; i++)
            {
                if (i == currentSelected) GUI.color = originalColor.SetA(0.5f);
                else GUI.color = originalColor;
                if (GUILayout.Button(categories.ElementAt(i), options))
                {
                    currentSelected = i;
                }
            }
            GUI.color = originalColor;
            EditorGUILayout.EndHorizontal();

            return currentSelected;
        }
    }
}