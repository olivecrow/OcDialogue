using System.Collections;
using System.Collections.Generic;
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
                $"<color=#79ff4d>{Attribute.trueLabel}</color>" : $"<color=#ff4d4d>{Attribute.falseLabel}</color>";
            var guiStyle = new GUIStyle(GUI.skin.button) {richText = true, alignment = TextAnchor.MiddleCenter};


            if (GUILayout.Button(valueText, guiStyle))
            {
                value = !value;
            }
            ValueEntry.SmartValue = value;
        }
    }
}
