using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    public class ArmorItem : ItemBase, IEquipment
    {
        [ReadOnly]public ArmorType subtype;
        public override string SubTypeString => subtype.ToString();

        public int MaxUpgrade => 3;
        public int CurrentUpgrade { get; set; }
        public int MaxDurability => maxDurability;
        public float CurrentDurability { get; set; }
        public float Weight => weight;
        public float Equipped { get; set; }

        public int maxDurability = 100;
        public float weight;

        public int defense;
        
        [Range(0f, 2f)]public float fireResistance = 1f;
        [Range(0f, 2f)]public float iceResistance = 1f;
        [Range(0f, 2f)]public float lighteningResistance = 1f;
        [Range(0f, 2f)]public float darkResistance = 1f;

        [Range(0f, 2f)]public float strikeResistance = 1f;
        [Range(0f, 2f)]public float sliceResistance = 1f;
        [Range(0f, 2f)]public float thrustResistance = 1f;
        public int stability;
        
        protected override void ApplyTypeProperty(ItemBase baseCopy)
        {
            var copy = baseCopy as ArmorItem;
            copy.defense = defense;
            copy.maxDurability = maxDurability;
            
            copy.fireResistance = fireResistance;
            copy.iceResistance = iceResistance;
            copy.lighteningResistance = lighteningResistance;
            copy.darkResistance = darkResistance;

            copy.strikeResistance = strikeResistance;
            copy.sliceResistance = sliceResistance;
            copy.thrustResistance = thrustResistance;

            copy.stability = stability;
        }
#if UNITY_EDITOR
        public override void SetSubTypeFromString(string subtypeName)
        {
            subtype = (ArmorType) Enum.Parse(typeof(ArmorType), subtypeName);
        }
        
#endif
    }
}
