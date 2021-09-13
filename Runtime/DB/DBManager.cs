using System;
using OcUtility;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.DB
{
    [CreateAssetMenu(fileName = "DB Manager", menuName = "Oc Dialogue/DB/DB Manager", order = 0)]
    public class DBManager : OcData
    {
        public override OcData Parent => null;
        public override string Address => "DB Manager";
        public const string AssetPath = "DB Manager";
        
        public static DBManager Instance => _instance;
        static DBManager _instance;
        
        public static event Action OnRuntimeInitilized;
        public static bool RuntimeInitialized => _runtimeInitialized;
        static bool _runtimeInitialized;

        public DialogueAsset DialogueAsset;
        public GameProcessDB GameProcessDB;
        public ItemDatabase ItemDatabase;
        public QuestDB QuestDatabase;
        public NPCDB NpcDatabase;
        public EnemyDB EnemyDatabase;
        public bool SaveOnChanged = true;
        
#if UNITY_EDITOR
        /// <summary> Editor Only. </summary>
        [Button][InitializeOnLoadMethod]
        static void EditorInit()
        {
            _instance = Resources.Load<DBManager>(AssetPath);
        }
#endif
        
        [RuntimeInitializeOnLoadMethod]
        static void RuntimeInit()
        {
            _instance = Resources.Load<DBManager>(AssetPath);
            
            Printer.Print($"[DB Manager V2] RuntimeInit : 모든 데이터 초기화");
            _instance.GameProcessDB.Init();
            _instance.QuestDatabase.Init();
            _instance.NpcDatabase.Init();
            _instance.EnemyDatabase.Init();

            _runtimeInitialized = true;
            OnRuntimeInitilized?.Invoke();
        }

        void OnDestroy()
        {
            _runtimeInitialized = false;
        }
    }
}