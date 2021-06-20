using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class ExplicitToggleDrawer : OdinAttributeDrawer<ExplicitToggleAttribute, bool>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect rect = EditorGUILayout.GetControlRect();
    
            var value = ValueEntry.SmartValue;
            var valueText = value ? 
                $"<color=#79ff4d>{Attribute.trueLabel}</color>" : $"<color=#ff4d4d>{Attribute.falseLabel}</color>";
            var guiStyle = new GUIStyle();
            guiStyle.richText = true;
            
            value = GUI.Toggle(rect, value, valueText, guiStyle);
            ValueEntry.SmartValue = value;
        }
    }
}
