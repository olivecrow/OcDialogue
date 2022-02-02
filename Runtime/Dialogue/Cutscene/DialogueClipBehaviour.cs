using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace OcDialogue.Cutscene
{
    [Serializable]
    public class DialogueClipBehaviour : PlayableBehaviour
    {
        public static event Action<DialogueClipBehaviour, Conversation, PlayableDirector> OnStart;
        public static event Action<DialogueClipBehaviour, Conversation, PlayableDirector> OnFadeOut;
        public static event Action<DialogueClipBehaviour, Conversation, PlayableDirector> OnEnd;
        public Balloon balloon;
        [ShowInInspector]public string subtitle => balloon == null ? "" : balloon.text;
        public DialogueTrack Track { get; private set; }
        public bool hasToPause;

        PlayableDirector director;
        Conversation conversation;
        DisplayParameter param => Track.param;
        bool clipPlayed;
        bool pauseScheduled;
        bool fadeOutInvoked;
        double __duration;
        double __time;
        double LeftTime => __duration - __time;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        static void Init()
        {
            Application.quitting += () =>
            {
                OnStart = null;
                OnEnd = null;
            };
        }
#endif
        

        public override void OnPlayableCreate(Playable playable)
        {
            director = playable.GetGraph().GetResolver() as PlayableDirector;
            Track = director.playableAsset.outputs.FirstOrDefault(x => x.sourceObject as DialogueTrack)
                .sourceObject as DialogueTrack;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if(!Application.isPlaying) return;
            if(!clipPlayed && info.weight > 0f)
            {
                if(hasToPause)
                {
                    pauseScheduled = true;
                }
                clipPlayed = true;
                __duration = playable.GetDuration();
                conversation = playerData as Conversation;
                OnStart?.Invoke(this, conversation, director);
            }

            __time = playable.GetTime();
            if (!fadeOutInvoked && LeftTime < param.textFadeOutDuration)
            {
                fadeOutInvoked = true;
                OnFadeOut?.Invoke(this, conversation, director);
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (pauseScheduled)
            {
                pauseScheduled = false;
                director.Pause();
            }
            else
            {
                if(clipPlayed)
                {
                    OnEnd?.Invoke(this, conversation, director);
                }
            }
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            clipPlayed = false;
        }
    }
}