using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MyDB
{
    public class WeaponItem : ItemBase, IEquipment
    {
        public override ItemType type => ItemType.Weapon;
        public WeaponType subtype;
        public override string SubTypeString => subtype.ToString();
        public virtual int MaxUpgrade => 10;

        public virtual int CurrentUpgrade
        {
            get => _currentUpgrade;
            set => _currentUpgrade = value;
        }
        public virtual int MaxDurability => maxDurability;

        public virtual float CurrentDurability
        {
            get => _currentDurability;
            set => _currentDurability = value;
        }
        public virtual float Weight => weight;

        public bool IsEquipped
        {
            get => _isEquipped;
            set => _isEquipped = value;
        }

        public WeaponEquipState weaponEquipState
        {
            get => _weaponEquipState;
            set => _weaponEquipState = value;
        }

        public int maxDurability = 100;
        public float weight;

        public BattleStat AttackStat => attackStat;
        [SerializeField]BattleStat attackStat;

        public AssetReference Avatar => avatar;
        public ItemBase ItemBase => this;
        public AssetReference avatar;

        float _currentDurability;
        int _currentUpgrade;
        bool _isEquipped;
        WeaponEquipState _weaponEquipState;
        
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

        protected override void AppendAdditionalSaveData(CommonSaveData saveData)
        {
            var dict = saveData.Data;
            dict[fieldName_Durability] = $"{CurrentDurability: #.#}";
            dict[fieldName_Upgrade] = CurrentUpgrade.ToString();
            dict[fieldName_IsEquipped] = IsEquipped.ToString();
            dict[fieldName_WeaponEquipState] = weaponEquipState.ToString();
        }
        protected override void OverwriteAdditionalSaveData(CommonSaveData saveData)
        {
            var dict = saveData.Data;
            if(dict.ContainsKey(fieldName_Durability))
                float.TryParse(dict[fieldName_Durability], out _currentDurability);
            
            if(dict.ContainsKey(fieldName_Upgrade))
                int.TryParse(dict[fieldName_Upgrade], out _currentUpgrade);
            
            if(dict.ContainsKey(fieldName_IsEquipped))
                bool.TryParse(dict[fieldName_IsEquipped], out _isEquipped);
            
            if(dict.ContainsKey(fieldName_WeaponEquipState))
                Enum.TryParse(dict[fieldName_WeaponEquipState], out _weaponEquipState);
        }

#if UNITY_EDITOR
        public override void SetSubTypeFromString(string subtypeName)
        {
            subtype = (WeaponType) Enum.Parse(typeof(WeaponType), subtypeName);
        }
        
#endif
        public enum WeaponEquipState
        {
            None,
            Left,
            Right
        }
    }
}
