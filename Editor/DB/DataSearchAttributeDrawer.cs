using System;
using System.Linq;
using OcUtility;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OcDialogue.Editor
{
    public class DataSearchAttributeDrawer<T> : OdinAttributeDrawer<DataSearchAttribute, T> where T : OcData
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var value = ValueEntry.SmartValue;
            Property.Update();
            EditorGUILayout.BeginHorizontal();
            ValueEntry.SmartValue = EditorGUILayout.ObjectField(label, ValueEntry.SmartValue, typeof(T), false) as T;
            var style = new GUIStyle(GUI.skin.button);
            style.stretchWidth = false;
            var color = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.4f, 0.4f, 1f);
            if (GUILayout.Button("선택", style))
            {
                ShowWindow();
            }

            GUI.backgroundColor = color;
            EditorGUILayout.EndHorizontal();
            
            ValueEntry.SmartValue = value;
        }
        
        void ShowWindow()
        {
            SearchService.ShowObjectPicker(
                null,
                Tracking,
                "",
                $"t:{nameof(T)} -cs",
                typeof(T));
        }

        void Tracking(Object o)
        {
            this.ValueEntry.SmartValue = o as T;
        }
    }
}