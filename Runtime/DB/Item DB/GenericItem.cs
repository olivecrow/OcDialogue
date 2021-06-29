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
            
        }
        
#if UNITY_EDITOR
        public override void SetSubTypeFromString(string subtypeName)
        {
            subtype = (GenericType) Enum.Parse(typeof(GenericType), subtypeName);
        }
        
#endif
    }
}