using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OcDialogue.DB;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace OcDialogue.Tests.Editor
{
    public class DataBasicTest
    {

        [Test]
        public void ID_ValidationTest()
        {
            Debug.Log("[DataBasicTest] ID 유효성 검사 시작...");
            if(DBManager.Instance == null) return;

            var allData = new List<OcData>();
            allData.AddRange(DBManager.Instance.DBs);
            foreach (var db in DBManager.Instance.DBs)
            {
                allData.AddRange(db.AllData);
            }
            
            foreach (var data in allData)
            {
                if (data.id == 0)
                {
                    EditorUtility.SetDirty(data);
                    data.id = DBManager.GenerateID();
                    Debug.LogWarning($"{data.name} 의 id가 0이라서 새로 정해줌 | id = {data.id}");
                    continue;
                }

                if (allData.Any(x => x.id == data.id && x != data))
                {
                    EditorUtility.SetDirty(data);
                    var before = data.id;
                    data.id = DBManager.GenerateID();
                    Debug.LogWarning($"{data.name} 의 id와 중복된 id의 데이터가 있어서 새로 바꿔줌\n" +
                                     $"before : {before} | new : {data.id}");
                }
            }
            Debug.Log("[DataBasicTest] ID 유효성 검사 종료");
        }
    }
}