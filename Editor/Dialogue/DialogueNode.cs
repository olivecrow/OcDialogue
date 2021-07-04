using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace OcDialogue.Editor
{
    public class DialogueNode : Node
    {
        public Balloon Balloon;
        public Port InputPort;
        public Port OutputPort;
        public Button AddNodeButton;
        public TextField TextField;
        public VisualElement CheckerIcon;
        public VisualElement SetterIcon;
        public VisualElement CheckerWarningIcon;
        public VisualElement SetterWarningIcon;

        public Action<Edge, int, int> OnEdgeConnected;
        public Action<Edge, int, int> OnEdgeDisconnected;

        public sealed override string title
        {
            get => base.title;
            set => base.title = value;
        }

        public DialogueNode(Balloon balloon)
        {
            Balloon = balloon;
            UpdateTitle();
            switch (Balloon.type)
            {
                case Balloon.Type.Entry:
                    capabilities = Capabilities.Selectable;
                    capabilities |= Capabilities.Snappable;
                    break;
                case Balloon.Type.Dialogue:
                    InputPort = GeneratePort(Direction.Input);
                    inputContainer.Add(InputPort);
                    break;
                case Balloon.Type.Choice:
                    InputPort = GeneratePort(Direction.Input);
                    inputContainer.Add(InputPort);
                    break;
                case Balloon.Type.Event:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (Balloon.type != Balloon.Type.Entry)
            {
                TextField = new TextField();
                TextField.bindingPath = "text";
                var so = new SerializedObject(balloon);
                TextField.Bind(so);
                TextField.multiline = true;
                mainContainer.Add(TextField);
            }
            
            OutputPort = GeneratePort(Direction.Output);
            outputContainer.Add(OutputPort);
            outputContainer.parent.style.height = 15;

            RefreshPorts();
            RefreshExpandedState();
        }

        Port GeneratePort(Direction portDirection)
        {
            var port = InstantiatePort(Orientation.Horizontal, portDirection, Port.Capacity.Multi, typeof(float));
            port.portName = "";
            return port;
        }

        public void UpdateTitle()
        {
            switch (Balloon.type)
            {
                case Balloon.Type.Entry:
                    title = "Entry";
                    titleContainer.style.backgroundColor = new Color(0.8f, 0.5f, 0f);
                    break;
                case Balloon.Type.Dialogue:
                    title = Balloon.actor == null ? "No Actor" : Balloon.actor.NPCName;
                    var color = Balloon.actor == null ? new Color(0.2f, 0.6f, 0.7f) : Balloon.actor.color;
                    color.a = 1;
                    titleContainer.style.backgroundColor =  color;
                    break;
                case Balloon.Type.Choice:
                    title = "Choice";
                    titleContainer.style.backgroundColor =  new Color(0f, 0f, 1f);
                    break;
                case Balloon.Type.Event:
                    title = "Event";
                    titleContainer.style.backgroundColor =  new Color(0.7f, 0.7f, 0.9f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var label = titleContainer.Q<Label>();
            var rgb = label.style.color.value;
            var rgbSum = rgb.r + rgb.g + rgb.b;
            label.style.color = rgbSum > 1.5f ? Color.black : Color.white;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            Selection.activeObject = Balloon;
        }
    }
}
