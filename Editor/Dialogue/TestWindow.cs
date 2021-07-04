using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace OcDialogue.Editor
{
    public class TestWindow : EditorWindow
    {
        [MenuItem("Tools/Test Window")]
        static void Open()
        {
            GetWindow<TestWindow>();
        }
        void CreateGUI()
        {
            var gv = new DialogueGraphView(DialogueAsset.Instance.Conversations[0]);
            rootVisualElement.Add(gv);
            gv.StretchToParentSize();
        }
    }
}
