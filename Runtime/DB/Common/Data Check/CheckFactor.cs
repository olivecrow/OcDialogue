using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcUtility;
#if UNITY_EDITOR
using OcDialogue.Editor;
#endif
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    public interface ICheckable
    {
        int Index { get; }
        bool IsTrue();
        string ToExpression(bool useRichText = false);
    }
    
    [Serializable]
    public class CheckFactor : ICheckable
    {
        public int Index { get; set; }

        public DataSelector DataSelector;

        [LabelText("=>"), HorizontalGroup("Value", Width = 150), LabelWidth(40)]
        public Operator op;

        [HideLabel, HorizontalGroup("Value")] 
        [ShowIf("@DataSelector.GetValidFactor() == CompareFactor.Boolean || DataSelector.GetValidFactor() == CompareFactor.NpcEncounter"), ExplicitToggle()]
        public bool BoolValue;

        [HideLabel, HorizontalGroup("Value")] [ShowIf("@DataSelector.GetValidFactor() == CompareFactor.String")]
        public string StringValue;
        
        [HideLabel, HorizontalGroup("Value")] [ShowIf("@DataSelector.GetValidFactor() == CompareFactor.Int || DataSelector.GetValidFactor() == CompareFactor.ItemCount")]
        public int IntValue;

        [HideLabel, HorizontalGroup("Value")] [ShowIf("@DataSelector.GetValidFactor() == CompareFactor.Float")]
        public float FloatValue;
        [HideLabel, HorizontalGroup("Value")] [ShowIf("@DataSelector.GetValidFactor() == CompareFactor.QuestState")]
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
                    ItemBase item => IntValue,
                    NPC npc => IntValue,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        public bool IsTrue()
        {
            if (DataSelector.targetData == null)
            {
                Debug.LogError($"[{"CheckFactor".ToRichText(ColorExtension.Random(GetHashCode()))}] targetData가 비어있음".ToRichText(Color.cyan));
                return false;
            }
            
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return IsTrueEditorPreset();
            }
#endif
            switch (DataSelector.DBType)
            {
                case DBType.GameProcess:
                {
                    return GameProcessDatabase.Runtime.IsTrue(DataSelector.targetData.Key, op, TargetValue);
                }
                case DBType.Item:
                {
                    return Inventory.PlayerInventory.Count(DataSelector.targetData as ItemBase).IsTrue(op, IntValue);
                }
                case DBType.Quest:
                {
                    switch (DataSelector.targetData)
                    {
                        case Quest quest:
                            return QuestDatabase.Runtime.IsTrue(quest.key, op, QuestStateValue);
                        case DataRow dataRow:
                        {
                            var targetQuest = QuestDatabase.Runtime.FindQuest(dataRow);
                            if (targetQuest == null) return false;
                            return targetQuest.DataRowContainer.IsTrue(dataRow.key, op, TargetValue);
                        }
                    }
                    break;
                }
                case DBType.NPC:
                {
                    switch (DataSelector.targetData)
                    {
                        case NPC npc:
                            return NPCDatabase.Runtime.IsTrue(npc.Key, op, TargetValue);
                        case DataRow dataRow:
                        {
                            var targetNPC = NPCDatabase.Runtime.FindNPC(dataRow);
                            if (targetNPC == null) return false;
                            return targetNPC.DataRowContainer.IsTrue(dataRow.key, op, TargetValue);
                        }
                    }
                    break;
                }
                case DBType.Enemy:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        public string ToExpression(bool useRichText)
        {
            var isExplicitFactor = DataSelector.GetValidFactor() == CompareFactor.ItemCount || DataSelector.GetValidFactor() == CompareFactor.NpcEncounter ||
                                   DataSelector.GetValidFactor() == CompareFactor.QuestState;
            var text = $"{DataSelector.targetData.Key}{(isExplicitFactor ? "." + DataSelector.GetValidFactor() : "")} {op.ToOperationString()} {TargetValue}"; 
            return useRichText ? text.ToRichText(ColorExtension.Random(GetHashCode(), 0.5f)) : text;
        }

#if UNITY_EDITOR


        [HorizontalGroup("Expression"), HideLabel, ReadOnly]public string expression;
        

        /// <summary> 에디터에선 프리셋을 기준으로 출력하고, 플레이 모드에선 로드된 값을 기준으로 출력함. </summary>
        [HorizontalGroup("Expression"), Button("결과 출력")]
        void Check()
        {
            var isExplicitFactor = DataSelector.GetValidFactor() == CompareFactor.ItemCount || DataSelector.GetValidFactor() == CompareFactor.NpcEncounter ||
                                   DataSelector.GetValidFactor() == CompareFactor.QuestState;
            expression = $"{DataSelector.targetData.Key}{(isExplicitFactor ? "." + DataSelector.GetValidFactor() : "")} {op.ToOperationString()} {TargetValue} ? " +
                         $"=> {IsTrue()}";
        }


        bool IsTrueEditorPreset()
        {
            var isTrue = false;

            switch (DataSelector.DBType)
            {
                case DBType.GameProcess:
                {
                    var preset = GameProcessDatabase.Instance.editorPreset.dataRows.Find(x => x.Key == DataSelector.targetData.Key);
                    if (GameProcessDatabase.Instance.editorPreset.dataRows.Count == 0 || preset == null)
                    {
                        Debug.LogWarning("아직 GameProcessDatabase의 에디터 프리셋이 없음");
                        return false;
                    }

                    isTrue = preset.IsTrue(op, TargetValue);
                    break;
                }
                case DBType.Item:
                {
                    var count = ItemDatabase.Instance.editorPreset.ItemPresets.Count(x => x.Key == DataSelector.targetData.Key);
                    isTrue = count.IsTrue(op, IntValue);
                    break;
                }
                case DBType.Quest:
                {
                    if (DataSelector.targetData is Quest quest)
                    {
                        var preset = QuestDatabase.Instance.editorPreset.Quests.Find(x => x.Key == quest.Key);
                        if (QuestDatabase.Instance.editorPreset.Quests.Count == 0 || preset == null)
                        {
                            Debug.LogWarning("아직 QuestDatabase의 에디터 프리셋이 없음");
                            return false;
                        }

                        isTrue = preset.IsTrue(CompareFactor.QuestState, op, QuestStateValue);
                    }
                    else if (DataSelector.targetData is DataRow dataRow)
                    {
                        var preset = QuestDatabase.Instance.editorPreset.Quests.Find(x => x.quest.DataRowContainer.dataRows.Contains(dataRow));
                        if (QuestDatabase.Instance.editorPreset.Quests.Count == 0 || preset == null)
                        {
                            Debug.LogWarning("아직 QuestDatabase의 에디터 프리셋이 없음");
                            return false;
                        }

                        var presetRow = preset.overrideRows.Find(x => x.Key == DataSelector.targetData.Key);
                        isTrue = presetRow.IsTrue(op, TargetValue);
                    }

                    break;
                }
                case DBType.NPC:
                {
                    var preset = NPCDatabase.Instance.editorPreset.NPCs.Find(x => x.Key == DataSelector.targetData.Key);
                    if (NPCDatabase.Instance.editorPreset.NPCs.Count == 0 || preset == null)
                    {
                        Debug.LogWarning("아직 NPC Database의 에디터 프리셋이 없음");
                        return false;
                    }

                    if (DataSelector.targetData is NPC npc)
                    {
                        isTrue = preset.IsTrue(CompareFactor.NpcEncounter, op, BoolValue);
                    }
                    else if (DataSelector.targetData is DataRow dataRow)
                    {
                        var presetRow = preset.overrideRows.Find(x => x.Key == DataSelector.targetData.Key);
                        isTrue = presetRow.IsTrue(op, TargetValue);
                    }


                    break;
                }
                case DBType.Enemy:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return isTrue;
        }
#endif
    }
}