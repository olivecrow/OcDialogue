using System;
using System.Collections;
using System.Collections.Generic;
using OcUtility;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue.DB
{
    [Serializable]
    public class DataSetter : IOcDataSelectable
    {
        public enum Operator
        {
            Set,
            Add,
            Multiply,
            Divide
        }
        public OcData Data => TargetData;

        public string Detail => detail;

        [InfoBox("데이터가 비어있음", InfoMessageType.Error, VisibleIf = "@TargetData == null")]
        [HorizontalGroup("1")]
        [GUIColor(1f,1f,1.2f)]
        [InlineButton("OpenSelectWindow", " 선택 ")]
        [LabelText("@e_address")]
        [LabelWidth(180)]
        [SerializeField] OcData TargetData;
        
        /// <summary> TargetData내에서도 판단의 분류가 나뉘는 경우, 여기에 해당하는 값을 입력해서 그걸 기준으로 어떤 변수를 판단할지 정함. </summary>
        [HorizontalGroup("1", MaxWidth = 200)] [HideLabel] [HideIf("@string.IsNullOrWhiteSpace(Detail)")]
        [ValueDropdown("GetDetail")] [GUIColor(1f,1f,1f)]
        public string detail;
        
        [LabelText("@e_Label"), LabelWidth(250)][GUIColor(1,1,1,2f)]
        [HideLabel][ValueDropdown("GetOperator")][HorizontalGroup("2", MinWidth = 400)] 
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

        /// <summary> Setter를 실행함. </summary>
        public void Execute()
        {
            switch (TargetData)
            {
                case DataRow dataRow:
                    switch (dataRow.Type)
                    {
                        case DataRowType.Bool:
                            dataRow.SetValue(BoolValue);
                            break;
                        case DataRowType.Int:
                            dataRow.SetValue(dataRow.RuntimeValue.IntValue.CalcSetterOperator(op, IntValue));
                            break;
                        case DataRowType.Float:
                            dataRow.SetValue(dataRow.RuntimeValue.FloatValue.CalcSetterOperator(op, FloatValue));
                            break;
                        case DataRowType.String:
                            dataRow.SetValue(StringValue);
                            break;
                    }
                    break;
                case ItemBase item:
                    if (Inventory.PlayerInventory == null)
                    {
                        Printer.Print("[DataSetter] Inventory.PlayerInventory가 비어있음", LogType.Error);
                        return;
                    }

                    if (IntValue == 0)
                    {
                        Printer.Print("[DataSetter] 아이템의 개수 변경값이 0으로 되어있음", LogType.Error);
                        return;
                    }
                    
                    if (IntValue > 0) Inventory.PlayerInventory.AddItem(item, IntValue);
                    else if(IntValue < 0) Inventory.PlayerInventory.RemoveItem(item, Mathf.Abs(IntValue));
                    break;
                case Quest quest:
                    switch (detail)
                    {
                        case DataChecker.QUEST_STATE:
                            quest.SetState(QuestStateValue);
                            break;
                        case DataChecker.QUEST_CLEARAVAILABILITY:
                            Printer.Print($"[DataSetter] {DataChecker.QUEST_CLEARAVAILABILITY}는 읽기 전용 속성임.", LogType.Error);
                            break;
                        default:
                            Printer.Print($"[DataSetter] Quest에 대한 Detail 설정이 안 되어있음 | Detail : {detail}", LogType.Error);
                            break;
                    }
                    break;
                case NPC npc:
                    switch (detail)
                    {
                        case DataChecker.NPC_ENCOUNTERED:
                            npc.SetEncountered(BoolValue);
                            break;
                        default:
                            Printer.Print($"[DataSetter] NPC 대한 Detail 설정이 안 되어있음 | Detail : {detail}", LogType.Error);
                            break;
                    }
                    break;
                case Enemy enemy:
                    switch (detail)
                    {
                        case DataChecker.ENEMY_KILLCOUNT:
                            enemy.SetKillCount(IntValue);
                            break;
                        default:
                            Printer.Print($"[DataSetter] Enemy 대한 Detail 설정이 안 되어있음 | Detail : {detail}", LogType.Error);
                            break;
                    }
                    break;
            }
        }


#if UNITY_EDITOR
        string e_Label => TargetData == null ?
            "" :
            string.IsNullOrWhiteSpace(detail) ? $"< {TargetData.name} > 을(를)" : $"< {TargetData.name} >의 {detail} (을)를";
        [HideInInspector] public string e_address;
        public void UpdateAddress()
        {
            e_address = TargetData == null ? "" : TargetData.TotalAddress.Replace(TargetData.Address, "");
        }
        
        void OpenSelectWindow()
        {
            DataSelectWindow.Open(this);
        }

        /// <summary> Editor Only. </summary>
        public void SetTargetData(OcData data)
        {
            TargetData = data;
            switch (data)
            {
                case DataRow dataRow:
                    detail = null;
                    break;
                case ItemBase item:
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

            op = GetOperator()[0].Value;
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
        ValueDropdownList<string> GetDetail()
        {
            var list = new ValueDropdownList<string>();
            switch (TargetData)
            {
                case Quest quest:
                    list.Add(DataChecker.QUEST_STATE);
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
        ValueDropdownList<Operator> GetOperator()
        {
            var list = new ValueDropdownList<Operator>();

            switch (TargetData)
            {
                case Enemy enemy:
                case ItemBase item:
                    list.Add(Operator.Add);
                    return list;
            }
            
            list.Add(Operator.Set);

            if (GetDataType() is DataRowType &&
                ((DataRowType) GetDataType() == DataRowType.Int || (DataRowType) GetDataType() == DataRowType.Float))
            {
                list.Add(Operator.Add);
                list.Add(Operator.Multiply);
                list.Add(Operator.Divide);
            }

            return list;
        }

#endif

    }
}
