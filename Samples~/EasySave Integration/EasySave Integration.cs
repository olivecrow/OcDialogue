using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OcDialogue.DB;
using OcUtility;
using UnityEngine;

namespace OcDialogue.Samples
{
    public static class EasySaveIntegration
    {
        public static bool DebugLog { get; set; }
        public static ES3Settings ES3Settings;
        public static event Action OnSoftSaved;
        const string saveDataKey_PresetNames = "Preset";
        
        public static void Init()
        {
            if (!DBManager.RuntimeInitialized)
            {
                DBManager.OnRuntimeInitialized += Init;
                return;
            }
            ES3.CacheFile();
            ES3Settings = new ES3Settings(ES3.Location.Cache);
            InitSaveCallbacks();

#if UNITY_EDITOR

            var str = "";
            var names = GetPresetNames();
            names.ForEach(x => str += $"\n{x}");
            Debug.Log($"[EasySave Integration] 초기화\n" +
                          $"현재 저장된 프리셋 : {names.Count}\n" +
                          $"{str}");
#endif
        }
        static string Name(string address, string presetName) => $"{presetName}({address})";

        static void InitSaveCallbacks()
        {
            foreach (var db in DBManager.Instance.DBs) db.OnRuntimeValueChanged += () => SaveDB(db, "");
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
                Debug.Log($"[EasySave Integration] 게임 저장".Rich(Color.cyan));
            }
        }

        public static void SaveDB(OcDB db, string presetName = "", bool saveAsFile = false)
        {
            var saveData = db.GetSaveData();
            if(DebugLog)
            {
                if (saveData != null)
                {
                    var sb = new StringBuilder();
                    sb.Append($"SaveData ==> {Name(db.Address, presetName)}");
                    foreach (var data in saveData)
                    {
                        sb.Append($"\nKey : {data.Key}");
                        sb.Append("\nData");
                        if (data.Data != null)
                        {
                            foreach (var dict in data.Data)
                            {
                                sb.Append($"\n   {dict.Key} : {dict.Value}");
                            }
                        }

                        sb.Append("\nDataRowContainer");
                        if (data.DataRowContainerDict != null)
                        {
                            foreach (var dict in data.DataRowContainerDict)
                            {
                                sb.Append($"\n   {dict.Key} : {dict.Value}");
                            }
                        }
                    }

                    Debug.Log($"[EasySave Integration] SaveDB ({db.name}) | as file ? {saveAsFile.DRT()} | saveData is null ? {(saveData == null).DRT()}");
                }
            }
            ES3.Save(Name(db.Address, presetName), saveData, ES3Settings);
            SavePresetName(Name(db.Address, presetName));
            if(saveAsFile)
            {
                ES3.StoreCachedFile(ES3Settings);
            }
            else
            {
                OnSoftSaved?.Invoke();
            }
        }
        static void SavePresetName(string presetName)
        {
            // if(string.IsNullOrWhiteSpace(presetName)) return;
            var existPresets = GetPresetNames();
            if(!existPresets.Contains(presetName))
            {
                existPresets.Add(presetName);
                ES3.Save(saveDataKey_PresetNames, existPresets, ES3Settings);
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
                Debug.LogWarning($"[EasySave Integration] 저장된 프리셋이 없음 | presetName : {presetName}");
                return;
            }

            Debug.Log($"[EasySave Integration]Load Save Data | presetName : {presetName}");
            foreach (var db in DBManager.Instance.DBs)
            {
                LoadFromPreset(db, presetName);
            }
        }
        public static void LoadFromPreset(OcDB db, string presetName)
        {
            if (!ES3.KeyExists(Name(db.Address, presetName), ES3Settings))
            {
                Debug.LogWarning($"[EasySave Integration] {db.name} | ES3에 해당 키가 없음 | key : {Name(db.Address, presetName)} | presetName : {presetName}");
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

        public static bool HasPreset()
        {
            return GetPresetNames().Count > 0;
        }
        public static bool HasPreset(string presetName)
        {
            return GetPresetNames().Contains(presetName);
        }
    }

}