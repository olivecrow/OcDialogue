using System.Linq;
using System.Text;
using DebuggingEssentials;
using MyDB;
using OcDialogue;

namespace Samples.Debugging_Essential_Integration
{
    public static class InventoryControl
    {
        static Inventory inv => Inventory.PlayerInventory;
        [ConsoleCommand("","ItemDatabase 내의 해당 파라미터의 이름을 포함하는 아이템 목록을 출력함")]
        public static void PrintItems(string name)
        {
            var items = ItemDB.Instance.Items.Where(x => x.itemName.Contains(name));
            var sb = new StringBuilder();
            sb.Append($"다음 문자열을 포함하는 아이템 : {name}");
            foreach (var item in items)
            {
                sb.Append($"\n{item.itemName} | {item.type} | {item.SubTypeString}");
            }
            RuntimeConsole.Log(sb.ToString());
        }

        [ConsoleCommand("", "Inventory.PlayerInventory에 해당 아이템을 개수만큼 추가함")]
        public static void AddItem(string name, int count = 1)
        {
            if(!TryFindItem(name, out var item)) return;

            inv.AddItem(item, count);
        }

        [ConsoleCommand("", "아이템을 인벤토리에서 제거함. 버리지 못하는 아이템도 가능")]
        public static void RemoveItem(string name)
        {
            if(!TryFindItem(name, out var item)) return;
            inv.RemoveSingleItem(item);
        }

        static bool TryFindItem(string name, out ItemBase item)
        {
            item = null;
            if (inv == null)
            {
                RuntimeConsole.Log("플레이어 인벤토리가 없음");
                return false;
            }

            item = ItemDB.Instance.FindItem(name);
            if (item == null)
            {
                RuntimeConsole.Log($"해당 아이템이 데이터베이스 내에 존재하지 않음 | itemName : {name}");
                return false;
            }

            return true;
        }
    }
}