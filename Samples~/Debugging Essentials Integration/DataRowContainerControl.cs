using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebuggingEssentials;
using OcDialogue.DB;
using UnityEngine;

namespace Samples.Debugging_Essential_Integration
{
    public static class DataRowContainerControl
    {
        /// <summary>
        /// 모든 DataRow 목록을 반환함.
        /// </summary>
        /// <param name="container"></param>
        public static void PrintDataRows(this DataRowContainer container)
        {
            var sb = new StringBuilder();
            sb.Append($"모든 DataRow 목록");
            foreach (var dataRow in container.DataRows)
            {
                sb.Append($"\n{dataRow}");
            }
            RuntimeConsole.Log(sb.ToString());
            
        }
        /// <summary>
        /// 지정된 문자열을 포함하는 DataRow목록을 반환함.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="name"></param>
        public static void PrintDataRows(this DataRowContainer container, string name)
        {
            var data = container.DataRows.Where(x => x.Name.Contains(name));
            var sb = new StringBuilder();
            sb.Append($"다음 문자열을 포함하는 DataRow 목록 : {name}");
            foreach (var dataRow in data)
            {
                sb.Append($"\n{dataRow}");
            }
            RuntimeConsole.Log(sb.ToString());
        }
        
        public static void SetValue(this DataRowContainer container, string key, string value)
        {
            var data = container.Get(key);
            if (data == null)
            {
                RuntimeConsole.Log($"해당 키를 가진 DataRow를 찾을 수 없음 | key : {key}", LogType.Warning);
                return;
            }

            switch (data.Type)
            {
                case DataRowType.Bool:
                {
                    if (bool.TryParse(value, out var v)) data.SetValue(v);
                    else wrongTypeWarning();
                    break;
                }
                case DataRowType.Int:
                {
                    if (int.TryParse(value, out var v)) data.SetValue(v);
                    else wrongTypeWarning();
                    break;
                }
                case DataRowType.Float:
                {
                    if (float.TryParse(value, out var v)) data.SetValue(v);
                    else wrongTypeWarning();
                    break;
                }
                case DataRowType.String:
                {
                    data.SetValue(value);
                    break;
                }
            }
            
            void wrongTypeWarning() => RuntimeConsole.Log($"잘못된 데이터 타입 | Type : {data.Type} | Input : {value}", LogType.Warning);
        }
    }
}