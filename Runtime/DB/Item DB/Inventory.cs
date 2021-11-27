using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcUtility;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcDialogue
{
    public class Inventory
    {
        /// <summary> 런타임에서 사용되는 플레이어의 인벤토리. 시작 시 여기에 할당해줘야함.
        /// Application.quitting에서 자동으로 해제되기때문에 에디터에서 따로 해제할 필요는 없음.</summary>
        public static Inventory PlayerInventory
        {
            get => _playerInventory;
            set
            {
                var isNew = _playerInventory != value;
                _playerInventory = value;
                if(isNew) OnPlayerInventoryChanged?.Invoke(value);
                
#if UNITY_EDITOR
                Application.quitting += ReleaseEvent;
#endif
            }
        }
        static Inventory _playerInventory;
        public Inventory()
        {
            _items = new List<ItemBase>();
        }

        public Inventory(IEnumerable<ItemBase> items)
        {
            _items = new List<ItemBase>();
            foreach (var item in items)
            {
                _items.Add(item.GetCopy());
            }
        }

        public IEnumerable<ItemBase> Items => _items;
        List<ItemBase> _items;

        public event Action<ItemBase, int> OnItemAdded;
        public event Action<ItemBase, int> OnItemRemoved;
        public event Action OnInventoryChanged;
        public event Action<ItemBase> OnStackOverflow;
        public static event Action<Inventory> OnPlayerInventoryChanged;
        

        /// <summary> 아이템 추가. 내부적으로 카피를 생성하기 때문에 아무거나 집어넣으면 됨. 한 개라도 성공하면 true를 반환.</summary>
        public bool AddItem(ItemBase item, int count = 1)
        {
            if (count < 1)
            {
                Printer.Print($"[Inventory] 잘못된 개수가 입력됨 | count : {count}", LogType.Error);
                return false;
            }

            var addedCount = 0;
            if (item.isStackable)
            {
                var exist = _items.Find(x => x.GUID == item.GUID);
                if(exist == null)
                {
                    var copy = item.GetCopy();
                    copy.AddStack(count);
                    AddNewItem(copy);
                }
                else
                {
                    var existCount = Count(exist); 
                    if(existCount == exist.maxStackCount)
                    {
                        OnStackOverflow?.Invoke(exist);
                        return false;
                    }
                    
                    addedCount = existCount + count > exist.maxStackCount ? 
                        count - (exist.maxStackCount - existCount) : count;
                    exist.AddStack(count, () => OnStackOverflow?.Invoke(exist));
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    addedCount++;
                    AddNewItem(item.GetCopy());
                }
            }
            OnItemAdded?.Invoke(_items.Find(x => x.GUID == item.GUID), addedCount);
            OnInventoryChanged?.Invoke();
            Printer.Print($"[Inventory] 아이템 추가됨. item : {item.itemName} | count : {count}");
            return true;
        }

        void AddNewItem(ItemBase item)
        {
            _items.Add(item);
        }

        /// <summary> 인벤토리의 아이템 제거. 원본이든 카피든 상관 없이 동일한 GUID를 가지는 아이템을 개수만큼 제거함. 파라미터 아이템의 CurrentStack은 고려하지 않음. </summary>
        public void RemoveItem(ItemBase item, int count = 1)
        {
            if (count <= 0)
            {
                Printer.Print($"[Inventory] 잘못된 아이템 개수가 입력됨. item : {item.itemName} | count : {count}", LogType.Error);
                return;
            }
            var exist = _items.Find(x => x.GUID == item.GUID);
            if(exist == null)
            {
                Printer.Print($"[Inventory] 존재하지 않는 아이템을 인벤토리에서 제거하려 함. item : {item.itemName} | count : {count}", LogType.Error);
                return;
            }
            if (item.isStackable)
            {
                exist.RemoveStack(count, () => RemoveSingleItem(exist));
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    RemoveSingleItem(exist);
                    exist = _items.Find(x => x.GUID == item.GUID);
                    if(exist == null) break;
                }
            }
            OnInventoryChanged?.Invoke();
            OnItemRemoved?.Invoke(exist, count);
            Printer.Print($"[Inventory] 아이템 제거됨. item : {item.itemName} | count : {count}");
        }

        /// <summary> 현재 인벤토리에 존재하는 아이템의 개수를 반환함. isStackable인 경우, StackAmount를 반환하고, 아닌 경우에 총 개수를 반환함. </summary>
        public int Count(ItemBase item)
        {
            var exist = _items.Find(x => x.GUID == item.GUID);
            if (exist == null) return 0;

            return item.isStackable ? exist.CurrentStack : _items.Count(x => x.GUID == item.GUID);
        }

        public ItemBase Find(string name)
        {
            return _items.Find(x => x.itemName == name);
        }

        public ItemBase Find(int guid)
        {
            return _items.Find(x => x.GUID == guid);
        }
        public T Find<T>(string name) where T : ItemBase
        {
            return _items.Find(x => x.itemName == name) as T;
        }
        public T Find<T>(int guid) where T : ItemBase
        {
            return _items.Find(x => x.GUID == guid) as T;
        }

        /// <summary> 아이템을 개수에 상관 없이 삭제함. </summary>
        public void RemoveSingleItem(ItemBase item)
        {
            _items.Remove(item);
        }

        public List<ItemSaveData> GetSaveData()
        {
            var list = new List<ItemSaveData>();
            foreach (var itemBase in _items)
            {
                list.Add(new ItemSaveData(itemBase));
            }

            return list;
        }
        
        public void Overwrite(IEnumerable<ItemSaveData> data)
        {
            _items = new List<ItemBase>();
            foreach (var itemSaveData in data)
            {
                var copy = ItemDatabase.Instance.FindItem(itemSaveData.GUID).GetCopy();
                if (copy == null)
                {
                    Debug.LogError($"아이템 데이터베이스에서 해당 아이템을 찾을 수 없음 | {itemSaveData.itemName}");
                }
                copy.AddStack(itemSaveData.count);
                if (copy is IEquipment equipment)
                {
                    equipment.CurrentUpgrade = itemSaveData.upgrade;
                    equipment.CurrentDurability = itemSaveData.durability;
                    equipment.IsEquipped = itemSaveData.isEquipped;
                }
                AddNewItem(copy);
            }
        }

#if DEBUG
        public static void AddItemToPlayerInventory(string itemName, int count)
        {
            if (_playerInventory == null)
            {
                Debug.LogWarning("플레이어 인벤토리가 없음");
                return;
            }
            var item = ItemDatabase.Instance.Items.Find(x => x.itemName == itemName);
            if(item == null) return;
            _playerInventory.AddItem(item, count);
        }

        public static void AddItemToPlayerInventory(int guid, int count)
        {
            if (_playerInventory == null)
            {
                Debug.LogWarning("플레이어 인벤토리가 없음");
                return;
            }

            var item = ItemDatabase.Instance.FindItem(guid);
            if(item == null) return;
            _playerInventory.AddItem(item, count);
        }
#endif

#if UNITY_EDITOR
        static void ReleaseEvent()
        {
            _playerInventory = null;
            OnPlayerInventoryChanged = null;
            Application.quitting -= ReleaseEvent;
        }
#endif
    }
}
