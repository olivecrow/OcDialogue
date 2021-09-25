using System;
using System.Linq;
using System.Text;
using DebuggingEssentials;
using OcDialogue.DB;
using UnityEngine;

namespace Samples.Debugging_Essential_Integration
{
    public static class GameProcessDBControl
    {
        [ConsoleCommand("", "파라미터의 문자열을 포함하는 모든 DataRow를 반환함")]
        public static void PrintDataRows(string containName)
        {
            GameProcessDB.Instance.DataRowContainer.PrintDataRows(containName);
        }
        [ConsoleCommand("", "지정된 Key의 DataRow 값을 변경함")]
        public static void Set(string key, string value)
        {
            GameProcessDB.Instance.DataRowContainer.SetValue(key, value);
        }
    }
}