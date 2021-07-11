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
    public class CheckFacter : ICheckable
    {
        public int Index { get; set; }
        [HideLabel, HorizontalGroup("Value"), InlineButton("OpenSelectWindow", "선택"), LabelWidth(200)]
        public DataRow targetRow;

        [HideLabel, HorizontalGroup("Value"), LabelWidth(100)]
        public Operator op;

        [HideLabel, HorizontalGroup("Value")] [ShowIf("UseBoolValue"), ExplicitToggle(), LabelWidth(100)]
        public bool BoolValue;

        [HideLabel, HorizontalGroup("Value")] [ShowIf("UseStringValue")]
        public string StringValue;

        [HideLabel, HorizontalGroup("Value")] [ShowIf("UseIntValue")]
        public int IntValue;

        [HideLabel, HorizontalGroup("Value")] [ShowIf("UseFloatValue")]
        public float FloatValue;

        public object TargetValue
        {
            get
            {
                return targetRow.type switch
                {
                    DataRow.Type.Boolean => BoolValue,
                    DataRow.Type.Int => IntValue,
                    DataRow.Type.Float => FloatValue,
                    DataRow.Type.String => StringValue,
                    _ => 0
                };
            }
        }

        public bool IsTrue()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var presetRow = GameProcessDataPreset.Instance.GetCopy(targetRow.key);
                return presetRow.IsTrue(presetRow.type.ToCompareFactor(), op, TargetValue);
            }
#endif
            // TODO : 런타임용 데이터에서 값을 읽기.
            return false;
        }

        public override string ToString()
        {
            return $"{targetRow.key} {op.ToOperationString()} {TargetValue}";
        }

#if UNITY_EDITOR
        bool UseBoolValue => targetRow != null && targetRow.type == DataRow.Type.Boolean;
        bool UseStringValue => targetRow != null && targetRow.type == DataRow.Type.String;
        bool UseIntValue => targetRow != null && targetRow.type == DataRow.Type.Int;
        bool UseFloatValue => targetRow != null && targetRow.type == DataRow.Type.Float;

        void OpenSelectWindow()
        {
            var window = DataSelectWindow.Open();
            window.Target = this;
        }

        /// <summary> 에디터에선 프리셋을 기준으로 출력하고, 플레이 모드에선 로드된 값을 기준으로 출력함. </summary>
        [Button("결과 출력")]
        void Check()
        {
            if (Application.isPlaying)
            {
                // TODO : 런타임에 DB Manager에서 GameProcessDataUser 등 DataUser를 캐싱해서 거기서 참조를 얻고 값을 출력할 것.
            }
            else
            {
                var presetRow = GameProcessDataPreset.Instance.GetCopy(targetRow.key);
                Debug.Log($"{targetRow.key} {op.ToOperationString()} {TargetValue} ? " +
                          $"=> {presetRow.IsTrue(presetRow.type.ToCompareFactor(), op, TargetValue)}");
            }
        }
#endif
    }
}