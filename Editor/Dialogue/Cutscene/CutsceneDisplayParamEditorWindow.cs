using System;
using OcDialogue.Cutscene;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor.Cutscene
{
    public class CutsceneDisplayParamEditorWindow : EditorWindow
    {
        UnityEditor.Editor _paramEditor;
        [MenuItem("OcDialogue/Cutscene Display Parameter")]
        static void Open()
        {
            var window = GetWindow<CutsceneDisplayParamEditorWindow>("Cutscene Display Param");
            window.minSize = new Vector2(480, 520);
            window.Show();
        }

        void OnEnable()
        {
            _paramEditor = UnityEditor.Editor.CreateEditor(DialogueDisplayParameter.Instance);
        }

        void OnGUI()
        {
            _paramEditor.OnInspectorGUI();
        }
    }
}