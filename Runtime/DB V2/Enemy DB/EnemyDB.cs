using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.DB
{
    [CreateAssetMenu(fileName = "Enemy DB", menuName = "Oc Dialogue/DB V2/Enemy DB")]
    public class EnemyDB : AddressableData, IStandardDB
    {
        public string[] CategoryRef => Category;
        public IEnumerable<AddressableData> AllData => Enemies;
        public override string Address => "EnemyDB";
        public static EnemyDB Instance => DBManagerV2.Instance.EnemyDatabase;
        [HideInInspector]public string[] Category;
        public List<EnemyV2> Enemies;

        
#if UNITY_EDITOR
        void Reset()
        {
            if (Category == null || Category.Length == 0) Category = new[] {"Main"};
        }
        public EnemyV2 AddEnemy(string category)
        {
            var enemy = CreateInstance<EnemyV2>();

            enemy.Category = category;
            enemy.name = OcDataUtility.CalculateDataName($"New {category} Enemy", Enemies.Select(x => x.name));
            enemy.SetParent(this);
            
            Enemies.Add(enemy);
            OcDataUtility.Repaint();
            var assetFolderPath = AssetDatabase.GetAssetPath(this).Replace($"/{name}.asset", "");
            var targetFolderPath = OcDataUtility.CreateFolderIfNull(assetFolderPath, category);
            
            var path = targetFolderPath + $"/{enemy.name}.asset";
            
            AssetDatabase.CreateAsset(enemy, path);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();

            return enemy;
        }
        
        public void DeleteEnemy(string key)
        {
            var enemy = Enemies.FirstOrDefault(x => x.name == key);
            if (enemy == null)
            {
                var path = AssetDatabase.GetAssetPath(this);
                var allAssets = AssetDatabase.LoadAllAssetsAtPath(path).Select(x => x as EnemyV2);
                enemy = allAssets.FirstOrDefault(x => x.name == key);
                if(enemy == null)
                {
                    Debug.LogWarning($"해당 이름의 enemy가 없어서 삭제에 실패함 : {key}");
                    return;
                }
            }

            Enemies.Remove(enemy);
            
            OcDataUtility.Repaint();
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(enemy));
            Enemies = Enemies.Where(x => x != null).ToList();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        
        /// <summary> 각종 문제들을 해결함. </summary>
        public void Resolve()
        {
            foreach (var enemy in Enemies)
            {
                enemy.SetParent(this);
                enemy.Resolve();
            }
            
            
            AssetDatabase.SaveAssets();
        }
#endif
    }
}
