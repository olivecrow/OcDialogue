using System;
using OcUtility;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.DB
{
    [CreateAssetMenu(fileName = "DB Manager V2", menuName = "Oc Dialogue/DB V2/DB Manager V2", order = 0)]
    public class DBManagerV2 : AddressableData
    {
        public override AddressableData Parent => null;
        public override string Address => "DB Manager";
        public const string AssetPath = "DB Manager";
        
        public static DBManagerV2 Instance => _instance;
        static DBManagerV2 _instance;

        public DialogueAsset DialogueAsset;
        public GameProcessDB_V2 GameProcessDB;
        public ItemDatabase ItemDatabase;
        public QuestDB QuestDatabase;
        public NPCDB NpcDatabase;
        public EnemyDB EnemyDatabase;
#if UNITY_EDITOR
        /// <summary> Editor Only. </summary>
        [InitializeOnLoadMethod]
        static void EditorInit()
        {
            _instance = Resources.Load<DBManagerV2>(AssetPath);
        }
#endif

        [RuntimeInitializeOnLoadMethod]
        static void RuntimeInit()
        {
            _instance = Resources.Load<DBManagerV2>(AssetPath);
            
            Printer.Print($"[DB Manager V2] RuntimeInit : 모든 데이터 초기화");
            _instance.GameProcessDB.Load();
        }
    }
}