using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Node = UnityEditor.Graphs.Node;

namespace OcDialogue.Editor
{
    public class DialogueNode : Node
    {
        public Balloon Balloon;
        public Port InputPort;
        public Port OutputPort;
        public Button CreateNodeButton;
        public TextField TextField;
        public VisualElement CheckerIcon;
        public VisualElement SetterIcon;
        public VisualElement CheckerWarningIcon;
        public VisualElement SetterWarningIcon;
        
    }
}
