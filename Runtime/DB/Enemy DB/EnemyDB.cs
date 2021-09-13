using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.DB
{
    [CreateAssetMenu(fileName = "Enemy DB", menuName = "Oc Dialogue/DB/Enemy DB")]
    public class EnemyDB : OcData, IStandardDB
    {
        public string[] CategoryRef => Category;
        public IEnumerable<OcData> AllData => Enemies;
        public override string Address => "EnemyDB";
        public static EnemyDB Instance => DBManager.Instance.EnemyDatabase;
        [HideInInspector]public string[] Category;
        public List<Enemy> Enemies;
        public event Action OnRuntimeValueChanged;
        public void Init()
        {
            foreach (var enemy in Enemies)
            {
                enemy.GenerateRuntimeData();
                enemy.OnRuntimeValueChanged += enemy1 => OnRuntimeValueChanged?.Invoke();
            }
        }
        public List<CommonSaveData> GetSaveData()
        {
            var list = new List<CommonSaveData>();
            foreach (var enemy in Enemies)
            {
                list.Add(enemy.GetSaveData());
            }

            return list;
        }

        public void Overwrite(IEnumerable<CommonSaveData> data)
        {
            foreach (var enemy in Enemies)
            {
                var targetData = data.FirstOrDefault(x => x.Key == enemy.Name);
                if (targetData == null)
                {
                    Debug.LogError($"해당 키값의 CommonSaveData가 없음 | key : {enemy.Name}");
                    continue;
                }
                enemy.Load(targetData);
            }
        }
        
#if UNITY_EDITOR
        void Reset()
        {
            if (Category == null || Category.Length == 0) Category = new[] {"Main"};
        }
        public Enemy AddEnemy(string category)
        {
            var enemy = CreateInstance<Enemy>();

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
                var allAssets = AssetDatabase.LoadAllAssetsAtPath(path).Select(x => x as Enemy);
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
