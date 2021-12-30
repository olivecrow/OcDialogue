using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OcDialogue.DB;
using OcUtility;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.DB
{
    [Serializable]
    public class DataChecker
    {
        [TableList]public CheckFactor[] factors;
        public List<Binding> bindings;

        public bool IsTrue()
        {
            if (factors == null || factors.Length == 0) return true;
            if (bindings == null || bindings.Count == 0)
            {
                return factors.All(x => x.IsTrue());
            }

            var groups = CreateCheckGroups();

            return groups[groups.Count - 1].IsTrue();
        }

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
        
#if UNITY_EDITOR
        [InfoBox("@e_bindingErrMsg", InfoMessageType.Error, nameof(HasUnusedCheckables))]
        [HorizontalGroup("binding")] [ShowInInspector] [HideLabel] [TextArea(1,3)][ReadOnly]
        public string e_bindingExpression;
        string e_bindingErrMsg;
        [VerticalGroup("binding/btn")][Button("바인딩 윈도우")]
        void OpenBindingWindow()
        {
            var window = CheckableBindingWindow.Open();
            window.SetChecker(this);
        }

        [VerticalGroup("binding/btn")]
        [Button("결과 출력")]
        void PrintResult()
        {
            var result = IsTrue() ? "True".ToRichText(Color.green) : "False".ToRichText(Color.red);
            var prefix = Application.isPlaying
                ? "(Runtime)".ToRichText(Color.cyan) 
                : "(Editor)".ToRichText(Color.yellow);
            Printer.Print($"[DataChecker] {prefix} {ToExpression().ToRichText(Color.cyan)} => {result}");
        }

        public bool HasUnusedCheckables()
        {
            if (bindings == null || bindings.Count == 0) return false;
            var maxIndex = factors.Length + bindings.Count - 1;
            var indices = new List<int>();
            for (int i = 0; i < maxIndex; i++)
            {
                indices.Add(i);
            }

            foreach (var binding in bindings)
            {
                foreach (var checkable in binding.checkables)
                {
                    if (indices.Contains(checkable)) indices.Remove(checkable);
                }
            }

            if (indices.Count > 0)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < indices.Count; i++)
                {
                    sb.Append(indices[i]);
                    if(i < indices.Count - 1)sb.Append(", ");
                }

                e_bindingErrMsg = $"바인딩에 포함되지 않은 요소가 있음 | index : {sb}";
                return true;
            }

            return false;
        }

        public void UpdateExpression()
        {
            e_bindingExpression = ToExpression();
        }

        public string ToExpression()
        {
            if (bindings == null || bindings.Count == 0)
            {
                return "바인딩 없음. 모든 Factor에 And 연산을 적용함.";
            }

            var groups = CreateCheckGroups();
            return groups[groups.Count - 1].ToExpression();
        }

        public bool IsWarningOn()
        {
            if (factors == null) return false;
            return factors.Any(x => x.TargetData == null) || !string.IsNullOrWhiteSpace(e_bindingErrMsg);
        }
#endif
    }
}
