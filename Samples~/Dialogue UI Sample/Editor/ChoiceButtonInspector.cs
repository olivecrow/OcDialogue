using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace OcDialogue.Samples.Editor
{
    [CustomEditor(typeof(ChoiceButton))]
    public class ChoiceButtonInspector : OdinEditor
    {
        ChoiceButton _target;
        SelectableEditor _selectableEditor;
        protected override void OnEnable()
        {
            base.OnEnable();
            _target = target as ChoiceButton;
            _selectableEditor = CreateEditor(_target, typeof(SelectableEditor)) as SelectableEditor;
        }

        public override void OnInspectorGUI()
        {
            _selectableEditor.OnInspectorGUI();
        }
    }
}
