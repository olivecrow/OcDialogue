using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcDialogue.DB;
using UnityEngine;

namespace OcDialogue.Samples
{
    public static class EasySaveIntegration
    {
        public static ES3Settings ES3Settings;
        const string saveDataKey_PresetNames = "Preset";
        const string saveDataKey_DataRowContainer = "DataRowContainer";
        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            if (!DBManager.RuntimeInitialized)
            {
                DBManager.OnRuntimeInitialized += Init;
                return;
            }
            ES3.CacheFile();
            ES3Settings = new ES3Settings(ES3.Location.Cache);
            LoadAllDBs();
            InitSaveCallbacks();
        }

        static void LoadAllDBs(string presetName = "")
        {
            foreach (var db in DBManager.Instance.DBs)
            {
                Load(db, presetName);
            }
        }

        static void Load(OcDB db, string presetName)
        {
            if (!ES3.KeyExists(Name(db.Address, presetName), ES3Settings)) return;
            var saveData = ES3.Load<List<CommonSaveData>>(Name(db.Address, presetName), ES3Settings);
            
            db.Overwrite(saveData);
        }

        static void InitSaveCallbacks()
        {
            foreach (var db in DBManager.Instance.DBs) db.OnRuntimeValueChanged += () => SaveDB(db);
        }
        
        /// <summary>
        /// 데이터를 수동으로 저장함.
        /// </summary>
        public static void Save(string presetName = "", bool saveAsFile = false)
        {
            foreach (var db in DBManager.Instance.DBs)
            {
                SaveDB(db, presetName);
            }
            SavePresetName(presetName);
            if(saveAsFile)
            {
                ES3.StoreCachedFile(ES3Settings);
                Debug.Log($"[EasySave Integration] Store Cached File");
            }
        }

        public static void SaveDB(OcDB db, string presetName = "", bool saveAsFile = false)
        {
            ES3.Save(Name(db.Address, presetName), db.GetSaveData(), ES3Settings);
            SavePresetName(Name(db.Address, presetName));
            if(saveAsFile)
            {
                ES3.StoreCachedFile(ES3Settings);
                Debug.Log($"[EasySave Integration] Store Cached File ({db.name})");
            }
        }

        public static void StoreCachedFile()
        {
            ES3.StoreCachedFile(ES3Settings);
            Debug.Log($"[EasySave Integration] Store Cached File");
        }

        public static void LoadFromPreset(string presetName)
        {
            if (!ES3.KeyExists(saveDataKey_PresetNames, ES3Settings))
            {
                Debug.LogWarning($"저장된 프리셋이 없음");
                return;
            }
            LoadAllDBs(presetName);
        }
        public static void LoadFromPreset(OcDB db, string presetName)
        {
            if (!ES3.KeyExists(Name(db.Address, saveDataKey_PresetNames), ES3Settings))
            {
                Debug.LogWarning($"저장된 프리셋이 없음");
                return;
            }

            var saveData = ES3.Load<List<CommonSaveData>>(Name(db.Address, presetName), ES3Settings);
            db.Overwrite(saveData);
        }

        public static List<string> GetPresetNames()
        {
            return ES3.KeyExists(saveDataKey_PresetNames, ES3Settings) ? 
                ES3.Load<List<string>>(saveDataKey_PresetNames, ES3Settings) : new List<string>();
        }
        public static List<string> GetPresetNames(OcDB db)
        {
            return ES3.KeyExists(saveDataKey_PresetNames, ES3Settings) ?
                ES3.Load<List<string>>(saveDataKey_PresetNames, ES3Settings).Where(x => x.Contains(db.Address)).ToList() :
                new List<string>();
        }

        public static void RemovePreset(string presetName)
        {
            var keys = GetPresetNames();
            keys.Remove(presetName);
            ES3.Save(saveDataKey_PresetNames, keys, ES3Settings);
        }
        public static void RemovePreset(OcDB db, string presetName)
        {
            var keys = GetPresetNames(db);
            keys.Remove(presetName);
            ES3.Save(Name(db.Address, saveDataKey_PresetNames), keys, ES3Settings);
        }

        static void SavePresetName(string presetName)
        {
            if(string.IsNullOrWhiteSpace(presetName)) return;
            var existPresets = GetPresetNames();
            if(!existPresets.Contains(presetName))
            {
                existPresets.Add(presetName);
                ES3.Save(saveDataKey_PresetNames, existPresets, ES3Settings);
            }
        }

        static string Name(string address, string presetName) => $"{presetName}({address})";
    }

}