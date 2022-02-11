using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    [CustomEditor(typeof(DialogueAsset))]
    public class DialogueAssetEditor : OdinEditor
    {
        DialogueAsset _target;
        List<Conversation> Conversations => _target.Conversations;
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            _target = target as DialogueAsset;
            if (EditorGUI.EndChangeCheck() && DialogueEditorWindow.Instance != null)
            {
                Debug.Log($"New");
                wait.editorFrame(1, DialogueEditorWindow.Instance.ForceRepaint);
            }
        }
        
    }
}