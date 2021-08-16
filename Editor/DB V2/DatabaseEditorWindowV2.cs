using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcDialogue.DB;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class DatabaseEditorWindowV2 : OdinMenuEditorWindow
    {
        public DBType DBType
        {
            get => _dbType;
            set
            {
                var isDirty = value != _dbType;
                _dbType = value;
                if(isDirty)ForceMenuTreeRebuild();
            }
        }
        DBType _dbType;
        ItemType _itemType;
        string _itemSubType;
        string _category;
        bool _isCategoryEditMode;
        [MenuItem("Window/OcDialogue/DB 에디터 v2")]
        static void Open()
        {
            var wnd = GetWindow<DatabaseEditorWindowV2>("DB 에디터 v2", true, typeof(SceneView));
            wnd.minSize = new Vector2(720, 480);
        }

        protected override void OnBeginDrawEditors()
        {
            bool rebuildRequest = false;
            bool isDirty = false;
            int drawEnumToolbar(Color color, int currentValue, string[] names, int height)
            {
                SirenixEditorGUI.BeginHorizontalToolbar();
                GUI.backgroundColor = color;
                GUI.contentColor = color;
                var newValue = GUILayout.Toolbar(currentValue, names, GUILayoutOptions.Height(height));
                if(currentValue != newValue) rebuildRequest = true;
                GUI.backgroundColor = Color.white; GUI.contentColor = Color.white;
                SirenixEditorGUI.EndHorizontalToolbar();
                return newValue;
            }
            void drawCategory(Color color, string[] categories, in SerializedObject serializedObject)
            {
                SirenixEditorGUI.BeginHorizontalToolbar();
                GUI.contentColor = color; GUI.backgroundColor = color;
                if(categories.Length > 0 && !_isCategoryEditMode)
                {
                    var categoryList = categories.ToList();
                    var currentIdx = categoryList.Contains(_category) ? categoryList.IndexOf(_category) : 0;

                    var idx = GUILayout.Toolbar(currentIdx, categories, GUILayoutOptions.Height(25));

                    if (_category != categories[idx]) rebuildRequest = true;
                    _category = categories[idx];
                }

                if (_isCategoryEditMode)
                {
                    GUI.enabled = true;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Category"));
                    serializedObject.ApplyModifiedProperties();
                    if (SirenixEditorGUI.ToolbarButton("Done")) _isCategoryEditMode = false;
                    GUI.enabled = false;
                }
                else
                {
                    if (SirenixEditorGUI.ToolbarButton("Edit")) _isCategoryEditMode = true;
                }
                GUI.contentColor = Color.white; GUI.backgroundColor = Color.white;
                SirenixEditorGUI.EndHorizontalToolbar();
            }
            void nullDBWarning()
            {
                GUILayout.Label("해당 데이터베이스가 없는듯?");
                GUILayout.Label("DB Manager에 등록이 안 되었는지 확인해볼것.");
            }

            GUI.enabled = !_isCategoryEditMode;
            DBType = (DBType) drawEnumToolbar(new Color(2f, 1f, 1f), (int)DBType, Enum.GetNames(typeof(DBType)), 40);

            switch (DBType)
            {
                case DBType.GameProcess:
                    if(GameProcessDB_V2.Instance == null) nullDBWarning();
                    break;
                case DBType.Item:
                {
                    if (ItemDatabase.Instance == null)
                    {
                        nullDBWarning();
                        return;
                    }
                    _itemType = (ItemType) drawEnumToolbar(new Color(2f, 1.5f, 0.5f), (int) _itemType, Enum.GetNames(typeof(ItemType)),
                        25);
                    var subTypeNames = Enum.GetNames(ItemDatabase.GetSubType(_itemType));
                    var selectedIdx = subTypeNames.ToList().IndexOf(_itemSubType);
                    var idx = drawEnumToolbar(new Color(2f, 1.8f, 1f), selectedIdx, Enum.GetNames(ItemDatabase.GetSubType(_itemType)), 20);
                    if (idx < 0) idx = 0;
                    _itemSubType = subTypeNames[idx];
                    
                    SirenixEditorGUI.BeginHorizontalToolbar();
                    {
                        ItemBase selectedItem = MenuTree.Selection?.SelectedValue as ItemBase;
                        if (SirenixEditorGUI.ToolbarButton("Create"))
                        {
                            var item = ItemDatabase.Instance.AddItem(_itemType, _itemSubType);
                            TrySelectMenuItemWithObject(item);
                        }
                        if (SirenixEditorGUI.ToolbarButton("Delete"))
                        {
                            if(selectedItem != null)
                            {
                                if(!EditorUtility.DisplayDialog("아이템 삭제", 
                                    $"해당 아이템을 삭제하시겠습니까?\n{selectedItem.itemName}", 
                                    "삭제", "취소")) return;
                                ItemDatabase.Instance.DeleteItem(selectedItem);
                            }
                        }
                        if (SirenixEditorGUI.ToolbarButton("Refresh"))
                        {
                            foreach (var item in ItemDatabase.Instance.Items)
                            {
                                if (item.itemName != item.name)
                                {
                                    item.name = item.itemName;
                                    EditorUtility.SetDirty(item);
                                    isDirty = true;
                                }
                            }
                        }
                        if (SirenixEditorGUI.ToolbarButton("Duplicate"))
                        {
                            if(selectedItem == null) return;
                            if (selectedItem.itemName != selectedItem.name)
                            {
                                selectedItem.name = selectedItem.itemName;
                                EditorUtility.SetDirty(selectedItem);
                            }
                            
                            var item = ItemDatabase.Instance.AddItem(_itemType, _itemSubType);
                            EditorUtility.CopySerialized(selectedItem, item);
                            item.itemName += "_Copy";
                            item.name = item.itemName;
                            EditorUtility.SetDirty(item);
                            isDirty = true;

                            TrySelectMenuItemWithObject(item);
                        }
                    }
                    SirenixEditorGUI.EndHorizontalToolbar();
                    
                    break;
                }
                case DBType.Quest:
                {
                    if(QuestDB.Instance == null)
                    {
                        nullDBWarning();
                        return;
                    }
                    drawCategory(new Color(1f, 1.3f, 2f), QuestDB.Instance.Category, UnityEditor.Editor.CreateEditor(QuestDB.Instance).serializedObject);
                    
                    SirenixEditorGUI.BeginHorizontalToolbar();
                    {
                        QuestV2 selectedQuest = MenuTree.Selection?.SelectedValue as QuestV2;
                        if (SirenixEditorGUI.ToolbarButton("Create"))
                        {
                            var item = QuestDB.Instance.AddQuest(_category);
                            TrySelectMenuItemWithObject(item);
                        }
                        if (SirenixEditorGUI.ToolbarButton("Delete"))
                        {
                            if(selectedQuest != null)
                            {
                                if(!EditorUtility.DisplayDialog("퀘스트 삭제", 
                                    $"해당 퀘스트를 삭제하시겠습니까?\n{selectedQuest.name}", 
                                    "삭제", "취소")) return;
                                QuestDB.Instance.DeleteQuest(selectedQuest.name);
                            }
                        }
                        if (SirenixEditorGUI.ToolbarButton("Resolve"))
                        {
                            QuestDB.Instance.Resolve();
                        }
                    }
                    SirenixEditorGUI.EndHorizontalToolbar();
                    break;
                }
                case DBType.NPC:
                    if (NPCDB.Instance == null)
                    {
                        nullDBWarning();
                        return;
                    }
                    drawCategory(new Color(1.5f, 1f, 1.6f), NPCDB.Instance.Category, UnityEditor.Editor.CreateEditor(NPCDB.Instance).serializedObject);

                    SirenixEditorGUI.BeginHorizontalToolbar();
                    {
                        NPCV2 selectedNPC = MenuTree.Selection?.SelectedValue as NPCV2;
                        if (SirenixEditorGUI.ToolbarButton("Create"))
                        {
                            var item = NPCDB.Instance.AddNPC(_category);
                            TrySelectMenuItemWithObject(item);
                        }
    
                        if (SirenixEditorGUI.ToolbarButton("Delete"))
                        {
                            if (selectedNPC != null)
                            {
                                if (!EditorUtility.DisplayDialog("퀘스트 삭제",
                                    $"해당 NPC를 삭제하시겠습니까?\n{selectedNPC.name}",
                                    "삭제", "취소")) return;
                                NPCDB.Instance.DeleteNPC(selectedNPC.name);
                            }
                        }
    
                        if (SirenixEditorGUI.ToolbarButton("Resolve"))
                        {
                            NPCDB.Instance.Resolve();
                        }
                    }   
                    SirenixEditorGUI.EndHorizontalToolbar();
                    break;
                case DBType.Enemy:
                    
                    if (NPCDB.Instance == null)
                    {
                        nullDBWarning();
                        return;
                    }
                    drawCategory(new Color(1.8f, 1f, 1.4f), EnemyDB.Instance.Category, UnityEditor.Editor.CreateEditor(EnemyDB.Instance).serializedObject);

                    SirenixEditorGUI.BeginHorizontalToolbar();
                {
                    EnemyV2 selectedEnemy = MenuTree.Selection?.SelectedValue as EnemyV2;
                    if (SirenixEditorGUI.ToolbarButton("Create"))
                    {
                        var item = EnemyDB.Instance.AddEnemy(_category);
                        TrySelectMenuItemWithObject(item);
                    }
    
                    if (SirenixEditorGUI.ToolbarButton("Delete"))
                    {
                        if (selectedEnemy != null)
                        {
                            if (!EditorUtility.DisplayDialog("Enemy 삭제",
                                $"해당 Enemy를 삭제하시겠습니까?\n{selectedEnemy.name}",
                                "삭제", "취소")) return;
                            EnemyDB.Instance.DeleteEnemy(selectedEnemy.name);
                        }
                    }
    
                    if (SirenixEditorGUI.ToolbarButton("Resolve"))
                    {
                        EnemyDB.Instance.Resolve();
                    }
                }   
                    SirenixEditorGUI.EndHorizontalToolbar();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if(rebuildRequest) ForceMenuTreeRebuild();
            if(isDirty) AssetDatabase.SaveAssets();
        }


        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();
            tree.Selection.SelectionConfirmed += selection =>
            {
                Selection.activeObject = selection.SelectedValue as UnityEngine.Object;
                EditorGUIUtility.PingObject(Selection.activeObject);
                _isCategoryEditMode = false;
            };
            MenuWidth = 250;

            switch (DBType)
            {
                case DBType.GameProcess:
                    tree.Add("GameProcess DB", GameProcessDB_V2.Instance, CreateIcon(Color.green));
                    break;
                case DBType.Item:
                    tree.Add("Item DB", ItemDatabase.Instance, CreateIcon(Color.green));
                    tree.Add("Inventory Editor Preset", ItemDatabase.Instance.editorPreset, CreateIcon(Color.yellow));
                    if (EditorApplication.isPlaying)
                    {
                        var serializer = new InventorySerializer(Inventory.PlayerInventory);
                        tree.Add("런타임 Player Inventory", serializer, CreateIcon(Color.red));
                        serializer.DrawItems(_itemType);
                    }

                    foreach (var item in ItemDatabase.Instance.Items)
                    {
                        if (item.type == _itemType && item.SubTypeString == _itemSubType)
                            tree.Add(item.itemName, item, item.editorIcon);
                    }
                    break;
                case DBType.Quest:
                    tree.Add("Quest DB", QuestDB.Instance, CreateIcon(Color.green));

                    if (!QuestDB.Instance.Category.Contains(_category)) _category = QuestDB.Instance.Category[0];
                    foreach (var quest in QuestDB.Instance.Quests)
                    {
                        if(quest.Category == _category) tree.Add(quest.name, quest);
                    }
                    break;
                case DBType.NPC:
                    tree.Add("NPC DB", NPCDB.Instance, CreateIcon(Color.green));
                    
                    if (!NPCDB.Instance.Category.Contains(_category)) _category = NPCDB.Instance.Category[0];
                    foreach (var npc in NPCDB.Instance.NPCs)
                    {
                        if(npc.Category == _category) tree.Add(npc.name, npc);
                    }
                    break;
                case DBType.Enemy:
                    tree.Add("Enemy DB", EnemyDB.Instance, CreateIcon(Color.green));
                    
                    if (!EnemyDB.Instance.Category.Contains(_category)) _category = EnemyDB.Instance.Category[0];
                    foreach (var enemy in EnemyDB.Instance.Enemies)
                    {
                        if(enemy.Category == _category) tree.Add(enemy.name, enemy);
                    }
                    break;
            }

            return tree;
        }
        
        Texture2D CreateIcon(Color color)
        {
            var i = new Texture2D(1, 1);
            i.SetPixel(0,0, color);
            i.Apply();
            return i;
        }
    }
}
