using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcDialogue;
using OcUtility;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MyDB
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
                if(_playerInventory == null) return;
#if UNITY_EDITOR
                _playerInventory.OnInventoryChanged += (item, type) => Printer.Print($"[Inventory] OnInventoryChanged ({item.itemName}, {type})");
                Application.quitting += ReleaseEvent;
#endif
            }
        }
        static Inventory _playerInventory;
        public static List<Inventory> CreatedInventories;
        public Inventory()
        {
            if (CreatedInventories == null) CreatedInventories = new List<Inventory>();
            CreatedInventories.Add(this);
            name = "New Inventory";
            _items = new List<ItemBase>();
        }

        public Inventory(string name)
        {
            if (CreatedInventories == null) CreatedInventories = new List<Inventory>();
            CreatedInventories.Add(this);
            this.name = name;
            _items = new List<ItemBase>();
        }
        public Inventory(IEnumerable<ItemBase> items)
        {
            if (CreatedInventories == null) CreatedInventories = new List<Inventory>();
            CreatedInventories.Add(this);
            name = "New Inventory";
            _items = new List<ItemBase>();
            foreach (var item in items)
            {
                _items.Add(item.GetCopy());
            }
        }
        public Inventory(string name, IEnumerable<ItemBase> items)
        {
            if (CreatedInventories == null) CreatedInventories = new List<Inventory>();
            CreatedInventories.Add(this);
            this.name = name;
            _items = new List<ItemBase>();
            foreach (var item in items)
            {
                _items.Add(item.GetCopy());
            }
        }

        public string name;
        public IEnumerable<ItemBase> Items => _items;
        List<ItemBase> _items;

        public event Action<ItemBase, InventoryChangeType> OnInventoryChanged;
        public static event Action<Inventory> OnPlayerInventoryChanged;
        

        /// <summary> 아이템 추가. 내부적으로 카피를 생성하기 때문에 아무거나 집어넣으면 됨. 추가된 인벤토리 내의 아이템 리스트를 반환함</summary>
        public List<ItemBase> AddItem(ItemBase item, int count = 1, Action onOverflow = null)
        {
            if (count < 1)
            {
                Printer.Print($"[Inventory] 잘못된 개수가 입력됨 | count : {count}", LogType.Error);
                return null;
            }

            InventoryChangeType changeType;
            var addedItem = new List<ItemBase>(); 
            if (item.isStackable)
            {
                var exist = _items.Find(x => x.GUID == item.GUID);
                if(exist == null)
                {
                    var copy = item.GetCopy();
                    copy.AddStack(count);
                    AddNewItem(copy);
                    addedItem.Add(copy);
                    changeType = InventoryChangeType.AddSingle;
                }
                else
                {
                    var existCount = Count(exist); 
                    if(existCount == exist.maxStackCount)
                    {
                        onOverflow?.Invoke();
                        return null;
                    }
                    
                    exist.AddStack(count, () => onOverflow?.Invoke());
                    addedItem.Add(exist);
                    changeType = InventoryChangeType.AddStack;
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    var copy = item.GetCopy();
                    addedItem.Add(copy);
                    AddNewItem(copy);
                }

                changeType = InventoryChangeType.AddSingle;
            }
            OnInventoryChanged?.Invoke(item, changeType);
            return addedItem;
        }

        void AddNewItem(ItemBase item)
        {
            item.Inventory = this;
            _items.Add(item);
        }

        /// <summary> 인벤토리의 아이템 제거. 아이템의 스택을 초과해서 한 슬롯이 통째로 없어질때 onEmpty를 호출함.
        /// 반환값은 그 개수만큼의 스택을 가진 아이템.
        /// Stackable이 아닌 아이템을 여러개 삭제할 경우, 마지막 한 번만 onEmpty를 호출하고 해당 아이템 하나만 반환함.</summary>
        public ItemEliminationResult RemoveItem(ItemBase item, int count, out ItemBase removed,
            Action onEmpty = null, ItemMatchingType matchingType = ItemMatchingType.GUID)
        {
            removed = null;
            var exist = matchingType == ItemMatchingType.GUID ? 
                _items.Find(x => x.GUID == item.GUID) : _items.Find(x => x == item);
            if(exist == null)
            {
                Printer.Print($"[Inventory] 존재하지 않는 아이템을 인벤토리에서 제거하려 함. " +
                              $"item : {item.itemName} | count : {count}", LogType.Error);
                return ItemEliminationResult.None;
            }

            if (!exist.canBeTrashed)
            {
                Printer.Print($"[Inventory] 버릴 수 없는 아이템. item : {exist.itemName}", LogType.Warning);
                return ItemEliminationResult.None;
            }

            if (count == 0)
            {
                Printer.Print($"[Inventory] 0이 입력됨 item : {exist.itemName}", LogType.Warning);
                return ItemEliminationResult.None;
            }

            if (count < 0)
            {
                Printer.Print($"[Inventory] 음수의 개수가 입력됨. count를 양수로 바꿈 " +
                              $"item : {item.itemName} | count : {count}", LogType.Warning);
                count *= -1;
            }

            var result = ItemEliminationResult.Success;
            InventoryChangeType changeType;
            ItemBase removedItem;
            if (item.isStackable)
            {
                changeType = InventoryChangeType.RemoveStack;
                var removeCount = exist.RemoveStack(count, () =>
                {
                    _items.Remove(item);
                    onEmpty?.Invoke();
                    changeType = InventoryChangeType.RemoveSingle;
                    result = ItemEliminationResult.Empty;
                });
                
                removedItem = exist.GetCopy();
                removedItem.AddStack(removeCount);
            }
            else
            {
                result = ItemEliminationResult.Empty;
                removedItem = exist;
                removedItem.Inventory = null;
                for (int i = 0; i < count; i++)
                {
                    _items.Remove(exist);
                    exist.Inventory = null;
                    exist = _items.Find(x => x.GUID == item.GUID);
                    if(exist == null) break;
                }

                changeType = InventoryChangeType.RemoveSingle;
                onEmpty?.Invoke();
            }
            OnInventoryChanged?.Invoke(removedItem, changeType);
            removed = removedItem;
            return result;
        }
        
        /// <summary> 인벤토리에 존재하는 해당 아이템을 강제로 삭제함.  </summary>
        /// <param name="item"></param>
        public void RemoveSingleItem(ItemBase item)
        {
            item.Inventory = null;
            _items.Remove(item);
            OnInventoryChanged?.Invoke(item, InventoryChangeType.RemoveSingle);
        }

        /// <summary> 현재 인벤토리에 존재하는 아이템의 개수를 반환함. isStackable인 경우, StackAmount를 반환하고, 아닌 경우에 총 개수를 반환함. </summary>
        public int Count(ItemBase item, ItemMatchingType matchingType = ItemMatchingType.GUID)
        {
            var exist = matchingType == ItemMatchingType.GUID ?
                _items.Find(x => x.GUID == item.GUID) : _items.Find(x => x == item);
            if (exist == null) return 0;

            return item.isStackable ? exist.CurrentStack : 
                matchingType == ItemMatchingType.GUID ? 
                    _items.Count(x => x.GUID == item.GUID) : _items.Count(x => x == item);
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

        public List<CommonSaveData> GetSaveData()
        {
            var list = new List<CommonSaveData>();
            foreach (var itemBase in _items)
            {
                var saveData = new CommonSaveData();
                saveData.Key = itemBase.GUID.ToString();
                saveData.Data = new Dictionary<string, string>();
                // TODO : 아이템베이스로 세이브데이터 작성하기.
                list.Add(saveData);
            }

            return list;
        }

        public void Overwrite(List<CommonSaveData> saveData)
        {
            _items = new List<ItemBase>();
            foreach (var data in saveData)
            {
                var copy = ItemDB.Instance.FindItem(data.Key).GetCopy();
                copy.Overwrite(data);
                _items.Add(copy);
            }
        }


        public void Clear()
        {
            _items.Clear(); ;
        }

#if DEBUG
        public static void AddItemToPlayerInventory(string itemName, int count)
        {
            if (_playerInventory == null)
            {
                Debug.LogWarning("플레이어 인벤토리가 없음");
                return;
            }
            var item = ItemDB.Instance.Items.Find(x => x.itemName == itemName);
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

            var item = ItemDB.Instance.FindItem(guid);
            if(item == null) return;
            _playerInventory.AddItem(item, count);
        }
#endif

#if UNITY_EDITOR
        static void ReleaseEvent()
        {
            CreatedInventories = null;
            _playerInventory = null;
            OnPlayerInventoryChanged = null;
            Application.quitting -= ReleaseEvent;
        }
#endif
    }

    public enum ItemMatchingType
    {
        GUID,
        Instance
    }

    public enum InventoryChangeType
    {
        AddStack,
        AddSingle,
        RemoveStack,
        RemoveSingle
    }
}
