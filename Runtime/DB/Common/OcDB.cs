using System;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue.DB
{
    public abstract class OcDB : OcData
    {
        public Action OnRuntimeValueChanged;
        public string[] Category;
        public abstract void Init();
        public abstract void Overwrite(List<CommonSaveData> saveData);
        public abstract List<CommonSaveData> GetSaveData();
        public bool IsInitialized { get; protected set; }
        public abstract IEnumerable<OcData> AllData { get; }
    }
}