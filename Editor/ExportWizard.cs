using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OcDialogue.DB;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Settings;

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
        public enum ExportType
        {
            Dialogue,
            ItemDB,
            QuestDB,
            NPCDB,
            EnemyDB
        }

        [EnumToggleButtons]public ExportType type;
        [FolderPath]public string folderPath;
        [SuffixLabel("@labelPreview")]public string fileNamePrefix = "My Project";
        [Space]
        [LabelText("논리적인 순서 사용")] public bool useLogicalOrder = true;
        [Space]
        [ShowIf(nameof(type), ExportType.Dialogue)] public string conversationFieldName = "대화 분류";
        [ShowIf(nameof(type), ExportType.Dialogue)] public string actorFieldName = "인물";
        [ShowIf(nameof(type), ExportType.Dialogue)] public string choiceType_actorName = "(선택지)";
        [ShowIf(nameof(type), ExportType.Dialogue)] public string subtitleFieldName = "대사";
        
        [Space]
        [ShowIf(nameof(type), ExportType.ItemDB)] [LabelText("타입별로 따로 저장")] public bool separateByItemType;

        [ShowIf(nameof(type), ExportType.ItemDB)] [ShowIf(nameof(separateByItemType))] 
        [LabelText("추출할 타입")] [Indent()] [EnumToggleButtons]
        public ExportItemTypeFlags exportItemTypeFlags;
        [ShowIf(nameof(type), ExportType.ItemDB)] [ShowIf(nameof(separateByItemType))] 
        [LabelText("변수 필드 포함")] public bool includeFields;
        [ShowIf(nameof(type), ExportType.ItemDB)] [LabelText("타입 분류를 파일로 Export")] public bool exportItemTypeTerms;
        [ShowIf(nameof(type), ExportType.ItemDB)] public string itemNameFieldName = "아이템";
        [ShowIf(nameof(type), ExportType.ItemDB)] public string itemTypeFieldName = "대분류";
        [ShowIf(nameof(type), ExportType.ItemDB)] public string itemSubtypeFieldName = "소분류";
        [ShowIf(nameof(type), ExportType.ItemDB)] public string itemDescFieldName = "아이템 설명";
        [Space]
        
        [ShowIf(nameof(type), ExportType.QuestDB)] public string questNameFieldName = "타이틀";
        [ShowIf(nameof(type), ExportType.QuestDB)] public string questDescriptionFieldName = "퀘스트 설명";
        
        [ShowIf(nameof(type), ExportType.QuestDB)] public string npcNameFieldName = "이름";
        [ShowIf(nameof(type), ExportType.QuestDB)] public string npcGenderFieldName = "성별";
        [ShowIf(nameof(type), ExportType.QuestDB)] public string npcDescriptionFieldName = "NPC 설명";
        
        [ShowIf(nameof(type), ExportType.QuestDB)] public string enemyNameFieldName = "이름";
        [ShowIf(nameof(type), ExportType.QuestDB)] public string enemyDescriptionFieldName = "몬스터 설명";
        
        [Space]
        public string categoryFieldName = "분류";
        public string guidFieldName = "GUID";
        public string commentFieldName = "비고";

        string labelPreview => $"{fileNamePrefix}_{type}.csv";
        const string key_pathPrefs = "OcDialogue_ExportPath";
        
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
        }

        [Button]
        void Export()
        {
            switch (type)
            {
                case ExportType.Dialogue:
                    ExportDialogue();
                    break;
                case ExportType.ItemDB:
                    ExportItemDB();
                    break;
                case ExportType.QuestDB:
                    ExportQuestDB();
                    break;
                case ExportType.NPCDB:
                    ExportNPCDB();
                    break;
                case ExportType.EnemyDB:
                    ExportEnemyDB();
                    break;
            }
            EditorPrefs.SetString(key_pathPrefs, folderPath);
        }

        [Button("번역용 CSV 테이블 Export")]
        void ExportLocalizationTemplate()
        {
            var keys = new List<string>();
            keys.Add("Key");
            keys.Add("Id");
            keys.AddRange(LocalizationSettings.AvailableLocales.Locales.Select(x => x.Identifier.ToString()));
            var writer = new CSVWriter(keys.ToArray());

            writer.Save(folderPath, $"{fileNamePrefix}_Localization Template");
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
            
            writer.Save(folderPath, $"{fileNamePrefix}_{type}");

            void addData(Conversation conversation, Balloon balloon)
            {
                switch (balloon.type)
                {
                    case Balloon.Type.Entry:
                        return;
                    case Balloon.Type.Dialogue:
                        writer.Add(
                            $"{conversation.Category}/{conversation.key}/{balloon.GUID}",
                            balloon.GUID,
                            balloon.actor == null ? "" : balloon.actor.Name,
                            balloon.text,
                            balloon.description + getAutoComment(balloon)
                        );
                        break;
                    case Balloon.Type.Choice:
                        writer.Add(
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


        #region ItemDB

        void ExportItemDB()
        {
            if (ItemDatabase.Instance == null)
            {
                Debug.LogWarning($"아이템 데이터베이스의 인스턴스가 없음.");
                return;
            }

            if (exportItemTypeTerms)
            {
                var writer = new CSVWriter("대분류", "소분류");
                var typeNames = Enum.GetNames(typeof(ItemType));
                foreach (var typeName in typeNames)
                {
                    var itemType = (ItemType)Enum.Parse(typeof(ItemType), typeName);
                    var subTypeNames = Enum.GetNames(ItemDatabase.GetSubType(itemType));
                    for (int i = 0; i < subTypeNames.Length; i++)
                    {
                        if(i == 0) writer.Add(typeName, subTypeNames[i]);
                        else writer.Add("", subTypeNames[i]);
                    }
                }
                writer.Save(folderPath, $"{fileNamePrefix}_ItemTypes");
            }


            if (separateByItemType)
            {
                var typeNames = Enum.GetNames(typeof(ItemType));
                var key = new []
                {
                    guidFieldName,
                    itemSubtypeFieldName,
                    itemNameFieldName,
                    itemDescFieldName,
                    commentFieldName
                };

                foreach (var itemType in typeNames)
                {
                    var asEnum = (ItemType)Enum.Parse(typeof(ItemType), itemType);
                    if(!exportItemTypeFlags.HasFlag(typeEnumToFlag(asEnum))) continue;
                    var t = getItemType((ItemType)Enum.Parse(typeof(ItemType), itemType));
                    var tKey = new List<string>(key);
                    
                    var fields = t.GetFields(
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                    if(includeFields)
                        foreach (var fieldInfo in fields)
                        {
                            if(fieldInfo.Name == "subtype" || fieldInfo.Name == "description" || 
                               fieldInfo.Name == "GUID" || fieldInfo.Name == "itemName") continue;
                            if(fieldInfo.IsNotSerialized) continue;
                            if(fieldInfo.IsDefined(typeof(CompilerGeneratedAttribute))) continue;
                            tKey.Add($"M_{fieldInfo.Name}");
                        }
                    
                    var writer = new CSVWriter(tKey.ToArray());
                    foreach (var itemBase in ItemDatabase.Instance.Items)
                    {
                        if(itemBase.type.ToString() != itemType) continue;
                        var rowData = new List<string>()
                        {
                            itemBase.GUID.ToString(),
                            itemBase.SubTypeString,
                            itemBase.itemName,
                            itemBase.description,
                            ""
                        };
                        
                        foreach (var fieldInfo in fields)
                        {
                            if(!tKey.Contains($"M_{fieldInfo.Name}")) continue;
                            var casted = cast(itemBase);
                            var value = fieldInfo.GetValue(casted);
                            rowData.Add(value == null ? "" : value.ToString());
                        }
                        
                        writer.Add(rowData.ToArray());
                        
                    }
                    writer.Save(folderPath, $"{fileNamePrefix}_{type}_{itemType}");
                }
            }
            else
            {
                var key = new[]
                {
                    guidFieldName,
                    itemTypeFieldName,
                    itemSubtypeFieldName,
                    itemNameFieldName,
                    itemDescFieldName,
                    commentFieldName
                };
                var writer = new CSVWriter(key);
                foreach (var itemBase in ItemDatabase.Instance.Items.OrderBy(x => x.type))
                {
                    writer.Add(
                        itemBase.GUID.ToString(),
                        itemBase.type.ToString(),
                        itemBase.SubTypeString,
                        itemBase.itemName,
                        itemBase.description,
                        "");
                }
            
                writer.Save(folderPath,$"{fileNamePrefix}_{type}");    
            }


            Type getItemType(ItemType type)
            {
                switch (type)
                {
                    case ItemType.Generic:
                        return typeof(GenericItem);
                    case ItemType.Armor:
                        return typeof(ArmorItem);
                    case ItemType.Weapon:
                        return typeof(WeaponItem);
                    case ItemType.Accessory:
                        return typeof(AccessoryItem);
                    case ItemType.Important:
                        return typeof(ImportantItem);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }

            object cast(ItemBase item)
            {
                switch (item.type)
                {
                    case ItemType.Generic:
                        return item as GenericItem;
                    case ItemType.Armor:
                        return item as ArmorItem;
                    case ItemType.Weapon:
                        return item as WeaponItem;
                    case ItemType.Accessory:
                        return item as AccessoryItem;
                    case ItemType.Important:
                        return item as ImportantItem;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            ExportItemTypeFlags typeEnumToFlag(ItemType t)
            {
                switch (t)
                {
                    case ItemType.Generic:
                        return ExportItemTypeFlags.Generic;
                    case ItemType.Armor:
                        return ExportItemTypeFlags.Armor;
                    case ItemType.Weapon:
                        return ExportItemTypeFlags.Weapon;
                    case ItemType.Accessory:
                        return ExportItemTypeFlags.Accessory;
                    case ItemType.Important:
                        return ExportItemTypeFlags.Important;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(t), t, null);
                }
            }
        }

        #endregion

        #region CommonDB

        void ExportQuestDB()
        {
            if (QuestDB.Instance == null)
            {
                Debug.LogWarning("퀘스트 데이터베이스의 인스턴스가 없음.");
                return;
            }

            var writer = new CSVWriter(categoryFieldName, questNameFieldName, questDescriptionFieldName);

            var quests = useLogicalOrder ? 
                QuestDB.Instance.Quests.OrderBy(x => x.Category + x.e_order + x.Name).ToArray() : 
                QuestDB.Instance.Quests.ToArray();
            
            foreach (var quest in quests)
            {
                writer.Add(quest.Category, quest.Name, quest.Description);
            }
            
            writer.Save(folderPath, $"{fileNamePrefix}_{type}");
        }


        void ExportNPCDB()
        {
            if (NPCDB.Instance == null)
            {
                Debug.LogWarning("NPC 데이터베이스의 인스턴스가 없음.");
                return;
            }

            var writer = new CSVWriter(categoryFieldName, npcNameFieldName, npcGenderFieldName, npcDescriptionFieldName);

            var NPCs = useLogicalOrder ? 
                NPCDB.Instance.NPCs.OrderBy(x => x.Category).ToArray() :
                NPCDB.Instance.NPCs.ToArray();
            
            foreach (var npc in NPCs)
            {
                writer.Add(npc.Category, npc.Name, npc.gender.ToString(), npc.Description);
            }
            
            writer.Save(folderPath, $"{fileNamePrefix}_{type}");
        }
        
        void ExportEnemyDB()
        {
            if (EnemyDB.Instance == null)
            {
                Debug.LogWarning("Enemy 데이터베이스의 인스턴스가 없음.");
                return;
            }

            var writer = new CSVWriter(categoryFieldName, enemyNameFieldName, enemyDescriptionFieldName);

            var Enemies = useLogicalOrder ? 
                EnemyDB.Instance.Enemies.OrderBy(x => x.Category).ToArray() :
                EnemyDB.Instance.Enemies.ToArray();
            
            foreach (var enemy in Enemies)
            {
                writer.Add(enemy.Category, enemy.Name, enemy.Description);
            }
            
            writer.Save(folderPath, $"{fileNamePrefix}_{type}");
        }

        #endregion
        
    }
}
