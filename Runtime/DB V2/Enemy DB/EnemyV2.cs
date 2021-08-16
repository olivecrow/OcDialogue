using System;
using System.Collections;
using System.Collections.Generic;
using OcUtility;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue.DB
{
    public class EnemyV2 : AddressableData, IDataRowUser
    {
        public override string Address => $"{Category}/{name}";
        public DataRowContainerV2 DataRowContainer => dataRowContainer;
        [InfoBox("몬스터가 여러개의 Damager를 가지는 경우, 여기에 적힌 값을 기본으로 두고, 각 Damager에 배율을 정해서 곱해서 사용할 것.")]
        public string Category;
        [InlineButton(nameof(CalcStatsFromLevelAndWeight), "스탯 반영")]public EnemyLevel EnemyLevel;
        public float Weight;
        [HorizontalGroup("BattleStat")]public BattleStat AttackStat;
        [HorizontalGroup("BattleStat")]public BattleStat DefenseStat;
        public float HP;
        public float Balance;
        public float Stability;
        public ItemDropInfo[] DropInfo;
        [BoxGroup("DataRow")]public DataRowContainerV2 dataRowContainer;
#if UNITY_EDITOR
        
        public RuntimeValue EditorPreset => _editorPreset;
        [SerializeField]
        [DisableIf("@UnityEditor.EditorApplication.isPlaying")]
        [BoxGroup("Debug")]
        [HorizontalGroup("Debug/1")]
        RuntimeValue _editorPreset;
#endif
        [ShowInInspector]
        [HorizontalGroup("Debug/1")]
        [EnableIf("@UnityEngine.Application.isPlaying")]
        public RuntimeValue Runtime
        {
            get => _runtime;
            set => _runtime = value;
        }
        RuntimeValue _runtime;


        [Serializable]
        public struct RuntimeValue
        {
            [LabelWidth(120)]public int KillCount;
        }
#if UNITY_EDITOR
        
        void Reset()
        {
            if (DataRowContainer == null) dataRowContainer = new DataRowContainerV2();
            DataRowContainer.Parent = this;
        }

        public void Resolve()
        {
            if(DataRowContainer.Parent != this) Printer.Print($"[Quest]{name}) DataRowContainer의 Parent를 재설정");
            DataRowContainer.Parent = this;
            DataRowContainer.MatchParent();
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
#endif
    }
}
