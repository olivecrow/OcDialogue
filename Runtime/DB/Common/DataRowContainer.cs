using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OcDialogue
{
    public enum DataStorageType
    {
        Single,
        Embeded
    }
    [Serializable]
    public class DataRowContainer
    {
        [HideInInspector] public ScriptableObject owner;
        [TableList(IsReadOnly = true), PropertyOrder(20)]
        public List<DataRow> dataRows;

        /// <summary>
        /// DataRow 리스트를 포함하는 범용 컨테이너를 만듦..
        /// </summary>
        /// <param name="owner">이 컨테이너를 직접적으로 필드로써 가지는 ScriptableObject. 없으면 null을 넣으면 됨. </param>
        /// <param name="rows">컨테이너에 넣을 DataRow의 목록. 카피를 만드려는 경우, 미리 카피 목록을 생성한 후, 넣어야함. </param>
        public DataRowContainer(ScriptableObject owner, IEnumerable<DataRow> rows)
        {
            this.owner = owner;
            if (rows == null) dataRows = new List<DataRow>();
            else dataRows = rows.ToList();
        }

        public List<DataRow> GetAllCopies()
        {
            var list = new List<DataRow>();
            foreach (var dataRow in dataRows)
            {
                var copy = dataRow.GetCopy();
                list.Add(copy);
            }

            return list;
        }

        public void SetValue<T>(string key, T value)
        {
            var target = dataRows.Find(x => x.key == key);
            if (target == null)
            {
                Debug.LogWarning($"해당 키값의 DataRow가 없음 | key : {key}");
                return;
            }

            switch (value)
            {
                case bool b:
                    target.boolValue = b;
                    break;
                case int i:
                    target.intValue = i;
                    break;
                case float f:
                    target.floatValue = f;
                    break;
                case string s:
                    target.stringValue = s;
                    break;
                default:
                    Debug.LogWarning($"유효하지 않은 데이터 타입이 전달됨 | type : {value.GetType()}");
                    return;
            }
        }

        public T GetValue<T>(string key)
        {
            var target = dataRows.Find(x => x.key == key);
            if (target == null)
            {
                Debug.LogWarning($"해당 키값의 DataRow가 없음 | key : {key}");
                return default;
            }

            return (T) target.TargetValue;
        }

        public bool Contains(DataRow dataRow)
        {
            return dataRows.Contains(dataRow);
        }

        public bool HasKey(string key)
        {
            return dataRows.Any(x => x.key == key);
        }
        
        public DataRow FindData(string key)
        {
            var data = dataRows.Find(x => x.key == key);
            
            if(data == null)
            {
                Debug.LogWarning($"해당 키값의 DataRow가 없음 | key : {key}");
                return null;
            }

            return data;
        }
        public bool IsTrue(string key, Operator op, object value)
        {
            var data = FindData(key);
            if (data == null) return false;
            return data.IsTrue(op, value);
        }

#if UNITY_EDITOR
        public DataRow AddData(DBType dbType, DataStorageType method)
        {
            var row = ScriptableObject.CreateInstance<DataRow>();
            row.ownerDB = dbType;
            row.name = OcDataUtility.CalculateDataName("New DataRow", dataRows.Select(x => x.key));
            row.key = row.name;
            dataRows.Add(row);
            OcDataUtility.Repaint();

            if (method == DataStorageType.Single)
            {
                var ownerPath = AssetDatabase.GetAssetPath(owner);
                var path = ownerPath.Replace($"{owner.name}", $"{row.name}");
                AssetDatabase.CreateAsset(row, path);
            }
            else if (method == DataStorageType.Embeded)
            {
                AssetDatabase.AddObjectToAsset(row, owner);
            }
            EditorUtility.SetDirty(owner);
            AssetDatabase.SaveAssets();

            return row;
        }

        public void DeleteRow(string key, DataStorageType method)
        {
            var row = dataRows.FirstOrDefault(x => x.key == key);
            if (row == null)
            {
                Debug.LogWarning($"해당 키값의 DataRow가 없어서 삭제에 실패함 : {key}");
                return;
            }
            dataRows.Remove(row);

            OcDataUtility.Repaint();

            if (method == DataStorageType.Single)
            {
                var path = AssetDatabase.GetAssetPath(row);
                AssetDatabase.DeleteAsset(path);
            }
            else if (method == DataStorageType.Embeded)
            {
                AssetDatabase.RemoveObjectFromAsset(row);   
            }
            EditorUtility.SetDirty(owner);
            AssetDatabase.SaveAssets();
        }
        
        /// <summary> 모든 데이터의 카와 이름이 다른지 체크함. 이름 변경시마다 계속 저장하면 에디터가 느려지니까 체크와 적용을 따로하기 위함. </summary>
        public void CheckNames()
        {
            if(dataRows == null) return;
            foreach (var dataRow in dataRows)
            {
                dataRow.CheckName();
            }
        }
        
        /// <summary> 모든 데이터의 이름을 키값으로 맞춤. </summary>
        public void MatchDataRowNames()
        {
            foreach (var dataRow in dataRows)
            {
                if (dataRow.name != dataRow.key) dataRow.name = dataRow.key;
            }
            CheckNames();
            EditorUtility.SetDirty(owner);
            AssetDatabase.SaveAssets();
        }
        
#endif
    }
}