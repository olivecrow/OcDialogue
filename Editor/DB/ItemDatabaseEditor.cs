using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class ItemDatabaseEditor
    {
        event Action Rebuild;

        bool _rebuildRequest;
        public ItemDatabaseEditor(Action RebuildEvent)
        {
            Rebuild += RebuildEvent;
        }
        public void Draw(ref ItemType itemType, ref string subTypeName, ItemBase selectedItem)
        {
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                var newType = (ItemType) GUILayout.Toolbar((int) itemType, Enum.GetNames(typeof(ItemType)));
                if(itemType != newType) _rebuildRequest = true;
                itemType = newType;
            }
            SirenixEditorGUI.EndHorizontalToolbar();
            
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                var subTypeNames = Enum.GetNames(ItemDatabase.GetSubType(itemType));
                var selectedIdx = subTypeNames.ToList().IndexOf(subTypeName);
                var idx = GUILayout.Toolbar(selectedIdx, subTypeNames);
                if (idx < 0) idx = 0;
                
                var newSubtype = subTypeNames[idx];
                if (subTypeName != newSubtype) _rebuildRequest = true;
                subTypeName = newSubtype;
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
                    ItemDatabase.Instance.AddItem(itemType, subTypeName);
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
                Rebuild?.Invoke();
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
            Rebuild?.Invoke();
        }
    }
}
