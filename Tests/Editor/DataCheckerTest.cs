using System.Collections;
using System.Linq;
using NUnit.Framework;
using OcDialogue;
using OcDialogue.DB;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;


public static class DataCheckerTest
{
    public static bool HasNull(DataChecker checker)
    {
        if (checker.factors.Any(x => x == null || x.Data == null)) return true;
        if (checker.factors.Length == 0) return true;
        return false;
    }

    public static bool HasUnusedCheckeables(DataChecker checker)
    {
        return checker.HasUnusedCheckables();
    }

    /// <summary>
    /// 죽은 데이터 (DB에서 사라진 데이터)는 컴파일이 이루어지면 없어지는 일시적인 데이터임. 만약 이게 검사되면 다시 컴파일을 해 볼 것.
    /// </summary>
    /// <param name="checker"></param>
    /// <returns></returns>
    public static bool HasDeadData(DataChecker checker)
    {
        return checker.factors.Any(HasDeadData);
    }

    public static bool HasDeadData(CheckFactor factor)
    {
        switch (factor.Data)
        {
            case DataRow row: 
                if (row.TotalAddress.Contains(GameProcessDB.Instance.Address))
                    return !GameProcessDB.Instance.DataRowContainer.DataRows.Contains(row);
                if (row.TotalAddress.Contains(QuestDB.Instance.Address))
                    return !QuestDB.Instance.Quests.Any(x => x.DataRowContainer.DataRows.Contains(row));
                if(row.TotalAddress.Contains(NPCDB.Instance.Address))
                    return !NPCDB.Instance.NPCs.Any(x => x.DataRowContainer.DataRows.Contains(row));
                if(row.TotalAddress.Contains(EnemyDB.Instance.Address))
                    return !EnemyDB.Instance.Enemies.Any(x => x.DataRowContainer.DataRows.Contains(row));
                return false;
            
            case ItemBase item:
                return !ItemDatabase.Instance.Items.Contains(item);
            
            case Quest quest:
                return !QuestDB.Instance.Quests.Contains(quest);
            
            case NPC npc:
                return !NPCDB.Instance.NPCs.Contains(npc);
            
            case Enemy enemy:
                return !EnemyDB.Instance.Enemies.Contains(enemy);
        }

        return false;
    }
}