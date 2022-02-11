using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

namespace OcDialogue.Editor
{
    [CustomEditor(typeof(Balloon))][CanEditMultipleObjects]
    public class BalloonInspector : OdinEditor
    {
        Balloon[] _targets;
        
        List<DialogueNode> _nodes;

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
                for (int i = 0; i < _nodes.Count; i++)
                {
                    var node = _nodes[i];
                    node.RefreshTitle();
                    node.RefreshSubtitleLabel();
                    node.RefreshDescription();
                    node.RefreshIcons();    
                }
                
            }

            if (GUILayout.Button("Insert TMP Sprite Keyword"))
            {
                TMPSpriteSearchWindow.Open(InsertTMP_SpriteAtlas_Keyword);
            }
        }

        void FindNode()
        {
            if(DialogueEditorWindow.Instance == null) return;
            if(DialogueEditorWindow.Instance.GraphView == null) return;
            var conv = DialogueEditorWindow.Instance.Conversation;
            var balloonOfCurrentConversation = _targets.Where(x => conv.Balloons.Contains(x));

            var count = balloonOfCurrentConversation.Count();
            _nodes = new List<DialogueNode>();
            for (int i = 0; i < count; i++)
            {
                _nodes.Add(
                    DialogueEditorWindow.Instance.GraphView.Nodes
                        .Find(x => x.Balloon.GUID == balloonOfCurrentConversation.ElementAt(i).GUID)
                    );  
            }
        }

        void InsertTMP_SpriteAtlas_Keyword(TMP_SpriteAsset spriteAsset, Sprite sprite)
        {
            foreach (var character in spriteAsset.spriteCharacterTable)
            {
                if(character.name != sprite.name) continue;
                foreach (var balloon in _targets)
                {
                    balloon.text += $"<sprite=\"{spriteAsset.name}\" name=\"{sprite.name}\">";
                }
            }
        }
    }
}
