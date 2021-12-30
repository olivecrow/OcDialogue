using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Linq;
using OcDialogue;
using OcDialogue.DB;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MyDB
{
    [CreateAssetMenu(fileName = "Item Database", menuName = "Oc Dialogue/DB/Item Database")]
    public sealed class ItemDB : OcDB
    {
        public override string Address => "ItemDatabase";
        public override string[] CategoryOverride => Enum.GetNames(typeof(ItemType));

        public static ItemDB Instance
        {
            get
            {
                if(_instance == null) 
                    _instance = DBManager.Instance.DBs.Find(x => x is ItemDB) as ItemDB;
                return _instance;
            }
        }

        static ItemDB _instance;
        
        public List<ItemBase> Items;

        public override void Init()
        {
            IsInitialized = true;
        }

        public override void Overwrite(List<CommonSaveData> saveData)
        {
            if (Inventory.PlayerInventory != null)
            {
                Inventory.PlayerInventory.Overwrite(saveData);
            }
            else
            {
                Inventory.OnPlayerInventoryChanged += inventory => inventory.Overwrite(saveData);
            }
        }

        public override List<CommonSaveData> GetSaveData()
        {
            if (Inventory.PlayerInventory == null) return null;
            return Inventory.PlayerInventory.GetSaveData();
        }

        public override IEnumerable<OcData> AllData => Items;

        public ItemBase FindItem(int guid)
        {
            return Items.Find(x => x.GUID == guid);
        }

        public ItemBase FindItem(string itemName)
        {
            return Items.Find(x => x.itemName == itemName);
        }
        public static Type GetSubType(ItemType type)
        {
            var t = type switch
            {
                ItemType.Generic => typeof(GenericType),
                ItemType.Armor => typeof(ArmorType),
                ItemType.Weapon => typeof(WeaponType),
                ItemType.Important => typeof(ImportantItemType),
                ItemType.Accessory => typeof(AccessoryType),
                _ => throw new ArgumentOutOfRangeException()
            };
            return t;
        }
        
        
        public override bool IsTrue(string fieldName, CheckFactor.Operator op, object checkValue)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(string fieldName)
        {
            throw new NotImplementedException();
        }

        public override string[] GetFieldNames()
        {
            throw new NotImplementedException();
        }

        public override void SetValue(string fieldName, DataSetter.Operator op, object value)
        {
            throw new NotImplementedException();
        }

        
#if UNITY_EDITOR
        [HideInInspector]public InventoryEditorPreset editorPreset;
#endif

    }
}
