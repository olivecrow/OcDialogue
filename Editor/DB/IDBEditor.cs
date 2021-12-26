using Sirenix.OdinInspector.Editor;

namespace OcDialogue.Editor
{
    public interface IDBEditor
    {
        OdinMenuEditorWindow Window { get; set; }
        void AddTreeMenu(OdinMenuTree tree);
        void DrawToolbar();
        void OnInspectorGUI();
    }
}