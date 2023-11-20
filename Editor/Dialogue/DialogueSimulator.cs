using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

namespace OcDialogue.Editor
{
    [Serializable]
    public class DialogueSimulator : EditorWindow
    {
        public Conversation _conv;
        public Balloon _entry;
        public Balloon _balloon;
        public PopupField<Balloon> _entries;
        public TextField _textField;
        public TextField _noteField;
        public RadioButtonGroup _choiceGroup;

        Button _nextButton;
        Button _restartButton;
        public List<SignalAsset> _signals = new List<SignalAsset>();
        public List<Balloon> _choices;
        int _cycleIndex;
        [MenuItem("OcDialogue/Dialogue Simulator")]
        private static void ShowWindow()
        {
            var window = GetWindow<DialogueSimulator>();
            window.titleContent = new GUIContent("Dialogue Simulator");
            window.Show();
        }


        private void CreateGUI()
        {
            var objField = new ObjectField("Conversation");
            objField.objectType = typeof(Conversation);
            objField.RegisterValueChangedCallback(evt => InitializeConv(evt.newValue as Conversation));
            
            rootVisualElement.Add(objField);

            _entries = new PopupField<Balloon>();
            _entries.formatListItemCallback += balloon =>
                balloon.type == Balloon.Type.Entry ? "Default" : balloon.subEntryTrigger;
            _entries.RegisterValueChangedCallback(evt => InitializeEntry(evt.newValue));

            rootVisualElement.Add(_entries);
            
            _noteField = new TextField();
            _noteField.multiline = true;
            rootVisualElement.Add(_noteField);
            
            _textField = new TextField();
            _textField.multiline = true;
            _textField.label = "text";
            rootVisualElement.Add(_textField);
            
            rootVisualElement.Add(new VisualElement(){style = { height = 20}});

            _choiceGroup = new RadioButtonGroup("Choices");
            rootVisualElement.Add(_choiceGroup);

            rootVisualElement.Add(new VisualElement(){style = { height = 50}});
            
            _nextButton = new Button(Next);
            _nextButton.text = "Next";
            rootVisualElement.Add(_nextButton);

            _restartButton = new Button(Restart);
            _restartButton.text = "Restart";
            rootVisualElement.Add(_restartButton);
         
            
            
            
            if(_conv != null)
            {
                InitializeConv(_conv);
                objField.SetValueWithoutNotify(_conv);
            }
            if(_entry != null)
            {
                InitializeEntry(_entry);
                _entries.SetValueWithoutNotify(_entry);
            }
            if(_balloon != null) UpdateBalloon(_balloon);
        }

        void InitializeConv(Conversation conv)
        {
            _conv = conv;
            if (conv == null)
            {
                _entries.choices = null;
            }
            else
            {
                _entries.choices = conv.Balloons.Where(x =>
                    x.type == Balloon.Type.Entry ||
                    (x.type == Balloon.Type.Action && x.actionType == Balloon.ActionType.SubEntry)).ToList();
            }
            _textField.value = "";
            _noteField.value = "";
        }

        void InitializeEntry(Balloon entryBalloon)
        {
            _entry = entryBalloon;
            _balloon = entryBalloon;
            _noteField.value = _balloon.description;
            UpdateBalloon(_balloon);

            _choices = _balloon.linkedBalloons.Where(x => x.type == Balloon.Type.Choice && x.IsAvailable).ToList();
            _choiceGroup.choices = _choices.Select(x => x.text);
        }


        void Next()
        {
            var next = _choices.Count == 0 ?
                _balloon.GetNext(_choices, ref _cycleIndex) :
                _choices[_choiceGroup.value].GetNext(_choices, ref _cycleIndex);
            
            if (_choices != null && _choices.Count > 0)
            {
                _choiceGroup.choices = _choices.Select(x => x.text);
            }
            else
            {
                _choiceGroup.choices = null;
            }

            _balloon = next;
            if(_balloon != null)_balloon.UseBalloon(null);
            UpdateBalloon(_balloon);
        }

        void Restart()
        {
            InitializeEntry(_entry);
        }

        void UpdateBalloon(Balloon balloon)
        {
            if (balloon == null)
            {
                _textField.value = "더이상 없음";
                _noteField.value = "";
                return;
            }
            _balloon = balloon;
            _textField.value = _balloon.text;

            _noteField.value =
                $"type : {balloon.type}\n" +
                $"signal : {(balloon.signal == null ? "" : balloon.signal.name)}\n" +
                $"cycleIndex : {_cycleIndex}";
        }
    }
}