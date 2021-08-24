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
                Attribute.trueLabel.ToRichText(Color.green) : Attribute.falseLabel.ToRichText(Color.red);
            // var valueText = value ? Attribute.trueLabel : Attribute.falseLabel;
            var guiStyle = new GUIStyle(GUI.skin.button) {richText = true, alignment = TextAnchor.MiddleCenter};

            var rect = EditorGUILayout.GetControlRect();

            if (label != null) rect = EditorGUI.PrefixLabel(rect, label);
            
            GUIHelper.PushLabelWidth(rect.width);
            GUIHelper.PopLabelWidth();
            
            if (GUI.Button(rect, valueText, guiStyle))
            {
                value = !value;
            }

            ValueEntry.SmartValue = value;
        }
    }
}
