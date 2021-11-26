using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace OcDialogue
{
    public class AccessoryItem : ItemBase, IEquipment
    {
        public override ItemType type => ItemType.Accessory;
        public virtual AccessoryType subtype { get; protected set; }
        public override string SubTypeString => subtype.ToString();
        public AssetReference Avatar => null;
        public ItemBase ItemBase => this;

        public int MaxUpgrade => 0;

        public virtual int CurrentUpgrade
        {
            get => 0;
            set => value = 0;
        }

        public virtual int MaxDurability => 100;

        public virtual float CurrentDurability
        {
            get => 100;
            set => value = 100;
        }

        public virtual float Weight => weight;
        public bool IsEquipped { get; set; }
        public float weight = 0.5f;
#if UNITY_EDITOR
        public override void SetSubTypeFromString(string subtypeName)
        {
            subtype = (AccessoryType) Enum.Parse(typeof(AccessoryType), subtypeName);
        }  
#endif

        protected override ItemBase CreateInstance()
        {
            return CreateInstance<AccessoryItem>();
        }

        protected override void ApplyTypeProperty(ItemBase baseCopy)
        {
            var copy = baseCopy as AccessoryItem;
            copy.subtype = subtype;
            copy.weight = weight;
            
        }

        
    }
}
