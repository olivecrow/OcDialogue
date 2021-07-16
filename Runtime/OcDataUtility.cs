using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif
using Random = UnityEngine.Random;

namespace OcDialogue
{
    public static class OcDataUtility
    {
#if UNITY_EDITOR
        /// <summary> Selection을 재설정해서 현재 인스펙터를 다시 그림. </summary>
        public static void Repaint()
        {

            var currentSelected = Selection.activeObject;
            EditorApplication.delayCall += () => Selection.activeObject = currentSelected;
            Selection.activeObject = null;
        }

        public static string CalculateDataName(string defaultName, IEnumerable<string> existNames)
        {
            var sameNameCount = -1;
            if (existNames == null || !existNames.Any()) sameNameCount = 0;
            else
            {
                sameNameCount = existNames.Count(x => x.Contains(defaultName));
            }

            return $"{defaultName} {sameNameCount}";
        }

        /// <summary> ItemDatabase에서 사용할 아이템의 GUID를 중복되지 않게 계산해서 반환함. </summary>
        public static int CalcItemGUID()
        {
            int id;
            do
            {
                id = Random.Range(int.MinValue, int.MaxValue);
            } while (ItemDatabase.Instance.Items.Any(x => x.GUID == id));

            return id;
        }

        public static string ToConditionString(this DataChecker.Condition condition)
        {
            return condition switch
            {
                DataChecker.Condition.And => "&&",
                DataChecker.Condition.Or => "||",
                _ => "??"
            };
        }

        /// <summary> 비교 연산자를 문자열로 출력함. (==, !=, >= 등) </summary>
        public static string ToOperationString(this Operator op)
        {
            switch (op)
            {
                case Operator.Equal: return "==";
                case Operator.NotEqual: return "!=";
                case Operator.Greater: return ">";
                case Operator.GreaterEqual: return ">=";
                case Operator.Less: return "<";
                case Operator.LessEqual: return "<=";
            }

            return "?";
        }

        /// <summary> DataRow의 Type인 기본타입에서 비교용 enum인 CompareFactor에 대응돠는 값을 반환함. </summary>
        public static CompareFactor ToCompareFactor(this DataRow.Type type)
        {
            return type switch
            {
                DataRow.Type.Boolean => CompareFactor.Boolean,
                DataRow.Type.Int => CompareFactor.Int,
                DataRow.Type.Float => CompareFactor.Float,
                DataRow.Type.String => CompareFactor.String,
                _ => CompareFactor.Boolean
            };
        }

        /// <summary> DataChecker의 요소들에서 문자열로 된 조건 표현식을 출력함. </summary>
        public static string ToStringFromBindings(CheckFactor[] factors, DataChecker.Binding[] bindings)
        {
            if (bindings == null || bindings.Length == 0)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < factors.Length; i++)
                {
                    sb.Append(factors[i]);
                    if(i < factors.Length - 1) sb.Append($" {DataChecker.Condition.And.ToConditionString()} ");
                }

                return sb.ToString();
            }

            for (int i = 0; i < factors.Length; i++)
            {
                factors[i].Index = i;
            }
            var allCheckables = new List<ICheckable>();
            var allGroups = new List<DataChecker.CheckGroup>();
            allCheckables.AddRange(factors);
            for (int i = 0; i < bindings.Length; i++)
            {
                var group = new DataChecker.CheckGroup(bindings[i]);
                allCheckables.Add(group);
                allGroups.Add(group);
            }

            for (int i = 0; i < allGroups.Count; i++)
            {
                allGroups[i].SetCheckables(allCheckables);
            }
            return allGroups[allGroups.Count - 1].ToString();
        }
#endif
    }
}
