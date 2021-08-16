using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
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
        ItemDatabaseToolbarDrawer _itemDatabaseToolbarDrawer;
        QuestDatabaseToolbarDrawer _questDatabaseToolbarDrawer;
        NPCDatabaseToolbarDrawer _npcDatabaseToolbarDrawer;
        EnemyDatabaseToolbarDrawer _enemyDatabaseToolbarDrawer;
        Texture2D _yellowIcon, _greenIcon, _redIcon;
        InventorySerializer _playerInventorySerializer;
        [MenuItem("Tools/데이터베이스 에디터")]
        static void Open()
        {
            var window = GetWindow<DatabaseEditorWindow>();
            window.Show();
        }

        void Awake()
        {
            ResizableMenuWidth = false;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            CreateToolbarDrawers();
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        void OnPlayModeChanged(PlayModeStateChange change)
        {
            switch (change)
            {
                case PlayModeStateChange.EnteredEditMode:
                    EditorApplication.delayCall += ForceMenuTreeRebuild;
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    EditorApplication.delayCall += ForceMenuTreeRebuild;
                    break;
            }
        }

        protected override void OnBeginDrawEditors()
        {
            CreateToolbarDrawers();
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                GUI.backgroundColor = new Color(2f, 1f, 0f);
                GUI.contentColor = Color.yellow;
                DBType = (DBType) GUILayout.Toolbar((int) DBType, Enum.GetNames(typeof(DBType)), GUILayoutOptions.Height(40));
                GUI.backgroundColor = Color.white;
                GUI.contentColor = Color.white;
            }
            SirenixEditorGUI.EndHorizontalToolbar();

            void nullDBWarning()
            {
                GUI.Label(EditorGUILayout.GetControlRect(), "해당 데이터베이스가 없는듯?");
                GUI.Label(EditorGUILayout.GetControlRect(), "DB Manager에 등록이 안 되었는지 확인해볼것.");
            }
            
            switch (DBType)
            {
                case DBType.GameProcess:
                    if(GameProcessDatabase.Instance == null)
                    {
                        nullDBWarning();
                        return;
                    }
                    break;
                case DBType.Item:
                {
                    if(ItemDatabase.Instance == null)
                    {
                        nullDBWarning();
                        return;
                    }
                    
                    _itemDatabaseToolbarDrawer.Draw(MenuTree?.Selection?.SelectedValue as ItemBase);
                    break;
                }
                case DBType.Quest:
                {
                    if (QuestDatabase.Instance == null)
                    {
                        nullDBWarning();
                        return;
                    }
                    _questDatabaseToolbarDrawer.Draw();
                    if (MenuTree?.Selection?.SelectedValue is QuestEditorPreset preset)
                    {
                        preset.tmpList = 
                            QuestDatabase.Instance.editorPreset.Quests
                                .Where(x => x.quest.Category == _questDatabaseToolbarDrawer.CurrentCategory)
                                .ToList();
                    }
                    break;
                }
                case DBType.NPC:
                    if (NPCDatabase.Instance == null)
                    {
                        nullDBWarning();
                        return;
                    }
                    _npcDatabaseToolbarDrawer.Draw();
                    break;
                case DBType.Enemy:
                    if (EnemyDatabase.Instance == null)
                    {
                        nullDBWarning();
                        return;
                    }
                    _enemyDatabaseToolbarDrawer.Draw();
                    break;
            }
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            CreateToolbarDrawers();
            var tree = new OdinMenuTree();
            tree.Selection.SelectionConfirmed += selection =>
            {
                Selection.activeObject = selection.SelectedValue as UnityEngine.Object;
                EditorGUIUtility.PingObject(Selection.activeObject);
            };
            MenuWidth = 250;

            _yellowIcon = CreateIcon(Color.yellow);
            _greenIcon = CreateIcon(Color.green);
            _redIcon = CreateIcon(Color.red);

            switch (DBType)
            {
                case DBType.GameProcess:
                    
                    tree.Add("GameProcess DB", GameProcessDatabase.Instance, _greenIcon);
                    tree.Add("GameProcess Editor Preset", GameProcessDatabase.Instance.editorPreset, _yellowIcon);
                    if(Application.isPlaying) tree.Add("런타임 GameProcess Data", GameProcessDatabase.Runtime, _redIcon);
                    break;
                case DBType.Item:
                    
                    tree.Add("Item DB", ItemDatabase.Instance, _greenIcon);
                    tree.Add("Inventory Editor Preset", ItemDatabase.Instance.editorPreset, _yellowIcon);
                    if(Application.isPlaying)
                    {
                        tree.Add("런타임 Player Inventory", _playerInventorySerializer, _redIcon);
                        _playerInventorySerializer.DrawItems(_itemDatabaseToolbarDrawer.itemType);
                    }
                    
                    foreach (var itemBase in ItemDatabase.Instance.Items)
                    {
                        if(itemBase.SubTypeString != _itemDatabaseToolbarDrawer.subTypeName) continue;
                        tree.Add(itemBase.itemName, itemBase, itemBase.editorIcon); 
                    }
                    break;
                case DBType.Quest:
                    
                    tree.Add("Quest DB", QuestDatabase.Instance, _greenIcon);
                    tree.Add("Quest Editor Preset", QuestDatabase.Instance.editorPreset, _yellowIcon);
                    if(Application.isPlaying) tree.Add("런타임 Quest DB", QuestDatabase.Runtime, _redIcon);
                    
                    foreach (var quest in QuestDatabase.Instance.Quests)
                    {
                        if(quest.Category == _questDatabaseToolbarDrawer.CurrentCategory) tree.Add(quest.key, quest);
                    }
                    break;
                case DBType.NPC:
                    
                    tree.Add("NPC DB", NPCDatabase.Instance, _greenIcon);
                    tree.Add("NPC Editor Preset", NPCDatabase.Instance.editorPreset, _yellowIcon);
                    if(Application.isPlaying) tree.Add("런타임 NPC DB", NPCDatabase.Runtime, _redIcon);

                    foreach (var npc in NPCDatabase.Instance.NPCs)
                    {
                        if(npc.Category == _npcDatabaseToolbarDrawer.CurrentCategory) tree.Add(npc.NPCName, npc);
                    }
                    break;
                case DBType.Enemy:
                    tree.Add("Enemy DB", EnemyDatabase.Instance, _greenIcon);
                    tree.Add("Enemy Editor Preset", EnemyDatabase.Instance.editorPreset, _yellowIcon);
                    if(Application.isPlaying) tree.Add("런타임 Enemy DB", EnemyDatabase.Runtime, _redIcon);
                    
                    foreach (var enemy in EnemyDatabase.Instance.Enemies)
                    {
                        if(enemy.Category == _enemyDatabaseToolbarDrawer.CurrentCategory) tree.Add(enemy.key, enemy);
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
        void CreateToolbarDrawers()
        {
            if(_itemDatabaseToolbarDrawer == null) _itemDatabaseToolbarDrawer =  new ItemDatabaseToolbarDrawer(this);
            if(_questDatabaseToolbarDrawer == null) _questDatabaseToolbarDrawer = new QuestDatabaseToolbarDrawer(this);
            if (_npcDatabaseToolbarDrawer == null) _npcDatabaseToolbarDrawer = new NPCDatabaseToolbarDrawer(this);
            if (_enemyDatabaseToolbarDrawer == null) _enemyDatabaseToolbarDrawer = new EnemyDatabaseToolbarDrawer(this);
            
            _playerInventorySerializer = new InventorySerializer(Inventory.PlayerInventory);
        }
    }
}
