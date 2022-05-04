using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OcDialogue.DB;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Settings;

#if ENABLE_LOCALIZATION
using UnityEngine.Localization.Settings;
#endif


namespace OcDialogue.Editor
{
    [Flags]
    public enum ExportItemTypeFlags
    {
        None = 0,
        Generic = 2,
        Armor = 4,
        Weapon = 8,
        Accessory = 16,
        Important = 32,
        All = int.MaxValue
    }
    public class ExportWizard : OdinEditorWindow
    {
        public enum ExportTarget
        {
            DialogueDB,
            OtherDB
        }

        [EnumToggleButtons]public ExportTarget exportTarget;
        [ShowIf(nameof(exportTarget), ExportTarget.OtherDB)] [ValueDropdown(nameof(GetAvailableDBList))] public OcDB targetDB;

        [FolderPath]public string folderPath = "Asset";
        [SuffixLabel("@labelPreview")]public string fileNamePrefix = "My Project";
        [Space]
        [LabelText("논리적인 순서 사용")] public bool useLogicalOrder = true;
        [Space]
        [ShowIf(nameof(exportTarget), ExportTarget.DialogueDB)] public string conversationFieldName = "대화 분류";
        [ShowIf(nameof(exportTarget), ExportTarget.DialogueDB)] public string actorFieldName = "인물";
        [ShowIf(nameof(exportTarget), ExportTarget.DialogueDB)] public string choiceType_actorName = "(선택지)";
        [ShowIf(nameof(exportTarget), ExportTarget.DialogueDB)] public string subtitleFieldName = "대사";
        
        [Space]
        [ShowIf(nameof(exportTarget), ExportTarget.DialogueDB)] public string categoryFieldName = "분류";
        [ShowIf(nameof(exportTarget), ExportTarget.DialogueDB)] public string guidFieldName = "GUID";
        [ShowIf(nameof(exportTarget), ExportTarget.DialogueDB)] public string commentFieldName = "비고";

        string _fileName => $"{fileNamePrefix}_{(exportTarget == ExportTarget.DialogueDB ? "Dialogue" : targetDB == null ? "null" : targetDB.name)}";
        string labelPreview => $"{_fileName}.csv";
        const string key_pathPrefs = "OcDialogue_ExportPath";
        const string key_prefixPrefs = "OcDialogue_ExportPrefix";
        
        [MenuItem("OcDialogue/Export Wizard")]
        static void Open()
        {
            var wnd = GetWindow<ExportWizard>(true);
            wnd.minSize = new Vector2(720, 480);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            folderPath = EditorPrefs.GetString(key_pathPrefs);
            fileNamePrefix = EditorPrefs.HasKey(key_prefixPrefs) ? EditorPrefs.GetString(key_prefixPrefs) : "My Project";
        }

        [Button]
        void Export()
        {
            switch (exportTarget)
            {
                case ExportTarget.DialogueDB:
                    ExportDialogue();
                    break;
                case ExportTarget.OtherDB:
                    ExportTargetDB();
                    break;
            }
            EditorPrefs.SetString(key_pathPrefs, folderPath);
            EditorPrefs.SetString(key_prefixPrefs, fileNamePrefix);
        }

        [Button("번역용 CSV 테이블 Export")]
        void ExportLocalizationTemplate()
        {
            if (exportTarget == ExportTarget.OtherDB && targetDB == null)
            {
                Debug.LogWarning($"선택된 DB가 없음");
                return;
            }
            
            var fileName = exportTarget == ExportTarget.DialogueDB ?
                $"{fileNamePrefix}_Dialogue Localization" : $"{fileNamePrefix}_{targetDB.name} Localization";
            var path = $"{folderPath}/{fileName}.csv";
            if (File.Exists(path))
            {
                if(!EditorUtility.DisplayDialog("이미 파일이 존재함", "이미 존재하는 파일을 덮어씌우시겠습니까?", "덮어쓰기", "취소")) return;
            }
            var keys = new List<string>();
            keys.Add("Key");
            keys.Add("Id");
            keys.Add("Shared Comments");
            keys.AddRange(LocalizationSettings.AvailableLocales.Locales.Select(x => x.Identifier.ToString()));
            var koreanTableName = LocalizationSettings.AvailableLocales.Locales.
                FirstOrDefault(x => x.Identifier.ToString().Contains("Korean"))!.Identifier;
            var indexOfKorean = keys.IndexOf(x => x == koreanTableName.ToString());
            var writer = new CSVWriter(keys.ToArray());

            switch (exportTarget)
            {
                case ExportTarget.DialogueDB:

                    Debug.Log($"Dialogue DB에 대해선 따로 번역 테이블 생성이 없음. 그냥 Export한다음에 빈 테이블에 붙여넣을것.");
                    return;
                case ExportTarget.OtherDB:
                    var editor = UnityEditor.Editor.CreateEditor(targetDB) as IDBEditor;
                    foreach (var data in editor.GetLocalizationData())
                    {
                        var row = new string[keys.Count];
                        row[0] = data.key;
                        row[indexOfKorean] = data.korean;
                        writer.Add(row);
                    }
                    break;
            }

            writer.Save(folderPath, fileName);
            
            EditorPrefs.SetString(key_pathPrefs, folderPath);
            EditorPrefs.SetString(key_prefixPrefs, fileNamePrefix);
        }

