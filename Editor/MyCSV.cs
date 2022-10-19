using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace OcDialogue.Editor
{
    [Serializable]
    public class MyCSV
    {
        /// <summary>
        /// Combined header strings with comma(,)
        /// </summary>
        public string RawHeader;
        /// <summary>
        /// First line of the csv file.
        /// </summary>
        public string[] Header;
        
        /// <summary>
        /// Lines of csv file without split.
        /// </summary>
        public List<string> RawLines = new List<string>();
        
        /// <summary>
        /// Lines of csv with split.
        /// </summary>
        public List<string[]> Lines = new List<string[]>();
        
        /// <summary>
        /// Create CSV Instance For Write
        /// </summary>
        /// <param name="header"></param>
        public MyCSV(string[] header)
        {
            Header = header;
            RawHeader = FilterLine(header);
        }

        /// <summary>
        /// Create CSV Instance For Read. You can overwrite the opened file.
        /// </summary>
        /// <param name="filePath"></param>
        public MyCSV(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                try
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        RawLines.Add(line);

                        var fields = CSVParser.Split(line);

                        for (int i = 0; i < fields.Length; i++)
                        {
                            fields[i] = fields[i].TrimStart(' ', '"');
                            fields[i] = fields[i].TrimEnd('"');
                        }
                    
                        if(Header == null || Header.Length == 0)
                        {
                            Header = fields;
                            RawHeader = line;
                            continue;
                        }
                        Lines.Add(fields);
                    }
                }
                catch (Exception e)
                {
                    reader.Close();
                    throw;
                }
            }

            Debug.Log($"Read CSV | path : {filePath}\n" +
                      $"Header : {string.Join('|', Header)}\n" +
                      $"{string.Join('\n', Lines.Select(x => string.Join('|', x)))}");
        }

        public void AppendLine(params string[] line)
        {
            if(!IsValidLine(line)) return;
            
            Lines.Add(line);

            RawLines.Add(FilterLine(line));
        }

        /// <summary>
        /// if this csv has a line with same id, it overwrite the line.
        /// </summary>
        /// <param name="idIndex"> the id column index of line. it detects the same column value of it. </param>
        /// <param name="line">data to append.</param>
        public void OverwriteOrAppendLine(int idIndex, params string[] line)
        {
            if(!IsValidLine(line)) return;

            var id = line[idIndex];

            for (int i = 0; i < Lines.Count; i++)
            {
                if (Lines[i][idIndex] == id)
                {
                    // has same line;
                    Lines[i] = line;
                    RawLines[i] = FilterLine(line);
                    return;
                }
            }
            AppendLine(line);
        }

        /// <summary>
        /// Save text as file in given path.
        /// </summary>
        /// <param name="path">absolute path for saving file</param>
        public void SaveAsFile(string path)
        {
            using (var writer = new StreamWriter(path))
            {
                try
                {
                    writer.WriteLine(RawHeader);
                    for (int i = 0; i < Lines.Count; i++)
                    {
                        writer.WriteLine(RawLines[i]);
                    }
                }
                catch (Exception e)
                {
                    writer.Close();
                    throw;
                }
            }
        }

        bool IsValidLine(params string[] line)
        {
            if(line == null)
                throw new NullReferenceException("Cannot append null line. If you want to append empty line, please fill all data as empty strings");
            
            if (Header == null)
                throw new NullReferenceException("No Header Assigned. Please Assign Header.");

            if (Header.Length != line.Length)
                throw new IndexOutOfRangeException(
                    $"line.Length ({line.Length}) is different with Header.Length ({Header.Length}). " +
                    $"Please Match the length of your data with the length of Header.");

            return true;
        }
        
        static string FilterLine(string[] line)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                sb.Append(FilterString(line[i]));
                if (i < line.Length - 1) sb.Append(',');
            }

            return sb.ToString();
        }
        static string FilterString(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            text = text.Replace("\"", @"""");
            if (text.Contains(',') || text.Contains(';') || text.Contains('|') || text.Contains('\n')) text = $"\"{text}\"";

            return text;
        }
    }
}