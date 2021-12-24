using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    [CustomEditor(typeof(ItemDatabase))]
    public class ItemDBEditor : OdinEditor, IDBEditor
    {
        ItemDatabase _target;
        protected override void OnEnable()
        {
            base.OnEnable();
            _target = target as ItemDatabase;
        }

        public void AddTreeMenu(OdinMenuTree tree)
        {
            foreach (var item in _target.Items)
            {
                tree.Add(item.itemName, item, item.IconPreview as Texture);
                // if (item.type == _itemType && item.SubTypeString == _itemSubType)
                // {
                //     tree.Add(item.itemName, item, item.IconPreview as Texture);
                // }
            }
        }

        public void DrawToolbarButtons()
        {
            
        }
    }
}