using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class ItemDatabaseEditor
    {
        DatabaseEditorWindow EditorWindow;
        public ItemType itemType;
        public string subTypeName;
        bool _rebuildRequest;
        public ItemDatabaseEditor(DatabaseEditorWindow editorWindow)
        {
            EditorWindow = editorWindow;
        }
        public void Draw(ItemBase selectedItem)
        {
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                GUI.backgroundColor = new Color(2f, 1.5f, 0.5f);
                GUI.contentColor = new Color(1f, 1f, 0.5f);
                var newType = (ItemType) GUILayout.Toolbar((int) itemType, Enum.GetNames(typeof(ItemType)), GUILayoutOptions.Height(25));
                if(itemType != newType) _rebuildRequest = true;
                itemType = newType;
                GUI.backgroundColor = Color.white;
                GUI.contentColor = Color.white;
            }
            SirenixEditorGUI.EndHorizontalToolbar();
            
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                GUI.backgroundColor = new Color(2f, 1.8f, 1f);;
                GUI.contentColor = Color.white;
                var subTypeNames = Enum.GetNames(ItemDatabase.GetSubType(itemType));
                var selectedIdx = subTypeNames.ToList().IndexOf(subTypeName);
                var idx = GUILayout.Toolbar(selectedIdx, subTypeNames);
                if (idx < 0) idx = 0;
                
                var newSubtype = subTypeNames[idx];
                if (subTypeName != newSubtype) _rebuildRequest = true;
                subTypeName = newSubtype;
                GUI.backgroundColor = Color.white;
                GUI.contentColor = Color.white;
            }
            SirenixEditorGUI.EndHorizontalToolbar();
            
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                if (SirenixEditorGUI.ToolbarButton("Refresh"))
                {
                    Refresh();
                }
                if (SirenixEditorGUI.ToolbarButton("Create"))
                {
                    var item = ItemDatabase.Instance.AddItem(itemType, subTypeName);
                    EditorWindow.TrySelectMenuItemWithObject(item);
                }
                if (SirenixEditorGUI.ToolbarButton("Duplicate"))
                {
                    if(selectedItem == null) return;
                    var item = ItemDatabase.Instance.AddItem(itemType, subTypeName);
                    EditorUtility.CopySerialized(selectedItem, item);
                    item.itemName += "_Copy";
                    EditorUtility.SetDirty(item);
                    Refresh();
                    AssetDatabase.SaveAssets();
                    
                    EditorWindow.TrySelectMenuItemWithObject(item);
                }
                if (SirenixEditorGUI.ToolbarButton("Delete"))
                {
                    if(selectedItem == null) return;
                    ItemDatabase.Instance.DeleteItem(selectedItem);
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
            
            if(_rebuildRequest)
            {
                EditorWindow.ForceMenuTreeRebuild();
                _rebuildRequest = false;
            }
        }
        
        void Refresh()
        {
            foreach (var item in ItemDatabase.Instance.Items)
            {
                if (item.itemName != item.name)
                {
                    item.name = item.itemName;
                }
            }
            AssetDatabase.SaveAssets();
            EditorWindow.ForceMenuTreeRebuild();
            _rebuildRequest = false;
        }
    }
}
