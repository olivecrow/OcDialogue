using System;
using System.Collections;
using System.Collections.Generic;
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
    }
    
    [Serializable]
    public class CheckFactor : ICheckable
    {
        public int Index { get; set; }
        [ReadOnly]
        public string path;
        [HideInInspector] public DBType DBType;
        [HideLabel, HorizontalGroup("Value"), InlineButton("OpenSelectWindow", "선택"), LabelWidth(200)]
        public ComparableData targetData;

        [HideLabel, HorizontalGroup("Value"), LabelWidth(100)]
        public Operator op;

        [HideLabel, HorizontalGroup("Value")] 
        [ShowIf("GetValidFactor", CompareFactor.Boolean | CompareFactor.NpcEncounter), ExplicitToggle(), LabelWidth(100)]
        public bool BoolValue;

        [HideLabel, HorizontalGroup("Value")] [ShowIf("GetValidFactor", CompareFactor.String)]
        public string StringValue;
        
        [HideLabel, HorizontalGroup("Value")] [ShowIf("GetValidFactor", CompareFactor.Int | CompareFactor.ItemCount)]
        public int IntValue;

        [HideLabel, HorizontalGroup("Value")] [ShowIf("GetValidFactor", CompareFactor.Float)]
        public float FloatValue;
        [HideLabel, HorizontalGroup("Value")] [ShowIf("GetValidFactor", CompareFactor.QuestState)]
        public Quest.State QuestStateValue;

        public object TargetValue
        {
            get
            {
                return targetData switch
                {
                    DataRow dataRow => dataRow.TargetValue,
                    Quest quest => QuestStateValue,
                    ItemBase item => IntValue,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        public bool IsTrue()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // var presetRow = GameProcessDataEditorPreset.Instance.GetCopy(targetData.Key);
                // return presetRow.IsTrue(presetRow.type.ToCompareFactor(), op, TargetValue);
            }
#endif
            // TODO : 런타임용 데이터에서 값을 읽기.
            return false;
        }
        
#if UNITY_EDITOR
        public override string ToString()
        {
            return $"{targetData.Key} {op.ToOperationString()} {TargetValue}";
        }

        /// <summary> 현재 값을 추론하려는 데이터의 비교값 타입을 반환함. </summary>
        public CompareFactor GetValidFactor()
        {
            return targetData switch
            {
                DataRow dataRow => dataRow.type switch
                {
                    DataRow.Type.Boolean => CompareFactor.Boolean,
                    DataRow.Type.String => CompareFactor.String,
                    DataRow.Type.Int => CompareFactor.Int,
                    DataRow.Type.Float => CompareFactor.Float,
                },
                Quest quest => CompareFactor.QuestState,
                ItemBase item => CompareFactor.ItemCount,
                NPC npc => CompareFactor.NpcEncounter,
            };
        }

        [HorizontalGroup("Expression"), HideLabel, ReadOnly]public string expression;
        
        void OpenSelectWindow()
        {
            var window = DataSelectWindow.Open();
            window.Target = this;
        }
        
        /// <summary> 에디터에선 프리셋을 기준으로 출력하고, 플레이 모드에선 로드된 값을 기준으로 출력함. </summary>
        [HorizontalGroup("Expression"), Button("결과 출력")]
        void Check()
        {
            if (Application.isPlaying)
            {
                // TODO : 런타임에 DB Manager에서 GameProcessDataUser 등 DataUser를 캐싱해서 거기서 참조를 얻고 값을 출력할 것.
            }
            else
            {
                // 플레이 모드가 아닐때 사용하는 디버그용 코드.
                // var presetRow = DBType switch
                // {
                //     DBType.GameProcess => GameProcessDataEditorPreset.Instance.dataRows.Find(x => x.key == targetData.Key),
                //     DBType.Item => InventoryEditorPreset.Instance.ItemPresets.Find(x => x.item.Key == targetData.Key),
                //     DBType.Quest => QuestEditorPreset.Instance.Quests.Find(x => x.quest.Key == targetData.Key),
                //     DBType.NPC => expr,
                //     DBType.Enemy => expr,
                //     _ => throw new ArgumentOutOfRangeException()
                // }
                // expression = $"{targetData.Key} {op.ToOperationString()} {TargetValue} ? " +
                //              $"=> {presetRow.IsTrue(presetRow.type.ToCompareFactor(), op, TargetValue)}";
            }
        }
#endif
    }
}