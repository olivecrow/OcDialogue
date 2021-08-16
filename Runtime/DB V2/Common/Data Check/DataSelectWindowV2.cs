using System;
using System.Collections.Generic;
using System.Linq;
using OcDialogue.DB;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.DB
{
    public interface IAddressableDataContainer
    {
        AddressableData Data { get; set; }
        string Detail { get; set; }
        void UpdateAddress();
    }
    public class DataSelectWindowV2 : OdinEditorWindow
    {
        public IAddressableDataContainer dataContainer;
        [OnValueChanged(nameof(OnDBTypeChanged))][EnumToggleButtons]public DBType DBType;
        [ShowIf(nameof(_useDataSelector))]public DataRowSelector DataRowSelector;
        [ShowIf(nameof(DBType), OcDialogue.DBType.Item)]public ItemSelector ItemSelector;
        [ShowIf(nameof(DBType), OcDialogue.DBType.Quest)]public StandardDataSelector QuestSelector;
        [ShowIf(nameof(DBType), OcDialogue.DBType.NPC)] public StandardDataSelector NPCSelector;
        [ShowIf(nameof(DBType), OcDialogue.DBType.Enemy)] public StandardDataSelector EnemySelector;
        bool _useDataSelector;
        public static void Open(IAddressableDataContainer dataContainer)
        {
            var wnd = GetWindow<DataSelectWindowV2>(true);
            wnd.minSize = new Vector2(700, 300);
            wnd.maxSize = new Vector2(700, 300);
            wnd.dataContainer = dataContainer;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            OnDBTypeChanged();
        }

        void OnDBTypeChanged()
        {
            _useDataSelector = false;
            DataRowSelector = null;
            ItemSelector = null;
            QuestSelector = null;
            NPCSelector = null;
            EnemySelector = null;
            switch (DBType)
            {
                case DBType.GameProcess:
                    _useDataSelector = true;
                    if(DataRowSelector == null) DataRowSelector = new DataRowSelector(GameProcessDB_V2.Instance.DataRowContainer.DataRows);
                    break;
                case DBType.Item:
                    if(ItemSelector == null) ItemSelector = new ItemSelector(ItemDatabase.Instance.Items);
                    break;
                case DBType.Quest:
                    if(QuestSelector == null) QuestSelector = new StandardDataSelector(QuestDB.Instance);
                    break;
                case DBType.NPC:
                    if(NPCSelector == null) NPCSelector = new StandardDataSelector(NPCDB.Instance);
                    break;
                case DBType.Enemy:
                    if(EnemySelector == null) EnemySelector = new StandardDataSelector(EnemyDB.Instance);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [Button]
        void Apply()
        {
            switch (DBType)
            {
                case DBType.GameProcess:
                    dataContainer.Data = DataRowSelector.DataRow;
                    break;
                case DBType.Item:
                    dataContainer.Data = ItemSelector.Item;
                    break;
                case DBType.Quest:
                    dataContainer.Data = QuestSelector.ValidData;
                    dataContainer.Detail = DataCheckerV2.QUEST_STATE;
                    break;
                case DBType.NPC:
                    dataContainer.Data = NPCSelector.ValidData;
                    dataContainer.Detail = DataCheckerV2.NPC_ENCOUNTERED;
                    break;
                case DBType.Enemy:
                    dataContainer.Data = EnemySelector.ValidData;
                    dataContainer.Detail = DataCheckerV2.ENEMY_KILLCOUNT;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            dataContainer.UpdateAddress();
            Close();
        }
    }

    [Serializable]
    public class DataRowSelector
    {
        [ValueDropdown(nameof(GetDataRowList))]public DataRowV2 DataRow;

        IEnumerable<DataRowV2> _allRow;
        public DataRowSelector(IEnumerable<DataRowV2> dataRows)
        {
            _allRow = dataRows;
        }

        ValueDropdownList<DataRowV2> GetDataRowList()
        {
            if (_allRow == null) return null;
            var list = new ValueDropdownList<DataRowV2>();
            foreach (var data in _allRow)
            {
                list.Add(data.name, data);
            }

            return list;
        }
    }

    [Serializable]
    public class ItemSelector
    {
        [EnumToggleButtons]public ItemType ItemType;
        [ValueDropdown(nameof(GetSubTypeList))]public string ItemSubType;
        [ValueDropdown(nameof(GetItemList))]public ItemBase Item;

        IEnumerable<ItemBase> _Items;
        public ItemSelector(IEnumerable<ItemBase> items)
        {
            _Items = items;
        }
        
        ValueDropdownList<string> GetSubTypeList()
        {
            var list = new ValueDropdownList<string>();
            var names = Enum.GetNames(ItemDatabase.GetSubType(ItemType));
            foreach (var name in names)
            {
                list.Add(name);
            }

            return list;
        }

        ValueDropdownList<ItemBase> GetItemList()
        {
            if (_Items == null) return null;
            var list = new ValueDropdownList<ItemBase>();
            foreach (var item in _Items.Where(x => x.type == ItemType && x.SubTypeString == ItemSubType))
            {
                list.Add(item.itemName, item);
            }

            return list;
        }
    }

    public interface IStandardDB
    {
        string[] CategoryRef { get; }
        IEnumerable<AddressableData> AllData { get; }
    }

    public interface IDataRowUser
    {
        DataRowContainerV2 DataRowContainer { get; }
    }
    
    [Serializable]
    public class StandardDataSelector
    {
        public enum DataSelectionType
        {
            Target,
            Target_DataRow
        }
        [ValueDropdown(nameof(GetCategory))]public string Category;
        [EnumToggleButtons]public DataSelectionType dataSelectionType;
        [ValueDropdown(nameof(GetData))][OnValueChanged(nameof(UpdateDataContainer))] 
        public AddressableData Data;
        [ShowIf(nameof(dataSelectionType), DataSelectionType.Target_DataRow)]
        public DataRowSelector DataRowSelector;
        
        public AddressableData ValidData => dataSelectionType switch
        {
            DataSelectionType.Target => Data,
            DataSelectionType.Target_DataRow => DataRowSelector.DataRow,
        };

        IStandardDB _DB;
        public StandardDataSelector(IStandardDB db)
        {
            _DB = db;
            Category = db.CategoryRef[0];
        }

        ValueDropdownList<string> GetCategory()
        {
            if (_DB == null) return null;
            var list = new ValueDropdownList<string>();
            foreach (var category in _DB.CategoryRef)
            {
                list.Add(category);
            }

            return list;
        }
        ValueDropdownList<AddressableData> GetData()
        {
            if (_DB == null) return null;
            var list = new ValueDropdownList<AddressableData>();
            foreach (var data in _DB.AllData)
            {
                if(data.Address.Contains(Category)) list.Add(data);
            }

            return list;
        }

        void UpdateDataContainer()
        {
            var user = Data as IDataRowUser;
            DataRowSelector = new DataRowSelector(user.DataRowContainer.DataRows);
        }
    }
}