using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OcUtility;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UIElements;
using OcDialogue.DB;
using Random = UnityEngine.Random;

namespace OcDialogue
{
    public static class OcDataUtility
    {
        /// <summary> ComparableData의 데이터 검사 메서드 </summary>
        public static bool IsTrue(this bool source, CheckFactor.Operator op, bool value)
        {
            return op switch
            {
                CheckFactor.Operator.Equal    => source == value,
                CheckFactor.Operator.NotEqual => source != value,
                _ => false
            };
        }
        /// <summary> ComparableData의 데이터 검사 메서드 </summary>
        public static bool IsTrue(this int source, CheckFactor.Operator op, int value)
        {
            return op switch
            {
                CheckFactor.Operator.Equal        => source == value,
                CheckFactor.Operator.NotEqual     => source != value,
                CheckFactor.Operator.Greater      => source >  value,
                CheckFactor.Operator.GreaterEqual => source >= value,
                CheckFactor.Operator.Less         => source <  value,
                CheckFactor.Operator.LessEqual    => source <= value,
                _ => false
            };
        }
        /// <summary> ComparableData의 데이터 검사 메서드 </summary>
        public static bool IsTrue(this float source, CheckFactor.Operator op, float value)
        {
            return op switch
            {
                CheckFactor.Operator.Equal        => Math.Abs(value - source) < 0.0001f,
                CheckFactor.Operator.NotEqual     => Math.Abs(value - source) > 0.0001f,
                CheckFactor.Operator.Greater      => source >  value,
                CheckFactor.Operator.GreaterEqual => source >= value,
                CheckFactor.Operator.Less         => source <  value,
                CheckFactor.Operator.LessEqual    => source <= value,
                _ => false
            };
        }
        /// <summary> ComparableData의 데이터 검사 메서드 </summary>
        public static bool IsTrue(this string source, CheckFactor.Operator op, string value)
        {
            return op switch
            {
                CheckFactor.Operator.Equal    => source == value,
                CheckFactor.Operator.NotEqual => source != value,
                _ => false
            };
        }

        /// <summary> DataRow의 Type인 기본타입에서 비교용 enum인 CompareFactor에 대응돠는 값을 반환함. </summary>
        // public static CompareFactor ToCompareFactor(this DataRow.Type type)
        // {
        //     return type switch
        //     {
        //         DataRow.Type.Boolean => CompareFactor.Boolean,
        //         DataRow.Type.Int => CompareFactor.Int,
        //         DataRow.Type.Float => CompareFactor.Float,
        //         DataRow.Type.String => CompareFactor.String,
        //         _ => CompareFactor.Boolean
        //     };
        // }

        public static int CalcSetterOperator(this int source, DataSetter.Operator op, int targetValue)
        {
            switch (op)
            {
                case DataSetter.Operator.Set:
                    return targetValue;
                case DataSetter.Operator.Add:
                    return source + targetValue;
                case DataSetter.Operator.Multiply:
                    return source * targetValue;
                case DataSetter.Operator.Divide:
                    if (targetValue != 0) return source / targetValue;
                    Printer.Print("[OcDataUtility] CalcSetterOperator) 0으로 나눌 수 없음. 0을 반환함", LogType.Error);
                    return 0;
            }
            return targetValue;
        }
        
        public static float CalcSetterOperator(this float source, DataSetter.Operator op, float targetValue)
        {
            switch (op)
            {
                case DataSetter.Operator.Set:
                    return targetValue;
                case DataSetter.Operator.Add:
                    return source + targetValue;
                case DataSetter.Operator.Multiply:
                    return source * targetValue;
                case DataSetter.Operator.Divide:
                    if (targetValue != 0) return source / targetValue;
                    Printer.Print("[OcDataUtility] CalcSetterOperator) 0으로 나눌 수 없음. 0을 반환함", LogType.Error);
                    return 0;
            }
            return targetValue;
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
        public static int CalcItemGUID(IEnumerable<int> existGUIDs)
        {
            int id;
            do
            {
                id = Random.Range(int.MinValue, int.MaxValue);
            } while (existGUIDs.Any(x => x == id));

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
        public static string ToOperationString(this CheckFactor.Operator op)
        {
            switch (op)
            {
                case CheckFactor.Operator.Equal: return "==";
                case CheckFactor.Operator.NotEqual: return "!=";
                case CheckFactor.Operator.Greater: return ">";
                case CheckFactor.Operator.GreaterEqual: return ">=";
                case CheckFactor.Operator.Less: return "<";
                case CheckFactor.Operator.LessEqual: return "<=";
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

        public static void FindMissingDataRows(OcData asset, DataRowContainer container)
        {
            var missingList = new List<DataRow>();
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));
            foreach (var a in allAssets)
            {
                if (a is DataRow dataRow && !container.DataRows.Contains(dataRow))
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
