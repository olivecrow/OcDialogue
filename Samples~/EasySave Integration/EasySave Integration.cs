using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue.DB;
using UnityEngine;

namespace OcDialogue.Samples
{
    public static class EasySaveIntegration
    {
        const string saveDataKey_PresetNames = "Preset";
        const string saveDataKey_DataRowContainer = "DataRowContainer";
        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            if (!DBManager.RuntimeInitialized)
            {
                DBManager.OnRuntimeInitilized += Init;
                return;
            }
            Load();
            InitSaveCallbacks();
        }

        public static void LoadGameProcessDB(string presetName = "")
        {
            Debug.Log($"Load GameProcessDB | key : {presetName + GameProcessDB.Instance.Address}");
            if (!ES3.KeyExists(presetName + GameProcessDB.Instance.Address)) return;
            Debug.Log("Overwrite");
            var saveData = ES3.Load<GameProcessSaveData>(presetName + GameProcessDB.Instance.Address);
            GameProcessDB.Instance.Overwrite(saveData);
        }

        public static void LoadInventory(string presetName = "")
        {
            if (!ES3.KeyExists(presetName + ItemDatabase.Instance.Address)) return;
            var saveData = ES3.Load<List<ItemSaveData>>(presetName + ItemDatabase.Instance.Address);
            if (Inventory.PlayerInventory == null)
            {
                Inventory.OnPlayerInventoryChanged += inventory => inventory.Overwrite(saveData);
            }
            else
            {
                Inventory.PlayerInventory.Overwrite(saveData);
            }
        }

        public static void LoadQuestDB(string presetName = "")
        {
            if (!ES3.KeyExists(presetName + QuestDB.Instance.Address)) return;
            var saveData = ES3.Load<List<CommonSaveData>>(presetName + QuestDB.Instance.Address);
            QuestDB.Instance.Overwrite(saveData);
        }

        public static void LoadNPCDB(string presetName = "")
        {
            if (!ES3.KeyExists(presetName + NPCDB.Instance.Address)) return;
            var saveData = ES3.Load<List<CommonSaveData>>(presetName + NPCDB.Instance.Address);
            NPCDB.Instance.Overwrite(saveData);
        }

        public static void LoadEnemyDB(string presetName = "")
        {
            if (!ES3.KeyExists(presetName + EnemyDB.Instance.Address)) return;
            var saveData = ES3.Load<List<CommonSaveData>>(presetName + EnemyDB.Instance.Address);
            EnemyDB.Instance.Overwrite(saveData);
        }
        static void Load(string presetName = "")
        {
            LoadGameProcessDB(presetName);
            LoadInventory(presetName);
            LoadQuestDB(presetName);
            LoadNPCDB(presetName);
            LoadEnemyDB(presetName);
        }

        static void InitSaveCallbacks()
        {
            GameProcessDB.Instance.OnRuntimeValueChanged += () => SaveGameProcessDB();

            if (Inventory.PlayerInventory != null)
            {
                Inventory.PlayerInventory.OnInventoryChanged += () => SaveInventory();
            }
            else
            {
                Inventory.OnPlayerInventoryChanged += inventory => inventory.OnInventoryChanged += () => SaveInventory();
            }
            
            QuestDB.Instance.OnRuntimeValueChanged += () => SaveQuestDB();
            NPCDB.Instance.OnRuntimeValueChanged += () => SaveNPCDB();
            EnemyDB.Instance.OnRuntimeValueChanged += () => SaveEnemyDB();
        }
        public static void SaveGameProcessDB(string presetName = "")
        {
            ES3.Save(presetName + GameProcessDB.Instance.Address, GameProcessDB.Instance.DataRowContainer.GetSaveData());
            SavePresetName(DBType.GameProcess, presetName);
        }

        public static void SaveInventory(string presetName = "")
        {
            ES3.Save(presetName + ItemDatabase.Instance.Address, Inventory.PlayerInventory.GetSaveData());
            SavePresetName(DBType.Item, presetName);
        }
        
        public static void SaveQuestDB(string presetName = "")
        {
            ES3.Save(presetName + QuestDB.Instance.Address, QuestDB.Instance.GetSaveData());
            SavePresetName(DBType.Quest, presetName);
        }

        public static void SaveNPCDB(string presetName = "")
        {
            ES3.Save(presetName + NPCDB.Instance.Address, NPCDB.Instance.GetSaveData());
            SavePresetName(DBType.NPC, presetName);
        }

        public static void SaveEnemyDB(string presetName = "")
        {
            ES3.Save(presetName + EnemyDB.Instance.Address, EnemyDB.Instance.GetSaveData());
            SavePresetName(DBType.Enemy, presetName);
        }

        /// <summary>
        /// 데이터를 수동으로 저장함.
        /// </summary>
        /// <param name="presetName"></param>
        public static void Save(string presetName = "")
        {
            SaveGameProcessDB(presetName);
            SaveInventory(presetName);
            SaveQuestDB(presetName);
            SaveNPCDB(presetName);
            SaveEnemyDB(presetName);

            SavePresetName(presetName);
        }

        public static void LoadFromPreset(string presetName)
        {
            if (!ES3.KeyExists(saveDataKey_PresetNames))
            {
                Debug.LogWarning($"저장된 프리셋이 없음");
                return;
            }
            Load(presetName);
        }
        public static void LoadFromPreset(DBType dbType, string presetName)
        {
            if (!ES3.KeyExists(dbType + saveDataKey_PresetNames))
            {
                Debug.LogWarning($"저장된 프리셋이 없음");
                return;
            }

            switch (dbType)
            {
                case DBType.GameProcess:
                    LoadGameProcessDB(presetName);
                    break;
                case DBType.Item:
                    LoadInventory(presetName);
                    break;
                case DBType.Quest:
                    LoadQuestDB(presetName);
                    break;
                case DBType.NPC:
                    LoadNPCDB(presetName);
                    break;
                case DBType.Enemy:
                    LoadEnemyDB(presetName);
                    break;
            }
        }

        public static List<string> GetPresetNames()
        {
            return ES3.KeyExists(saveDataKey_PresetNames) ? ES3.Load<List<string>>(saveDataKey_PresetNames) : new List<string>();
        }
        public static List<string> GetPresetNames(DBType dbType)
        {
            return ES3.KeyExists(dbType + saveDataKey_PresetNames) ? ES3.Load<List<string>>(dbType + saveDataKey_PresetNames) : new List<string>();
        }

        public static void RemovePreset(string presetName)
        {
            var keys = GetPresetNames();
            keys.Remove(presetName);
            ES3.Save(saveDataKey_PresetNames, keys);
        }
        public static void RemovePreset(DBType dbType, string presetName)
        {
            var keys = GetPresetNames(dbType);
            keys.Remove(presetName);
            ES3.Save(dbType + saveDataKey_PresetNames, keys);
        }

        static void SavePresetName(string presetName)
        {
            if(string.IsNullOrWhiteSpace(presetName)) return;
            var existPresets = GetPresetNames();
            if(!existPresets.Contains(presetName))
            {
                existPresets.Add(presetName);
                ES3.Save(saveDataKey_PresetNames, existPresets);
            }
        }

        static void SavePresetName(DBType dbType, string presetName)
        {
            if(string.IsNullOrWhiteSpace(presetName)) return;

            var existPreset = GetPresetNames(dbType);
            if (!existPreset.Contains(presetName))
            {
                existPreset.Add( presetName);
                ES3.Save(dbType + saveDataKey_PresetNames, existPreset);
                Debug.Log($"saveKey | {dbType + saveDataKey_PresetNames} = {existPreset[0]}");
            }
        }
    }

}