#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace OcDialogue.DB
{
    public class DataSelectWindow : OdinEditorWindow
    {
        public IOcDataSelectable DataSelectable;
        [OnValueChanged(nameof(OnDBTypeChanged))][EnumToggleButtons]public DBType DBType;
        [BoxGroup][HideLabel][ShowIf(nameof(_useDataSelector))]public DataRowSelector DataRowSelector;
        [BoxGroup][HideLabel][ShowIf(nameof(DBType), OcDialogue.DBType.Item)]public ItemSelector ItemSelector;
        [BoxGroup][HideLabel][ShowIf(nameof(DBType), OcDialogue.DBType.Quest)]public StandardDataSelector QuestSelector;
        [BoxGroup][HideLabel][ShowIf(nameof(DBType), OcDialogue.DBType.NPC)] public StandardDataSelector NPCSelector;
        [BoxGroup][HideLabel][ShowIf(nameof(DBType), OcDialogue.DBType.Enemy)] public StandardDataSelector EnemySelector;
        [HideInInspector]public Action<OcData> OnDataSelected;
#pragma warning disable 414
        bool _useDataSelector;
#pragma warning restore 414
        public static DataSelectWindow Open(IOcDataSelectable dataSelectable)
        {
            var wnd = GetWindow<DataSelectWindow>(true);
            wnd.minSize = new Vector2(700, 300);
            wnd.maxSize = new Vector2(700, 300);
            wnd.DataSelectable = dataSelectable;
            return wnd;
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
                    if(DataRowSelector == null) DataRowSelector = new DataRowSelector(GameProcessDB.Instance.DataRowContainer.DataRows);
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
            OcData targetData = null;
            switch (DBType)
            {
                case DBType.GameProcess:
                    targetData = DataRowSelector.DataRow;
                    break;
                case DBType.Item:
                    targetData = ItemSelector.Item;
                    break;
                case DBType.Quest:
                    targetData = QuestSelector.ValidData;
                    break;
                case DBType.NPC:
                    targetData = NPCSelector.ValidData;
                    break;
                case DBType.Enemy:
                    targetData = EnemySelector.ValidData;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            DataSelectable?.SetTargetData(targetData);
            DataSelectable?.UpdateAddress();
            OnDataSelected?.Invoke(targetData);
            Close();
        }
    }

    [Serializable]
    public class DataRowSelector
    {
        [ValueDropdown(nameof(GetDataRowList))]public DataRow DataRow;

        IEnumerable<DataRow> _allRow;
        public DataRowSelector(IEnumerable<DataRow> dataRows)
        {
            _allRow = dataRows;
        }

        ValueDropdownList<DataRow> GetDataRowList()
        {
            if (_allRow == null) return null;
            var list = new ValueDropdownList<DataRow>();
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
        public OcData Data;
        [ShowIf(nameof(dataSelectionType), DataSelectionType.Target_DataRow)]
        public DataRowSelector DataRowSelector;
        
        public OcData ValidData => dataSelectionType switch
        {
            DataSelectionType.Target => Data,
            DataSelectionType.Target_DataRow => DataRowSelector.DataRow,
            _ => throw new ArgumentOutOfRangeException()
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
        ValueDropdownList<OcData> GetData()
        {
            if (_DB == null) return null;
            var list = new ValueDropdownList<OcData>();
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
#endif