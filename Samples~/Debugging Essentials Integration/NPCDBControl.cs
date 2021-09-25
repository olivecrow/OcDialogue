using System.Linq;
using System.Text;
using DebuggingEssentials;
using OcDialogue;
using OcDialogue.DB;

namespace Samples.Debugging_Essential_Integration
{
    public static class NPCDBControl
    {
        [ConsoleCommand("", "지정된 이름을 포함하는 NPC를 출력함")]
        public static void PrintNPCs(string name)
        {
            var NPCs = NPCDB.Instance.NPCs.Where(x => x.Name.Contains(name));
            var sb = new StringBuilder();
            sb.Append($"해당 이름을 포함하는 NPC 목록 : {name}");
            foreach (var npc in NPCs)
            {
                sb.Append($"\n{npc.Name} | {npc.Category}");
            }
        }

        [ConsoleCommand("", "지정된 NPC의 DataRow 목록을 출력함")]
        public static void PrintNPCDataRow(string npcName)
        {
            if(!TryFindNPC(npcName, out var npc)) return;
            npc.DataRowContainer.PrintDataRows();
        }
        
        [ConsoleCommand("", "지정된 NPC의 isEncounter를 변경함")]
        public static void SetNPCEncounter(string npcName, bool isEncounter)
        {
            if(!TryFindNPC(npcName, out var npc)) return;
            npc.SetEncountered(isEncounter);
        }
        
        [ConsoleCommand("", "지정된 Enemy의 지정된 DataRow의 값을 설정함")]
        public static void SetNPCDataRow(string npcName, string dataRowName, string value)
        {
            if(!TryFindNPC(npcName, out var npc)) return;
            npc.DataRowContainer.SetValue(dataRowName, value);
        }

        static bool TryFindNPC(string npcName, out NPC npc)
        {
            npc = NPCDB.Instance.NPCs.Find(x => x.Name == npcName);
            if (npc == null)
            {
                RuntimeConsole.Log($"해당 이름의 NPC를 찾을 수 없음 : {npcName}");
                return false;
            }
            return true;
        }
    }
}