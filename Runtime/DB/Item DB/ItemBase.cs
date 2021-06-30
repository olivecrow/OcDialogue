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
        [BoxGroup("ReadOnly")][ReadOnly]public int GUID;
        [BoxGroup("ReadOnly")][ReadOnly]public ItemType type;
        public string itemName;
        /// <summary> 사용 가능한 아이템인지 여부. 현재 사용 가능한지 여부가 아니라 아이템의 특성을 나타내는 것. </summary>
        public bool isUsable;
        public bool isStackable;
        // TODO : 인게임 아이콘은 Addressable을 사용해서 구현할것.
#if UNITY_EDITOR
        [PropertyOrder(-10)]public Texture editorIcon;
        /// <summary> Editor Only. </summary>
        public abstract string SubTypeString { get; }
        /// <summary> Editor Only. 각 아이템 타입에 맞는 SubType을 할당함. </summary>
        public abstract void SetSubTypeFromString(string subtypeName);
        /// <summary> Editor Only. 데이터베이스 편집기에서 보여주는 참조된 아이템 설명. </summary>
        [PropertyOrder(101), ShowInInspector, MultiLineProperty(10), ReadOnly, ShowIf("referOtherDescription")]
        public string descriptionRefPreview => descriptionReference == null ? "No Reference" : descriptionReference.description;
#endif
        [ShowIf("isStackable")]public int maxStackCount;
        public int CurrentStack { get; protected set; }
        /// <summary> 원본이 인벤토리에 들어가는 것을 막기위한 값. 새 카피를 생성해서 인벤토리에 넣을 때, 이 부분을 true로 바꿔야함. </summary>
        public bool IsCopy { get; set; }
        /// <summary> 독자적인 설명을 가질지, 참조할지 여부. </summary>
        [PropertyOrder(100)]public bool referOtherDescription;
        /// <summary> 설명을 참조할 다른 방어구. </summary>
        [PropertyOrder(100), ShowIf("referOtherDescription"), OnValueChanged("OnOtherReferenceChanged")] public ItemBase descriptionReference;
        [PropertyOrder(100), TextArea(10, 20), HideIf("referOtherDescription")] public string description;

        /// <summary> 아이템 개수를 늘림. 1~maxCount의 개수로 제한되며, 오버될 경우, onStackOverflow가 호출됨. stackable아이템이 아니거나 count가 1보다 작은 경우 작동하지 않음. </summary>
        public void AddStack(int count, Action onStackOverflow = null)
        {
            if(!isStackable) return;
            if(count < 1) return;
            CurrentStack += count;
            if(CurrentStack > maxStackCount) onStackOverflow?.Invoke();
            CurrentStack = Mathf.Clamp(CurrentStack, 1, maxStackCount);
        }

        /// <summary> 아이템 개수를 제거함. 개수가 0 이하가 되는 경우, onEmpty가 호출됨. stackable아이템이 아니거나 count가 1보다 작은 경우 작동하지 않음. </summary>
        public void RemoveStack(int count, Action onEmpty = null)
        {
            if(!isStackable) return;
            if(count < 1) return;
            CurrentStack -= count;
            if(CurrentStack < 0) onEmpty?.Invoke();
        }

        /// <summary> 아이템의 복사본을 반환함. 실제 런타임에서 쓰이는 데이터. </summary>
        public abstract ItemBase GetCopy();
        public abstract bool IsNowUsable();
        public abstract void Use();
        /// <summary> 전달된 아이템에 ItemBase 속성 및 필드를 적용함. </summary>
        protected void ApplyBase(ItemBase baseCopy)
        {
            baseCopy.GUID = GUID;
            baseCopy.type = type;
            baseCopy.itemName = itemName;
            baseCopy.isUsable = isUsable;
            baseCopy.isStackable = isStackable;
            baseCopy.maxStackCount = maxStackCount;
            baseCopy.CurrentStack = 1;
            baseCopy.IsCopy = true;
            baseCopy.description = description;
        }

        /// <summary> GetCopy에서 생성된 복사본을 전달받아서 각 타입에서 구현해야 할 속성 및 필드를 반영하여 반환함. </summary>
        protected abstract void ApplyTypeProperty(ItemBase baseCopy);
        
#if UNITY_EDITOR
        void OnOtherReferenceChanged()
        {
            if (descriptionReference != null)
            {
                ItemBase target = descriptionReference;
                do
                {
                    if (descriptionReference.referOtherDescription)
                    {
                        target = descriptionReference.descriptionReference;
                        
                        Debug.LogWarning($"해당 참조 아이템의 설명이 다른 참조를 가지고 있어서 참조를 거슬러올라감 : {descriptionReference.itemName} -> {target.itemName}");
                    }
                } while (target.referOtherDescription);

                descriptionReference = target;
            }
        }  
#endif
    }
}
