using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

namespace OcDialogue
{
    public class ArmorItem : ItemBase, IEquipment
    {
        [BoxGroup("ReadOnly")][ReadOnly]public ArmorType subtype;
        public override string SubTypeString => subtype.ToString();

#if UNITY_EDITOR
        /// <summary> Editor Only. 방어력 계산 등을 위해 에디터에서만 사용하는 타입. </summary>
        public MaterialForEditor material;
#endif
        public int MaxUpgrade => 3;
        public int CurrentUpgrade { get; set; }
        public int MaxDurability => maxDurability;
        public float CurrentDurability { get; set; }
        public float Weight => weight;
        public float Equipped { get; set; }

        public int maxDurability = 100;
        [InlineButton("CalcDefenseFromWeight", "무게로 방어력 계산")][InlineButton("MoreDefense", "▶")][InlineButton("LessDefense", "◀")]
        public float weight;

        [Range(0f, 50f)]public float stability;
        
        [BoxGroup("Physical Defense"), ShowInInspector, ReadOnly, LabelText("평균 물리 방어")]
        public float AveragePhysicsResistance => (strikeDefense + sliceDefense + thrustDefense) / 3f;
        [Range(0, 100f), BoxGroup("Physical Defense"), LabelText("Strike"), LabelWidth(100)] public float strikeDefense = 20;
        [Range(0, 100f), BoxGroup("Physical Defense"), LabelText("Slice"),  LabelWidth(100)] public float sliceDefense = 20;
        [Range(0, 100f), BoxGroup("Physical Defense"), LabelText("Thrust"), LabelWidth(100)] public float thrustDefense = 20;
        
        [BoxGroup("Elemental Defense"), ShowInInspector, ReadOnly, LabelText("평균 속성 방어")]
        public float AverageElementalResistance => (fireDefense + iceDefense + lighteningDefense + darkDefense) / 4f;
        
        [Range(0, 100f), BoxGroup("Elemental Defense"), LabelText("Fire"),  LabelWidth(100)] public float fireDefense       = 10;
        [Range(0, 100f), BoxGroup("Elemental Defense"), LabelText("Ice"),   LabelWidth(100)] public float iceDefense        = 10;
        [Range(0, 100f), BoxGroup("Elemental Defense"), LabelText("Light"), LabelWidth(100)] public float lighteningDefense = 10;
        [Range(0, 100f), BoxGroup("Elemental Defense"), LabelText("Dark"),  LabelWidth(100)] public float darkDefense       = 10;
        
        public AssetReference avatar;

        void OnValidate()
        {
            strikeDefense = float.Parse($"{strikeDefense:0.0}");
            sliceDefense = float.Parse($"{sliceDefense:0.0}");
            thrustDefense = float.Parse($"{thrustDefense:0.0}");

            fireDefense = float.Parse($"{fireDefense:0.0}");
            iceDefense = float.Parse($"{iceDefense:0.0}");
            lighteningDefense = float.Parse($"{lighteningDefense:0.0}");
            darkDefense = float.Parse($"{darkDefense:0.0}");
            
            stability = float.Parse($"{stability:0.0}");
        }

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
            copy.subtype = subtype;
            copy.maxDurability = maxDurability;
            copy.weight = weight;

            copy.fireDefense = fireDefense;
            copy.iceDefense = iceDefense;
            copy.lighteningDefense = lighteningDefense;
            copy.darkDefense = darkDefense;

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

        void CalcDefenseFromWeight()
        {
            strikeDefense = CalcDefence();
            sliceDefense  = CalcDefence();
            thrustDefense = CalcDefence();
            
            fireDefense       = CalcDefence();
            iceDefense        = CalcDefence();
            lighteningDefense = CalcDefence();
            darkDefense       = CalcDefence();

            switch (material)
            {
                case MaterialForEditor.Cloth:
                    stability = float.Parse($"{(Random.Range(weight * 0.85f, weight * 1.15f) + (weight * weight * 0.0245f)) * 0.1f * GetPartDefenseMultiplier() : 0.0}");
                    maxDurability = (int) (Random.Range(70, 110) * GetPartDefenseMultiplier() * 1.2f);

                    strikeDefense     *= 1f;
                    sliceDefense      *= 0.8f;
                    thrustDefense     *= 0.7f;
                    fireDefense       *= 0.5f;
                    iceDefense        *= 0.8f;
                    lighteningDefense *= 0.9f;
                    darkDefense       *= 1f;

                    break;
                case MaterialForEditor.Leather:
                    stability = float.Parse($"{(Random.Range(weight * 0.875f, weight * 1.05f) + (weight * weight * 0.0175f)) * GetPartDefenseMultiplier() : 0.0}");
                    maxDurability = (int) (Random.Range(90, 130) * GetPartDefenseMultiplier() * 1.3f);
                    
                    strikeDefense     *= 0.9f;
                    sliceDefense      *= 0.8f;
                    thrustDefense     *= 0.8f;
                    fireDefense       *= 0.8f;
                    iceDefense        *= 0.7f;
                    lighteningDefense *= 1f;
                    darkDefense       *= 0.7f;
                    
                    break;
                case MaterialForEditor.Metal:
                    stability = float.Parse($"{(Random.Range(weight * 0.9f, weight * 1.1f) + (weight * weight * 0.065f)) * GetPartDefenseMultiplier(): 0.0}");
                    maxDurability = (int) (Random.Range(80, 120) * GetPartDefenseMultiplier() * 1.2f);
                    
                    strikeDefense     *= 0.7f;
                    sliceDefense      *= 1f;
                    thrustDefense     *= 0.8f;
                    fireDefense       *= 0.9f;
                    iceDefense        *= 1f;
                    lighteningDefense *= 0.6f;
                    darkDefense       *= 0.6f;
                    
                    break;
            }
        }

        float GetPartDefenseMultiplier()
        {
            float partMult = 1;
            switch (subtype)
            {
                case ArmorType.Head:
                    partMult = 0.6f;
                    break;
                case ArmorType.Torso:
                    break;
                case ArmorType.Arm:
                    partMult = 0.3f;
                    break;
                case ArmorType.Leg:
                    partMult = 0.4f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return partMult;
        }
        float CalcDefence()
        {
            
            return float.Parse($"{Random.Range(weight * 0.85f, weight * 1.15f) + (weight * weight * 0.0245f) * GetPartDefenseMultiplier() : 0.0}");
        }
        void LessDefense()
        {
            strikeDefense     -= 0.5f;
            sliceDefense      -= 0.5f;
            thrustDefense     -= 0.5f;
            fireDefense       -= 0.5f;
            iceDefense        -= 0.5f;
            lighteningDefense -= 0.5f;
            darkDefense       -= 0.5f;

            stability -= 0.25f;
        }
        void MoreDefense()
        {
            strikeDefense     += 0.5f;
            sliceDefense      += 0.5f;
            thrustDefense     += 0.5f;
            fireDefense       += 0.5f;
            iceDefense        += 0.5f;
            lighteningDefense += 0.5f;
            darkDefense       += 0.5f;

            stability += 0.25f;
        }

        public enum MaterialForEditor
        {
            Cloth,
            Leather,
            Metal
        }
#endif
    }
}
