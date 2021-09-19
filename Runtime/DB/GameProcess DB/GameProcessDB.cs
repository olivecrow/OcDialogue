using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<DynamicData> DynamicDataList;

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

        public void Overwrite(GameProcessSaveData saveData)
        {
            dataRowContainer.Overwrite(saveData.DataRowContainerDict);
            DynamicDataList = saveData.DynamicDataList;
        }

        public GameProcessSaveData GetSaveData()
        {
            var data = new GameProcessSaveData()
            {
                DataRowContainerDict = dataRowContainer.GetSaveData(),
                DynamicDataList = new List<DynamicData>(DynamicDataList)
            };
            return data;
        }

        public void ReadDynamicDataOrRegister(DynamicDataBehaviour dynamicDataBehaviour)
        {
            var data = DynamicDataList.FirstOrDefault(x => x.Key == dynamicDataBehaviour.Key);
            if (data == null)
            {
                DynamicDataList.Add(new DynamicData(dynamicDataBehaviour));
            }
            else
            {
                if (dynamicDataBehaviour.UseTransformData)
                {
                    var transform = dynamicDataBehaviour.transform;
                    transform.position = data.TransformData.position;
                    transform.rotation = data.TransformData.rotation;
                    transform.localScale = data.TransformData.scale;
                }
                dynamicDataBehaviour.PrimitiveData = data.PrimitiveData;
            }
        }

        public void UpdateDynamicData(DynamicDataBehaviour dynamicDataBehaviour)
        {
            var data = DynamicDataList.FirstOrDefault(x => x.Key == dynamicDataBehaviour.Key);
            if (data == null)
            {
                DynamicDataList.Add(new DynamicData(dynamicDataBehaviour));
            }
            else
            {
                if (dynamicDataBehaviour.UseTransformData)
                {
                    data.TransformData = new TransformData(dynamicDataBehaviour.transform);
                }

                data.PrimitiveData = dynamicDataBehaviour.PrimitiveData;
            }
            OnRuntimeValueChanged?.Invoke();
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