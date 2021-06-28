using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public class Inventory
    {
        public IEnumerable<ItemBase> Items => _items;
        List<ItemBase> _items = new List<ItemBase>();

        public event Action<ItemBase> OnItemAdded;
        public event Action<ItemBase> OnItemRemoved;
        public event Action<ItemBase> OnStackOverflow;

        /// <summary> 아이템 추가. 데이터 베이스 원본이 아닌 카피를 넣어야 하며, stackable 아이템의 경우, 미리 개수를 정해둬야함. </summary>
        public void AddItem(ItemBase item)
        {
            if(!item.IsCopy)
            {
                Debug.LogWarning($"아이템 원본을 인벤토리에 추가하려 했음. item : {item.itemName}");
                return;
            }
            if (item.isStackable)
            {
                var exist = _items.Find(x => x.GUID == item.GUID);
                if(exist == null) AddNewItem(item);
                else
                {
                    exist.AddStack(item.CurrentStack, () => OnStackOverflow?.Invoke(exist));
                }
            }
            else
            {
                AddNewItem(item);
            }
            OnItemAdded?.Invoke(item);
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
        /// <summary> 아이템을 개수에 상관 없이 삭제함. </summary>
        void RemoveSingleItem(ItemBase item)
        {
            _items.Remove(item);
        }
    }
}
