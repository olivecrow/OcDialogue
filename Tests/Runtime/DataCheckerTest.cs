using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace OcDialogue.Runtime.Test
{
    public class DataCheckerTest
    {
        List<DataRow> _instancedDataRow;
        List<Quest> _instancedQuest;
        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator GameProcessDataTest()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;

            var checker = CreateChecker(1);
            var row = CreateDataRow(DBType.GameProcess, DataRow.Type.Boolean);
            checker.factors[0].DataSelector.targetData = row;
            Assert.IsTrue(checker.IsTrue());

            Debug.Log($"ASDASD");
            
            foreach (var dataRow in _instancedDataRow)
            {
                Debug.Log($"Row : key : {dataRow.key} | db  :{dataRow.ownerDB}");
                switch (dataRow.ownerDB)
                {
                    case DBType.GameProcess:
                        GameProcessDatabase.Instance.DataRowContainer.DeleteRow(dataRow.key, DataStorageType.Single);
                        Debug.Log($"Delete Row : {dataRow.key}");
                        break;
                }
            }

            foreach (var quest in _instancedQuest)
            {
                QuestDatabase.Instance.DeleteQuest(quest.key, true);
            }
        }

        DataChecker CreateChecker(int factorCount)
        {
            var testChecker = new GameObject("TestChecker").AddComponent<DataCheckEventTrigger>();
            testChecker.checker = new DataChecker();
            var factors = new List<CheckFactor>();
            for (int i = 0; i < factorCount; i++)
            {
                var factor = new CheckFactor();
                factor.DataSelector = new DataSelector();
                factors.Add(factor);
            }

            testChecker.checker.factors = factors.ToArray();
            return testChecker.checker;
        }

        DataRow CreateDataRow(DBType dbType, DataRow.Type type)
        {
            if (_instancedDataRow == null) _instancedDataRow = new List<DataRow>();
            DataRow row = null;
            switch (dbType)
            {
                case DBType.GameProcess:
                {
                    row = GameProcessDatabase.Instance.DataRowContainer.AddData(DBType.GameProcess,
                        DataStorageType.Single);
                    row.key = "tmp row";
                    row.description = "this is instantiated by TestRunner";
                    row.type = type;
                    
                    break;
                }
                case DBType.Item:
                    return null;
                case DBType.Quest:
                {
                    var q = CreateQuest();
                    row = q.DataRowContainer.AddData(DBType.Quest, DataStorageType.Embeded);
                    row.type = type;
                    _instancedDataRow.Add(row);
                    break;
                }
                case DBType.NPC:
                    break;
                case DBType.Enemy:
                    break;
                default: return null;
            }
            
            row.key = "tmp row";
            row.description = "this is instantiated by TestRunner";

            return row;
        }

        Quest CreateQuest()
        {
            if (_instancedQuest == null) _instancedQuest = new List<Quest>();
            var q = QuestDatabase.Instance.AddQuest("tmp");
            q.key = "tmp quest";
            q.DataRowContainer = new DataRowContainer(q, null);
            _instancedQuest.Add(q);
            return q;
        }
    }
}
