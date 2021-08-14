using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OcUtility;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
#endif
using Random = UnityEngine.Random;

namespace OcDialogue
{
    public static class OcDataUtility
    {
        /// <summary> ComparableData의 데이터 검사 메서드 </summary>
        public static bool IsTrue(this bool source, Operator op, bool value)
        {
            return op switch
            {
                Operator.Equal    => source == value,
                Operator.NotEqual => source != value,
                _ => false
            };
        }
        /// <summary> ComparableData의 데이터 검사 메서드 </summary>
        public static bool IsTrue(this int source, Operator op, int value)
        {
            return op switch
            {
                Operator.Equal        => source == value,
                Operator.NotEqual     => source != value,
                Operator.Greater      => source >  value,
                Operator.GreaterEqual => source >= value,
                Operator.Less         => source <  value,
                Operator.LessEqual    => source <= value,
                _ => false
            };
        }
        /// <summary> ComparableData의 데이터 검사 메서드 </summary>
        public static bool IsTrue(this float source, Operator op, float value)
        {
            return op switch
            {
                Operator.Equal        => Math.Abs(value - source) < 0.0001f,
                Operator.NotEqual     => Math.Abs(value - source) > 0.0001f,
                Operator.Greater      => source >  value,
                Operator.GreaterEqual => source >= value,
                Operator.Less         => source <  value,
                Operator.LessEqual    => source <= value,
                _ => false
            };
        }
        /// <summary> ComparableData의 데이터 검사 메서드 </summary>
        public static bool IsTrue(this string source, Operator op, string value)
        {
            return op switch
            {
                Operator.Equal    => source == value,
                Operator.NotEqual => source != value,
                _ => false
            };
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

        public static string ToConditionString(this Condition condition)
        {
            return condition switch
            {
                Condition.And => "&&",
                Condition.Or => "||",
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

        public static void SetVisible(this VisualElement source, bool isVisible)
        {
            source.style.display = new StyleEnum<DisplayStyle>(isVisible ? DisplayStyle.Flex : DisplayStyle.None);
        }
#endif
    }
}
