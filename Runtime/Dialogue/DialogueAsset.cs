using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace OcDialogue
{
    [CreateAssetMenu(fileName = "Dialogue Asset", menuName = "Oc Dialogue/Dialogue Asset")]
    public class DialogueAsset : ScriptableObject
    {
        public static DialogueAsset Instance => DBManager.Instance.DialogueAsset;

        public string[] Categories;
        public List<Conversation> Conversations;

        public DialogueAsset()
        {
            Categories = new[] {"Main"};
        }

        [Button]
        public Conversation AddConversation()
        {
            var conv = CreateInstance<Conversation>();
            conv.key = OcDataUtility.CalculateDataName("New Conversation", Conversations.Select(x => x.key));
            conv.name = conv.key;
            
            if (Conversations == null) Conversations = new List<Conversation>();
            Conversations.Add(conv);

            var path = AssetDatabase.GetAssetPath(this).Replace($"{name}.asset", $"{conv.name}.asset");
            AssetDatabase.CreateAsset(conv, path);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();

            return conv;
        }
        [Button]
        public void RemoveAllConversation()
        {
            var count = Conversations.Count;
            for (int i = 0; i < count; i++)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(Conversations[0]));
                Conversations.RemoveAt(0);
            }

            Conversations = new List<Conversation>();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}
