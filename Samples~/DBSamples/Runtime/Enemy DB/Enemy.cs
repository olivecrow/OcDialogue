using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue;
using OcDialogue.DB;
using OcUtility;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MyDB
{
    public class Enemy : OcData, IDataRowUser
    {
        public override string Address => $"{Category}/{name}";

        public override string Category
        {
            get => category;
            set => category = value;
        }

        public DataRowContainer DataRowContainer => dataRowContainer;
        [ValueDropdown("GetCategory")][PropertyOrder(-2)]
        public string category;
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
        [InlineButton("UpdateDamagerInfoFromWeapon")]
        public OcDictionary<WeaponItem, DamagerTag> e_DamagerReference;
        public RuntimeValue EditorPreset => _editorPreset;
        [SerializeField]
        [DisableIf("@UnityEditor.EditorApplication.isPlaying")]
        [BoxGroup("Debug")]
        [HorizontalGroup("Debug/1")]
        RuntimeValue _editorPreset;
#endif

        public const string fieldName_KillCount = "KillCount";
        public const string fieldName_TotalKillCount = "TotalKillCount";
        
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
        
        public override bool IsTrue(string fieldName, CheckFactor.Operator op, object checkValue)
        {
            if(checkValue is int asInt)
            {
                if (fieldName == fieldName_KillCount) return _runtime.KillCount.IsTrue(op, asInt);
                if (fieldName == fieldName_TotalKillCount) return _runtime.KillCount.IsTrue(op, asInt);
            }
            Debug.LogWarning($"[{fieldName}]유효하지 않은 검사 | fieldName : {fieldName} | checkValueType : {checkValue.GetType()}" +
                             $"| IsTrue ==> false");
            return false;
        }

        public override object GetValue(string fieldName)
        {
            if (fieldName == fieldName_KillCount) return _runtime.KillCount;
            if (fieldName == fieldName_TotalKillCount) return _runtime.TotalKillCount;
            
            Debug.LogWarning($"[{fieldName}]유효하지 않은 fieldName : {fieldName} | GetValue ==> null");

            return null;
        }

        public override string[] GetFieldNames()
        {
            return new[] { fieldName_KillCount, fieldName_TotalKillCount };
        }

        public override void SetValue(string fieldName, DataSetter.Operator op, object value)
        {
            switch (op)
            {
                case DataSetter.Operator.Set:
                {
                    if (value is int asInt)
                    {
                        if (fieldName == fieldName_KillCount) _runtime.KillCount = asInt;
                        else if (fieldName == fieldName_TotalKillCount) _runtime.TotalKillCount = asInt;
                    }
                    else error_wrongType();
                    break;
                }
                case DataSetter.Operator.Add:
                {
                    if (value is int asInt)
                    {
                        if (fieldName == fieldName_KillCount) _runtime.KillCount += asInt;
                        else if (fieldName == fieldName_TotalKillCount) _runtime.TotalKillCount += asInt;
                    }
                    else error_wrongType();
                    break;
                }
                case DataSetter.Operator.Multiply:
                {
                    if (value is int asInt)
                    {
                        if (fieldName == fieldName_KillCount) _runtime.KillCount *= asInt;
                        else if (fieldName == fieldName_TotalKillCount) _runtime.TotalKillCount *= asInt;
                    }
                    else error_wrongType();
                    break;
                }
                case DataSetter.Operator.Divide:
                {
                    if (value is int asInt)
                    {
                        if (fieldName == fieldName_KillCount) _runtime.KillCount /= asInt;
                        else if (fieldName == fieldName_TotalKillCount) _runtime.TotalKillCount /= asInt;
                    }
                    else error_wrongType();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, null);
            }

            void error_wrongType() =>
                Debug.LogWarning($"[{fieldName}]유효하지 않은 타입 | fieldName : {fieldName} | checkValueType : {value.GetType()}");
            
        }
        public override DataRowType GetValueType(string fieldName)
        {
            if (fieldName == fieldName_KillCount) return DataRowType.Int;
            if (fieldName == fieldName_TotalKillCount) return DataRowType.Int;
            
            Debug.LogWarning($"[{fieldName}]유효하지 않은 fieldName : {fieldName} | GetValueType ==> DataRowType.String");
            return DataRowType.String;
        }

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

        public CommonSaveData GetSaveData()
        {
            var data = new CommonSaveData
            {
                Key = Name,
                DataRowContainerDict = dataRowContainer.GetSaveData(),
                Data = new Dictionary<string, string>
                {
                    [fieldName_KillCount] = _runtime.KillCount.ToString(),
                    [fieldName_TotalKillCount] = _runtime.TotalKillCount.ToString()
                }
            };
            return data;
        }

        public void Overwrite(CommonSaveData data)
        {
            dataRowContainer.Overwrite(data.DataRowContainerDict);
            if (data.Data.ContainsKey(fieldName_KillCount)) 
                int.TryParse(data.Data[fieldName_KillCount], out _runtime.KillCount);
            
            if (data.Data.ContainsKey(fieldName_TotalKillCount))
                int.TryParse(data.Data[fieldName_TotalKillCount], out _runtime.TotalKillCount);
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
            foreach (var category in EnemyDB.Instance.Categories)
            {
                list.Add(category);
            }

            return list;
        }

        void UpdateDamagerInfoFromWeapon()
        {
            foreach (var refer in e_DamagerReference)
            {
                var info = DamagerInfo.Find(x => x.tag == refer.Value);
                if(info == null) return;
                info.stat = refer.Key.AttackStat;
                info.weight = refer.Key.weight;
            }
        }
#endif
    }
}
