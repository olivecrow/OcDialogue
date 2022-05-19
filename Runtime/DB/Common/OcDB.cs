using System;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue.DB
{
    public abstract class OcDB : OcData
    {
        public Action OnRuntimeValueChanged;
        public virtual string[] CategoryOverride => Categories;
        public string[] Categories;

        public override string Category
        {
            get => Address;
            set => Debug.LogWarning($"[{Address}] DB는 카테고리가 지정될 수 없음");
        }
        public abstract void Overwrite(List<CommonSaveData> saveData);
        public abstract List<CommonSaveData> GetSaveData();
        public bool IsInitialized { get; protected set; }
        public abstract IEnumerable<OcData> AllData { get; }
        public override DataRowType GetValueType(string fieldName)
        {
            return DataRowType.String;
        }
    }
}