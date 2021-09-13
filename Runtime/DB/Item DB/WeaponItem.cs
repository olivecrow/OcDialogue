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
        public bool IsEquipped { get; set; }

        public int maxDurability = 100;
        public int weight;

        public BattleStat AttackStat => attackStat;
        [SerializeField]BattleStat attackStat;

        public AssetReference Avatar => avatar;
        public ItemBase ItemBase => this;
        public AssetReference avatar;

        protected override ItemBase CreateInstance()
        {
            return CreateInstance<WeaponItem>();
        }

        protected override void ApplyTypeProperty(ItemBase baseCopy)
        {
            var copy = baseCopy as WeaponItem;
            copy.subtype = subtype;
            copy.maxDurability = maxDurability;
            copy.weight = weight;
            
            copy.attackStat.Strike = attackStat.Strike;
            copy.attackStat.Slice = attackStat.Slice;
            copy.attackStat.Thrust = attackStat.Thrust;
            
            copy.attackStat.Fire = attackStat.Fire;
            copy.attackStat.Ice = attackStat.Ice;
            copy.attackStat.Lightening = attackStat.Lightening;
            copy.attackStat.Dark = attackStat.Dark;

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
