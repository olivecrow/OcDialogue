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
    [CreateAssetMenu(fileName = "GameProcess DB", menuName = "Oc Dialogue/DB V2/GameProcess DB", order = 0)]
    public class GameProcessDB_V2 : AddressableData, IDataRowUser
    {
        public static GameProcessDB_V2 Instance => DBManagerV2.Instance.GameProcessDB;
        public override string Address => "GameProcessData";
        public DataRowContainerV2 DataRowContainer => dataRowContainer;
        public DataRowContainerV2 dataRowContainer;

        public void Load()
        {
            DataRowContainer.GenerateRuntimeData();
        }

#if UNITY_EDITOR
        void Reset()
        {
            DataRowContainer.Parent = this;
        }

        
        [Button("누락된 데이터 찾기")]
        void FindMissingDataRow()
        {
            OcDataUtility.FindMissingDataRows(this, dataRowContainer);
        }
#endif
    }
}