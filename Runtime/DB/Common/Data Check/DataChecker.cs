using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OcUtility;
#if UNITY_EDITOR
using OcDialogue.Editor;
#endif
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    [Serializable]
    public class DataChecker
    {
        /*
         * targetRow는 데이터 베이스의 초기화값을 가르키고, 실제 값을 판단할 때는 오버라이드 된 값으로 판단해야함.
         */
        
        public CheckFactor[] factors;
        [InfoBox("바인딩이 없음. 모든 Factor에 대해 And 연산을 적용함.", InfoMessageType.Warning, VisibleIf = "@bindings != null && bindings.Count == 0")]
        [InfoBox("@expression")]
        [HorizontalGroup("Binding", Width = 550)]public List<Binding> bindings;

        public static List<CheckGroup> CreateCheckGroups(List<CheckFactor> factors, List<Binding> bindings)
        {
            for (int i = 0; i < factors.Count; i++)
            {
                factors[i].Index = i;
            }
            var groups = new List<CheckGroup>();
            var allCheckables = new List<ICheckable>();
            allCheckables.AddRange(factors);
            for (int i = 0; i < bindings.Count; i++)
            {
                bindings[i].Index = factors.Count + i;
                var group = new CheckGroup(bindings[i]);
                groups.Add(group);
                allCheckables.Add(group);
            }

            for (int i = 0; i < groups.Count; i++)
            {
                groups[i].SetCheckables(allCheckables);
            }

            return groups;
        }

        public List<CheckGroup> CreateCheckGroups()
        {
            return CreateCheckGroups(factors.ToList(), bindings);
        }

        public bool IsTrue()
        {
            if (bindings == null || bindings.Count == 0)
            {
                return factors.All(x => x.IsTrue());
            }

            var groups = CreateCheckGroups();

            return groups[groups.Count - 1].IsTrue();
        }

        public string ToExpression(bool useRichText = false)
        {
            if (bindings == null || bindings.Count == 0)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < factors.Length; i++)
                {
                    var factorText = factors[i].ToExpression(useRichText);
                    sb.Append(factorText);
                    if (i < factors.Length - 1) sb.Append($" {Condition.And.ToConditionString()} ");
                }

                return sb.ToString();
            }

            var groups = CreateCheckGroups();
            return groups[groups.Count - 1].ToExpression(useRichText);
        }

#if UNITY_EDITOR
        
        string expression;
        
        [VerticalGroup("Binding/Button"), Button("바인딩 윈도우 열기")]
        void OpenBindWindow()
        {
            var window = CheckableBindingWindow.Open();
            window.SetChecker(this);
        }  
        
        [VerticalGroup("Binding/Button"), Button( "결과 출력")]
        void Check()
        {
            var prefix = Application.isPlaying ? "런타임)  ".ToRichText(Color.green) : "에디터)  ".ToRichText(Color.yellow);
            var isTrueText = IsTrue() ? "True".ToRichText(Color.green) : "False".ToRichText(Color.red);
            expression = $"{prefix}{ToExpression(true)} => {isTrueText}";
        }
#endif
    }
}
