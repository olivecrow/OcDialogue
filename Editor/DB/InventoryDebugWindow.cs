using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class InventoryDebugWindow : OdinMenuEditorWindow
    {
        [MenuItem("OcDialogue/인벤토리 디버그 윈도우")]
        private static void ShowWindow()
        {
            var window = GetWindow<InventoryDebugWindow>();
            window.titleContent = new GUIContent("인벤토리 디버그 윈도우");
            window.Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();
            tree.Selection.SelectionConfirmed += selection =>
            {
                Selection.activeObject = selection.SelectedValue as UnityEngine.Object;
                EditorGUIUtility.PingObject(Selection.activeObject);
            };

            foreach (var inventory in Inventory.CreatedInventories)
            {
                tree.Add(inventory.name, inventory);
            }
            
            return tree;
        }
    }
}