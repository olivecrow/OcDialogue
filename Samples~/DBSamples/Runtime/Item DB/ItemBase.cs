using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OcDialogue;
using OcDialogue.DB;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

[assembly: InternalsVisibleTo("DBSample_Editor")]
namespace MyDB
{
    public abstract class ItemBase : OcData
    {
        public override string Address => $"{type}/{SubTypeString}/{itemName}";

        public override string Category
        {
            get => type.ToString();
            set => Debug.LogWarning($"[{type}][{SubTypeString}][{itemName}] ItemBase의 타입을 바꿀 수 없음");
        }

        [BoxGroup("ReadOnly")][ReadOnly]public int GUID;
        [BoxGroup("ReadOnly")][ReadOnly]public virtual ItemType type { get; protected set; }
        [Delayed]public string itemName;
        public AssetReferenceSprite IconReference;
        public bool canBeTrashed = true;
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

        public virtual int CurrentStack
        {
            get => _currentStack;
            protected set
            {
                var before = _currentStack;
                _currentStack = Mathf.Clamp(value, 0, maxStackCount);
                if(before != _currentStack && _currentStack == 0 && Inventory != null) 
                    Inventory.RemoveSingleItem(this);
            }
        }
        public Inventory Inventory { get; set; }
        /// <summary> 원본이 인벤토리에 들어가는 것을 막기위한 값. 새 카피를 생성해서 인벤토리에 넣을 때, 이 부분을 true로 바꿔야함. </summary>
        internal bool IsCopy { get; set; }
        /// <summary> 독자적인 설명을 가질지, 참조할지 여부. </summary>
        [PropertyOrder(100)]public bool referOtherDescription;
        /// <summary> 설명을 참조할 다른 방어구. </summary>
        [PropertyOrder(100), ShowIf("referOtherDescription"), OnValueChanged("OnOtherReferenceChanged")] public ItemBase descriptionReference;
        [PropertyOrder(100), TextArea(10, 20), HideIf("referOtherDescription")] public string description;

        public const string fieldName_Stack = "Stack";
        public const string fieldName_IsEquipped = "IsEquipped";
        public const string fieldName_WeaponEquipState = "WeaponEquipState";
        public const string fieldName_Durability = "Durability";
        public const string fieldName_Upgrade = "Upgrade";

        int _currentStack;
        /// <summary> 아이템 개수를 늘림. 1~maxCount의 개수로 제한되며, 오버될 경우, onStackOverflow가 호출됨. stackable아이템이 아니거나 count가 1보다 작은 경우 작동하지 않음. </summary>
        internal void AddStack(int count, Action onStackOverflow = null)
        {
            if(!IsCopy) return;
            if(!isStackable) return;
            if(count < 1) return;
            _currentStack += count;
            if(_currentStack > maxStackCount) onStackOverflow?.Invoke();
            _currentStack = Mathf.Clamp(_currentStack, 1, maxStackCount);
        }

        /// <summary> 아이템 개수를 제거함. 개수가 0 이하가 되는 경우, onEmpty가 호출됨. stackable아이템이 아니거나 count가 1보다 작은 경우 작동하지 않음.
        /// 삭제된 개수를 반환함. </summary>
        internal int RemoveStack(int count, Action onEmpty = null)
        {
            if(!IsCopy) return 0;
            if(!isStackable) return 0;
            if(count < 1) return 0;

            var diff = _currentStack >= count ? _currentStack - count : _currentStack;
            _currentStack -= count;
            if(_currentStack <= 0) onEmpty?.Invoke();

            return diff;
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
        protected void ApplyBase(ItemBase copy)
        {
            copy.GUID = GUID;
            copy.type = type;
            copy.name = name;
            copy.itemName = itemName;
            copy.canBeTrashed = canBeTrashed;
            copy.isStackable = isStackable;
            copy.maxStackCount = maxStackCount;
            copy.IsCopy = true;
            copy.description = description;
            copy.IconReference = IconReference;
            
            if (!isStackable) copy._currentStack = 1;
        }
        
        /// <summary> GetCopy에서 생성된 복사본을 전달받아서 각 타입에서 구현해야 할 속성 및 필드를 반영하여 반환함. </summary>
        protected abstract void ApplyTypeProperty(ItemBase copy);
        
        
        public override bool IsTrue(string fieldName, CheckFactor.Operator op, object checkValue)
        {
            return Inventory.PlayerInventory.Count(this).IsTrue(op, (int)checkValue);
        }

        public override object GetValue(string fieldName)
        {
            return Inventory.PlayerInventory.Count(this);
        }

        public override string[] GetFieldNames()
        {
            return null;
        }

        public override void SetValue(string fieldName, DataSetter.Operator op, object value)
        {
            var inv = Inventory.PlayerInventory;
            var count = inv.Count(this);
            var valueInt = (int)value;
            switch (op)
            {
                case DataSetter.Operator.Set:
                    set(valueInt);
                    break;
                case DataSetter.Operator.Add:
                    if (valueInt > 0) inv.AddItem(this, valueInt);
                    else inv.RemoveItem(this, -valueInt, out var removed);
                    break;
                case DataSetter.Operator.Multiply:
                    inv.AddItem(this, count * valueInt - count);
                    break;
                case DataSetter.Operator.Divide:
                    var divideResult = count / valueInt;
                    set(divideResult);
                    break;
            }

            void set(int v)
            {
                var diff = count - v;
                if (diff > 0) // 원래 가진 개수가 더 많았음 => 삭제
                    inv.RemoveItem(this, diff, out var removed);
                else // value가 더 많음 => 추가
                    inv.AddItem(this, -diff);
            }
        }

        public override DataRowType GetValueType(string fieldName)
        {
            return DataRowType.Int;
        }

        public CommonSaveData GetSaveData()
        {
            var data = new CommonSaveData
            {
                Key = GUID.ToString(),
                Data = new Dictionary<string, string>
                {
                    [fieldName_Stack] = CurrentStack.ToString()
                }
            };
            AppendAdditionalSaveData(data);

            return data;
        }

        protected abstract void AppendAdditionalSaveData(CommonSaveData saveData);

        public void Overwrite(CommonSaveData saveData)
        {
            if (isStackable && saveData.Data.ContainsKey(fieldName_Stack)) 
                int.TryParse(saveData.Data[fieldName_Stack], out _currentStack);
            OverwriteAdditionalSaveData(saveData);
        }

        protected abstract void OverwriteAdditionalSaveData(CommonSaveData saveData);

#if UNITY_EDITOR
        void OnValidate()
        {
            if (name != itemName)
            {
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(this), itemName);
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
