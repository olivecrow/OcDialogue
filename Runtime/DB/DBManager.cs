using System;
using System.Linq;
using System.Collections.Generic;
using OcUtility;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

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
        public DialogueAsset DialogueAsset;

        public List<OcDB> DBs;
        
     

#if UNITY_EDITOR
        
        [InitializeOnLoadMethod]
        static void EditorInit()
        {
            _instance = Resources.Load<DBManager>("DB Manager");
        }
        
        /// <summary>
        /// (Editor Only) OcData에 사용할 id를 생성함.
        /// </summary>
        /// <returns></returns>
        public static int GenerateID()
        {
            while (true)
            {
                var id = Random.Range(int.MinValue, int.MaxValue);
                if (_instance == null) return id;
                if (_instance.DBs.Count == 0) return id;
                if (_instance.DBs.All(db => db.AllData.All(x => x.id != id))) return id;
            }
        }
#endif

        public void UnInitialization()
        {
            foreach (var db in DBs)
            {
                db.UnInitialization();
            }
        }
    }
}