using System.Collections;
using NUnit.Framework;
using OcDialogue.DB;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace MyDB.Test.Editor
{
    public class NPCDBTest
    {
        NPCDB db => NPCDB.Instance;

        [Test]
        public void ParentTest()
        {
            foreach (var npc in db.NPCs)
            {
                if (npc.Parent == db) continue;
                Debug.LogWarning($"[{npc.Address}] 유효하지 않은 Parent를 수정함");
                npc.SetParent(db);
                EditorUtility.SetDirty(npc);

                DataRowContainerTest.ParentTest(npc);
            }
        }

        [Test]
        public void OverlapTest()
        {
            foreach (var npc in db.NPCs)
            {
                var count = db.NPCs.Count(x => x.Name == npc.Name);
                if (count > 1) Debug.LogError($"[{npc.Address}] 중복된 이름이 존재함");

                DataRowContainerTest.OverlapTest(npc);
            }
        }
    }
}