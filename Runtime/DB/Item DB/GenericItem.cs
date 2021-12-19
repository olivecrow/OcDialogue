using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
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
        
        public virtual bool IsNowUsable()
        {
            if (!isUsable) return false;
            //TODO
            if (isStackable && CurrentStack <= 0) return false;
            return true;
        }
        public virtual bool Use(Action onEmpty = null)
        {
            if(!IsNowUsable()) return false;
            if (isStackable) CurrentStack--;
            if(CurrentStack <= 0) onEmpty?.Invoke();
            return true;
            //TODO : 아마 소비에 관한 것.
        }


#if UNITY_EDITOR
        internal override void SetSubTypeFromString(string subtypeName)
        {
            subtype = (GenericType) Enum.Parse(typeof(GenericType), subtypeName);
        }
        
#endif
    }
}
