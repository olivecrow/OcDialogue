using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    [CustomEditor(typeof(Balloon))]
    public class BalloonInspector : OdinEditor
    {
        Balloon _target;
        protected override void OnEnable()
        {
            base.OnEnable();
            _target = target as Balloon;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                if(DialogueEditorWindow.Instance == null) return;
                var selections = DialogueEditorWindow.Instance.GraphView.selection; 
                if(selections.Count == 0) return;
                foreach (var selection in selections)
                {
                    if (selection is DialogueNode node)
                    {
                        // TextField는 노드 내에서 바인딩으로 연결함.
                        node.UpdateTitle();
                    
                    }
                }
            }
        }

    }
}
