using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Timeline;

namespace OcDialogue
{
    public class DataCheckEventTrigger : MonoBehaviour, IDialogueUser
    {
        public enum TriggerType
        {
            Manual,
            TriggerEnter
        }

        public SignalReceiver SignalReceiver => signalReceiver;
        public Conversation Conversation => conversation;

        public DataChecker checker;
        
        // conversation
        [BoxGroup("Conversation")][LabelWidth(150)]
        public bool startConversation;
        [HorizontalGroup("Conversation/1"), ShowIf("startConversation"), ValueDropdown("GetCategoryList")][LabelText("Conversation"), LabelWidth(150)] 
        public string category;
        [HorizontalGroup("Conversation/1"), ShowIf("startConversation"), ValueDropdown("GetConversationList")][HideLabel][OnValueChanged("CheckEventUsage")]
        public Conversation conversation;

        [InfoBox("이 대화에는 이벤트 리시버가 필요함.", InfoMessageType.Warning, "e_isReceiverNeed")]
        [BoxGroup("Conversation"), LabelWidth(150)][InlineButton("AddSignalReceiver", "Add")] public SignalReceiver signalReceiver;

        [BoxGroup("Conversation"), LabelWidth(150)] public DialogueDisplayMode dialogueDisplayMode;
        
        
        public UnityEvent e;

        [Button(ButtonSizes.Medium), GUIColor(0.5f, 1.5f, 0.5f)]
        public void TryInvoke()
        {
            if (!checker.IsTrue()) return;
            if (startConversation && conversation != null)
            {
                DialogueUI.StartConversation("DialogueUI Sample", conversation, this, dialogueDisplayMode);
            }
            e.Invoke();
        }

#if UNITY_EDITOR
        bool e_isReceiverNeed;
        ValueDropdownList<string> GetCategoryList()
        {
            var list = new ValueDropdownList<string>();
            foreach (var c in DialogueAsset.Instance.Categories)
            {
                list.Add(c);
            }

            return list;
        }
        ValueDropdownList<Conversation> GetConversationList()
        {
            var list = new ValueDropdownList<Conversation>();
            foreach (var conv in DialogueAsset.Instance.Conversations)
            {
                if(conv.Category == category) list.Add(conv);
            }

            return list;
        }

        void AddSignalReceiver()
        {
            if (signalReceiver != null)
            {
                Debug.LogWarning($"[{name}] 이미 리시버가 있음.");
                return;
            }

            signalReceiver = gameObject.AddComponent<SignalReceiver>();
        }

        void CheckEventUsage()
        {
            e_isReceiverNeed = false;
            if (conversation != null)
            {
                foreach (var balloon in conversation.Balloons)
                {
                    if (!balloon.useEvent) continue;
                    if(signalReceiver == null) e_isReceiverNeed = true;
                    break;
                }
            }
        }
#endif
    }
}
