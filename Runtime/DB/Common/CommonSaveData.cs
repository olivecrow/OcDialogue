using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue.DB;
using OcUtility;
using UnityEngine;

namespace OcDialogue
{
    [Serializable]
    public class CommonSaveData
    {
        public string Key;
        public List<SavedDataRow> DataRows;
        public Dictionary<string, string> Data;
        public DateTime SaveTime;

        public CommonSaveData()
        {
            SaveTime = DateTime.Now;
        }
    }

    [Serializable]
    public struct SavedDataRow
    {
        public string name;
        public int id;
        public DataRowType type;
        public PrimitiveValue value;
        public bool isCreatedRuntime;

        public SavedDataRow(DataRow dataRow)
        {
            name = dataRow.name;
            id = dataRow.id;
            type = dataRow.Type;
            value = dataRow.RuntimeValue;
            isCreatedRuntime = dataRow.isCreatedRuntime;
        }

        /// <summary>
        /// name, id가 같을 경우, value를 destination의 runtimeValue로 복사함
        /// </summary>
        /// <param name="destination"></param>
        public void CopyTo(DataRow destination)
        {
            if (!destination.name.equal(name))
            {
                Debug.LogError($"{destination.DRT(true)} 동일하지 않은 이름의 데이터로의 복사가 감지됨");
                return;
            }
            if (id != destination.id)
            {
                Debug.LogError($"{destination.DRT(true)} 동일하지 않은 ID의 데이터로의 복사가 감지됨 | ID : {id} <=> destination ID : {destination.id}");
                return;
            }
            
            destination.RuntimeValue = value;
            destination.isCreatedRuntime = isCreatedRuntime;
        }
    }
}
