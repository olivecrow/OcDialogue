using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    [Serializable]
    public class RuntimeGameProcessData
    {
        [ShowInInspector]readonly DataRowContainer _dataRowContainer;

        /// <summary> 카피된 데이터를 전달받아서 DataContainer를 갖는 클래스를 생성함. </summary>
        public RuntimeGameProcessData(IEnumerable<DataRow> copies)
        {
            _dataRowContainer = new DataRowContainer(GameProcessDatabase.Instance, copies);

#if UNITY_EDITOR
            CheckOverriden();
#endif
        }

        public DataRow FindData(string key)
        {
            return _dataRowContainer.FindData(key);
        }

        public T GetValue<T>(string key)
        {
            return (T)FindData(key).TargetValue;
        }

        public void SetValue(string key, object value)
        {
            var data = FindData(key);
            if(data == null) return;
            
#if UNITY_EDITOR
            PrintAsNewValue(data, data.TargetValue, value);            
#endif
            
            switch (data.type)
            {
                case DataRow.Type.Boolean:
                    data.boolValue = (bool)value;
                    break;
                case DataRow.Type.Int:
                    data.intValue = (int)value;
                    break;
                case DataRow.Type.Float:
                    data.floatValue = (float)value;
                    break;
                case DataRow.Type.String:
                    data.stringValue = (string)value;
                    break;
            }
        }
        
        public void SetValue(string key, bool value)
        {
            var data = FindData(key);
            
#if UNITY_EDITOR
            PrintAsNewValue(data, data.TargetValue, value);            
#endif
            
            if(data != null) data.boolValue = value;
        }
        public void SetValue(string key, string value)
        {
            var data = FindData(key);
            
#if UNITY_EDITOR
            PrintAsNewValue(data, data.TargetValue, value);            
#endif
            
            if(data != null) data.stringValue = value;
        }
        public void SetValue(string key, int value)
        {
            var data = FindData(key);
            
#if UNITY_EDITOR
            PrintAsNewValue(data, data.TargetValue, value);            
#endif
            
            if(data != null) data.intValue = value;
        }
        public void SetValue(string key, float value)
        {
            var data = FindData(key);
            
#if UNITY_EDITOR
            PrintAsNewValue(data, data.TargetValue, value);            
#endif
            
            if(data != null) data.floatValue = value;
        }

        public bool IsTrue(string key, Operator op, object value)
        {
            var data = FindData(key);
            if (data == null) return false;
            return data.IsTrue(op, value);
        }

#if UNITY_EDITOR
        [HorizontalGroup("B"), Button("에디터 프리셋에 적용"), GUIColor(1,1,0)]
        void ApplyToEditorPreset()
        {
            GameProcessDatabase.Instance.editorPreset.Apply(_dataRowContainer.dataRows);
        }
        [HorizontalGroup("B"), Button("오버라이드 체크"), GUIColor(1,1,0)]
        void CheckOverriden()
        {
            for (int i = 0; i < _dataRowContainer.dataRows.Count; i++)
            {
                var dataRow = _dataRowContainer.dataRows[i];

                if (dataRow.IsTrue(Operator.NotEqual, GameProcessDatabase.Instance.DataRowContainer.dataRows[i].TargetValue))
                {
                    dataRow.description = $"에디터 프리셋에서 오버라이드 | InitialValue = {GameProcessDatabase.Instance.DataRowContainer.dataRows[i].TargetValue}";   
                }
            }
        }

        void PrintAsNewValue(DataRow targetRow, object before, object after)
        {
            targetRow.description = $"런타임에서 값 변경 | {before} => {after}";
        }
#endif
    }
}
