using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue.DB.Enemy_DB;
using OcUtility;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcDialogue.DB
{
    public class Enemy : OcData, IDataRowUser
    {
        public override string Address => $"{Category}/{name}";
        public DataRowContainer DataRowContainer => dataRowContainer;
        [InfoBox("몬스터가 여러개의 Damager를 가지는 경우, 여기에 적힌 값을 기본으로 두고, 각 Damager에 배율을 정해서 곱해서 사용할 것.")]
        [ValueDropdown("GetCategory")][PropertyOrder(-2)]
        public string Category;
        [ShowInInspector, PropertyOrder(-1)][DelayedProperty]
        public string Name
        {
            get => name;
            set
            {
                name = value;
#if UNITY_EDITOR
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(this), name);          
#endif
            }
        }

        [TextArea]public string Description;
        [InlineButton("CalcStatsFromLevelAndWeight", "스탯 반영")]public EnemyClass enemyClass;
        public float Weight;
        [HorizontalGroup("BattleStat")]public BattleStat DefenseStat;
        public float HP;
        public float Balance;
        public float Stability;
        public List<DamagerInfo> DamagerInfo;
        [TableList]public ItemDropInfo[] DropInfo;
        [BoxGroup("DataRow")]public DataRowContainer dataRowContainer;
        public event Action<Enemy> OnRuntimeValueChanged;
#if UNITY_EDITOR
        
        public RuntimeValue EditorPreset => _editorPreset;
        [SerializeField]
        [DisableIf("@UnityEditor.EditorApplication.isPlaying")]
        [BoxGroup("Debug")]
        [HorizontalGroup("Debug/1")]
        RuntimeValue _editorPreset;
#endif

        public const string saveDataKey_KillCount = "KillCount";
        public const string saveDataKey_TotalKillCount = "TotalKillCount";
        
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
            [Tooltip("이번 회차의 킬 카운트")]
            [LabelWidth(120)]public int KillCount;
            [Tooltip("모든 회차의 킬 카운트")]
            [LabelWidth(120)]public int TotalKillCount;
        }

        public void GenerateRuntimeData()
        {
            _runtime = new RuntimeValue();
            dataRowContainer.GenerateRuntimeData();
            dataRowContainer.OnRuntimeValueChanged += row => OnRuntimeValueChanged?.Invoke(this);
        }

        public void AddKillCount()
        {
            _runtime.TotalKillCount++;
            _runtime.KillCount++;
            OnRuntimeValueChanged?.Invoke(this);
        }

#if DEBUG
        /// <summary>
        /// TotalKillCount와 KillCount를 동시에 설정함. 디버그 목적이 아니면 쓰지 말 것.
        /// </summary>
        /// <param name="value"></param>
        public void SetKillCount(int value)
        {
            _runtime.TotalKillCount = value;
            _runtime.KillCount = value;
            OnRuntimeValueChanged?.Invoke(this);
        }
#endif

        public CommonSaveData GetSaveData()
        {
            var data = new CommonSaveData
            {
                Key = Name,
                DataRowContainerDict = dataRowContainer.GetSaveData(),
                Data = new Dictionary<string, string>
                {
                    [saveDataKey_KillCount] = _runtime.KillCount.ToString(),
                    [saveDataKey_TotalKillCount] = _runtime.TotalKillCount.ToString()
                }
            };
            return data;
        }

        public void Load(CommonSaveData data)
        {
            dataRowContainer.Overwrite(data.DataRowContainerDict);
            if (data.Data.ContainsKey(saveDataKey_KillCount) &&
                int.TryParse(data.Data[saveDataKey_KillCount], out var killCount))
            {
                _runtime.KillCount = killCount;
            }
            
            if (data.Data.ContainsKey(saveDataKey_TotalKillCount) &&
                int.TryParse(data.Data[saveDataKey_TotalKillCount], out var totalKillCount))
            {
                _runtime.KillCount = totalKillCount;
            }
        }

        public DamagerInfo GetDamagerInfo(DamagerTag tag)
        {
            return DamagerInfo.Find(x => x.tag == tag);
        }
        
        
#if UNITY_EDITOR
        
        void Reset()
        {
            if (DataRowContainer == null) dataRowContainer = new DataRowContainer();
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
                foreach (var damagerInfo in DamagerInfo)
                {
                    damagerInfo.stat.Strike = uniformValue;
                    damagerInfo.stat.Slice  = uniformValue;
                    damagerInfo.stat.Thrust = uniformValue;
                    damagerInfo.weight = Weight * 0.1f;
                }
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
            switch (enemyClass)
            {
                case EnemyClass.Standard:
                    HP = Weight * 10;
                    Balance = Weight;
                    setPhysicalAttack(Weight * 1.2f);
                    setPhysicalDefense(Weight * 0.5f);
                    setElementalDefense(1);
                    break;
                case EnemyClass.Ranger:
                    HP = Weight * 7;
                    Balance = Weight * 0.8f;
                    setPhysicalDefense(Weight * 0.3f);
                    setElementalDefense(1);
                    break;
                case EnemyClass.Magician:
                    HP = Weight * 5;
                    Balance = Weight * 0.6f;
                    setPhysicalAttack(Weight * 0.7f);
                    setPhysicalDefense(Weight * 0.2f);
                    setElementalDefense(Weight);
                    break;
                case EnemyClass.Heavy:
                    HP = Weight * 15;
                    Balance = Weight * 1.2f;
                    setPhysicalAttack(Weight * 1.5f);
                    setPhysicalDefense(Weight * 1.5f);
                    setElementalDefense(1);
                    break;
                case EnemyClass.Named:
                    HP = Weight * 20;
                    Balance = Weight;
                    setPhysicalAttack(Weight * 2f);
                    setPhysicalDefense(Weight * 0.7f);
                    setElementalDefense(Weight * 0.3f);
                    break;
                case EnemyClass.FieldBoss:
                    HP = Weight * 25;
                    Balance = Weight * 2f;
                    setPhysicalAttack(Weight * 1.8f);
                    setPhysicalDefense(Weight);
                    setElementalDefense(Weight * 0.5f);
                    break;
                case EnemyClass.Boss:
                    HP = Weight * 30;
                    Balance = Weight * 2f;
                    setPhysicalAttack(Weight * 2f);
                    setPhysicalDefense(Weight * 1.2f);
                    setElementalDefense(Weight);
                    break;
            }
        }
        
        ValueDropdownList<string> GetCategory()
        {
            var list = new ValueDropdownList<string>();
            foreach (var category in EnemyDB.Instance.Category)
            {
                list.Add(category);
            }

            return list;
        }
#endif
    }
}
