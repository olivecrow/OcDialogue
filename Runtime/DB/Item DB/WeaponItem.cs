using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    public class WeaponItem : ItemBase, IEquipment
    {
        [ReadOnly] public WeaponType subtype;
        public override string SubTypeString => subtype.ToString();
        public int MaxUpgrade => 10;
        public int CurrentUpgrade { get; set; }
        public int MaxDurability => maxDurability;
        public float CurrentDurability { get; set; }
        public float Weight => weight;
        public float Equipped { get; set; }
        public int TotalPower => physicalPower + firePower + icePower + lighteningPower + darkPower;

        public int maxDurability = 100;
        public int weight;
        public int physicalPower;
        public int firePower;
        public int icePower;
        public int lighteningPower;
        public int darkPower;
        [Range(0f, 2f)]public float strikeMultiplier = 1f;
        [Range(0f, 2f)]public float sliceMultiplier = 1f;
        [Range(0f, 2f)]public float thrustMultiplier = 1f;
        protected override void ApplyTypeProperty(ItemBase baseCopy)
        {
            var copy = baseCopy as WeaponItem;
            copy.maxDurability = maxDurability;
            copy.weight = weight;
            
            copy.physicalPower = physicalPower;
            copy.firePower = firePower;
            copy.icePower = icePower;
            copy.lighteningPower = lighteningPower;
            copy.darkPower = darkPower;
            
            copy.strikeMultiplier = strikeMultiplier;
            copy.sliceMultiplier = sliceMultiplier;
            copy.thrustMultiplier = thrustMultiplier;
        }
#if UNITY_EDITOR
        public override void SetSubTypeFromString(string subtypeName)
        {
            subtype = (WeaponType) Enum.Parse(typeof(WeaponType), subtypeName);
        }
        
#endif
    }
}
