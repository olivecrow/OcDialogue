using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace OcDialogue.Cutscene
{
    [TrackColor(0.5f, 0.8f, 1f)]
    [TrackClipType(typeof(DialogueClip))]
    [TrackBindingType(typeof(Conversation))]
    public class DialogueTrack : TrackAsset, IDialogueUser
    {
        public string DialogueSceneName => UISceneName;
        public SignalReceiver SignalReceiver => _signalReceiver;
        public Conversation Conversation => conversation;
        public Conversation conversation;

        public string UISceneName = "Dialogue UI";
        public DisplayParameter param;
        public float MinimumClipDuration => 
            param == null ? 0f : param.textFadeInDuration + param.textFadeOutDuration;

        SignalReceiver _signalReceiver;
        TimelineClip _clip;

        protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
        {
            _signalReceiver = gameObject.GetComponent<SignalReceiver>();
            _clip = clip;
            return base.CreatePlayable(graph, gameObject, clip);
        }

#if UNITY_EDITOR
        public string e_directorSceneName;
        public string e_gaoName;
        void OnDestroy()
        {
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                if (conversation != null) conversation.e_CutsceneReference.Remove(this);
            }
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                e_directorSceneName = director.gameObject.scene.name;
                e_gaoName = director.gameObject.name;
                conversation = director.GetGenericBinding(this) as Conversation;
                if(conversation != null)
                {
                    if (!conversation.e_CutsceneReference.Contains(this))
                    {
                        conversation.e_CutsceneReference.Add(this);
                        UnityEditor.EditorUtility.SetDirty(conversation);
                        UnityEditor.AssetDatabase.SaveAssetIfDirty(conversation);
                    }
                }
            }
            base.GatherProperties(director, driver);
        }
#endif
        
    }
}