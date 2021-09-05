using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

namespace OcDialogue.Editor
{
    [CustomEditor(typeof(Balloon))]
    public class BalloonInspector : OdinEditor
    {
        DialogueNode _node;
        Balloon _target;
        protected override void OnEnable()
        {
            base.OnEnable();
            _target = target as Balloon;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (_node == null) _node = FindNode();
            if (_node != null)
            {
                // TextField는 노드 내에서 바인딩으로 연결함.
                _node.RefreshTitle();
                _node.RefreshIcons();
            }
        }

        DialogueNode FindNode()
        {
            if(DialogueEditorWindow.Instance == null) return null;
            if(DialogueEditorWindow.Instance.GraphView == null) return null;
            return DialogueEditorWindow.Instance.GraphView.Nodes.Find(x => x.Balloon.GUID == _target.GUID);
        }
    }
}
