using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcDialogue.DB
{
    public abstract class OcDB : OcData
    {
        public override event Action changed;
        public virtual string[] CategoryOverride => Categories;
        public string[] Categories;

        public override string Category
        {
            get => Address;
            set => Debug.LogWarning($"[{Address}] DB는 카테고리가 지정될 수 없음");
        }

        public abstract void InitFromEditor();
        public abstract void Initialize(List<CommonSaveData> saveData);

        public virtual void UnInitialize()
        {
            changed = null;
            IsInitialized = false;
        }
        public abstract List<CommonSaveData> GetSaveData();
        
        public bool IsInitialized { get; protected set; }
        public abstract IEnumerable<OcData> AllData { get; }
        public override DataRowType GetValueType(string fieldName)
        {
            return DataRowType.String;
        }

#if UNITY_EDITOR
      
        protected void ReleaseEvent(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                changed = null;
                EditorApplication.playModeStateChanged -= ReleaseEvent;
            }
        }  
#endif
    }
}