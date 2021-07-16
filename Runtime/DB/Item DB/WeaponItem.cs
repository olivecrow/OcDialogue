using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace OcDialogue
{
    public class WeaponItem : ItemBase, IEquipment
    {
        [ReadOnly, BoxGroup("ReadOnly")] public WeaponType subtype;
        public override string SubTypeString => subtype.ToString();
        public int MaxUpgrade => 10;
        public int CurrentUpgrade { get; set; }
        public int MaxDurability => maxDurability;
        public float CurrentDurability { get; set; }
        public float Weight => weight;
        public bool Equipped { get; set; }

        public int maxDurability = 100;
        public int weight;

        [BoxGroup("Physical Attack"), ShowInInspector, ReadOnly] public float AveragePhysicalAttack => (strikeAttack + sliceAttack + thrustAttack) / 3f;
        [BoxGroup("Physical Attack")][Range(0f, 200f)]public int strikeAttack = 100;
        [BoxGroup("Physical Attack")][Range(0f, 200f)]public int sliceAttack = 100;
        [BoxGroup("Physical Attack")][Range(0f, 200f)]public int thrustAttack = 100;
        
        [BoxGroup("Elemental Attack"), ShowInInspector, ReadOnly] public float AverageElementalAttack => (fireAttack + iceAttack + lighteningAttack + darkAttack) / 4f;
        [BoxGroup("Elemental Attack")] public int fireAttack;
        [BoxGroup("Elemental Attack")] public int iceAttack;
        [BoxGroup("Elemental Attack")] public int lighteningAttack;
        [BoxGroup("Elemental Attack")] public int darkAttack;

        public AssetReference Avatar => avatar;
        public ItemBase ItemBase => this;
        public AssetReference avatar;
        public override ItemBase GetCopy()
        {
            var copy = CreateInstance<WeaponItem>();
            ApplyBase(copy);
            ApplyTypeProperty(copy);
            return copy;
        }

        protected override void ApplyTypeProperty(ItemBase baseCopy)
        {
            var copy = baseCopy as WeaponItem;
            copy.subtype = subtype;
            copy.maxDurability = maxDurability;
            copy.weight = weight;
            
            copy.fireAttack = fireAttack;
            copy.iceAttack = iceAttack;
            copy.lighteningAttack = lighteningAttack;
            copy.darkAttack = darkAttack;
            
            copy.strikeAttack = strikeAttack;
            copy.sliceAttack = sliceAttack;
            copy.thrustAttack = thrustAttack;

            copy.avatar = avatar;
        }
        
        
#if UNITY_EDITOR
        public override void SetSubTypeFromString(string subtypeName)
        {
            subtype = (WeaponType) Enum.Parse(typeof(WeaponType), subtypeName);
        }
        
#endif
    }
}
