using System.Linq;
using System.Text;
using DebuggingEssentials;
using OcDialogue.DB;

namespace Samples.Debugging_Essential_Integration
{
    public static class EnemyDBControl
    {
        
        [ConsoleCommand("", "지정된 이름을 포함하는 Enemy를 출력함")]
        public static void PrintEnemies(string name)
        {
            var enemies = EnemyDB.Instance.Enemies.Where(x => x.Name.Contains(name));
            var sb = new StringBuilder();
            sb.Append($"해당 이름을 포함하는 Enemy 목록 : {name}");
            foreach (var enemy in enemies)
            {
                sb.Append($"\n{enemy.Name} | {enemy.Category}");
            }
        }

        [ConsoleCommand("", "지정된 Enemy의 DataRow 목록을 출력함")]
        public static void PrintEnemyDataRow(string enemyName)
        {
            if(!TryFindEnemy(enemyName, out var enemy)) return;
            enemy.DataRowContainer.PrintDataRows();
        }
        
        [ConsoleCommand("", "지정된 Enemy의 isEncounter를 변경함")]
        public static void SetEnemyTotalKillCount(string enemyName, int killCount)
        {
            if(!TryFindEnemy(enemyName, out var enemy)) return;
            enemy.SetKillCount(killCount);
        }
        
        [ConsoleCommand("", "지정된 Enemy의 지정된 DataRow의 값을 설정함")]
        public static void SetEnemyDataRow(string enemyName, string dataRowName, string value)
        {
            if(!TryFindEnemy(enemyName, out var enemy)) return;
            enemy.DataRowContainer.SetValue(dataRowName, value);
        }

        static bool TryFindEnemy(string enemyName, out Enemy enemy)
        {
            enemy = EnemyDB.Instance.Enemies.Find(x => x.Name == enemyName);
            if (enemy == null)
            {
                RuntimeConsole.Log($"해당 이름의 Enemy를 찾을 수 없음 : {enemyName}");
                return false;
            }
            return true;
        }
    }
}