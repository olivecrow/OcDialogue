using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    public class WeaponItem : ItemBase
    {
        [ReadOnly] public WeaponType subtype;
        public override string SubTypeString => subtype.ToString();
#if UNITY_EDITOR
        public override void SetSubTypeFromString(string subtypeName)
        {
            subtype = (WeaponType) Enum.Parse(typeof(WeaponType), subtypeName);
        }  
#endif
    }
}
