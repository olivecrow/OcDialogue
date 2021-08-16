using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue.Editor;
using OcUtility;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    [Serializable]
    public class DataSetter
    {
        [OnValueChanged("CheckValidation")]
        [InfoBox("@e_message", InfoMessageType.Warning, "@!string.IsNullOrWhiteSpace(e_message)")]
        public DataSelector DataSelector;

        [OnValueChanged("CheckValidation")]
        [ExplicitToggle]
        [LabelText("@e_Label"), LabelWidth(200)] [SuffixLabel("@e_SuffixLabel")]
        [HorizontalGroup("1", Width = 300, MinWidth = 200, MaxWidth = 500, VisibleIf = "@DataSelector.GetValidFactor() == CompareFactor.Boolean || DataSelector.GetValidFactor() == CompareFactor.NpcEncounter")]
        public bool BoolValue;

        [OnValueChanged("CheckValidation")]
        [LabelText("@e_Label"), LabelWidth(200)] [SuffixLabel("!e_SuffixLabel")]
        [HorizontalGroup("2", Width = 300,  MinWidth = 200, MaxWidth = 500, VisibleIf = "@DataSelector.GetValidFactor() == CompareFactor.String")]
        public string StringValue;
        
        [OnValueChanged("CheckValidation")]
        [LabelText("@e_Label"), LabelWidth(200)] [SuffixLabel("@e_SuffixLabel")]
        [HorizontalGroup("3", Width = 300,  MinWidth = 200, MaxWidth = 500, VisibleIf = "@DataSelector.GetValidFactor() == CompareFactor.Int || DataSelector.GetValidFactor() == CompareFactor.ItemCount")]
        public int IntValue;

        [OnValueChanged("CheckValidation")]
        [LabelText("@e_Label"), LabelWidth(200)] [SuffixLabel("@e_SuffixLabel")]
        [HorizontalGroup("4", Width = 300, MinWidth = 200, MaxWidth = 500, VisibleIf = "@DataSelector.GetValidFactor() == CompareFactor.Float")]
        public float FloatValue;
        
        [OnValueChanged("CheckValidation")]
        [LabelText("@e_Label"), LabelWidth(200)] [SuffixLabel("@e_SuffixLabel")]
        [HorizontalGroup("5", Width = 300, MinWidth = 200, MaxWidth = 500, VisibleIf = "@DataSelector.GetValidFactor() == CompareFactor.QuestState")]
        public Quest.State QuestStateValue;

        public object TargetValue
        {
            get
            {
                return DataSelector.targetData switch
                {
                    DataRow dataRow => dataRow.type switch
                    {
                        DataRow.Type.Boolean => BoolValue,
                        DataRow.Type.Int => IntValue,
                        DataRow.Type.Float => FloatValue,
                        DataRow.Type.String => StringValue,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    Quest quest => QuestStateValue,
                    NPC npc => IntValue,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
        
        
        /// <summary> 설정된 Setter를 실행함. </summary>
        public void Execute()
        {
            e_SuffixLabel = "";
            switch (DataSelector.DBType)
            {
                case DBType.GameProcess:
                    GameProcessDatabase.Runtime.SetValue(DataSelector.targetData.Key, TargetValue);
                    break;
                case DBType.Item:
                    // if (Inventory.PlayerInventory == null)
                    // {
                    //     Debug.LogError("[DataSetter] Inventory.PlayerInventory가 설정되어있지 않음");
                    //     return;
                    // }
                    // var item = ((ItemBase)DataSelector.targetData).GetCopy();
                    //
                    // if(IntValue > 0)
                    // {
                    //     Inventory.PlayerInventory.AddItem(item, IntValue);
                    // }
                    // if(IntValue < 0)
                    // {
                    //     Inventory.PlayerInventory.RemoveItem(item, -IntValue);
                    // }
                    // else
                    // {
                    //     Debug.LogError("[DataSetter] 아이템 증감의 개수가 0으로 설정됨".ToRichText(Color.cyan));
                    // }
                    break;
                case DBType.Quest:
                    var quest = DataSelector.targetData as Quest;
                    if (DataSelector.DetailTargetProperty == DataSelector.PROPERTY_QUESTSTATE)
                    {
                        QuestDatabase.Runtime.SetQuestState(quest.key, QuestStateValue);
                    }
                    break;
                case DBType.NPC:
                    Debug.LogWarning("[DataSetter] 아직 NPC에 대한 setter 설정이 없음".ToRichText(Color.cyan));
                    break;
                case DBType.Enemy:
                    Debug.LogWarning("[DataSetter] 아직 Enemy에 대한 setter 설정이 없음".ToRichText(Color.cyan));
                    break;
            }
            
            Printer.Print($"[DataSetter] < {DataSelector.targetData.Key.ToRichText(Color.cyan)} > (을)를 < {TargetValue} > 로 변경".ToRichText(Color.green.Brighten(0.5f)));
        }

#if UNITY_EDITOR
        string e_Label => DataSelector == null || DataSelector.targetData == null ? "" : $"< {DataSelector.targetData.name} > 을(를)";
        [HideInInspector] public string e_SuffixLabel;
        string e_message;
        void CheckValidation()
        {
            e_message = "";
            if(DataSelector.targetData == null) return;
            switch (DataSelector.targetData)
            {
                case DataRow dataRow:
                    e_SuffixLabel = "로 변경";
                    break;
                // case ItemBase item:
                //     e_SuffixLabel = IntValue > 0 ? "개 추가" : "개 제거";
                //     if (IntValue == 0) e_message += "- 아이템의 증감 개수는 0이 될 수 없음 \n";
                //     break;
                case Quest quest:
                    e_SuffixLabel = "로 변경";
                    if (DataSelector.DetailTargetProperty == DataSelector.PROPERTY_QUESTCLEARAVAILABILITY)
                    {
                        e_message += "- Quest Clear Availability는 읽기전용이라 무시됨.\n";
                    }
                    break;
                case NPC npc:
                    e_SuffixLabel = "로 변경";
                    break;
                case Enemy enemy:
                    e_SuffixLabel = "로 변경";
                    break;
            }
        }
#endif
    }
}
