using System.Linq;
using System.Text;
using DebuggingEssentials;
using OcDialogue;
using OcDialogue.DB;

namespace Samples.Debugging_Essential_Integration
{
    public static class QuestDBControl
    {
        [ConsoleCommand("", "지정된 이름을 포함하는 퀘스트를 출력함")]
        public static void PrintQuests(string name)
        {
            var quests = QuestDB.Instance.Quests.Where(x => x.Name.Contains(name));
            var sb = new StringBuilder();
            sb.Append($"해당 이름을 포함하는 퀘스트 목록 : {name}");
            foreach (var quest in quests)
            {
                sb.Append($"\n{quest.Name} | {quest.Runtime.QuestState}");
            }
        }

        [ConsoleCommand("", "지정된 퀘스트의 DataRow 목록을 출력함")]
        public static void PrintQuestDataRow(string questName)
        {
            if(!TryFindQuest(questName, out var quest)) return;
            quest.DataRowContainer.PrintDataRows();
        }
        
        [ConsoleCommand("", "지정된 퀘스트의 QuestState를 변경함")]
        public static void SetQuestState(string questName, QuestState state)
        {
            if(!TryFindQuest(questName, out var quest)) return;
            quest.SetState(state);
        }
        
        [ConsoleCommand("", "지정된 퀘스트의 지정된 DataRow의 값을 설정함")]
        public static void SetQuestDataRow(string questName, string dataRowName, string value)
        {
            if(!TryFindQuest(questName, out var quest)) return;
            quest.DataRowContainer.SetValue(dataRowName, value);
        }

        static bool TryFindQuest(string questName, out Quest quest)
        {
            quest = QuestDB.Instance.Quests.Find(x => x.Name == questName);
            if (quest == null)
            {
                RuntimeConsole.Log($"해당 이름의 퀘스트를 찾을 수 없음 : {questName}");
                return false;
            }
            return true;
        }
    }
}