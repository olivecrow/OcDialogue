using System.Linq;
using NUnit.Framework;
using OcDialogue.DB;
using UnityEditor;
using UnityEngine;

namespace MyDB.Test.Editor
{
    public class QuestDBTest
    {
        QuestDB db => QuestDB.Instance;

        [Test]
        public void ParentTest()
        {
            foreach (var quest in db.Quests)
            {
                if (quest.Parent == db) continue;
                Debug.LogWarning($"[{quest.Address}] 유효하지 않은 Parent를 수정함");
                quest.SetParent(db);
                EditorUtility.SetDirty(quest);

                DataRowContainerTest.ParentTest(quest);
            }
        }

        [Test]
        public void OverlapTest()
        {
            foreach (var quest in db.Quests)
            {
                var count = db.Quests.Count(x => x.Name == quest.Name);
                if (count > 1) Debug.LogError($"[{quest.Address}] 중복된 이름이 존재함");

                DataRowContainerTest.OverlapTest(quest);
            }
        }

        [Test]
        public void ClearCheckerTest()
        {
            foreach (var quest in db.Quests)
            {
                if (quest.Checker.factors.Length == 0)
                    Debug.LogError($"[{quest.Address}] 퀘스트 클리어 조건이 비어있음");

                if (DataCheckerTest.HasNull(quest.Checker))
                    Debug.LogError($"[{quest.Address}] 퀘스트 클리어 조건 중, 빈 요소가 존재함");

                if (DataCheckerTest.HasUnusedCheckeables(quest.Checker))
                    Debug.LogError($"[{quest.Address}] 퀘스트 클리어 조건 중, 포함되지 않은 조건이 있음");
            }
        }
    }
}