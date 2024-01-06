using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEngine.UIElements;

namespace OcDialogue.Editor
{
    public interface IDBEditor
    {
        OdinMenuEditorWindow Window { get; set; }
        void CreateTree(OdinMenuTree tree, List<OdinMenuItem> items);
        void DrawToolbar();
        void OnInspectorGUI();
        void AddDialogueContextualMenu(ContextualMenuPopulateEvent evt, DialogueGraphView graphView);
    }
}