using System.Collections;
using NUnit.Framework;
using OcDialogue.DB;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace MyDB.Test.Editor
{
    public class EnemyDBTest
    {
        EnemyDB db => EnemyDB.Instance;

        [Test]
        public void ParentTest()
        {
            foreach (var enemy in db.Enemies)
            {
                if (enemy.Parent == db) continue;
                Debug.LogWarning($"[{enemy.Address}] 유효하지 않은 Parent를 수정함");
                enemy.SetParent(db);
                EditorUtility.SetDirty(enemy);

                // DataRowContainerTest.ParentTest(enemy);
            }
        }

        [Test]
        public void OverlapTest()
        {
            foreach (var enemy in db.Enemies)
            {
                var count = db.Enemies.Count(x => x.Name == enemy.Name);
                if (count > 1) Debug.LogError($"[{enemy.Address}] 중복된 이름이 존재함");

                // DataRowContainerTest.OverlapTest(enemy);
            }
        }

        [Test]
        public void StatTest()
        {
            foreach (var enemy in db.Enemies)
            {
                if (enemy.HP <= 0) Debug.LogWarning($"[{enemy.Address}] Hp가 0 이하임");
                if (enemy.Balance <= 0) Debug.LogWarning($"[{enemy.Address}] Balance가 0 이하임");
                if (enemy.Weight <= 0) Debug.LogWarning($"[{enemy.Address}] Weight가 0 이하임");
                if (enemy.DamagerInfo.Count > 0)
                {
                    if (enemy.DamagerInfo.Any(x => x.stat.PhysicalSum + x.stat.ElementalSum <= 0))
                        Debug.LogWarning($"[{enemy.Address}]공격이 0 이하임");
                    if (enemy.DamagerInfo.Any(x => x.weight <= 0))
                        Debug.LogWarning($"[{enemy.Address}]공격의 무게가 0 이하임");
                }

                if (string.IsNullOrWhiteSpace(enemy.Category)) Debug.LogWarning($"[{enemy.Address}] 카테고리가 비어있음");
            }
        }
    }
}