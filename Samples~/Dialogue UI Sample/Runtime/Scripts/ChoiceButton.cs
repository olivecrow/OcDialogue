using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OcDialogue.Samples
{
    public class ChoiceButton : Button
    {
        public Balloon ChoiceBalloon { get; private set; }

        public string Text
        {
            get => TMP.text;
            set => TMP.text = value;
        }
        public TextMeshProUGUI TMP;
        public void SetChoice(Balloon balloon)
        {
            ChoiceBalloon = balloon;
            Text = balloon.text;
        }
    }
}