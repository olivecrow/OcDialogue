using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

namespace OcDialogue.Editor
{
    [CustomEditor(typeof(Balloon))][CanEditMultipleObjects]
    public class BalloonInspector : OdinEditor
    {
        Balloon[] _targets;
        
        DialogueNode[] _nodes;

        protected override void OnEnable()
        {
            base.OnEnable();
            _targets = targets.Cast<Balloon>().ToArray();
            FindNode();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (_nodes != null)
            {
                for (int i = 0; i < _nodes.Length; i++)
                {
                    var node = _nodes[i];
                    node.RefreshTitle();
                    node.RefreshSubtitleLabel();
                    node.RefreshDescription();
                    node.RefreshIcons();    
                }
                
            }
        }

        void FindNode()
        {
            if(DialogueEditorWindow.Instance == null) return;
            if(DialogueEditorWindow.Instance.GraphView == null) return;
            
            _nodes = new DialogueNode[targets.Length];
            for (int i = 0; i < _nodes.Length; i++)
            {
                _nodes[i] = DialogueEditorWindow.Instance.GraphView.Nodes.Find(x => x.Balloon.GUID == _targets[i].GUID);  
            }
        }
    }
}
