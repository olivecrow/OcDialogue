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
    [CreateAssetMenu(fileName = "NPC DB", menuName = "Oc Dialogue/DB/NPC DB")]
    public sealed class NPCDB : OcDB, IDialogueActorDB
    {
        public override string Address => "NPCDB";

        public static NPCDB Instance
        {
            get
            {
                if(_instance == null) 
                    _instance = DBManager.Instance.DBs.Find(x => x is NPCDB) as NPCDB;
                return _instance;
            }
        }

        static NPCDB _instance;
        public override IEnumerable<OcData> AllData => NPCs;
        public List<NPC> NPCs;
        public override void Init()
        {
            foreach (var npc in NPCs)
            {
                npc.GenerateRuntimeData();
                npc.OnRuntimeValueChanged += npc1 => OnRuntimeValueChanged?.Invoke();
            }

            IsInitialized = true;
        }

        public override void Overwrite(List<CommonSaveData> saveData)
        {
            foreach (var npc in NPCs)
            {
                var targetData = saveData.Find(x => x.Key == npc.Name);
                if (targetData == null)
                {
                    Debug.LogError($"해당 키값의 saveData가 없음 | key : {npc.Name}");
                    continue;
                }
                npc.Overwrite(targetData);
            }
        }

        public override List<CommonSaveData> GetSaveData()
        {
            var list = new List<CommonSaveData>();
            foreach (var npc in NPCs)
            {
                list.Add(npc.GetSaveData());
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

      
        public ValueDropdownList<OcNPC> GetOdinDropDown()
        {
            var list = new ValueDropdownList<OcNPC>();
            foreach (var npc in NPCs)
            {
                list.Add(npc);
            }

            return list;
        }
        
#if UNITY_EDITOR
        /// <summary> 각종 문제들을 해결함. </summary>
        public void Resolve()
        {
            foreach (var npc in NPCs)
            {
                npc.SetParent(this);
                npc.Resolve();
            }

            AssetDatabase.SaveAssets();
        }
#endif
    }
}
