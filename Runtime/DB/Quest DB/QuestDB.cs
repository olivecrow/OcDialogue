using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.DB
{
    [CreateAssetMenu(fileName = "Quest DB", menuName = "Oc Dialogue/DB/Quest DB")]
    public class QuestDB : OcData, IStandardDB
    {
        public string[] CategoryRef => Category;
        public IEnumerable<OcData> AllData => Quests;
        public override string Address => "QuestDB";
        public static QuestDB Instance => DBManager.Instance.QuestDatabase;
        [HideInInspector]public string[] Category;
        public List<Quest> Quests;

        public void Load()
        {
            foreach (var quest in Quests)
            {
                quest.GenerateRuntimeData();
            }
        }


#if UNITY_EDITOR
        void Reset()
        {
            if (Category == null || Category.Length == 0) Category = new[] {"Main"};
        }
        public Quest AddQuest(string category)
        {
            var asset = CreateInstance<Quest>();

            asset.name = OcDataUtility.CalculateDataName($"New {category} Quest", Quests.Select(x => x.name));
            asset.Category = category;
            asset.SetParent(this);
            Quests.Add(asset);
            OcDataUtility.Repaint();
            var assetFolderPath = AssetDatabase.GetAssetPath(this).Replace($"/{name}.asset", "");
            var targetFolderPath = OcDataUtility.CreateFolderIfNull(assetFolderPath, category);
            
            var path = targetFolderPath + $"/{asset.name}.asset";
            AssetDatabase.CreateAsset(asset, path);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            return asset;
        }
        
        public void DeleteQuest(string key)
        {
            var asset = Quests.FirstOrDefault(x => x.name == key);
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
                quest.SetParent(this);
                quest.Resolve();
            }
            
            
            AssetDatabase.SaveAssets();
        }
#endif
    }
}
