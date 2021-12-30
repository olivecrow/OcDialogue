using System;
using System.Collections.Generic;
using System.Linq;
using OcDialogue;
using OcDialogue.DB;
using OcDialogue.Editor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyDB.Editor
{
    [CustomEditor(typeof(GameProcessDB))]
    public sealed class GameProcessDBEditor : DBEditorBase
    {
        GameProcessData CurrentData => SelectedData as GameProcessData;

        GameProcessDB GameProcessDB
        {
            get
            {
                if(_gameProcessDB == null) _gameProcessDB = target as GameProcessDB;
                return _gameProcessDB;
            }
        }
        GameProcessDB _gameProcessDB;
        public override void DrawToolbar()
        {
            serializedObject.Update();
            GUI.color = new Color(2f, 0.5f, 0.8f);
            CurrentCategoryIndex =
                OcEditorUtility.DrawCategory(CurrentCategoryIndex, GameProcessDB.Categories, GUILayout.Height(25));
            GUI.color = Color.white;

            SirenixEditorGUI.BeginHorizontalToolbar();
            
            if(SirenixEditorGUI.ToolbarButton("Select DB")) EditorGUIUtility.PingObject(GameProcessDB);
            if (SirenixEditorGUI.ToolbarButton("Edit Category"))
            {
                CategoryEditWindow.Open(GameProcessDB.Categories, 
                    t => OcEditorUtility.EditCategory(GameProcessDB, t, a => AddData(a), d => DeleteData(d as GameProcessData)));
            }
            
            SirenixEditorGUI.EndHorizontalToolbar();
            serializedObject.ApplyModifiedProperties();
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Data"));
            if (CurrentData == null)
            {
                GUILayout.Label($"현재 카테고리에 맞는 데이터가 없음 : {CurrentCategory}");
                if (GUILayout.Button($"GameProcessData 생성"))
                {
                    SelectedData = AddData(CurrentCategory);
                }
            }
            else
            {
                EditorGUILayout.LabelField($"[GameProcessData] {CurrentData.name}", 
                    new GUIStyle(GUI.skin.label){fontSize = 20, fontStyle = FontStyle.Bold}, GUILayout.Height(22));
            }

            serializedObject.ApplyModifiedProperties();
        }

        GameProcessData AddData(string category)
        {
            var data = CreateInstance<GameProcessData>();
            Undo.RecordObject(data, "Add Data");
            data.category = category;
            data.name = category;
            data.SetParent(GameProcessDB);
            if (data.dataRowContainer == null) data.dataRowContainer = new DataRowContainer();
            data.dataRowContainer.Parent = data;
            EditorUtility.SetDirty(data);

            var path = AssetDatabase.GetAssetPath(GameProcessDB)
                .Replace($"{GameProcessDB.name}.asset", $"{data.name}.asset");
            Debug.Log($"[GameProcessData] 에셋 생성 | category : {category} | path : {path}");
            GameProcessDB.Data.Add(data);
            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();
            return data;
        }

        void DeleteData(GameProcessData data)
        {
            var path = AssetDatabase.GetAssetPath(data);
            GameProcessDB.Data.Remove(data);
            Debug.Log($"[GameProcessData] 에셋 삭제 | category : {data.category} | path : {path}");
            AssetDatabase.DeleteAsset(path);
            EditorUtility.SetDirty(GameProcessDB);
            AssetDatabase.SaveAssets();
        }
    }
}