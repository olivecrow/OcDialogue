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
using OcDialogue.DB;
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

        /// <summary> 전달받은 디렉토리 내부에 folderName의 폴더가 없으면 생성하고, 해당 폴더의 패스를 반환함. </summary>
        public static string CreateFolderIfNull(string parentFolderPath, string folderName)
        {
            var folderPath = parentFolderPath + $"/{folderName}";
            if(!AssetDatabase.IsValidFolder(folderPath))
            {
                var folderGUID =AssetDatabase.CreateFolder(parentFolderPath, folderName);
                return AssetDatabase.GUIDToAssetPath(folderGUID);
            }

            return folderPath;
        }

        public static void FindMissingDataRows(AddressableData asset, DataRowContainerV2 container)
        {
            var missingList = new List<DataRowV2>();
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));
            foreach (var a in allAssets)
            {
                if (a is DataRowV2 dataRow && !container.DataRows.Contains(dataRow))
                {
                    missingList.Add(dataRow);
                }
            }

            if (missingList.Count == 0)
            {
                Printer.Print($"[{asset.Address}] 누락된 데이터를 찾을 수 없음.");
                return;
            }

            var assetNames = new StringBuilder();
            foreach (var data in missingList)
            {
                assetNames.Append($"{data.name}.asset\n");
            }

            if (!EditorUtility.DisplayDialog("누락된 데이터가 감지됨", 
                $"다음 데이터를 현재의 DataContainer에 추가하시겠습니까?\n{assetNames}",
                "추가", "취소")) return;
            
            container.DataRows.AddRange(missingList);
            foreach (var data in missingList)
            {
                data.SetParent(asset);  
            }
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}
