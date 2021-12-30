using System;
using System.Collections.Generic;
using System.Linq;
using OcDialogue;
using OcDialogue.Editor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace MyDB.Editor
{
    [CustomEditor(typeof(ItemDB))]
    public class ItemDBEditor : DBEditorBase
    {
        ItemBase CurrentItem => SelectedData as ItemBase;
        ItemType CurrentItemType
        {
            get => _currentItemType;
            set
            {
                var isNew = _currentItemType != value;
                _currentItemType = value;
                if(isNew)
                {
                    _subtypeNames = GetSubtypeNamesFromType(value);
                    _currentSubtypeIndex = -1;
                    CurrentSubtypeIndex = 0;
                }
            }
        }

        int CurrentSubtypeIndex
        {
            get => _currentSubtypeIndex;
            set
            {
                var isNew = _currentSubtypeIndex != value;
                _currentSubtypeIndex = value;
                if (isNew)
                {
                    Debug.Log($"subtype isNew | type : {_currentItemType} | subtype : {_currentSubtypeString}");
                    if(_subtypeNames != null)_currentSubtypeString = _subtypeNames[_currentSubtypeIndex];
                    Window.ForceMenuTreeRebuild();
                    var item = ItemDB.Items.Find(x => x.type == _currentItemType && x.SubTypeString == _currentSubtypeString);
                    if (item == null) SelectedData = null;
                    else SelectTreeMenu(item);
                }
            }
        }

        List<string> _subtypeNames;
        List<GUIContent> _typeToolbarContents;
        ItemType _currentItemType;
        string _currentSubtypeString;
        int _currentSubtypeIndex;
        ItemDB ItemDB;
        protected override void OnEnable()
        {
            base.OnEnable();
            ItemDB = target as ItemDB;
            _typeToolbarContents = new List<GUIContent>();
            _subtypeNames = GetSubtypeNamesFromType(ItemType.Generic);
            _currentSubtypeString = _subtypeNames[0];
            var types = Enum.GetNames(typeof(ItemType));
            foreach (var type in types)
            {
                var content = new GUIContent(type, Icon(type));
                _typeToolbarContents.Add(content);
            }
        }

        public override void CreateTree(OdinMenuTree tree)
        {
            foreach (var item in ItemDB.Items)
            {
                if (item.type == _currentItemType && item.SubTypeString == _currentSubtypeString)
                {
                    tree.Add(item.itemName, item, item.IconPreview as Texture);
                }
            }

            tree.Selection.SelectionChanged += type =>
            {
                switch (type)
                {
                    case SelectionChangedType.ItemRemoved:
                        SelectedData = null;
                        break;
                    case SelectionChangedType.ItemAdded:
                        SelectedData = tree.Selection.SelectedValue as ItemBase;
                        break;
                    case SelectionChangedType.SelectionCleared:
                        SelectedData = null;
                        break;
                }
            };
            tree.Selection.SelectionConfirmed += s =>
                EditorGUIUtility.PingObject(s.SelectedValue as Object);
        }

        public override void DrawToolbar()
        {
            GUI.color = new Color(2f, 1.5f, 0f);
            CurrentItemType = (ItemType) OcEditorUtility.DrawCategory(
                (int)_currentItemType, _typeToolbarContents, GUILayout.Height(25));
            GUI.color = new Color(1.5f, 2f, 0f);

            var lineBraking = _currentItemType == ItemType.Weapon ? 7 : -1;
            
            CurrentSubtypeIndex = OcEditorUtility.DrawCategory(
                _currentSubtypeIndex, _subtypeNames, lineBraking, GUILayout.MaxWidth(200));
            GUI.color = Color.white;

            SirenixEditorGUI.BeginHorizontalToolbar();
            if(SirenixEditorGUI.ToolbarButton("Select DB")) EditorGUIUtility.PingObject(ItemDB);
            if (SirenixEditorGUI.ToolbarButton("Create"))
            {
                var item = AddItem(_currentItemType, _currentSubtypeString);
                Window.ForceMenuTreeRebuild();
                Window.MenuTree.Selection.Add(Window.MenuTree.MenuItems.Find(x => (ItemBase)x.Value == item));
            }

            if (CurrentItem != null && SirenixEditorGUI.ToolbarButton("Delete"))
            {
                DeleteItem(CurrentItem);
                
                Window.ForceMenuTreeRebuild();
                Window.MenuTree.Selection
                    .Add(Window.MenuTree.MenuItems.FindLast(x => 
                            ((ItemBase)x.Value).type == _currentItemType && 
                             ((ItemBase)x.Value).SubTypeString == _currentSubtypeString));
            }

            if (CurrentItem != null && SirenixEditorGUI.ToolbarButton("Duplicate"))
            {
                var item = DuplicateItem(CurrentItem);
                
                Window.ForceMenuTreeRebuild();
                Window.MenuTree.Selection.Add(Window.MenuTree.MenuItems.Find(x => (ItemBase)x.Value == item));
            }

            if (CurrentItem != null && SirenixEditorGUI.ToolbarButton("Refresh"))
            {
                var selection = Window.MenuTree.Selection.SelectedValue;
                Window.ForceMenuTreeRebuild();
                Window.MenuTree.Selection.Add(Window.MenuTree.MenuItems.Find(x => x.Value == selection));
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Items"));
            if(CurrentItem != null) EditorGUILayout.LabelField($"[{CurrentItem.type}] {CurrentItem.itemName}", 
                new GUIStyle(GUI.skin.label){fontSize = 20, fontStyle = FontStyle.Bold}, GUILayout.Height(22));
            serializedObject.ApplyModifiedProperties();
        }
        public ItemBase AddItem(ItemType type, string subType)
        {
            ItemBase item = type switch
            {
                ItemType.Generic => CreateInstance<GenericItem>(),
                ItemType.Weapon => CreateInstance<WeaponItem>(),
                ItemType.Armor => CreateInstance<ArmorItem>(),
                ItemType.Important => CreateInstance<ImportantItem>(),
                ItemType.Accessory => CreateInstance<AccessoryItem>(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            item.GUID = OcDataUtility.CalcItemGUID(ItemDB.Items.Select(x => x.GUID));
            item.SetSubTypeFromString(subType);

            item.name = OcDataUtility.CalculateDataName($"New {subType}", ItemDB.Items.Select(x => x.itemName));
            item.itemName = item.name;
            
            return CreateAsAsset(item);
        }
        
        public void DeleteItem(ItemBase item)
        {
            if(!ItemDB.Items.Contains(item)) return;
            ItemDB.Items.Remove(item);
            Debug.Log($"[{item.type}] 에셋 삭제 | subtype : {item.SubTypeString} | path : {AssetDatabase.GetAssetPath(item)}");
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(item));
            EditorUtility.SetDirty(ItemDB);
            AssetDatabase.SaveAssets();
        }

        public ItemBase DuplicateItem(ItemBase item)
        {
            var copy = item.GetCopy();
            copy.IsCopy = false;
            copy.GUID = OcDataUtility.CalcItemGUID(ItemDB.Items.Select(x => x.GUID));
            copy.itemName = OcDataUtility.CalculateDataName($"{copy.itemName}_Copy", ItemDB.Items.Select(x => x.itemName));
            copy.name = copy.itemName;
            
            // IconReference가 원본에서 참조되면, 복사된 아이템의 아이콘이 바뀌면 원본까지 같이 바뀌어버려서 참조를 해제함.
            // IconReference의 인스턴스를 새로 할당하려고 해도, MenuTree에 반영이 안 되는 버그가 있어서 그냥 참조 해제만 함.
            copy.IconReference = null;

            return CreateAsAsset(copy);
        }

        ItemBase CreateAsAsset(ItemBase item)
        {
            var dbFolderPath = AssetDatabase.GetAssetPath(ItemDB).Replace($"/{ItemDB.name}.asset", "");
            OcDataUtility.CreateFolderIfNull(dbFolderPath, item.type.ToString());
            OcDataUtility.CreateFolderIfNull(dbFolderPath + $"/{item.type}", item.SubTypeString);
            AssetDatabase.CreateAsset(item, dbFolderPath + $"/{item.type}/{item.SubTypeString}/{item.itemName}.asset");
            ItemDB.Items.Add(item);
            EditorUtility.SetDirty(ItemDB);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[{item.type}] 에셋 생성 | subtype : {item.SubTypeString} | path : {AssetDatabase.GetAssetPath(item)}");
            
            return item;
        }

        Texture2D Icon(string type) => Resources.Load<Texture2D>($"{type}Item Icon");

        List<string> GetSubtypeNamesFromType(ItemType type)
        {
            return type switch
            {
                ItemType.Generic => Enum.GetNames(typeof(GenericType)).ToList(),
                ItemType.Armor => Enum.GetNames(typeof(ArmorType)).ToList(),
                ItemType.Weapon => Enum.GetNames(typeof(WeaponType)).ToList(),
                ItemType.Accessory => Enum.GetNames(typeof(AccessoryType)).ToList(),
                ItemType.Important => Enum.GetNames(typeof(ImportantItemType)).ToList(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}