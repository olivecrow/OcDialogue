using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public class RuntimeGameProcessData
    {
        List<DataRow> _dataRows;

        /// <summary> 오리지날 데이터에서 런타임 데이터 생성. 자동으로 카피함. </summary>
        public RuntimeGameProcessData(IEnumerable<DataRow> original)
        {
            _dataRows = new List<DataRow>();
            foreach (var dataRow in original)
            {
                _dataRows.Add(dataRow.GetCopy());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dict"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public RuntimeGameProcessData(IReadOnlyDictionary<string, object> dict)
        {
            _dataRows = new List<DataRow>();
            foreach (var dataRow in GameProcessDatabase.Instance.DataRowContainer.dataRows)
            {
                var newData = dataRow.GetCopy();
                switch (dataRow.type)
                {
                    case DataRow.Type.Boolean:
                        newData.boolValue = (bool) dict[newData.key];
                        break;
                    case DataRow.Type.Int:
                        newData.intValue = (int) dict[newData.key];
                        break;
                    case DataRow.Type.Float:
                        newData.floatValue = (float) dict[newData.key];
                        break;
                    case DataRow.Type.String:
                        newData.stringValue = (string) dict[newData.key];
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                _dataRows.Add(newData);
            }
        }

        DataRow FindData(string key)
        {
            var data = _dataRows.Find(x => x.key == key);
            if (data == null)
            {
                Debug.LogWarning($"해당 키값의 데이터를 찾을 수 없음 | key : {key}");
                return default;
            }

            return data;
        }

        public T GetValue<T>(string key) where T : unmanaged
        {
            return (T)FindData(key).TargetValue;
        }

        public void SetValue(string key, bool value)
        {
            var data = FindData(key);
            if(data != null) data.boolValue = value;
        }
        public void SetValue(string key, string value)
        {
            var data = FindData(key);
            if(data != null) data.stringValue = value;
        }
        public void SetValue(string key, int value)
        {
            var data = FindData(key);
            if(data != null) data.intValue = value;
        }
        public void SetValue(string key, float value)
        {
            var data = FindData(key);
            if(data != null) data.floatValue = value;
        }
    }
}
