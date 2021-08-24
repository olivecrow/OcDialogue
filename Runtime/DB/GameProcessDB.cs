using System;
using System.Collections.Generic;
using System.Text;
using OcDialogue.DB;
using OcUtility;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.DB
{
    [CreateAssetMenu(fileName = "GameProcess DB", menuName = "Oc Dialogue/DB/GameProcess DB", order = 0)]
    public class GameProcessDB : OcData, IDataRowUser
    {
        public static GameProcessDB Instance => DBManager.Instance.GameProcessDB;
        public override string Address => "GameProcessData";
        public DataRowContainer DataRowContainer => dataRowContainer;
        public DataRowContainer dataRowContainer;

        public void Load()
        {
            DataRowContainer.GenerateRuntimeData();
        }

#if UNITY_EDITOR
        void Reset()
        {
            if(dataRowContainer != null)
            {
                
                DataRowContainer.Parent = this;
            }
        }

        
        [Button("누락된 데이터 찾기")]
        void FindMissingDataRow()
        {
            OcDataUtility.FindMissingDataRows(this, dataRowContainer);
        }
#endif
    }
}