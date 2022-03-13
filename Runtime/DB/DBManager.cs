using System;
using System.Collections.Generic;
using OcUtility;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.DB
{
    [CreateAssetMenu(fileName = "DB Manager", menuName = "Oc Dialogue/DB/DB Manager", order = 0)]
    public class DBManager : ScriptableObject
    {
        public static DBManager Instance
        {
            get
            {
                if (_instance == null) _instance = Resources.Load<DBManager>("DB Manager");
                return _instance;
            }
        }
        static DBManager _instance;
        
        public static event Action OnRuntimeInitialized;
        public static bool RuntimeInitialized => _runtimeInitialized;
        static bool _runtimeInitialized;

        public DialogueAsset DialogueAsset;

        public List<OcDB> DBs;

#if UNITY_EDITOR
        /// <summary> Editor Only. </summary>
        [Button][InitializeOnLoadMethod]
        static void EditorInit()
        {
            _instance = Resources.Load<DBManager>("DB Manager");
            Application.quitting += Release;
        }

        static void Release()
        {
            _runtimeInitialized = false;
            Application.quitting -= Release;
        }
#endif
        
        [RuntimeInitializeOnLoadMethod]
        static void RuntimeInit()
        {
            _instance = Resources.Load<DBManager>("DB Manager");
            
            Printer.Print($"[DB Manager V2] RuntimeInit : 모든 데이터 초기화");
            foreach (var db in _instance.DBs)
            {
                db.Init();
            }

            _runtimeInitialized = true;
            OnRuntimeInitialized?.Invoke();
            OnRuntimeInitialized = null;
        }
    }
}