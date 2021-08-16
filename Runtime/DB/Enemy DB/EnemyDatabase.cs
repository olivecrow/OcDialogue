using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OcDialogue
{
    [CreateAssetMenu(fileName = "Enemy Database", menuName = "Oc Dialogue/DB/Enemy Database")]
    public class EnemyDatabase : ScriptableObject
    {
        public static EnemyDatabase Instance => DBManager.Instance.EnemyDatabase;
        public static RuntimeEnemyData Runtime => _runtime;
        static RuntimeEnemyData _runtime;
        
#if UNITY_EDITOR
        [HideInInspector] public EnemyEditorPreset editorPreset;
#endif
        [HideInInspector] public string[] Category;
        public List<Enemy> Enemies;
        
        [RuntimeInitializeOnLoadMethod]
        static void RuntimeInit()
        {
            // TODO : 세이브 & 로드 기능 넣기.
            _runtime = new RuntimeEnemyData(Instance.Enemies);
        }
        
        
        
        #if UNITY_EDITOR
        /// <summary> Editor Only. 이름 수정 후 매칭할때 사용함. </summary>
        [HideInInspector]public bool isNameDirty;
        
        void OnValidate()
        {
            foreach (var enemy in Enemies)
            {
                if(enemy.name == enemy.key) continue;
                isNameDirty = true;
                break;
            }
        }
        
        public void MatchAllNames()
        {
            foreach (var enemy in Enemies)
            {
                if(enemy.name == enemy.key) continue;
                enemy.name = enemy.key;
            }

            isNameDirty = false;
            AssetDatabase.SaveAssets();
        }
        
        public void AddEnemy(string category)
        {
            var enemy = CreateInstance<Enemy>();

            enemy.Category = category;
            enemy.name = OcDataUtility.CalculateDataName("New Enemy", Enemies.Select(x => x.key));
            enemy.key = enemy.name;
            Enemies.Add(enemy);
            OcDataUtility.Repaint();
            var path = AssetDatabase.GetAssetPath(this).Replace($"{name}.asset", $"{enemy.name}.asset");
            AssetDatabase.AddObjectToAsset(enemy, this);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        
        public void DeleteEnemy(string key)
        {
            if(!EditorUtility.DisplayDialog("삭제?", "정말 해당 Enemy를 삭제하겠습니까?", "OK", "Cancel"))
                return;
            var enemy = Enemies.FirstOrDefault(x => x.key == key);
            if (enemy == null)
            {
                var path = AssetDatabase.GetAssetPath(this);
                var allAssets = AssetDatabase.LoadAllAssetsAtPath(path).Select(x => x as Enemy);
                enemy = allAssets.FirstOrDefault(x => x.key == key);
                if(enemy == null)
                {
                    Debug.LogWarning($"해당 이름의 enemy가 없어서 삭제에 실패함 : {key}");
                    return;
                }
            }

            Enemies.Remove(enemy);
            
            OcDataUtility.Repaint();
            AssetDatabase.RemoveObjectFromAsset(enemy);
            Enemies = Enemies.Where(x => x != null).ToList();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public void Resolve()
        {
            MatchAllNames();
            foreach (var enemy in Enemies)
            {
                // 각 퀘스트의 DataRowContainer에 대해서 owner를 재설정함.
                enemy.DataRowContainer.owner = enemy;
                if (string.IsNullOrWhiteSpace(enemy.Category) && Category.Length > 0) enemy.Category = Category[0];

                // datarow의 ownerDB가 enemy가 아닌 것을 고침.
                foreach (var data in enemy.DataRowContainer.dataRows)
                {
                    if(data.ownerDB != DBType.Enemy)
                    {
                        Debug.Log($"[{enemy.key}] [{data.key}] ownerDB : {data.ownerDB} => Enemy");
                        data.ownerDB = DBType.Enemy;
                        EditorUtility.SetDirty(enemy);
                    }
                }
            }
        }
#endif
    }
}
