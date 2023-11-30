using System;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class PreCompileAction : AssetPostprocessor
    {
        void OnPreprocessAsset()
        {
            if (assetPath.EndsWith(".cs"))
            {
                if (DialogueEditorWindow.Instance != null && DialogueEditorWindow.Instance.Conversation != null)
                {
                    AssetDatabase.SaveAssetIfDirty(DialogueEditorWindow.Instance.Conversation);
                }
                
            }
        }
    }
}