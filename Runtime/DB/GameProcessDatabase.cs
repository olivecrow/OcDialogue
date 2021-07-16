using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcDialogue
{
    [CreateAssetMenu(fileName = "GameProcess Database", menuName = "Oc Dialogue/DB/GameProcess Database")]
    public class GameProcessDatabase : ScriptableObject
    {
        public static GameProcessDatabase Instance => DBManager.Instance.GameProcessDatabase;
        [TableList(IsReadOnly = true)]public List<DataRow> DataRows;

        /// <summary> 데이터 베이스 내의 모든 DataRow의 카피를 반환함. </summary>
        public List<DataRow> GetAllCopies()
        {
            var list = new List<DataRow>();
            foreach (var dataRow in DataRows)
            {
                var copy = dataRow.GetCopy();
                list.Add(copy);
            }

            return list;
        }
        
#if UNITY_EDITOR
        [HorizontalGroup("Buttons"), PropertyOrder(-100), Button(ButtonSizes.Medium)]
        public void Refresh()
        {
            foreach (var dataRow in DataRows)
            {
                if (dataRow.key != dataRow.name) dataRow.name = dataRow.key;
            }
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        [HorizontalGroup("Buttons"), PropertyOrder(-100), Button("+", ButtonSizes.Medium), GUIColor(0, 1, 1)]
        public void AddRow()
        {
            var row = CreateInstance<DataRow>();
            row.ownerDB = DBType.GameProcess;
            row.name = OcDataUtility.CalculateDataName("New DataRow", DataRows.Select(x => x.key));
            row.key = row.name;
            DataRows.Add(row);
            OcDataUtility.Repaint();
            AssetDatabase.AddObjectToAsset(row, this);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        /// <summary> Editor Only </summary>
        [HorizontalGroup("Buttons"), Button("Delete Row"), GUIColor(1, 0, 0)]
        public void DeleteRow(string key)
        {
            var row = DataRows.FirstOrDefault(x => x.key == key);
            if (row == null)
            {
                var path = AssetDatabase.GetAssetPath(this);
                var allAssets = AssetDatabase.LoadAllAssetsAtPath(path).Select(x => x as DataRow);
                row = allAssets.FirstOrDefault(x => x.key == key);
                if(row == null)
                {
                    Debug.LogWarning($"해당 키값의 DataRow가 없어서 삭제에 실패함 : {key}");
                    return;
                }
            }

            DataRows.Remove(row);
            
            OcDataUtility.Repaint();
            AssetDatabase.RemoveObjectFromAsset(row);
            DataRows = DataRows.Where(x => x != null).ToList();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        
        /// <summary> 각종 문제들을 해결함. </summary>
        [HorizontalGroup("Buttons"), Button("Resolve")]
        public void Resolve()
        {
            // datarow의 ownerDB가 Quest가 아닌 것을 고침.
            foreach (var data in DataRows)
            {
                if (data.ownerDB != DBType.GameProcess)
                {
                    Debug.Log($"[GameProcessData] [{data.key}] ownerDB : {data.ownerDB} => GameProcess");
                    data.ownerDB = DBType.GameProcess;
                }
            }
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}
