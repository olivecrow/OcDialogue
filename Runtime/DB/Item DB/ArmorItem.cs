using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    public class ArmorItem : ItemBase
    {
        [ReadOnly]public ArmorType subtype;
        public override string SubTypeString => subtype.ToString();
#if UNITY_EDITOR
        public override void SetSubTypeFromString(string subtypeName)
        {
            subtype = (ArmorType) Enum.Parse(typeof(ArmorType), subtypeName);
        }  
#endif
    }
}
