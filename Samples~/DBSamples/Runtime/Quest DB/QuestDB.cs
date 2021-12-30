using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcDialogue;
using OcDialogue.DB;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace MyDB
{
    [CreateAssetMenu(fileName = "Quest DB", menuName = "Oc Dialogue/DB/Quest DB")]
    public sealed class QuestDB : OcDB
    {
        public override string Address => "QuestDB";
        public override IEnumerable<OcData> AllData => Quests;

        public static QuestDB Instance
        {
            get
            {
                if(_instance == null) 
                    _instance = DBManager.Instance.DBs.Find(x => x is QuestDB) as QuestDB;
                return _instance;
            }
        }

        static QuestDB _instance;
        public List<Quest> Quests;

        public override void Init()
        {
            foreach (var quest in Quests)
            {
                quest.GenerateRuntimeData();
                quest.OnRuntimeValueChanged += q => OnRuntimeValueChanged?.Invoke();
            }

            IsInitialized = true;
        }

        public override void Overwrite(List<CommonSaveData> saveData)
        {
            foreach (var quest in Quests)
            {
                var targetData = saveData.Find(x => x.Key == quest.Name);
                if (targetData == null)
                {
                    Debug.LogError($"해당 키값의 saveData가 없음 | key : {quest.Name}");
                    continue;
                }
                quest.Overwrite(targetData);
            }
        }

        public override List<CommonSaveData> GetSaveData()
        {
            var list = new List<CommonSaveData>();
            foreach (var quest in Quests)
            {
                list.Add(quest.GetSaveData());
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
