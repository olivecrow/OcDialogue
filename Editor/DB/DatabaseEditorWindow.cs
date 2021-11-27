using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcDialogue.DB;
using OcUtility;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class DatabaseEditorWindow : OdinMenuEditorWindow
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
        UnityEditor.Editor _questDbEditor;
        UnityEditor.Editor _npcEditor;
        UnityEditor.Editor _enemyEditor;
        
        [MenuItem("OcDialogue/DB 에디터")]
        static void Open()
        {
            var wnd = GetWindow<DatabaseEditorWindow>("DB 에디터", true, typeof(SceneView));
            wnd.minSize = new Vector2(720, 480);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _questDbEditor = UnityEditor.Editor.CreateEditor(QuestDB.Instance);
            _npcEditor = UnityEditor.Editor.CreateEditor(NPCDB.Instance);
            _enemyEditor = UnityEditor.Editor.CreateEditor(EnemyDB.Instance);
        }

        protected override void OnBeginDrawEditors()
        {
            bool rebuildRequest = false;
            bool isDirty = false;
            int drawEnumToolbar(Color color, int currentValue, string[] names, int height, Texture2D[] icons = null)
            {
                GUI.backgroundColor = color;
                GUI.contentColor = color;
                var contents = new GUIContent[names.Length];
                int newValue = currentValue;
                for (int i = 0; i < names.Length; i++)
                {
                    contents[i] = new GUIContent(names[i], icons?[i]);
                }

                newValue = GUILayout.Toolbar(currentValue, contents, GUILayoutOptions.Height(height));
                if (currentValue != newValue) rebuildRequest = true;
                GUI.backgroundColor = Color.white;
                GUI.contentColor = Color.white;
                
                
                
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
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Category"));
                    if(EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
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
                    if(GameProcessDB.Instance == null) nullDBWarning();
                    break;
                case DBType.Item:
                {
                    Texture2D icon(string path) => Resources.Load<Texture2D>(path);
                    if (ItemDatabase.Instance == null)
                    {
                        nullDBWarning();
                        return;
                    }
                    _itemType = (ItemType) drawEnumToolbar(
                        new Color(2f, 1.5f, 0.5f), 
                        (int) _itemType, Enum.GetNames(typeof(ItemType)),
                        25,
                        new []{icon("GenericItem Icon"), icon("ArmorItem Icon"), icon("WeaponItem Icon"), icon("AccessoryItem Icon"), icon("ImportantItem Icon")});
                    
                    var subTypeNames = Enum.GetNames(ItemDatabase.GetSubType(_itemType));

                    if (_itemType != ItemType.Weapon)
                    {
                        var selectedIdx = subTypeNames.ToList().IndexOf(_itemSubType);
                        var idx = drawEnumToolbar(new Color(2f, 1.8f, 1f), selectedIdx, Enum.GetNames(ItemDatabase.GetSubType(_itemType)), 20);
                        if (idx < 0) idx = 0;
                        _itemSubType = subTypeNames[idx];
                    }
                    else
                    {
                        GUI.color = new Color(2f, 1.8f, 1f);
                        int selectedIdx = (int) WeaponType.OneHandSword;
                        if (Enum.TryParse(_itemSubType, out WeaponType weaponType))
                        {
                            selectedIdx = (int)weaponType;
                        }
                        if (selectedIdx < (int) WeaponType.OneHandSword) selectedIdx = (int) WeaponType.OneHandSword;
                        var selected = EditorGUILayout.EnumPopup((WeaponType)selectedIdx);
                        var newValue = ((WeaponType)selected).ToString();
                        if (_itemSubType != newValue) rebuildRequest = true;
                        _itemSubType = newValue;
                        GUI.color = Color.white;
                    }

                    SirenixEditorGUI.BeginHorizontalToolbar();
                    {
                        ItemBase selectedItem = MenuTree?.Selection?.SelectedValue as ItemBase;
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
                            ForceMenuTreeRebuild();
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
                    drawCategory(new Color(1f, 1.3f, 2f), QuestDB.Instance.Category, _questDbEditor.serializedObject);
                    
                    SirenixEditorGUI.BeginHorizontalToolbar();
                    {
                        Quest selectedQuest = MenuTree?.Selection?.SelectedValue as Quest;
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
                            rebuildRequest = true;
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
                    drawCategory(new Color(1.5f, 1f, 1.6f), NPCDB.Instance.Category, _npcEditor.serializedObject);

                    SirenixEditorGUI.BeginHorizontalToolbar();
                    {
                        NPC selectedNPC = MenuTree?.Selection?.SelectedValue as NPC;
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
                    drawCategory(new Color(1.8f, 1f, 1.4f), EnemyDB.Instance.Category, _enemyEditor.serializedObject);

                    SirenixEditorGUI.BeginHorizontalToolbar();
                {
                    Enemy selectedEnemy = MenuTree.Selection?.SelectedValue as Enemy;
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
                    tree.Add("GameProcess DB", GameProcessDB.Instance, CreateIcon(Color.green));
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
                        {
                            tree.Add(item.itemName, item, item.IconPreview as Texture);
                        }
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
