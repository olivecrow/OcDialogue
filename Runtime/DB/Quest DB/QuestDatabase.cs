using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace OcDialogue
{
    [CreateAssetMenu(fileName = "Quest Database", menuName = "Oc Dialogue/DB/Quest Database")]
    public class QuestDatabase : ScriptableObject
    {
        public static QuestDatabase Instance => DBManager.Instance.QuestDatabase;
        public static RuntimeQuestData Runtime => _runtime;
        static RuntimeQuestData _runtime;
        [HideInInspector]public string[] Category;
        [InlineEditor(InlineEditorModes.FullEditor)]public List<Quest> Quests;

        [RuntimeInitializeOnLoadMethod]
        static void RuntimeInit()
        {
            // TODO : 세이브 & 로드 기능 넣기.
            _runtime = new RuntimeQuestData(Instance.Quests);
        }
        
        #if UNITY_EDITOR

        [HideInInspector] public QuestEditorPreset editorPreset;
        

        void Reset()
        {
            if (Category == null || Category.Length == 0) Category = new[] {"Main"};
        }
        public void AddQuest(string category)
        {
            var asset = CreateInstance<Quest>();

            asset.name = OcDataUtility.CalculateDataName("New Quest", Quests.Select(x => x.key));
            asset.key = asset.name;
            asset.Category = category;
            Quests.Add(asset);
            OcDataUtility.Repaint();
            var path = AssetDatabase.GetAssetPath(this).Replace($"{name}.asset", $"{asset.name}.asset");
            AssetDatabase.CreateAsset(asset, path);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        
        public void DeleteQuest(string key)
        {
            if(!EditorUtility.DisplayDialog("삭제?", "정말 해당 Quest를 삭제하겠습니까?", "OK", "Cancel"))
                return;
            var asset = Quests.FirstOrDefault(x => x.key == key);
            if (asset == null)
            {
                Debug.LogWarning($"해당 이름의 Quest가 없어서 삭제에 실패함 : {key}");
                return;
            }

            Quests.Remove(asset);
            
            OcDataUtility.Repaint();
            var path = AssetDatabase.GetAssetPath(asset);
            AssetDatabase.DeleteAsset(path);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        /// <summary> 각종 문제들을 해결함. </summary>
        public void Resolve()
        {
            foreach (var quest in Quests)
            {
                // 각 퀘스트의 DataRowContainer에 대해서 owner를 재설정함.
                quest.DataRowContainer.owner = quest;
                
                // datarow의 ownerDB가 Quest가 아닌 것을 고침.
                foreach (var data in quest.DataRowContainer.dataRows)
                {
                    if(data.ownerDB != DBType.Quest)
                    {
                        Debug.Log($"[{quest.key}] [{data.key}] ownerDB : {data.ownerDB} => Quest");
                        data.ownerDB = DBType.Quest;
                        EditorUtility.SetDirty(quest);
                    }
                }
            }
            
            
            AssetDatabase.SaveAssets();
        }
#endif
    }
}
