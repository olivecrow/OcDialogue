using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace OcDialogue
{
    public class ArmorItem : ItemBase, IEquipment
    {
        [BoxGroup("ReadOnly")][ReadOnly]public ArmorType subtype;
        public override string SubTypeString => subtype.ToString();

        public int MaxUpgrade => 3;
        public int CurrentUpgrade { get; set; }
        public int MaxDurability => maxDurability;
        public float CurrentDurability { get; set; }
        public float Weight => weight;
        public float Equipped { get; set; }

        public int maxDurability = 100;
        public float weight;

        public int stability;
        
        [BoxGroup("Physical Defense"), ShowInInspector, ReadOnly, LabelText("평균 물리 방어")]
        public float AveragePhysicsResistance => (strikeDefense + sliceDefense + thrustDefense) / 3f;
        [Range(1, 100f), BoxGroup("Physical Defense"), LabelText("Strike"), LabelWidth(100)] public int strikeDefense = 20;
        [Range(1, 100f), BoxGroup("Physical Defense"), LabelText("Slice"),  LabelWidth(100)] public int sliceDefense = 20;
        [Range(1, 100f), BoxGroup("Physical Defense"), LabelText("Thrust"), LabelWidth(100)] public int thrustDefense = 20;
        
        [BoxGroup("Elemental Defense"), ShowInInspector, ReadOnly, LabelText("평균 속성 방어")]
        public float AverageElementalResistance => (fireResistance + iceResistance + lighteningResistance + darkResistance) / 4f;
        
        [Range(1, 50), BoxGroup("Elemental Defense"), LabelText("Fire"),  LabelWidth(100)] public int fireResistance       = 10;
        [Range(1, 50), BoxGroup("Elemental Defense"), LabelText("Ice"),   LabelWidth(100)] public int iceResistance        = 10;
        [Range(1, 50), BoxGroup("Elemental Defense"), LabelText("Light"), LabelWidth(100)] public int lighteningResistance = 10;
        [Range(1, 50), BoxGroup("Elemental Defense"), LabelText("Dark"),  LabelWidth(100)] public int darkResistance       = 10;
        
        public AssetReference avatar;

        public override ItemBase GetCopy()
        {
            var copy = CreateInstance<ArmorItem>();
            ApplyBase(copy);
            ApplyTypeProperty(copy);
            return copy;
        }
        
        protected override void ApplyTypeProperty(ItemBase baseCopy)
        {
            var copy = baseCopy as ArmorItem;
            copy.maxDurability = maxDurability;
            
            copy.fireResistance = fireResistance;
            copy.iceResistance = iceResistance;
            copy.lighteningResistance = lighteningResistance;
            copy.darkResistance = darkResistance;

            copy.strikeDefense = strikeDefense;
            copy.sliceDefense = sliceDefense;
            copy.thrustDefense = thrustDefense;

            copy.stability = stability;
            copy.avatar = avatar;
        }
#if UNITY_EDITOR
        public override void SetSubTypeFromString(string subtypeName)
        {
            subtype = (ArmorType) Enum.Parse(typeof(ArmorType), subtypeName);
        }

        void NormalizePhysicalResistance()
        {
            
        }
#endif
    }
}
