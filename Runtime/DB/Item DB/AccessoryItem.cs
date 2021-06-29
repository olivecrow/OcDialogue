using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public class AccessoryItem : ItemBase, IEquipment
    {
        public override string SubTypeString => subtype.ToString();
        public AccessoryType subtype;

        public int MaxUpgrade => 0;

        public int CurrentUpgrade
        {
            get => 0;
            set => value = 0;
        }

        public int MaxDurability => 100;

        public float CurrentDurability
        {
            get => 100;
            set => value = 100;
        }

        public float Weight => weight;
        public float Equipped { get; set; }
        public float weight = 0.5f;
#if UNITY_EDITOR
        public override void SetSubTypeFromString(string subtypeName)
        {
            subtype = (AccessoryType) Enum.Parse(typeof(ArmorType), subtypeName);
        }  
#endif

        public override ItemBase GetCopy()
        {
            var copy = CreateInstance<AccessoryItem>();
            ApplyBase(copy);
            ApplyTypeProperty(copy);
            return copy;
        }

        protected override void ApplyTypeProperty(ItemBase baseCopy)
        {
            var copy = baseCopy as AccessoryItem;
            
            copy.weight = weight;
        }

        
    }
}
