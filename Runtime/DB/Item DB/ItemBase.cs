using System;
using System.Collections;
using System.Collections.Generic;
using OcUtility;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace OcDialogue
{
    public abstract class ItemBase : OcData
    {
        public override string Address => $"{type}/{SubTypeString}/{itemName}";

        [BoxGroup("ReadOnly")][ReadOnly]public int GUID;
        [BoxGroup("ReadOnly")][ReadOnly]public ItemType type;
        public string itemName;
        public AssetReferenceSprite IconReference;
        public bool isStackable;
        /// <summary> Editor Only. </summary>
        public abstract string SubTypeString { get; }
#if UNITY_EDITOR
        [BoxGroup("ReadOnly")][PreviewField(ObjectFieldAlignment.Left)][ShowInInspector] public Object IconPreview => IconReference.editorAsset;
        /// <summary> Editor Only. 각 아이템 타입에 맞는 SubType을 할당함. </summary>
        public abstract void SetSubTypeFromString(string subtypeName);
        /// <summary> Editor Only. 데이터베이스 편집기에서 보여주는 참조된 아이템 설명. </summary>
        [PropertyOrder(101), ShowInInspector, MultiLineProperty(10), ReadOnly, ShowIf("referOtherDescription")]
        public string descriptionRefPreview => descriptionReference == null ? "No Reference" : descriptionReference.description;
#endif
        [ShowIf("isStackable")]public int maxStackCount = 999;
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
            if(!IsCopy) return;
            if(!isStackable) return;
            if(count < 1) return;
            CurrentStack += count;
            if(CurrentStack > maxStackCount) onStackOverflow?.Invoke();
            CurrentStack = Mathf.Clamp(CurrentStack, 1, maxStackCount);
        }

        /// <summary> 아이템 개수를 제거함. 개수가 0 이하가 되는 경우, onEmpty가 호출됨. stackable아이템이 아니거나 count가 1보다 작은 경우 작동하지 않음. </summary>
        public void RemoveStack(int count, Action onEmpty = null)
        {
            if(!IsCopy) return;
            if(!isStackable) return;
            if(count < 1) return;
            CurrentStack -= count;
            if(CurrentStack <= 0) onEmpty?.Invoke();
        }

        /// <summary> 아이템의 복사본을 반환함. 실제 런타임에서 쓰이는 데이터. </summary>
        public ItemBase GetCopy()
        {
            var copy = CreateInstance();
            ApplyBase(copy);
            ApplyTypeProperty(copy);
            return copy;
        }

        /// <summary> ItemBase를 상속받는 아이템 클래스에서 카피용 인스턴스를 생성 후, 반환함. ItemBase는 추상 클래스라 직접 생성이 안되기 때문. </summary>
        protected abstract ItemBase CreateInstance();
        /// <summary> 전달된 아이템에 ItemBase 속성 및 필드를 적용함. </summary>
        protected void ApplyBase(ItemBase baseCopy)
        {
            baseCopy.GUID = GUID;
            baseCopy.type = type;
            baseCopy.name = name;
            baseCopy.itemName = itemName;
            baseCopy.isStackable = isStackable;
            baseCopy.maxStackCount = maxStackCount;
            baseCopy.IsCopy = true;
            baseCopy.description = description;
        }
        
        /// <summary> GetCopy에서 생성된 복사본을 전달받아서 각 타입에서 구현해야 할 속성 및 필드를 반영하여 반환함. </summary>
        protected abstract void ApplyTypeProperty(ItemBase baseCopy);
        
        
#if UNITY_EDITOR
        void OnValidate()
        {
            if (name != itemName)
            {
                name = itemName;
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }

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
