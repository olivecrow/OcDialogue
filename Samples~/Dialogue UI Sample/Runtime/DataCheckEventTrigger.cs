using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue.DB;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace OcDialogue.Samples
{
    public class DataCheckEventTrigger : MonoBehaviour, IDialogueUser
    {
        public enum TriggerType
        {
            Manual,
            TriggerEnter,
            OnAwake
        }

        public TriggerType triggerType;
        public SignalReceiver SignalReceiver => signalReceiver;
        public Conversation Conversation => conversation;

        public DataChecker checker;
        
        // conversation
        [BoxGroup("Conversation")]
        [HorizontalGroup("Conversation/h")][LabelWidth(150)]
        public bool startConversation;

        [HorizontalGroup("Conversation/h")] [LabelWidth(50), LabelText("Scene")]
        public string dialogueSceneName;
        [HorizontalGroup("Conversation/1"), ShowIf("startConversation"), ValueDropdown("GetCategoryList")][LabelText("Conversation"), LabelWidth(150)] 
        public string category;
        [HorizontalGroup("Conversation/1"), ShowIf("startConversation"), ValueDropdown("GetConversationList")][HideLabel][OnValueChanged("CheckEventUsage")]
        public Conversation conversation;

        [BoxGroup("Conversation")] public UnityEvent OnDialogueEnd;
        
        [InfoBox("이 대화에는 이벤트 리시버가 필요함.", InfoMessageType.Warning, "e_isReceiverNeed")]
        [ShowIf("e_isReceiverNeed")]
        [BoxGroup("Conversation"), LabelWidth(150)][InlineButton("AddSignalReceiver", "Add")] 
        public SignalReceiver signalReceiver;

        public UnityEvent e;

        void Awake()
        {
            if(triggerType != TriggerType.OnAwake) return;
            TryInvoke();
        }

        void OnTriggerEnter(Collider other)
        {
            if(triggerType != TriggerType.TriggerEnter) return;
            TryInvoke();
        }

        [Button(ButtonSizes.Medium), GUIColor(0.5f, 1.5f, 0.5f)]
        public void TryInvoke()
        {
            if (!checker.IsTrue()) return;
            if (startConversation && conversation != null)
            {
                DialogueUI.StartConversation(dialogueSceneName, conversation, this, OnDialogueEnd.Invoke);
            }

            e.Invoke();
        }


#if UNITY_EDITOR
#pragma warning disable 414
        [HideInInspector]public bool e_isReceiverNeed;
#pragma warning restore 414
        void Reset()
        {
            dialogueSceneName = DialogueAsset.Instance.DefaultDialogueUISceneName;
        }

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
