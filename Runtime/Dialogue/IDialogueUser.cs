using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace OcDialogue
{
    public interface IDialogueUser
    {
        public SignalReceiver SignalReceiver { get; }
        public Conversation Conversation { get; }
    }
}
