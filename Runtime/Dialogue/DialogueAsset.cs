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

        IDialogueActorDB DialogueNPCDB
        {
            get
            {
                if (_dialogueActorDB == null) 
                    _dialogueActorDB = DBManager.Instance.DBs.Find(x => x is IDialogueActorDB) as IDialogueActorDB;

                return _dialogueActorDB;
            }
        }
        public List<Conversation> Conversations;
        public Dictionary<string, IEnumerable<Conversation>> CategorizedConversations;
        IDialogueActorDB _dialogueActorDB;
        
        public DialogueAsset()
        {
            Categories = new[] {"Main"};
        }

        public ValueDropdownList<OcNPC> GetNPCDropDown()
        {
            if (DialogueNPCDB == null) return null;

            return DialogueNPCDB.GetOdinDropDown();
        }

        public Balloon FindBalloon(string guid)
        {
            foreach (var conversation in Conversations)
            {
                foreach (var balloon in conversation.Balloons)
                {
                    if (string.CompareOrdinal(balloon.GUID, guid) == 0) return balloon;
                }
            }

            return null;
        }
        
        public Balloon FindBalloon(string category, string guid)
        {
            if (CategorizedConversations == null) CategorizedConversations = new Dictionary<string, IEnumerable<Conversation>>();
            if (CategorizedConversations.Count == 0)
            {
                CategorizeConversations();
            }
            foreach (var conversation in CategorizedConversations[category])
            {
                foreach (var balloon in conversation.Balloons)
                {
                    if (string.CompareOrdinal(balloon.GUID, guid) == 0) return balloon;
                }
            }

            return null;
        }

        void CategorizeConversations()
        {
            foreach (var c in Categories)
            {
                CategorizedConversations[c] = Conversations.Where(x => string.CompareOrdinal(x.Category, c) == 0);
            }
        }

#if UNITY_EDITOR
        public Conversation AddConversation(int categoryIndex)
        {
            Undo.RecordObject(this, "Add Conversation");
            var conv = CreateInstance<Conversation>();
            conv.key = OcDataUtility.CalculateDataName("New Conversation", Conversations.Select(x => x.key));
            conv.name = conv.key;
            
            if (Conversations == null) Conversations = new List<Conversation>();
            Conversations.Add(conv);

            // Dialogue Asset??? ?????? ????????? ??????.
            var assetFolderPath = AssetDatabase.GetAssetPath(this).Replace($"/{name}.asset", "");

            var folderPath = assetFolderPath + $"/{Categories[categoryIndex]}";
            if(!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(assetFolderPath, Categories[categoryIndex]);
            }

            var path = folderPath + $"/{conv.key}.asset";
            AssetDatabase.CreateAsset(conv, path);

            return conv;
        }
        [Button]
        public void RemoveAllConversation()
        {
            if (!EditorUtility.DisplayDialog("??????!!!!!!!!!!!!",
                "?????? ??? ????????? ????????? ?????? ????????????????????????? ??? ????????? ????????? ??? ????????????.", "??????", "??????")) return;

            if (!EditorUtility.DisplayDialog("!!! ?????? ??? ??? ?????? !!!!",
                "?????? ??? ????????? ????????? ?????? ???????????????? ????????? ??????????????? ??? ?????????!!!!!", "??????", "??????")) return;
            
            Undo.RecordObject(this, "Remove All Conversation");
            var count = Conversations.Count;
            for (int i = 0; i < count; i++)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(Conversations[0]));
                Conversations.RemoveAt(0);
            }

            Conversations = new List<Conversation>();
        }

        public void RemoveConversation(Conversation conv)
        {
            Undo.RecordObject(this, "Remove Conversation");
            Conversations.Remove(conv);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(conv));
        }
        
        [BoxGroup("???????????? ?????????"), Button("GUID??? Balloon??????")]
        void SelectBalloonFromGUID(string guid)
        {
            foreach (var conversation in Conversations)
            {
                var balloon = conversation.Balloons.Find(x => string.CompareOrdinal(x.GUID, guid) == 0);
                if (balloon == null) continue;

                Selection.activeObject = balloon;
                return;
            }
            Debug.Log($"[{guid}] ?????? GUID??? ?????? Balloon??? ??????");
        }
        
        public IEnumerable<string> GetLocalizationKey()
        {
            // Key | ID | SharedComments
            foreach (var conversation in Conversations)
            {
                foreach (var balloon in conversation.Balloons)
                {
                    yield return $"{conversation.Category}/{conversation.key}/{balloon.GUID}";
                    yield return "";
                    yield return $"{balloon.text}";
                }
            }
        }
#endif
        
    }
}
