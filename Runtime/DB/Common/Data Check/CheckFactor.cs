using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcUtility;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcDialogue.DB
{
    public interface ICheckable
    {
        public int Index { get;}
        public bool IsTrue();
#if UNITY_EDITOR
        public string ToExpression(bool useRichText);  
#endif
    }
    [Serializable]
    public class CheckFactor : IOcDataSelectable, ICheckable
    {
        public enum Operator
        {
            Equal,
            NotEqual,
            Greater,
            GreaterEqual,
            Less,
            LessEqual
        }
        public OcData Data
        {
            get => TargetData;
            set => TargetData = value;
        }

        public int Index
        {
            get => index;
            set => index = value;
        }
        [HideInInspector]public int index;
        
        [GUIColor(1f,1f,1f)]
        [InfoBox("@e_message", InfoMessageType.Error, VisibleIf = "@TargetData == null")]
        [HorizontalGroup("1")]
        [InlineButton("PrintResult", "결과 출력")]
        [InlineButton("OpenSelectWindow", " 선택 ")]
        [LabelText("@e_address")]
        [SerializeField]
        OcData TargetData;

        /// <summary> TargetData내에서도 판단의 분류가 나뉘는 경우, 여기에 해당하는 값을 입력해서 그걸 기준으로 어떤 변수를 판단할지 정함. </summary>
        [HorizontalGroup("2")] [HideLabel] [HideIf("@string.IsNullOrWhiteSpace(Detail)")]
        [ValueDropdown("GetDetail")] [GUIColor(1f,1f,1f)]
        public string detail;
        
        [HorizontalGroup("2")] [HideLabel] [HideIf("@TargetData == null")][GUIColor(1.2f, 1.2f, 1f)]
        [ValueDropdown("GetOperator")] 
        public Operator op;

        [GUIColor(1f,1f,1f)]
        [HorizontalGroup("2"), HideLabel] [ShowIf("GetDataType", DataRowType.Bool)] [ExplicitToggle()]
        public bool BoolValue;

        [GUIColor(1f,1f,1f)]
        [HorizontalGroup("2"), HideLabel] [ShowIf("GetDataType", DataRowType.Int)]
        public int IntValue;

        [GUIColor(1f,1f,1f)]
        [HorizontalGroup("2"), HideLabel] [ShowIf("GetDataType", DataRowType.Float)]
        public float FloatValue;

        [GUIColor(1f,1f,1f)]
        [HorizontalGroup("2"), HideLabel] [ShowIf("GetDataType", DataRowType.String)]
        public string StringValue;

        [GUIColor(1f,1f,1f)]
        [HorizontalGroup("2"), HideLabel] [ShowIf("GetDataType", typeof(QuestState))]
        public QuestState QuestStateValue;

        public string Detail
        {
            get => detail;
            set => detail = value;
        }
#pragma warning disable
        public object TargetValue => TargetData switch
        {
            DataRow data => data.Type switch
            {
                DataRowType.Bool => BoolValue,
                DataRowType.Int => IntValue,
                DataRowType.Float => FloatValue,
                DataRowType.String => StringValue
            },
            ItemBase item => IntValue,
            Quest quest => detail switch
            {
                DataChecker.QUEST_STATE => QuestStateValue,
                DataChecker.QUEST_CLEARAVAILABILITY => BoolValue
            },
            NPC npc => detail switch
            {
                DataChecker.NPC_ENCOUNTERED => BoolValue
            },
            Enemy enemy => detail switch
            {
                DataChecker.ENEMY_KILLCOUNT => IntValue,
            },
            _ => null
        };
#pragma warning restore
        
        public bool IsTrue()
        {
            switch (TargetData)
            {
                case DataRow dataRow: return dataRow.IsTrue(op, TargetValue);
                case ItemBase item:
#if UNITY_EDITOR
                    if (!EditorApplication.isPlaying)
                        return ItemDatabase.Instance.editorPreset.Count(item).IsTrue(op, IntValue);
#endif
                    if (Inventory.PlayerInventory == null)
                    {
                        Printer.Print($"[CheckFactor] Inventory.PlayerInventory가 비어있음", LogType.Error);
                        return false;
                    }

                    return Inventory.PlayerInventory.Count(item).IsTrue(op, IntValue);
                case Quest quest:
                    if (string.IsNullOrWhiteSpace(detail)) detail = DataChecker.QUEST_STATE;
                    return detail switch
                    {
                        DataChecker.QUEST_STATE => quest.IsAbleToClear(op, QuestStateValue),
                        DataChecker.QUEST_CLEARAVAILABILITY => quest.IsAbleToClear(op, BoolValue),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                case NPC npc:
                    if (string.IsNullOrWhiteSpace(detail)) detail = DataChecker.NPC_ENCOUNTERED;
                    return detail switch
                    {
                        DataChecker.NPC_ENCOUNTERED => npc.IsTrue(op, BoolValue),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                case Enemy enemy:
                    if (string.IsNullOrWhiteSpace(detail)) detail = DataChecker.ENEMY_KILLCOUNT;
                    return detail switch
                    {
                        DataChecker.ENEMY_KILLCOUNT => enemy.Runtime.KillCount.IsTrue(op, IntValue),
                        _ => throw new ArgumentOutOfRangeException()
                    };
            }

            return false;
        }


#if UNITY_EDITOR
        [HideInInspector] public string e_address;
        [HideInInspector] public string e_message => 
            string.IsNullOrWhiteSpace(e_cachedDataAddress) ? "데이터가 비어있음" : $"데이터가 비어있음 | 캐싱된 Address : {e_cachedDataAddress}";
        [SerializeField, HideInInspector] string e_cachedDataAddress;
        public void SetTargetData(OcData data)
        {
            TargetData = data;
            switch (data)
            {
                case DataRow dataRow:
                    detail = null;
                    break;
                case Quest quest:
                    detail = DataChecker.QUEST_STATE;
                    break;
                case NPC npc:
                    detail = DataChecker.NPC_ENCOUNTERED;
                    break;
                case Enemy enemy:
                    detail = DataChecker.ENEMY_KILLCOUNT;
                    break;
            }

            e_cachedDataAddress = data.TotalAddress;
        }

        public void SetTargetValue(object value)
        {
            switch (value)
            {
                case bool b:
                    BoolValue = b;
                    break;
                case int i:
                    IntValue = i;
                    break;
                case float f:
                    FloatValue = f;
                    break;
                case string s:
                    StringValue = s;
                    break;
                case QuestState qs:
                    QuestStateValue = qs;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        /// <summary> Editor Only. 데이터 라벨의 주소를 업데이트함. SelectWindow에서 적용 시 호출됨. </summary>
        public void UpdateAddress()
        {
            e_address = TargetData == null ? "" : TargetData.TotalAddress.Replace(TargetData.Address, "");
        }
        
        void OpenSelectWindow()
        {
            DataSelectWindow.Open(this);
        }

        object GetDataType()
        {
            switch (TargetData)
            {
                case DataRow data:
                    return data.Type;
                case ItemBase item:
                    return DataRowType.Int;
                case Quest quest:
                    return detail switch
                    {
                        DataChecker.QUEST_STATE => typeof(QuestState),
                        DataChecker.QUEST_CLEARAVAILABILITY => DataRowType.Bool,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                case NPC npc:
                    return detail switch
                    {
                        DataChecker.NPC_ENCOUNTERED => DataRowType.Bool,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                case Enemy enemy:
                    return detail switch
                    {
                        DataChecker.ENEMY_KILLCOUNT => DataRowType.Int,
                        _ => throw new ArgumentOutOfRangeException()
                    };
            }

            return null;
        }
        
        public string ToExpression(bool useRichText = false)
        {
            if (TargetData == null) return "";
            var addDetail = string.IsNullOrWhiteSpace(detail) ? "" : $".{detail}";
            return useRichText ? 
                $"{TargetData.Address.ToRichText(ColorExtension.Random(TargetData.GetHashCode()))}{addDetail.ToRichText(Color.cyan)} " +
                $"{op.ToOperationString()} {TargetValue.ToString().ToRichText(new Color(1f, 1f, 0.7f))}" :
                $"{TargetData.Address}{addDetail} {op.ToOperationString()} {TargetValue}";
        }

        void PrintResult()
        {
            var result = IsTrue() ? "True".ToRichText(Color.green) : "False".ToRichText(Color.red);
            var prefix = Application.isPlaying
                ? "(Runtime)".ToRichText(Color.cyan) 
                : "(Editor)".ToRichText(Color.yellow);
            Printer.Print($"[DataChecker] {prefix} {ToExpression()} ? => {result}");
        }

        ValueDropdownList<Operator> GetOperator()
        {
            var list = new ValueDropdownList<Operator>();
            
            list.Add(Operator.Equal);
            list.Add(Operator.NotEqual);

            if (GetDataType() is DataRowType &&
                ((DataRowType) GetDataType() == DataRowType.Int || (DataRowType) GetDataType() == DataRowType.Float))
            {
                list.Add(Operator.Greater);
                list.Add(Operator.GreaterEqual);
                list.Add(Operator.Less);
                list.Add(Operator.LessEqual);
            }

            return list;
        }

        ValueDropdownList<string> GetDetail()
        {
            var list = new ValueDropdownList<string>();
            switch (TargetData)
            {
                case Quest quest:
                    list.Add(DataChecker.QUEST_STATE);
                    list.Add(DataChecker.QUEST_CLEARAVAILABILITY);
                    break;
                case NPC npc:
                    list.Add(DataChecker.NPC_ENCOUNTERED);
                    break;
                case Enemy enemy:
                    list.Add(DataChecker.ENEMY_KILLCOUNT);
                    break;
            }

            return list;
        }
#endif
    }
}
