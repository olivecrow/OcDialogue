using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    public abstract class ItemBase : ScriptableObject
    {
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
        public int CurrentStack
        {
            get => _currentStack;
            set
            {
                if(!isStackable) return;
                if(_currentStack + value > maxStackCount) OnStackOverflow?.Invoke();
                _currentStack = Mathf.Clamp(_currentStack + value, 1, maxStackCount);
            }
        }
        int _currentStack;
        /// <summary> CurrentStack에 설정한 값이 maxStackCount를 초과하면 호출됨. </summary>
        public event Action OnStackOverflow;
        [PropertyOrder(100), TextArea]public string description;
    }
}
