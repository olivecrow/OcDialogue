using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcDialogue
{
    public class OcDataUtility
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
#endif
    }
}
