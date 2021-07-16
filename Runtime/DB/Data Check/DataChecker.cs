using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        [InfoBox("바인딩이 없음. 모든 Factor에 대해 And 연산을 적용함.", VisibleIf = "@bindings.Count == 0")]
        public CheckFactor[] factors;
        public List<Binding> bindings;
        public enum Condition
        {
            And,
            Or
        }

        public bool IsTrue()
        {
            if (bindings == null || bindings.Count == 0)
            {
                return factors.All(x => x.IsTrue());
            }

            for (int i = 0; i < factors.Length; i++)
            {
                factors[i].Index = i;
            }
            var groups = new List<CheckGroup>();
            var allCheckables = new List<ICheckable>();
            allCheckables.AddRange(factors);
            for (int i = 0; i < bindings.Count; i++)
            {
                bindings[i].Index = factors.Length + i;
                var group = new CheckGroup(bindings[i]);
                groups.Add(group);
                allCheckables.Add(group);
            }

            for (int i = 0; i < groups.Count; i++)
            {
                groups[i].SetCheckables(allCheckables);
            }

            return groups[groups.Count - 1].IsTrue();
        }

#if UNITY_EDITOR

        [HorizontalGroup("Expression"), HideLabel, ReadOnly][Multiline(2)]
        public string expression;
        [HorizontalGroup("Expression"), Button("결과 출력")]
        void Check()
        {
            if (Application.isPlaying)
            {
                // TODO : 런타임에 DB Manager에서 GameProcessDataUser 등 DataUser를 캐싱해서 거기서 참조를 얻고 값을 출력할 것.
            }
            else
            {
                expression = $"{OcDataUtility.ToStringFromBindings(factors, bindings.ToArray())}" +
                             $"=> {IsTrue()}";
            }
        }
        
        [Button("바인딩 윈도우 열기")]
        void OpenBindWindow()
        {
            var window = CheckableBindingWindow.Open();
            window.SetChecker(this);
        }  
#endif

        [Serializable]
        public class Binding
        {
            public int Index { get; set; }
            public List<int> checkables;
            public Condition condition;
        }

        public class CheckGroup : ICheckable
        {
            public int Index { get;}
            public Binding binding;
            public List<ICheckable> Checkables;
            public Condition condition;

            public CheckGroup(Binding binding)
            {
                this.binding = binding;
                Index = binding.Index;
                Checkables = new List<ICheckable>();
                condition = binding.condition;
            }

            public void SetCheckables(List<ICheckable> allCheckables)
            {
                for (int i = 0; i < allCheckables.Count; i++)
                {
                    if(binding.checkables.Contains(allCheckables[i].Index)) Checkables.Add(allCheckables[i]);
                }
            }
            public bool IsTrue()
            {
                if (condition == Condition.And)
                {
                    return Checkables.All(x => x.IsTrue());
                }
                else
                {
                    return Checkables.Any(x => x.IsTrue());
                }
            }
#if UNITY_EDITOR
            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append("( ");

                for (int i = 0; i < Checkables.Count; i++)
                {
                    sb.Append(Checkables[i]);

                    if (i != Checkables.Count - 1) sb.Append($" {condition.ToConditionString()} ");
                }

                sb.Append(" )");
                return sb.ToString();
            }
#endif
        }

        
    }
}
