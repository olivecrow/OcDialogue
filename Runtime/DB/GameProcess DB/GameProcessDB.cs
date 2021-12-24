using System;
using OcDialogue.DB.GameProcess_DB;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue.DB
{
    [CreateAssetMenu(fileName = "GameProcess DB", menuName = "Oc Dialogue/DB/GameProcess DB", order = 0)]
    public sealed class GameProcessDB : OcData, IDataRowUser
    {
        public static GameProcessDB Instance => DBManager.Instance.GameProcessDB;
        public IGamePlayer GamePlayer { get; set; }
        public DateTime GameTime { get; set; }
        public TimeSpan PlayTimeUntilLastSession { get; set; }
        public TimeSpan PlayTimeSinceThisSession { get; set; }
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

        public void Overwrite(GameProcessSaveData saveData)
        {
            dataRowContainer.Overwrite(saveData.DataRowContainerDict);
            GameTime = saveData.GameTime;
            PlayTimeUntilLastSession = saveData.PlayTime;
        }

        public GameProcessSaveData GetSaveData()
        {
            var data = new GameProcessSaveData()
            {
                DataRowContainerDict = dataRowContainer.GetSaveData(),
                PlayerTransform = GamePlayer.LastSafeTransformData,
                PlayerHP = GamePlayer.HP,
                GameTime = GameTime,
                PlayTime = PlayTimeUntilLastSession + PlayTimeSinceThisSession
            };
            return data;
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