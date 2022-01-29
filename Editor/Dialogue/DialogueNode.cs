using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcUtility;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace OcDialogue.Editor
{
    public sealed class DialogueNode : Node
    {
        public Balloon Balloon;
        public Port InputPort;
        public Port OutputPort;
        public Label TextField;
        public VisualElement CheckerIcon;
        public VisualElement SetterIcon;
        public VisualElement EventIcon;
        public VisualElement ImageIcon;
        public VisualElement WarningIcon;
        public Label Description;

        public Action<Edge, int, int> OnEdgeConnected;
        public Action<Edge, int, int> OnEdgeDisconnected;

        const int Number_Of_Subtitle_Text = 15;
        
        public sealed override string title
        {
            get => base.title;
            set => base.title = value;
        }

        public DialogueNode(Balloon balloon)
        {
            Balloon = balloon;
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
                case Balloon.Type.Action:
                    InputPort = GeneratePort(Direction.Input);
                    inputContainer.Add(InputPort);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (Balloon.type != Balloon.Type.Entry)
            {
                TextField = new Label();
                mainContainer.Add(TextField);

                Description = new Label();
                Description.style.color = new Color(0, 1f, 0f, 0.5f);

                style.maxWidth = 160;
                
                Add(Description);
                CreateIcons();
            }
            
            OutputPort = GeneratePort(Direction.Output);
            outputContainer.Add(OutputPort);
            outputContainer.parent.style.height = 15;

            RefreshTitle();
            RefreshSubtitleLabel();
            RefreshDescription();
            RefreshPorts();
            RefreshExpandedState();
        }

        void CreateIcons()
        {
            var iconBar = new VisualElement();
            iconBar.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            titleContainer.parent.Insert(0,iconBar);
            
            CheckerIcon = CreateIcon(Resources.Load<Texture2D>("Checker Icon"));
            iconBar.Add(CheckerIcon);
            
            SetterIcon = CreateIcon(Resources.Load<Texture2D>("Setter Icon"));
            iconBar.Add(SetterIcon);

            
            EventIcon = CreateIcon(Resources.Load<Texture2D>("Event Icon"));
            iconBar.Add(EventIcon);
            
            ImageIcon = CreateIcon(Resources.Load<Texture2D>("Image Icon"));
            iconBar.Add(ImageIcon);

            WarningIcon = CreateIcon(Resources.Load<Texture2D>("Warning Icon"));
            titleContainer.Add(WarningIcon);
            
            RefreshIcons();
        }

        static VisualElement CreateIcon(Texture2D texture)
        {
            var icon = new VisualElement();
            icon.style.width = texture.width;
            icon.style.height = texture.height;
            icon.style.backgroundImage = texture;
            return icon;
        }


        Port GeneratePort(Direction portDirection)
        {
            var port = InstantiatePort(Orientation.Horizontal, portDirection, Port.Capacity.Multi, typeof(float));
            port.portName = "";
            return port;
        }

        public void RefreshTitle()
        {
            switch (Balloon.type)
            {
                case Balloon.Type.Entry:
                    title = "Entry";
                    titleContainer.style.backgroundColor = new Color(0.8f, 0.5f, 0f);
                    break;
                case Balloon.Type.Dialogue:
                    title = Balloon.actor == null ? "No Actor" : Balloon.actor.name;
                    var color = Balloon.actor == null ? new Color(0.2f, 0.6f, 0.7f) : Balloon.actor.color;
                    color.a = 1;
                    titleContainer.style.backgroundColor =  color;
                    break;
                case Balloon.Type.Choice:
                    title = "Choice";
                    titleContainer.style.backgroundColor =  new Color(0f, 0f, 1f);
                    break;
                case Balloon.Type.Action:
                    title = "Action";
                    titleContainer.style.backgroundColor =  new Color(1f, 1f, 0f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var label = titleContainer.Q<Label>();
            var rgb = titleContainer.style.backgroundColor.value;
            var rgbSum = rgb.r + rgb.g + rgb.b;
            label.style.color = rgbSum > 1.5f ? Color.black : Color.white;
            if(WarningIcon != null) WarningIcon.style.unityBackgroundImageTintColor = rgbSum > 1.5f ? Color.red : Color.yellow;
        }

        public void RefreshSubtitleLabel()
        {
            if(Balloon.type == Balloon.Type.Entry) return;
            TextField.text = Balloon.text.Length > Number_Of_Subtitle_Text ? 
                $"{Balloon.text.Substring(0, Number_Of_Subtitle_Text)}..." : Balloon.text;
        }

        public void RefreshDescription()
        {
            if(Balloon.type == Balloon.Type.Entry) return;
            Description.text = Balloon.description;
        }

        public void RefreshIcons()
        {
            if(Balloon.type == Balloon.Type.Entry) return;
            CheckerIcon.SetVisible(Balloon.useChecker);
            SetterIcon.SetVisible(Balloon.useSetter);
            EventIcon.SetVisible(Balloon.useEvent);
            ImageIcon.SetVisible(Balloon.useImage);
            WarningIcon.SetVisible(Balloon.IsWarningOn());
        }

        public override void OnSelected()
        {
            base.OnSelected();
            DialogueEditorWindow.Instance.GraphView.OnSelectionChanged();
        }
    }
}
