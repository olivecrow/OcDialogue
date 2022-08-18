using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OcDialogue.DB;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class DBExportWizard : OdinEditorWindow
    {
        [InlineButton(nameof(Clear))]public MyCSV csv;
        [ValueDropdown(nameof(GetDBDropdown))]public ScriptableObject TargetDB;
        [MenuItem("OcDialogue/DB Export Wizard")]
        static void Open()
        {
            var window = GetWindow<DBExportWizard>();
            window.minSize = new Vector2(520, 520);
            window.Show();
        }

        void Clear()
        {
            csv = new MyCSV(new string[]{});
        }

        protected override void OnBeginDrawEditors()
        {
            GUILayout.Box("DB를 Export하면 기존의 데이터를 완전히 덮어씀. 그냥 새로 저장할 뿐임. \n" +
                          "그러니 나중에 번역한 후에 수정사항이 생길경우엔 그때 알아서 Key값으로 행을 덮어씌우는 코드를 짤 것.");
        }

        ValueDropdownList<ScriptableObject> GetDBDropdown()
        {
            var list = new ValueDropdownList<ScriptableObject>();
            if (DBManager.Instance == null) return null;

            if (DBManager.Instance.DialogueAsset != null) list.Add(DBManager.Instance.DialogueAsset);
            foreach (var db in DBManager.Instance.DBs)
            {
                if (db is not ICSVExportable) continue;
                list.Add(db);
            }

            return list;
        }

        [Button]
        public void Read([Sirenix.OdinInspector.FilePath(AbsolutePath = true)]string filePath)
        {
            csv = new MyCSV(filePath);
        }

        [Button]
        public void ExportCSV()
        {
            if(TargetDB == null)
                throw new NullReferenceException("DB가 선택되지 않음");
            var asExporter = TargetDB as ICSVExportable;

            csv = new MyCSV(asExporter.GetLocalizationCSVHeader());
            foreach (var row in asExporter.GetLocalizationCSVLines())
            {
                csv.AppendLine(
                    row.additional1, // 대화명 
                    row.additional2, // 인물
                    row.key,         // Key
                    row.korean,      // Korean(ko)
                    row.additional3  // 비고
                );
            }
            
            var path = EditorUtility.SaveFilePanel("CSV Save", Application.dataPath, "New CSV", "csv");
            
            // canceled
            if(string.IsNullOrWhiteSpace(path)) return;
            
            csv.SaveAsFile(path);
            var relativePath = "Assets" + path.Substring(Application.dataPath.Length);
            if(path.Contains(Application.dataPath)) AssetDatabase.ImportAsset(relativePath);
        }

        [Button, PropertyTooltip("번역된 테이블을 붙여넣고 로드하기 위해 헤더만 있는 CSV파일을 추출함.")]
        public void ExportLocalizationTable()
        {
            var header = new List<string>();
            header.Add("Key");
            header.Add("Id");
            var availableLocales = LocalizationEditorSettings.GetLocales();
            header.AddRange(availableLocales.Select(x => x.Identifier.ToString()));
            header.Add("Shared Comments");
            var csvTable = new MyCSV(header.ToArray());
            
            var path = EditorUtility.SaveFilePanel("CSV Save", Application.dataPath, "Localization Table", "csv");
            csvTable.SaveAsFile(path);
            var relativePath = "Assets" + path.Substring(Application.dataPath.Length);
            if(path.Contains(Application.dataPath)) AssetDatabase.ImportAsset(relativePath);
        }
    }
}