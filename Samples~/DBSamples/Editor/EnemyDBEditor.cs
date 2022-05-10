using System.Collections.Generic;
using System.Linq;
using OcDialogue;
using OcDialogue.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace MyDB.Editor
{
    [CustomEditor(typeof(EnemyDB))]
    public class EnemyDBEditor : DBEditorBase
    {
        Enemy _currentEnemy => SelectedData as Enemy;
        
        EnemyDB EnemyDB
        {
            get
            {
                if(_EnemyDB == null) _EnemyDB = target as EnemyDB;
                return _EnemyDB;
            }
        }
        EnemyDB _EnemyDB;
        public override void DrawToolbar()
        {
            serializedObject.Update();
            GUI.color = new Color(0.5f, 2f, 0.7f);
            CurrentCategoryIndex =
                OcEditorUtility.DrawCategory(CurrentCategoryIndex, EnemyDB.Categories, GUILayout.Height(25));
            GUI.color = Color.white;

            SirenixEditorGUI.BeginHorizontalToolbar();
            
            if(SirenixEditorGUI.ToolbarButton("Select DB")) EditorGUIUtility.PingObject(EnemyDB);
            if (SirenixEditorGUI.ToolbarButton("Edit Category"))
            {
                CategoryEditWindow.Open(EnemyDB.Categories, 
                    t => OcEditorUtility.EditCategory(EnemyDB, t, c => AddEnemy(c), d => DeleteEnemy(d as Enemy)));
            }

            if (SirenixEditorGUI.ToolbarButton("Create"))
            {
                var quest = AddEnemy(CurrentCategory);
                Window.ForceMenuTreeRebuild();
                Window.MenuTree.Selection.Add(Window.MenuTree.MenuItems.Find(x => x.Value as Enemy == quest));
            }

            if (_currentEnemy != null && SirenixEditorGUI.ToolbarButton("Delete"))
            {
                DeleteEnemy(_currentEnemy);
                Window.ForceMenuTreeRebuild();
                SelectCategoryLastItem();
            }
            
            SirenixEditorGUI.EndHorizontalToolbar();
            serializedObject.ApplyModifiedProperties();
        }

        public override string[] GetCSVFields()
        {
            return new[] { "분류", "이름", "설명" };
        }

        public override IEnumerable<string[]> GetCSVData()
        {
            foreach (var enemy in EnemyDB.Enemies.OrderBy(x => x.Category).ThenBy(x => x.Name))
            {
                yield return new[] { enemy.Category, enemy.name, enemy.Description };
            }
        }

        public override IEnumerable<LocalizationCSVRow> GetLocalizationData()
        {
            foreach (var enemy in EnemyDB.Enemies.OrderBy(x => x.Category).ThenBy(x => x.name))
            {
                var nameRow = new LocalizationCSVRow();
                nameRow.key = enemy.name;
                nameRow.korean = enemy.name;
                yield return nameRow;
                
                var descriptionRow = new LocalizationCSVRow();
                descriptionRow.key = $"{enemy.name}_Description";
                descriptionRow.korean = enemy.Description;
                yield return descriptionRow;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Enemies"));
            
            if(_currentEnemy != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"[{CurrentCategory} Enemy] {_currentEnemy.name}",
                    new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold }, GUILayout.Height(22));
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }

        public Enemy AddEnemy(string category)
        {
            var Enemy = CreateInstance<Enemy>();
            Enemy.Category = category;
            Enemy.name = OcDataUtility.CalculateDataName($"New {category} Enemy", _EnemyDB.Enemies.Select(x => x.name));
            Enemy.SetParent(_EnemyDB);
            OcDataUtility.Repaint();
            
            var assetFolderPath = AssetDatabase.GetAssetPath(_EnemyDB).Replace($"/{_EnemyDB.name}.asset", "");
            var targetFolderPath = OcDataUtility.CreateFolderIfNull(assetFolderPath, category);
            
            var path = targetFolderPath + $"/{Enemy.name}.asset";
            AssetDatabase.CreateAsset(Enemy, path);
            _EnemyDB.Enemies.Add(Enemy);
            EditorUtility.SetDirty(_EnemyDB);
            AssetDatabase.SaveAssets();

            return Enemy;
        }
        
        public void DeleteEnemy(Enemy Enemy)
        {
            _EnemyDB.Enemies.Remove(Enemy);
            
            OcDataUtility.Repaint();
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(Enemy));
            
            EditorUtility.SetDirty(_EnemyDB);
            AssetDatabase.SaveAssets();
        }
    }
}