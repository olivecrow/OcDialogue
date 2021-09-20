using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class CSVWriter
    {
        public readonly string[] Keys;
        public List<string[]> Data;
        public CSVWriter(params string[] keys)
        {
            this.Keys = keys;
            Data = new List<string[]>();
        }

        public void Add(params string[] data)
        {
            if (data.Length != Keys.Length)
            {
                Debug.LogError($"[CSVWriter] 키의 개수와 데이터의 개수가 일치하지 않음");
                return;
            }
            Data.Add(data);
        }

        /// <summary>
        /// 입력된 키와 데이터를 바탕으로 CSV 파일을 생성함.
        /// </summary>
        /// <param name="folderPath">파일이 저장될 위치. 프로젝트 폴더의 경로와 관계 없음.</param>
        /// <param name="fileName">확장자를 제외한 파일 이름.</param>
        public void Save(string folderPath, string fileName)
        {
            var sb = new StringBuilder();

            // key
            for (int i = 0; i < Keys.Length; i++)
            {
                sb.Append(Keys[i]);
                if (i < Keys.Length - 1) sb.Append(',');
                else sb.Append('\r');
            }
            
            // data
            for (int i = 0; i < Data.Count; i++)
            {
                for (int j = 0; j < Data[i].Length; j++)
                {
                    sb.Append($"\"{Data[i][j]}\"");
                    if(j < Keys.Length - 1) sb.Append(',');
                    else sb.Append('\r');
                }
            }

            var writer = File.CreateText(Path.Combine(folderPath, $"{fileName}.csv"));
            writer.WriteLine(sb);
            writer.Close();
            Debug.Log($"CSV Export 완료 | Path : {folderPath} | FileName : {fileName} | 항목 개수 : {Data.Count}");
        }
        
    }
}
