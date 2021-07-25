using System;
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
        public static RuntimeGameProcessData Runtime => _runtime;
        static RuntimeGameProcessData _runtime;

        /// <summary> 오리지널 데이터! 런타임 중에 수정하지 말 것! </summary>
        [DetailedInfoBox("Resolve 버튼이 뭔가욧?",
            "다음과 같은 각종 문제들을 해결함. \n" +
            "1. DataRowContainer의 owner를 재설정함." +
            "2. dataRow의 ownerDB가 GameProcess가 아닌 것을 고침.")]
        [HideLabel, BoxGroup, PropertyOrder(10)]public DataRowContainer DataRowContainer;

        [RuntimeInitializeOnLoadMethod]
        static void RuntimeInit()
        {
            // TODO : 세이브 & 로드 기능 넣기.
            _runtime = new RuntimeGameProcessData(Instance.DataRowContainer.dataRows);
        }

        void OnDisable()
        {
            _runtime = null;
        }

        /// <summary> 데이터 베이스 내의 모든 DataRow의 카피를 반환함. </summary>
        public List<DataRow> GetAllCopies()
        {
            var list = new List<DataRow>();
            foreach (var dataRow in DataRowContainer.dataRows)
            {
                var copy = dataRow.GetCopy();
                list.Add(copy);
            }

            return list;
        }
        
#if UNITY_EDITOR
        [HideInInspector]public GameProcessDataEditorPreset editorPreset;
        void Reset()
        {
            if(DataRowContainer == null) return;
            DataRowContainer.owner = this;
        }

        void OnValidate()
        {
            DataRowContainer.CheckNames();
        }

        ValueDropdownList<string> GetCategory()
        {
            var list = new ValueDropdownList<string>();
            foreach (var s in QuestDatabase.Instance.Category)
            {
                list.Add(s);
            }

            return list;
        }
        
        [Button, HorizontalGroup("Row"), PropertyOrder(9), GUIColor(0,1,1)]
        void AddData()
        {
            DataRowContainer.owner = this;
            DataRowContainer.AddData(DBType.GameProcess, DataStorageType.Embeded);
        }
        
        [Button, HorizontalGroup("Row"), PropertyOrder(9), GUIColor(1,0,0)]
        void DeleteData(string k)
        {
            DataRowContainer.DeleteRow(k, DataStorageType.Embeded);
        }

        [Button, HorizontalGroup("Row"), PropertyOrder(9)]
        void MatchNames()
        {
            DataRowContainer.MatchDataRowNames();
        }
        
        [HorizontalGroup("Buttons"), Button("Resolve")]
        public void Resolve()
        {
            // DataRowContainer의 owner를 재설정함.
            DataRowContainer.owner = this;
            
            // datarow의 ownerDB가 GameProcess가 아닌 것을 고침.
            foreach (var data in DataRowContainer.dataRows)
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
