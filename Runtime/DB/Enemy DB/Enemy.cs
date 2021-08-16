using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue.DB;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    public class Enemy : ComparableData
    {
        public override string Key { get; }
        [InfoBox("몬스터가 여러개의 Damager를 가지는 경우, 여기에 적힌 값을 기본으로 두고, 각 Damager에 배율을 정해서 곱해서 사용할 것.")]
        public string Category;
        public string key;
        [InlineButton(nameof(CalcStatsFromLevelAndWeight), "스탯 반영")]public EnemyLevel EnemyLevel;
        public float Weight;
        [HorizontalGroup("BattleStat")]public BattleStat AttackStat;
        [HorizontalGroup("BattleStat")]public BattleStat DefenseStat;
        public float HP;
        public float Balance;
        public float Stability;
        public ItemDropInfo[] DropInfo;

        [BoxGroup("Runtime")] [ShowInInspector] public int KillCount { get; set; }
        [BoxGroup("DataRow")]public DataRowContainer DataRowContainer;

        public Enemy GetCopy()
        {
            var copy = CreateInstance<Enemy>();
            copy.Category = Category;
            copy.key = key;
            copy.name = name;
            copy.Weight = Weight;
            copy.AttackStat = AttackStat;
            copy.DefenseStat = DefenseStat;
            copy.HP = HP;
            copy.Balance = Balance;
            copy.Stability = Stability;
            var dropInfo = new List<ItemDropInfo>();
            foreach (var info in DropInfo)
            {
                dropInfo.Add(info);
            }
            copy.DropInfo = dropInfo.ToArray();

            copy.DataRowContainer = new DataRowContainer(copy, DataRowContainer.GetAllCopies());
            
            return copy;
        }
        
        public override bool IsTrue(CompareFactor factor, Operator op, object value1)
        {
            switch (factor)
            {
                case CompareFactor.EnemyKillCount:
                    return op switch
                    {
                        Operator.Equal => KillCount == (int) value1,
                        Operator.NotEqual => KillCount != (int) value1,
                        Operator.Greater => KillCount > (int) value1,
                        Operator.GreaterEqual => KillCount >= (int) value1,
                        Operator.Less => KillCount < (int) value1,
                        Operator.LessEqual => KillCount <= (int) value1,
                        _ => false
                    };
                default: return false;
            }
        }

#if UNITY_EDITOR
        
        void Reset()
        {
            if(DataRowContainer != null) DataRowContainer.owner = this;
        }

        void OnValidate()
        {
            DataRowContainer.CheckNames();
        }
        
        void CalcStatsFromLevelAndWeight()
        {
            void setPhysicalAttack(float uniformValue)
            {
                AttackStat.Strike = uniformValue;
                AttackStat.Slice  = uniformValue;
                AttackStat.Thrust = uniformValue;
            }

            void setPhysicalDefense(float uniformValue)
            {
                DefenseStat.Strike = uniformValue;
                DefenseStat.Slice  = uniformValue;
                DefenseStat.Thrust = uniformValue;
            }

            void setElementalDefense(float uniformValue)
            {
                DefenseStat.Fire = uniformValue;
                DefenseStat.Ice = uniformValue;
                DefenseStat.Lightening = uniformValue;
                DefenseStat.Dark = uniformValue;
            }
            switch (EnemyLevel)
            {
                case EnemyLevel.Standard:
                    HP = Weight * 10;
                    Balance = Weight;
                    Stability = Mathf.Lerp(10, 40, Weight / 100f);
                    setPhysicalAttack(Weight * 1.2f);
                    setPhysicalDefense(Weight * 0.5f);
                    setElementalDefense(1);
                    break;
                case EnemyLevel.Ranger:
                    HP = Weight * 7;
                    Balance = Weight * 0.8f;
                    Stability = Weight * 0.3f;
                    Stability = Mathf.Lerp(10, 40, Weight / 120f);
                    setPhysicalDefense(Weight * 0.3f);
                    setElementalDefense(1);
                    break;
                case EnemyLevel.Magician:
                    HP = Weight * 5;
                    Balance = Weight * 0.6f;
                    Stability = Weight * 0.2f;
                    Stability = Mathf.Lerp(10, 40, Weight / 150f);
                    setPhysicalAttack(Weight * 0.7f);
                    setPhysicalDefense(Weight * 0.2f);
                    setElementalDefense(Weight);
                    break;
                case EnemyLevel.Heavy:
                    HP = Weight * 15;
                    Balance = Weight * 1.2f;
                    Stability = Weight * 0.5f;
                    Stability = Mathf.Lerp(10, 40, Weight / 50f);
                    setPhysicalAttack(Weight * 1.5f);
                    setPhysicalDefense(Weight * 1.5f);
                    setElementalDefense(1);
                    break;
                case EnemyLevel.Named:
                    HP = Weight * 20;
                    Balance = Weight;
                    Stability = Mathf.Lerp(10, 40, Weight / 100f);
                    setPhysicalAttack(Weight * 2f);
                    setPhysicalDefense(Weight * 0.7f);
                    setElementalDefense(Weight * 0.3f);
                    break;
                case EnemyLevel.FieldBoss:
                    HP = Weight * 25;
                    Balance = Weight * 2f;
                    Stability = Weight;
                    setPhysicalAttack(Weight * 1.8f);
                    setPhysicalDefense(Weight);
                    setElementalDefense(Weight * 0.5f);
                    break;
                case EnemyLevel.Boss:
                    HP = Weight * 30;
                    Balance = Weight * 2f;
                    Stability = Weight;
                    setPhysicalAttack(Weight * 2f);
                    setPhysicalDefense(Weight * 1.2f);
                    setElementalDefense(Weight);
                    break;
            }
        }
        [Button, HorizontalGroup("DataRow/Row"), PropertyOrder(9), GUIColor(0,1,1)]
        void AddData()
        {
            DataRowContainer.owner = this;
            DataRowContainer.AddData(DBType.Enemy, DataStorageType.Embeded);
        }
        
        [Button, HorizontalGroup("DataRow/Row"), PropertyOrder(9), GUIColor(1,0,0)]
        void DeleteData(string k)
        {
            DataRowContainer.DeleteRow(k, DataStorageType.Embeded);
        }

        [Button, HorizontalGroup("DataRow/Row"), PropertyOrder(9)]
        void MatchNames()
        {
            DataRowContainer.MatchDataRowNames();
        }
#endif
    }
}
