using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    public class GenericItem : ItemBase
    {
        [ReadOnly]public GenericType subtype;
        public override string SubTypeString => subtype.ToString();
        
        public override ItemBase GetCopy()
        {
            var copy = CreateInstance<GenericItem>();
            ApplyBase(copy);
            ApplyTypeProperty(copy);
            return copy;
        }
        
        protected override void ApplyTypeProperty(ItemBase baseCopy)
        {
            var copy = baseCopy as GenericItem;

            copy.subtype = subtype;
        }
        
        public override bool IsNowUsable()
        {
            //TODO
            return true;
        }
        public override void Use()
        {
            if(!IsNowUsable()) return;
            //TODO : 아마 소비에 관한 것.
        }

        
#if UNITY_EDITOR
        public override void SetSubTypeFromString(string subtypeName)
        {
            subtype = (GenericType) Enum.Parse(typeof(GenericType), subtypeName);
        }
        
#endif
    }
}
