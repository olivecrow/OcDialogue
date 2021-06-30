using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    public class ImportantItem : ItemBase
    {
        [ReadOnly] public ImportantItemType subtype;
        public override string SubTypeString => subtype.ToString();
        
        public override ItemBase GetCopy()
        {
            var copy = CreateInstance<ImportantItem>();
            ApplyBase(copy);
            ApplyTypeProperty(copy);
            return copy;
        }
        
        protected override void ApplyTypeProperty(ItemBase baseCopy)
        {
            var copy = baseCopy as ImportantItem;
            copy.subtype = subtype;
        }
        
        
        public override bool IsNowUsable()
        {
            //TODO:아마 이걸 구현할 아이템이 있으면 따로 클래스를 만들듯?
            return true;
        }
        public override void Use()
        {
            if(!IsNowUsable()) return;
            //TODO : 위와 동일.
        }

        
#if UNITY_EDITOR
        public override void SetSubTypeFromString(string subtypeName)
        {
            subtype = (ImportantItemType) Enum.Parse(typeof(ImportantItemType), subtypeName);
        }
#endif
    }
}
