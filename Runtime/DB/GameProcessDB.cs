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
        public bool usePreset;
        public DataRowContainer dataRowContainer;
        public event Action OnRuntimeValueChanged;
        public void Init()
        {
#if UNITY_EDITOR
            if (usePreset)
            {
                DataRowContainer.LoadFromEditorPreset();
                return;
            }

            Application.quitting += () => OnRuntimeValueChanged = null;
#endif
            DataRowContainer.GenerateRuntimeData();
            if (DBManager.Instance.SaveOnChanged)
            {
                dataRowContainer.OnRuntimeValueChanged += row => OnRuntimeValueChanged?.Invoke();
            }
            
        }

        public void Overwrite(Dictionary<string, string> dataRows)
        {
            dataRowContainer.Overwrite(dataRows);
        }

#if UNITY_EDITOR
        void Reset()
        {
            if(dataRowContainer == null)
            {
                dataRowContainer = new DataRowContainer();
                dataRowContainer.Parent = this;
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