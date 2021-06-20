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
    [CreateAssetMenu(fileName = "GameProcess Database", menuName = "Oc Dialogue/GameProcess Database")]
    public class GameProcessDatabase : ScriptableObject
    {
#if UNITY_EDITOR
        public const string AssetPath = "GameProcess DB/GameProcess Database";
        public static GameProcessDatabase Instance => _instance;
        static GameProcessDatabase _instance;
        
        [InitializeOnLoadMethod]
        static void Init()
        {
            _instance = Resources.Load<GameProcessDatabase>(AssetPath);
        }  
#endif
        
        [TableList(IsReadOnly = true)]public List<DataRow> DataRows;


#if UNITY_EDITOR

        [HorizontalGroup("Buttons"), PropertyOrder(-100), Button("+", ButtonSizes.Medium), GUIColor(0, 1, 1)]
        public void AddRow()
        {
            var row = CreateInstance<DataRow>();
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
#endif
    }
}
