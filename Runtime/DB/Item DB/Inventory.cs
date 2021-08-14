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

        public event Action<ItemBase> OnItemAdded;
        public event Action<ItemBase> OnItemRemoved;
        public event Action<ItemBase> OnStackOverflow;
        public static event Action<Inventory> OnPlayerInventoryChanged;
        

        /// <summary> 아이템 추가. 내부적으로 카피를 생성하기 때문에 아무거나 집어넣으면 됨.</summary>
        public void AddItem(ItemBase item, int count = 1)
        {
            if (count < 1)
            {
                Printer.Print($"[Inventory] 잘못된 개수가 입력됨 | count : {count}", LogType.Error);
                return;
            }
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
                    exist.AddStack(count, () => OnStackOverflow?.Invoke(exist));
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    AddNewItem(item.GetCopy());
                }
            }
            OnItemAdded?.Invoke(_items.Find(x => x.GUID == item.GUID));
        }

        void AddNewItem(ItemBase item)
        {
            _items.Add(item);
        }

        /// <summary> 인벤토리의 아이템 제거. 원본이든 카피든 상관 없이 동일한 GUID를 가지는 아이템을 개수만큼 제거함. 파라미터 아이템의 CurrentStack은 고려하지 않음.
        /// stackable이 아닌 아이템의 경우, 항상 1개만 제거됨. </summary>
        public void RemoveItem(ItemBase item, int count = 1)
        {
            var exist = _items.Find(x => x.GUID == item.GUID);
            if(exist == null)
            {
                Debug.LogWarning($"존재하지 않는 아이템을 인벤토리에서 제거하려 함. item : {item.itemName} | count : {count}");
                return;
            }
            if (item.isStackable)
            {
                exist.RemoveStack(count, () => RemoveSingleItem(exist));
            }
            else
            {
                RemoveSingleItem(exist);
            }
            OnItemRemoved?.Invoke(exist);
        }

        /// <summary> 현재 인벤토리에 존재하는 아이템의 개수를 반환함. isStackable인 경우, StackAmount를 반환하고, 아닌 경우에 총 개수를 반환함. </summary>
        public int Count(ItemBase item)
        {
            var exist = _items.Find(x => x.GUID == item.GUID);
            if (exist == null) return 0;

            return item.isStackable ? exist.CurrentStack : _items.Count(x => x.GUID == item.GUID);
        }
        
        /// <summary> 아이템을 개수에 상관 없이 삭제함. </summary>
        void RemoveSingleItem(ItemBase item)
        {
            _items.Remove(item);
        }


#if UNITY_EDITOR
        static void ReleaseEvent()
        {
            OnPlayerInventoryChanged = null;
            Application.quitting -= ReleaseEvent;
        }
#endif
    }
}
