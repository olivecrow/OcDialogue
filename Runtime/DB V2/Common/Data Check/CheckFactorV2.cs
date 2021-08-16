using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcUtility;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.DB
{
    [Serializable]
    public class CheckFactorV2 : IAddressableDataContainer, ICheckable
    {
        public AddressableData Data
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
        [InfoBox("데이터가 비어있음", InfoMessageType.Error, VisibleIf = "@TargetData == null")]
        [HorizontalGroup("1")]
        [InlineButton("PrintResult", "결과 출력")]
        [InlineButton("OpenSelectWindow", " 선택 ")]
        [LabelText("@e_address")]
        public AddressableData TargetData;

        /// <summary> TargetData내에서도 판단의 분류가 나뉘는 경우, 여기에 해당하는 값을 입력해서 그걸 기준으로 어떤 변수를 판단할지 정함. </summary>
        [HorizontalGroup("2")] [HideLabel] [HideIf("@string.IsNullOrWhiteSpace(Detail)")]
        [ValueDropdown(nameof(GetDetail))]
        public string detail;
        
        [HorizontalGroup("2")] [HideLabel] [HideIf("@TargetData == null")][GUIColor(1.2f, 1.2f, 1f)]
        [ValueDropdown(nameof(GetOperator))] 
        public Operator op;

        [HorizontalGroup("2"), HideLabel] [ShowIf(nameof(GetDataType), DataRowType.Bool)] [ExplicitToggle()]
        public bool BoolValue;

        [HorizontalGroup("2"), HideLabel] [ShowIf(nameof(GetDataType), DataRowType.Int)]
        public int IntValue;

        [HorizontalGroup("2"), HideLabel] [ShowIf(nameof(GetDataType), DataRowType.Float)]
        public float FloatValue;

        [HorizontalGroup("2"), HideLabel] [ShowIf(nameof(GetDataType), DataRowType.String)]
        public string StringValue;

        [HorizontalGroup("2"), HideLabel] [ShowIf(nameof(GetDataType), typeof(QuestState))]
        public QuestState QuestStateValue;

        public string Detail
        {
            get => detail;
            set => detail = value;
        }
#pragma warning disable
        public object TargetValue => TargetData switch
        {
            DataRowV2 data => data.Type switch
            {
                DataRowType.Bool => BoolValue,
                DataRowType.Int => IntValue,
                DataRowType.Float => FloatValue,
                DataRowType.String => StringValue
            },
            ItemBase item => IntValue,
            QuestV2 quest => detail switch
            {
                DataCheckerV2.QUEST_STATE => QuestStateValue,
                DataCheckerV2.QUEST_CLEARAVAILABILITY => BoolValue
            },
            NPCV2 npc => detail switch
            {
                DataCheckerV2.NPC_ENCOUNTERED => BoolValue
            },
            EnemyV2 enemy => detail switch
            {
                DataCheckerV2.ENEMY_KILLCOUNT => IntValue,
            },
            _ => null
        };
#pragma warning restore
        
        public bool IsTrue()
        {
            switch (TargetData)
            {
                case DataRowV2 dataRow: return dataRow.IsTrue(op, TargetValue);
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
                case QuestV2 quest:
                    if (string.IsNullOrWhiteSpace(detail)) detail = DataCheckerV2.QUEST_STATE;
                    return detail switch
                    {
                        DataCheckerV2.QUEST_STATE => quest.IsTrue(op, QuestStateValue),
                        DataCheckerV2.QUEST_CLEARAVAILABILITY => quest.IsTrue(op, BoolValue),
                    };
                case NPCV2 npc:
                    if (string.IsNullOrWhiteSpace(detail)) detail = DataCheckerV2.NPC_ENCOUNTERED;
                    return detail switch
                    {
                        DataCheckerV2.NPC_ENCOUNTERED => npc.IsTrue(op, BoolValue)
                    };
                case EnemyV2 enemy:
                    if (string.IsNullOrWhiteSpace(detail)) detail = DataCheckerV2.ENEMY_KILLCOUNT;
                    return detail switch
                    {
                        DataCheckerV2.ENEMY_KILLCOUNT => enemy.Runtime.KillCount.IsTrue(op, IntValue)
                    };
            }

            return false;
        }


#if UNITY_EDITOR
        [HideInInspector] public string e_address;

        public void SetTargetData(AddressableData data)
        {
            TargetData = data;
            switch (data)
            {
                case DataRowV2 dataRow:
                    detail = null;
                    break;
                case QuestV2 quest:
                    detail = DataCheckerV2.QUEST_STATE;
                    break;
                case NPCV2 npc:
                    detail = DataCheckerV2.NPC_ENCOUNTERED;
                    break;
                case EnemyV2 enemy:
                    detail = DataCheckerV2.ENEMY_KILLCOUNT;
                    break;
            }
        }
        
        /// <summary> Editor Only. 데이터 라벨의 주소를 업데이트함. SelectWindow에서 적용 시 호출됨. </summary>
        public void UpdateAddress()
        {
            e_address = TargetData == null ? "" : TargetData.TotalAddress.Replace(TargetData.Address, "");
        }
        
        void OpenSelectWindow()
        {
            DataSelectWindowV2.Open(this);
        }

        object GetDataType()
        {
            switch (TargetData)
            {
                case DataRowV2 data:
                    return data.Type;
                case ItemBase item:
                    return DataRowType.Int;
                case QuestV2 quest:
                    return detail switch
                    {
                        DataCheckerV2.QUEST_STATE => typeof(QuestState),
                        DataCheckerV2.QUEST_CLEARAVAILABILITY => DataRowType.Bool
                    };
                case NPCV2 npc:
                    return detail switch
                    {
                        DataCheckerV2.NPC_ENCOUNTERED => DataRowType.Bool,
                    };
                case EnemyV2 enemy:
                    return detail switch
                    {
                        DataCheckerV2.ENEMY_KILLCOUNT => DataRowType.Int,
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
            Printer.Print($"[DataChecker V2] {prefix} {ToExpression()} ? => {result}");
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
                case QuestV2 quest:
                    list.Add(DataCheckerV2.QUEST_STATE);
                    list.Add(DataCheckerV2.QUEST_CLEARAVAILABILITY);
                    break;
                case NPCV2 npc:
                    list.Add(DataCheckerV2.NPC_ENCOUNTERED);
                    break;
                case EnemyV2 enemy:
                    list.Add(DataCheckerV2.ENEMY_KILLCOUNT);
                    break;
            }

            return list;
        }
#endif
    }
}