        [Button("빈 Localization 테이블 생성하기")]
        void ExportEmptyLocalizationTable()
        {
            if (File.Exists($"{folderPath}/New Empty Localization table.csv"))
            {
                Debug.LogWarning($"이미 {folderPath}에 빈 LocalizationTable이 존재함");
                return;
            }
            var keys = new List<string>();
            keys.Add("Key");
            keys.Add("Id");
            keys.Add("Shared Comments");
            keys.AddRange(LocalizationSettings.AvailableLocales.Locales.Select(x => x.Identifier.ToString()));
            var writer = new CSVWriter(keys.ToArray());
            writer.Save(folderPath, "New Empty Localization Table");
        }

        void ExportTargetDB()
        {
            var editor = UnityEditor.Editor.CreateEditor(targetDB) as IDBEditor;
            var key = editor.GetCSVFields();

            var writer = new CSVWriter(key);
            foreach (var data in editor.GetCSVData())
            {
                writer.Add(data);
            }
            
            writer.Save(folderPath, $"{fileNamePrefix}_{targetDB.name}");
        }


        #region Dialogue

        void ExportDialogue()
        {
            if (DialogueAsset.Instance == null)
            {
                Debug.LogWarning($"DialogueAsset의 인스턴스가 없음.");
                return;
            }
            var key = new []
            {
                categoryFieldName,
                conversationFieldName,
                guidFieldName,
                actorFieldName,
                subtitleFieldName,
                commentFieldName
            };
            

            var writer = new CSVWriter(key);
            foreach (var conversation in DialogueAsset.Instance.Conversations)
            {
                List<Balloon> balloons;
                if (useLogicalOrder)
                {
                    balloons = new List<Balloon>();
                    foreach (var balloon in conversation.Balloons)
                    {
                        if(balloons.Contains(balloon)) continue;
                        balloons.Add(balloon);
                        queryLinkedBalloon(balloons, balloon);
                    }
                }
                else
                {
                    balloons = conversation.Balloons;
                }
                
                foreach (var balloon in balloons)
                {
                    addData(conversation, balloon);
                }
            }
            
            writer.Save(folderPath, _fileName);

            void addData(Conversation conversation, Balloon balloon)
            {
                switch (balloon.type)
                {
                    case Balloon.Type.Entry:
                        return;
                    case Balloon.Type.Dialogue:
                        writer.Add(
                            conversation.Category,
                            $"{conversation.Category}/{conversation.key}",
                            balloon.GUID,
                            balloon.actor == null ? "" : balloon.actor.name,
                            balloon.text,
                            balloon.description + getAutoComment(balloon)
                        );
                        break;
                    case Balloon.Type.Choice:
                        writer.Add(
                            conversation.Category,
                            $"{conversation.Category}/{conversation.key}/{balloon.GUID}",
                            balloon.GUID,
                            choiceType_actorName,
                            balloon.text,
                            balloon.description + getAutoComment(balloon)
                        );
                        break;
                    case Balloon.Type.Action:
                        return;
                }
            }

            void queryLinkedBalloon(ICollection<Balloon> balloons, Balloon balloon)
            {
                foreach (var linkedBalloon in balloon.linkedBalloons)
                {
                    if(balloons.Contains(linkedBalloon)) continue;
                    balloons.Add(linkedBalloon);
                    queryLinkedBalloon(balloons, linkedBalloon);
                }
            }

            string getAutoComment(Balloon balloon)
            {
                var comment = string.IsNullOrWhiteSpace(balloon.description) ? "" : "\r";
                if (balloon.linkedBalloons.Any(x => x.type == Balloon.Type.Choice))
                {
                    comment += "-> 선택지 제시 ";
                    foreach (var choice in balloon.linkedBalloons.Where(x => x.type == Balloon.Type.Choice))
                    {
                        comment += $"[{choice.text}]";
                    }
                }

                if (balloon.type == Balloon.Type.Choice && balloon.linkedBalloons == null || balloon.linkedBalloons.Count == 0)
                {
                    comment += "대화 종료";
                }
                
                return comment;
            }
        }

        #endregion


        ValueDropdownList<OcDB> GetAvailableDBList()
        {
            var list = new ValueDropdownList<OcDB>();
            if (DBManager.Instance == null) return list;
            foreach (var db in DBManager.Instance.DBs)
            {
                list.Add(db);
            }

            return list;
        }
    }
}
