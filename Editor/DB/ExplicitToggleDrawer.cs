using System.Collections;
using System.Collections.Generic;
using OcUtility;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace OcDialogue.Editor
{
    public class ExplicitToggleDrawer : OdinAttributeDrawer<ExplicitToggleAttribute, bool>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var value = ValueEntry.SmartValue;
            var valueText = value ? 
                Attribute.trueLabel.Rich(Color.green) : Attribute.falseLabel.Rich(Color.red);
            
            var guiStyle = new GUIStyle(GUI.skin.button) {richText = true, alignment = TextAnchor.MiddleCenter};
            EditorGUILayout.BeginHorizontal();
            if(label != null) GUILayout.Label(label);
            if (GUILayout.Button(valueText, guiStyle))
            {
                value = !value;
            }
            EditorGUILayout.EndHorizontal();
            
            ValueEntry.SmartValue = value;
        }
    }
}
