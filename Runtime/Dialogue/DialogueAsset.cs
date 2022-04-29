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
            foreach (var conversation in Conversations.Where(x => string.CompareOrdinal(x.Category, category) == 0))
            {
                foreach (var balloon in conversation.Balloons)
                {
                    if (string.CompareOrdinal(balloon.GUID, guid) == 0) return balloon;
                }
            }

            return null;
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

            // Dialogue Asset이 있는 폴더를 구함.
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
            if (!EditorUtility.DisplayDialog("잠깐!!!!!!!!!!!!",
                "정말 이 에셋의 대화를 모두 삭제하시겠습니까? 이 작업은 되돌릴 수 없습니다.", "삭제", "취소")) return;

            if (!EditorUtility.DisplayDialog("!!! 다시 한 번 확인 !!!!",
                "정말 이 에셋의 대화를 모두 삭제할거임? 에셋은 휴지통에도 안 남는다!!!!!", "삭제", "취소")) return;
            
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
        
        [BoxGroup("유틸리티 메서드"), Button("GUID로 Balloon선택")]
        void SelectBalloonFromGUID(string guid)
        {
            foreach (var conversation in Conversations)
            {
                var balloon = conversation.Balloons.Find(x => string.CompareOrdinal(x.GUID, guid) == 0);
                if (balloon == null) continue;

                Selection.activeObject = balloon;
                return;
            }
            Debug.Log($"[{guid}] 해당 GUID를 가진 Balloon이 없음");
        }
#endif
        
    }
}
