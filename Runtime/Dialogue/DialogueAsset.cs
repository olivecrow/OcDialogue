using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OcDialogue
{
    [CreateAssetMenu(fileName = "Dialogue Asset", menuName = "Oc Dialogue/Dialogue Asset")]
    public class DialogueAsset : ScriptableObject
    {
        public const string AssetPath = "Dialogue/Dialogue Asset";
        public static DialogueAsset Instance => _instance;
        static DialogueAsset _instance;

        public string[] Categories;
        public List<Conversation> Conversations;
        
        
        [InitializeOnLoadMethod]
        static void Init()
        {
            _instance = Resources.Load<DialogueAsset>(AssetPath);
        }
    }
}
