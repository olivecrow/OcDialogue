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
        public override ItemType type => ItemType.Armor;
        public virtual ArmorType subtype { get; protected set; }
        public override string SubTypeString => subtype.ToString();

#if UNITY_EDITOR
        /// <summary> Editor Only. 방어력 계산 등을 위해 에디터에서만 사용하는 타입. </summary>
        public MaterialForEditor material;
#endif
        public int MaxUpgrade => 3;
        public virtual int CurrentUpgrade { get; set; }
        public virtual int MaxDurability => maxDurability;
        public virtual float CurrentDurability { get; set; }
        public virtual float Weight => weight;
        public bool IsEquipped { get; set; }

        public int maxDurability = 100;
        [InlineButton("CalcDefenseFromWeight", "무게로 방어력 계산")][InlineButton("MoreDefense", "▶")][InlineButton("LessDefense", "◀")]
        public float weight;

        [Range(0f, 50f)]public float stability;

        public BattleStat DefenseStat => defenseStat;
        [SerializeField]BattleStat defenseStat;
        public AssetReference Avatar => avatar;
        public ItemBase ItemBase => this;
        public AssetReference avatar;

        void OnValidate()
        {
            defenseStat.Strike = float.Parse($"{defenseStat.Strike:0.0}");
            defenseStat.Slice = float.Parse($"{defenseStat.Slice:0.0}");
            defenseStat.Thrust = float.Parse($"{defenseStat.Thrust:0.0}");

            defenseStat.Fire = float.Parse($"{defenseStat.Fire:0.0}");
            defenseStat.Ice = float.Parse($"{defenseStat.Ice:0.0}");
            defenseStat.Lightening = float.Parse($"{defenseStat.Lightening:0.0}");
            defenseStat.Dark = float.Parse($"{defenseStat.Dark:0.0}");
            
            stability = float.Parse($"{stability:0.0}");
        }

        protected override ItemBase CreateInstance()
        {
            return CreateInstance<ArmorItem>();
        }

        protected override void ApplyTypeProperty(ItemBase baseCopy)
        {
            var copy = baseCopy as ArmorItem;
            copy.subtype = subtype;
            copy.maxDurability = maxDurability;
            copy.weight = weight;

            copy.defenseStat.Fire = defenseStat.Fire;
            copy.defenseStat.Ice = defenseStat.Ice;
            copy.defenseStat.Lightening = defenseStat.Lightening;
            copy.defenseStat.Dark = defenseStat.Dark;

            copy.defenseStat.Strike = defenseStat.Strike;
            copy.defenseStat.Slice = defenseStat.Slice;
            copy.defenseStat.Thrust = defenseStat.Thrust;

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
            defenseStat.Strike = CalcDefence();
            defenseStat.Slice  = CalcDefence();
            defenseStat.Thrust = CalcDefence();
            
            defenseStat.Fire       = CalcDefence();
            defenseStat.Ice        = CalcDefence();
            defenseStat.Lightening = CalcDefence();
            defenseStat.Dark       = CalcDefence();

            switch (material)
            {
                case MaterialForEditor.Cloth:
                    stability = float.Parse($"{(Random.Range(weight * 0.85f, weight * 1.15f) + (weight * weight * 0.0245f)) * 0.1f * GetPartDefenseMultiplier() : 0.0}");
                    maxDurability = (int) (Random.Range(70, 110) * GetPartDefenseMultiplier() * 1.2f);

                    defenseStat.Strike     *= 1f;
                    defenseStat.Slice      *= 0.8f;
                    defenseStat.Thrust     *= 0.7f;
                    defenseStat.Fire       *= 0.5f;
                    defenseStat.Ice        *= 0.8f;
                    defenseStat.Lightening *= 0.9f;
                    defenseStat.Dark       *= 1f;

                    break;
                case MaterialForEditor.Leather:
                    stability = float.Parse($"{(Random.Range(weight * 0.875f, weight * 1.05f) + (weight * weight * 0.0175f)) * GetPartDefenseMultiplier() : 0.0}");
                    maxDurability = (int) (Random.Range(90, 130) * GetPartDefenseMultiplier() * 1.3f);
                    
                    defenseStat.Strike     *= 0.9f;
                    defenseStat.Slice      *= 0.8f;
                    defenseStat.Thrust     *= 0.8f;
                    defenseStat.Fire       *= 0.8f;
                    defenseStat.Ice        *= 0.7f;
                    defenseStat.Lightening *= 1f;
                    defenseStat.Dark       *= 0.7f;
                    
                    break;
                case MaterialForEditor.Metal:
                    stability = float.Parse($"{(Random.Range(weight * 0.9f, weight * 1.1f) + (weight * weight * 0.065f)) * GetPartDefenseMultiplier(): 0.0}");
                    maxDurability = (int) (Random.Range(80, 120) * GetPartDefenseMultiplier() * 1.2f);
                    
                    defenseStat.Strike     *= 0.7f;
                    defenseStat.Slice      *= 1f;
                    defenseStat.Thrust     *= 0.8f;
                    defenseStat.Fire       *= 0.9f;
                    defenseStat.Ice        *= 1f;
                    defenseStat.Lightening *= 0.6f;
                    defenseStat.Dark       *= 0.6f;
                    
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
            defenseStat.Strike     -= 0.5f;
            defenseStat.Slice      -= 0.5f;
            defenseStat.Thrust     -= 0.5f;
            defenseStat.Fire       -= 0.5f;
            defenseStat.Ice        -= 0.5f;
            defenseStat.Lightening -= 0.5f;
            defenseStat.Dark       -= 0.5f;

            stability -= 0.25f;
        }
        void MoreDefense()
        {
            defenseStat.Strike     += 0.5f;
            defenseStat.Slice      += 0.5f;
            defenseStat.Thrust     += 0.5f;
            defenseStat.Fire       += 0.5f;
            defenseStat.Ice        += 0.5f;
            defenseStat.Lightening += 0.5f;
            defenseStat.Dark       += 0.5f;

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
