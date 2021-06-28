using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OcDialogue
{
    public abstract class ItemBase : ScriptableObject
    {
        [ReadOnly]public int GUID;
        [ReadOnly]public ItemType type;
        public string itemName;
        
        public bool isStackable;
        // TODO : 인게임 아이콘은 Addressable을 사용해서 구현할것.
#if UNITY_EDITOR
        [PropertyOrder(-10)]public Texture editorIcon;
        /// <summary> Editor Only. </summary>
        public abstract string SubTypeString { get; }
        /// <summary> Editor Only. 각 아이템 타입에 맞는 SubType을 할당함. </summary>
        public abstract void SetSubTypeFromString(string subtypeName);
#endif
        [ShowIf("isStackable")]public int maxStackCount;
        public int CurrentStack { get; protected set; }
        /// <summary> 원본이 인벤토리에 들어가는 것을 막기위한 값. 새 카피를 생성해서 인벤토리에 넣을 때, 이 부분을 true로 바꿔야함. </summary>
        public bool IsCopy { get; set; }
        [PropertyOrder(100), TextArea]public string description;

        /// <summary> 아이템 개수를 늘림. 1~maxCount의 개수로 제한되며, 오버될 경우, onStackOverflow가 호출됨. stackable아이템이 아니거나 count가 1보다 작은 경우 작동하지 않음. </summary>
        public void AddStack(int count, Action onStackOverflow)
        {
            if(!isStackable) return;
            if(count < 1) return;
            CurrentStack += count;
            if(CurrentStack > maxStackCount) onStackOverflow?.Invoke();
            CurrentStack = Mathf.Clamp(CurrentStack, 1, maxStackCount);
        }

        /// <summary> 아이템 개수를 제거함. 개수가 0 이하가 되는 경우, onEmpty가 호출됨. stackable아이템이 아니거나 count가 1보다 작은 경우 작동하지 않음. </summary>
        public void RemoveStack(int count, Action onEmpty)
        {
            if(!isStackable) return;
            if(count < 1) return;
            CurrentStack -= count;
            if(CurrentStack < 0) onEmpty?.Invoke();
        }
    }
}
