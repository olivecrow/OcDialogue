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

#if UNITY_EDITOR
        public void AddData(DBType dbType, DataStorageType method)
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