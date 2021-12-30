using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue;
using OcUtility;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyDB
{
    public class GenericItem : ItemBase, IUsableItem
    {
        public override ItemType type => ItemType.Generic;
        public GenericType subtype;
        public ItemBase ItemBase => this;
        public virtual int MaxStack => maxStackCount;

        public virtual bool IsUsableItem => isUsable;
        public override string SubTypeString => subtype.ToString();
        /// <summary> 사용할 수 있는 아이템인지 여부. 현재 상태가 아니라 아이템의 특성을 말하는 것. </summary>
        public bool isUsable;

        protected override ItemBase CreateInstance()
        {
            return CreateInstance<GenericItem>();
        }

        protected override void ApplyTypeProperty(ItemBase baseCopy)
        {
            var copy = baseCopy as GenericItem;

            copy.subtype = subtype;
            copy.isUsable = isUsable;
        }

        protected override void AppendAdditionalSaveData(CommonSaveData saveData)
        {
            
        }

        protected override void OverwriteAdditionalSaveData(CommonSaveData saveData)
        {
            
        }

        public virtual bool IsNowUsable()
        {
            if (!isUsable) return false;
            if (isStackable && CurrentStack <= 0) return false;

            return true;
        }
        public virtual ItemEliminationResult Use()
        {
            if(!IsNowUsable()) return ItemEliminationResult.None;
            CurrentStack--;
            UsageProcess();
            Printer.Print($"[{itemName}] 사용됨 | CurrentStack : {CurrentStack}");
            return CurrentStack <= 0 ? ItemEliminationResult.Empty : ItemEliminationResult.Success;
        }

        protected virtual void UsageProcess(){}

#if UNITY_EDITOR
        public override void SetSubTypeFromString(string subtypeName)
        {
            subtype = (GenericType) Enum.Parse(typeof(GenericType), subtypeName);
        }
        
#endif
    }
}
