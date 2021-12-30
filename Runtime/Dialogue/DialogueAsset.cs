using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcDialogue.DB;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcDialogue
{
    [CreateAssetMenu(fileName = "Dialogue Asset", menuName = "Oc Dialogue/Dialogue Asset")]
    public class DialogueAsset : ScriptableObject
    {
        public static DialogueAsset Instance => DBManager.Instance.DialogueAsset;

        public string DefaultDialogueUISceneName = "Dialogue UI";
        public string[] Categories;

        public IDialogueActorDB DialogueNPCDB
        {
            get
            {
                if (_dialogueActorDB == null) 
                    _dialogueActorDB = DBManager.Instance.DBs.Find(x => x is IDialogueActorDB) as IDialogueActorDB;

                return _dialogueActorDB;
            }
        }
        public List<Conversation> Conversations;
        IDialogueActorDB _dialogueActorDB;
        public DialogueAsset()
        {
            Categories = new[] {"Main"};
        }

#if UNITY_EDITOR
        public Conversation AddConversation(int categoryIndex)
        {
            var conv = CreateInstance<Conversation>();
            conv.key = OcDataUtility.CalculateDataName("New Conversation", Conversations.Select(x => x.key));
            conv.name = conv.key;
            
            if (Conversations == null) Conversations = new List<Conversation>();
            Conversations.Add(conv);

            // Dialogue Asset이 있는 폴더를 구함.
            var assetFolderPath = AssetDatabase.GetAssetPath(this).Replace($"/{name}.asset", "");

            var folderPath = assetFolderPath + $"/{Categories[categoryIndex]}";
            if(!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(assetFolderPath, Categories[categoryIndex]);
            }

            var path = folderPath + $"/{conv.key}.asset";
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

        public void RemoveConversation(Conversation conv)
        {
            Conversations.Remove(conv);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(conv));
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        
        [BoxGroup("유틸리티 메서드"), Button("GUID로 Balloon선택")]
        void SelectBalloonFromGUID(string guid)
        {
            foreach (var conversation in Conversations)
            {
                var balloon = conversation.Balloons.Find(x => x.GUID == guid);
                if (balloon == null) continue;

                Selection.activeObject = balloon;
                return;
            }
            Debug.Log($"[{guid}] 해당 GUID를 가진 Balloon이 없음");
        }
#endif
        
    }
}
