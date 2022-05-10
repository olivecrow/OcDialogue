using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class CSVWriter
    {
        public string[] Keys;
        public List<string[]> Data;
        public CSVWriter(params string[] keys)
        {
            Keys = keys;
            Data = new List<string[]>();
        }

        public void Read(string text)
        {
            var rows = text.Split('\r');
            
            if(string.IsNullOrWhiteSpace(rows[^1])) Array.Resize(ref rows, rows.Length - 1);

            var isKeyQueried = false;
            foreach (var row in rows)
            {
                var data = row.Split(',');
                if (!isKeyQueried)
                {
                    Keys = data;
                    isKeyQueried = true;
                    continue;
                }

                var trimmedData = new string[data.Length];
                for (int i = 0; i < data.Length; i++)
                {
                    var s = data[i];
                    if (s.StartsWith('"') && s.EndsWith('"'))
                        s = s.Substring(1, s.Length - 2);
                    
                    trimmedData[i] = s;
                }
                Data.Add(trimmedData);
            }
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

        public void Overwrite(int index, params string[] data)
        {
            if (data.Length != Keys.Length)
            {
                Debug.LogError($"[CSVWriter] 키의 개수와 데이터의 개수가 일치하지 않음");
                return;
            }
            if (Data.Count - 1 < index)
            {
                Debug.LogError($"[CSVWriter] 데이터의 크기가 인덱스보다 작음 | data.Length : {Data.Count} | index : {index}");
                return;
            }

            Data[index] = data;
        }

        /// <summary>
        /// idKey와 같은 id의 행이 존재한다면 덮어씌우고, 없다면 새로 추가함.
        /// </summary>
        /// <param name="idKey">Keys 중에서 id로써 사용할 Key의 이름</param>
        /// <param name="ignoreColumns">data 중에서 덮어쓰지 않을 행의 이름. 주로 설명이나 비고 등에 사용. 이미 존재하는 데이터가 없으면 그냥 작성된다.</param>
        /// <param name="data">덮어 쓸 데이터</param>
        public void AppendWithIgnoreColumn(string idKey, IEnumerable<string> ignoreColumns, params string[] data)
        {
            if (data.Length != Keys.Length)
            {
                Debug.LogError($"[CSVWriter] 키의 개수와 데이터의 개수가 일치하지 않음");
                return;
            }
            if (!Keys.Contains(idKey))
            {
                Debug.LogError($"[CSVWriter] 해당 id를 가진 열이 없음 | key : {idKey}");
                return;
            }

            var indexOfIDColumn = Keys.IndexOf(idKey);
            var isDataExist = Data.Any(x => x[indexOfIDColumn] == data[indexOfIDColumn]);

            // 무시될 행은 새로운 값을 덮어쓰지 않고 기존의 값을 그대로 씀.
            if (isDataExist)
            {
                var existDataIndex = Data.IndexOf(x => x[indexOfIDColumn] == data[indexOfIDColumn]);
                
                if(ignoreColumns != null)
                {
                    var ignoreColumnIndices = ignoreColumns.Select(x => Keys.IndexOf(x));
                    for (int i = 0; i < data.Length; i++)
                    {
                        if (!ignoreColumnIndices.Contains(i)) continue;
                        data[i] = Data[existDataIndex][i];
                    }
                }
                Data[existDataIndex] = data;
            }
            else
            {
                Add(data);
            }
        }
        /// <summary>
        /// idKey와 같은 id의 행이 존재한다면 덮어씌우고, 없다면 새로 추가함.
        /// </summary>
        /// <param name="idKey">Keys 중에서 id로써 사용할 Key의 이름</param>
        /// <param name="modifyColumn">data 중에서 덮어쓰려는 행의 이름. 예를 들어, 데이터의 이름과 설명 외에는 변경하지 않으려눈 경우, 이름과 설명의 키값을 넣으면 된다.</param>
        /// <param name="data">덮어 쓸 데이터</param>
        public void AppendColumn(string idKey, IEnumerable<string> modifyColumn, params string[] data)
        {
            if (data.Length != Keys.Length)
            {
                Debug.LogError($"[CSVWriter] 키의 개수와 데이터의 개수가 일치하지 않음");
                return;
            }
            if (!Keys.Contains(idKey))
            {
                Debug.LogError($"[CSVWriter] 해당 id를 가진 열이 없음 | key : {idKey}");
                return;
            }

            if (modifyColumn == null || !modifyColumn.Any())
            {
                Debug.LogError($"[CSVWriter] 변경에 포함될 키가 없음");
                return;
            }

            var indexOfIDColumn = Keys.IndexOf(idKey);
            var isDataExist = Data.Any(x => x[indexOfIDColumn] == data[indexOfIDColumn]);

            // 무시될 행은 새로운 값을 덮어쓰지 않고 기존의 값을 그대로 씀.
            if (isDataExist)
            {
                var existDataIndex = Data.IndexOf(x => x[indexOfIDColumn] == data[indexOfIDColumn]);
                
                var modifyTargetColumnIndices = modifyColumn.Select(x => Keys.IndexOf(x));
                for (int i = 0; i < data.Length; i++)
                {
                    if (!modifyTargetColumnIndices.Contains(i)) continue;
                    Data[existDataIndex][i] = data[i];
                }
            }
            else
            {
                Add(data);
            }
        }

        string DataToText()
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
                    sb.Append($"{Data[i][j]}");
                    if(j < Keys.Length - 1) sb.Append(',');
                    else sb.Append('\r');
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 입력된 키와 데이터를 바탕으로 CSV 파일을 생성함.
        /// </summary>
        /// <param name="folderPath">파일이 저장될 위치. 프로젝트 폴더의 경로와 관계 없음.</param>
        /// <param name="fileName">확장자를 제외한 파일 이름.</param>
        public void Save(string folderPath, string fileName)
        {
            var text = DataToText();

            var path = Path.Combine(folderPath, $"{fileName}.csv");
            var writer = File.CreateText(path);
            writer.WriteLine(text);
            writer.Close();
            if (folderPath.Contains("Assets/"))
            {
                var relative = AbsoluteToRelativePath(path);
                AssetDatabase.ImportAsset(relative);
            }
            Debug.Log($"CSV Export 완료 | Path : {folderPath} | FileName : {fileName} | 항목 개수 : {Data.Count}");
        }

        /// <param name="path">확장자를 포함한 전체 패스</param>
        public void Save(string path)
        {
            var text = DataToText();

            var writer = File.CreateText(path);
            writer.WriteLine(text);
            writer.Close();
            if (path.Contains(Application.dataPath))
            {
                var relative = AbsoluteToRelativePath(path);
                AssetDatabase.ImportAsset(relative);
            }
            Debug.Log($"CSV Export 완료 | Path : {path} | 항목 개수 : {Data.Count}");
        }
     
        public static string AbsoluteToRelativePath(string absolute)
        {
            if (absolute.Contains(Application.dataPath))
                return "Assets" + absolute.Substring(Application.dataPath.Length);
            
            // Debug.LogError("path is not belonged to project directory");
            return absolute;
        }
    }
}
