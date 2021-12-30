using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace MyDB.Editor
{
    public class InventoryDebugWindow : OdinMenuEditorWindow
    {
        Inventory CurrentInventory
        {
            get => _currentInventory;
            set
            {
                var isNew = _currentInventory != value;
                _currentInventory = value;
                if (isNew)
                {
                    if (value == null) _ordered = null;
                    else _ordered = value.Items.OrderBy(x => x.type).ThenBy(x => x.SubTypeString).ToList();
                }
            }
        }
        Inventory _currentInventory;
        List<ItemBase> _ordered;
        [MenuItem("OcDialogue/인벤토리 디버그 윈도우")]
        private static void ShowWindow()
        {
            if(!Application.isPlaying) return;
            var window = GetWindow<InventoryDebugWindow>();
            window.titleContent = new GUIContent("인벤토리 디버그 윈도우");
            window.minSize = new Vector2(720, 480);
            window.Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();

            tree.Selection.SelectionChanged += type =>
            {
                switch (type)
                {
                    case SelectionChangedType.ItemRemoved:
                        CurrentInventory = null;
                        break;
                    case SelectionChangedType.ItemAdded:
                        CurrentInventory = tree.Selection.SelectedValue as Inventory;
                        break;
                    case SelectionChangedType.SelectionCleared:
                        CurrentInventory = null;
                        break;
                }
            };
            
            tree.Selection.SelectionConfirmed += selection =>
            {
                Selection.activeObject = selection.SelectedValue as UnityEngine.Object;
                EditorGUIUtility.PingObject(Selection.activeObject);
            };

            foreach (var inventory in Inventory.CreatedInventories)
            {
                tree.Add(inventory.name, inventory);
            }

            CurrentInventory = Inventory.CreatedInventories[0];
            
            return tree;
        }

        protected override void OnBeginDrawEditors()
        {
            if(CurrentInventory == null) return;

            bool isModified = false;
            foreach (var item in _ordered)
            {
                GUI.color = Color.white;
                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = TypeColor(item.type) * 2;
                GUILayout.Box(item.type.ToString(), GUILayout.Width(80));
                GUI.backgroundColor = subtypeColor(item.SubTypeString) * 2;
                GUILayout.Box(item.SubTypeString, GUILayout.Width(120));
                GUI.backgroundColor = Color.white;
                var content = new GUIContent(item.itemName, item.IconPreview as Texture);
                EditorGUILayout.LabelField(content, GUILayout.MaxWidth(400));
                if(item.isStackable)
                {
                    if (GUILayout.Button("-"))
                    {
                        var result = CurrentInventory.RemoveItem(item, 1, out var removed);
                        if (result == ItemEliminationResult.Empty) isModified = true;
                    }
                    GUILayout.Label(item.CurrentStack.ToString());
                    if (GUILayout.Button("+")) CurrentInventory.AddItem(item, 1);
                }
                else
                {
                    if (GUILayout.Button("Duplicate"))
                    {
                        _currentInventory.AddItem(item);
                        isModified = true;
                    }
                }

                if (item is IEquipment equipment)
                {
                    if(equipment.IsEquipped)
                    {
                        GUI.color = new Color(1, 0.5f, 0);
                        if (item is WeaponItem weaponItem)
                        {
                            GUILayout.Label(weaponItem.weaponEquipState == WeaponItem.WeaponEquipState.Left ?
                                "장착중 : 왼손" : "장착중 : 오른손");
                        }
                        else GUILayout.Label("장착중");
                        GUI.color = Color.white;
                    }
                }

                GUI.color = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(50)))
                {
                    CurrentInventory.RemoveSingleItem(item);
                    isModified = true;
                }
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
            }

            if (isModified)
            {
                Debug.Log("Refesh");
                var tmp = _currentInventory;
                _currentInventory = null;
                CurrentInventory = tmp;
            }
        }

        static Color TypeColor(ItemType type)
        {
            return type switch
            {
                ItemType.Generic => new Color(1f, 0.8f, 0.5f),
                ItemType.Armor => new Color(1f, 0.6f, 0f),
                ItemType.Weapon => new Color(0f, 0.5f, 1f),
                ItemType.Accessory => new Color(0.7f, 0.1f, 0.9f),
                ItemType.Important => new Color(0f, 1f, 0.5f),
                _ => Color.gray
            };
        }

        static Color subtypeColor(string subtype)
        {
            if (Enum.TryParse(subtype, out GenericType g))
            {
                return g switch
                {
                    GenericType.Material => new Color(0f, 0.4f, 0.5f),
                    GenericType.Consumable => new Color(0.5f, 0.2f, 0.2f),
                    GenericType.Ammo => new Color(0.6f, 0.4f, 0.3f),
                    _ => Color.gray
                };
            }

            if (Enum.TryParse(subtype, out ArmorType a))
            {
                return a switch
                {
                    ArmorType.Head => new Color(0f, 0.2f, 0.7f),
                    ArmorType.Torso => new Color(0.7f, 0.2f, 0.5f),
                    ArmorType.Arm => new Color(0.6f, 0.4f, 0f),
                    ArmorType.Leg => new Color(0.1f, 0.6f, 0.2f),
                    _ => Color.gray
                };
            }

            if (Enum.TryParse(subtype, out WeaponType w))
            {
                return w switch
                {
                    WeaponType.OneHandSword => new Color(0.7f, 0.5f, 0.5f),
                    WeaponType.Axe => new Color(0.5f, 0.7f, 0.5f),
                    WeaponType.Hammer => new Color(0.5f, 0.5f, 0.7f),
                    WeaponType.Staff => new Color(0.3f, 0.5f, 0.8f),
                    WeaponType.Dagger => new Color(0.6f, 0.6f, 0.5f),
                    WeaponType.Spear => new Color(0.6f, 0.5f, 0.6f),
                    WeaponType.Rapier => new Color(0.5f, 0.6f, 0.6f),
                    WeaponType.TwoHandSword => new Color(0.2f, 0.5f, 0.5f),
                    WeaponType.TwoHandAxe => new Color(0.5f, 0.2f, 0.2f),
                    WeaponType.TwoHandHammer => new Color(0.3f, 0.3f, 0.5f),
                    WeaponType.Scythe => new Color(0.3f, 0.5f, 0.3f),
                    WeaponType.Hallberd => new Color(0.5f, 0.3f, 0.3f),
                    WeaponType.Bow => new Color(0.8f, 0.5f, 0.2f),
                    _ => Color.gray
                };
            }

            if (Enum.TryParse(subtype, out AccessoryType ac))
            {
                return ac switch
                {
                    AccessoryType.Amulet => new Color(0.6f, 0.7f, 0.5f),
                    AccessoryType.Other => new Color(0.5f, 0.5f, 0.5f)
                };
            }

            if (Enum.TryParse(subtype, out ImportantItemType i))
            {
                return i switch
                {
                    ImportantItemType.Key => new Color(0.5f, 0.7f, 0.5f),
                    ImportantItemType.Book => new Color(0.5f, 0.5f, 0.7f),
                    ImportantItemType.Other => new Color(0.5f, 0.5f, 0.5f),
                    _ => Color.gray
                };
            }

            return new Color(0.4f, 0.4f, 0.4f);
        }
    }
}