using System;
using System.Collections.Generic;
using System.Linq;
using OcDialogue;
using OcDialogue.DB;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyDB
{
    [CreateAssetMenu(fileName = "GameProcess DB", menuName = "Oc Dialogue/DB/GameProcess DB", order = 0)]
    public sealed class GameProcessDB : OcDB
    {
        public static GameProcessDB Instance
        {
            get
            {
                if (_instance == null) 
                    _instance = DBManager.Instance.DBs.Find(x => x is GameProcessDB) as GameProcessDB;
                return _instance;
            }
        }
        static GameProcessDB _instance;
        public override string Address => "GameProcessDB";
        public IGamePlayer GamePlayer { get; set; }
        public DateTime GameTime { get; set; }
        public TimeSpan PlayTimeUntilLastSession { get; set; }
        public TimeSpan PlayTimeSinceThisSession { get; set; }
        public override IEnumerable<OcData> AllData => Data;
        public bool usePreset;
        public List<GameProcessData> Data;

        public override void Init()
        {
#if UNITY_EDITOR
            if (usePreset)
            {
                foreach (var data in Data)
                {
                    data.DataRowContainer.LoadFromEditorPreset();    
                }
                return;
            }

            Application.quitting += () => OnRuntimeValueChanged = null;
#endif
            foreach (var gameProcessData in Data)
            {
                gameProcessData.DataRowContainer.GenerateRuntimeData();
                gameProcessData.DataRowContainer.OnRuntimeValueChanged += row => OnRuntimeValueChanged?.Invoke();
            }
            
            
            IsInitialized = true;
        }

        public override void Overwrite(List<CommonSaveData> saveData)
        {
            foreach (var gameProcessData in Data)
            {
                var data = saveData.Find(x => x.Key == gameProcessData.name);
                if (data == null)
                {
                    Debug.LogError($"해당 키값의 saveData가 없음 : {gameProcessData.name}");
                    continue;
                }
                gameProcessData.Overwrite(data);
            }
        }

        public override List<CommonSaveData> GetSaveData()
        {
            var list = new List<CommonSaveData>();
            foreach (var gameProcessData in Data)
            {
                var data = new CommonSaveData()
                {
                    Key = gameProcessData.name,
                    DataRowContainerDict = gameProcessData.DataRowContainer.GetSaveData()
                };
                list.Add(data);
            }
            
            return list;
        }
        
        public override bool IsTrue(string fieldName, CheckFactor.Operator op, object checkValue)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(string fieldName)
        {
            throw new NotImplementedException();
        }

        public override string[] GetFieldNames()
        {
            throw new NotImplementedException();
        }

        public override void SetValue(string fieldName, DataSetter.Operator op, object value)
        {
            throw new NotImplementedException();
        }

    }
}