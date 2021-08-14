using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OcUtility;
using UnityEngine;

namespace OcDialogue
{
    public class CheckGroup : ICheckable
    {
        public int Index { get; }
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
                if (binding.checkables.Contains(allCheckables[i].Index)) Checkables.Add(allCheckables[i]);
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
        public string ToExpression(bool useRichText = false)
        {
            var sb = new StringBuilder();
            sb.Append("( ");

            for (int i = 0; i < Checkables.Count; i++)
            {
                var checkableText = Checkables[i].ToExpression(useRichText);
                if (useRichText) checkableText = checkableText.ToRichText(ColorExtension.Random(Checkables[i].GetHashCode(), 0.5f));
                sb.Append(checkableText);

                if (i != Checkables.Count - 1) sb.Append($" {condition.ToConditionString()} ");
            }

            sb.Append(" )");
            return sb.ToString();
        }
#endif
    }
}