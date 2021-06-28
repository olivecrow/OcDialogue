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
            
        }
#if UNITY_EDITOR
        public override void SetSubTypeFromString(string subtypeName)
        {
            subtype = (ImportantItemType) Enum.Parse(typeof(ImportantItemType), subtypeName);
        }
#endif
    }
}
