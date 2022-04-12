using System;
using System.Collections.Generic;
using System.Linq;
using OcUtility;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace OcDialogue.Cutscene
{
    public class CutsceneBehaviour : MonoBehaviour, IDialogueUser
    {
        public static bool IsCutscenePlaying => _isCutscenePlaying;
        public static bool IsCutscenePaused => _isCutscenePaused;
        public static CutsceneBehaviour ActiveCutscene => _activeCutscene;
        public static event Action<CutsceneBehaviour> OnCutsceneStart;
        public static event Action<CutsceneBehaviour> OnCutsceneEnd;
        
        public static event Action<DialogueClipBehaviour> OnClipStart;
        public static event Action<DialogueClipBehaviour> OnClipFadeOut;
        public static event Action<DialogueClipBehaviour> OnClipEnd;

        static bool _isCutscenePlaying;
        static bool _isCutscenePaused;
        static CutsceneBehaviour _activeCutscene;

        public string DialogueSceneName => dialogueSceneName;
        public SignalReceiver SignalReceiver => signalReceiver;
        public Conversation Conversation => _dialogueTrack.Conversation;
        [FoldoutGroup("Dialogue")]public string dialogueSceneName = "Dialogue UI";
        [FoldoutGroup("Dialogue")][Range(0.5f, 0.99f)]public double autoPauseNormalizedTime = 0.95;
        [FoldoutGroup("Dialogue")][Range(0, 5)]public double balloonFadeOutDuration = 1;
        [InfoBox("이 대화에는 SignalReceiver가 필요함", InfoMessageType.Error, VisibleIf = "IsSignalReceiverRequired")]
        public SignalReceiver signalReceiver;

        public PlayableDirector director;
        
        [FoldoutGroup("OnAwake")] public UnityEvent OnAwake;
        [FoldoutGroup("OnPlay")] public UnityEvent OnPlay;
        [FoldoutGroup("OnEnd")] public UnityEvent OnEnd;

        public DialogueTrack DialogueTrack => _dialogueTrack;
        DialogueTrack _dialogueTrack;
        protected virtual void Awake()
        {
            OnAwake?.Invoke();
            
            _dialogueTrack = director.playableAsset.outputs
                .FirstOrDefault(x => x.outputTargetType == typeof(Conversation))
                .sourceObject as DialogueTrack;
            _dialogueTrack?.Init(this,
                dialogueClipBehaviour => OnClipStart?.Invoke(dialogueClipBehaviour), 
                dialogueClipBehaviour => OnClipFadeOut?.Invoke(dialogueClipBehaviour), 
                dialogueClipBehaviour => OnClipEnd?.Invoke(dialogueClipBehaviour));

            director.stopped += playableDirector => InternalEnd();
        }

        void InternalEnd()
        {
            PreEnd();
            DialogueTrack.Release();
            OnEnd.Invoke();
            _isCutscenePlaying = false;
            _isCutscenePaused = false;
            _activeCutscene = null;
            PostEnd();
        }


        [EnableIf("@EditorApplication.isPlaying")][Button]
        public void Play()
        {
            PrePlay();
            _isCutscenePlaying = true;
            _activeCutscene = this;
            director.Play();
            OnPlay.Invoke();
            OnCutsceneStart?.Invoke(this);
            PostPlay();
        }

        public static void Pause()
        {
            if (ActiveCutscene == null)
            {
                Printer.Print($"현재 재생중인 컷씬이 없음", LogType.Warning);
                return;
            }
            // ActiveCutscene.director.Pause() 말고 Speed 를 0으로 해서 쓰기.
            ActiveCutscene.director.playableGraph.GetRootPlayable(0).SetSpeed(0);
            _isCutscenePaused = true;
        }

        public static void Resume()
        {
            if (ActiveCutscene == null)
            {
                Printer.Print($"현재 재생중인 컷씬이 없음", LogType.Warning);
                return;
            }
            // ActiveCutscene.director.Resume() 말고 Speed를 1로 해서 쓰기.
            ActiveCutscene.director.playableGraph.GetRootPlayable(0).SetSpeed(1);
            _isCutscenePaused = false;
        }

        public static void SkipToNextClip()
        {
            if (ActiveCutscene == null)
            {
                Printer.Print($"현재 재생중인 컷씬이 없음", LogType.Warning);
                return;
            }
            
            var currentClipIndex = ActiveCutscene.DialogueTrack.GetCurrentClipIndex();

            if (currentClipIndex + 1 > ActiveCutscene.DialogueTrack.ClipCount - 1)
            {
                Stop();
            }
            else
            {
                ActiveCutscene.director.time = ActiveCutscene.DialogueTrack.GetClipStartTime(currentClipIndex + 1);
                Resume();
            }
        }

        public static void Stop()
        {
            if (ActiveCutscene == null)
            {
                Printer.Print($"현재 재생중인 컷씬이 없음", LogType.Warning);
                return;
            }
            ActiveCutscene.director.Stop();
        }

        protected virtual void PrePlay(){}
        protected virtual void PostPlay(){}
        protected virtual void PreEnd(){}
        protected virtual void PostEnd(){}

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        static void Init()
        {
            Application.quitting += Release;
        }

        static void Release()
        {
            OnCutsceneStart = null;
            OnCutsceneEnd = null;
            
            OnClipStart = null;
            OnClipFadeOut = null;
            OnClipEnd = null;
            _isCutscenePlaying = false;
            _activeCutscene = null;
            

            Application.quitting -= Release;
        }

        void Reset()
        {
            director = GetComponent<PlayableDirector>();
        }

        bool IsSignalReceiverRequired()
        {
            if (director == null) return false;
            if(_dialogueTrack == null) _dialogueTrack = director.playableAsset.outputs
                .FirstOrDefault(x => x.outputTargetType == typeof(Conversation))
                .sourceObject as DialogueTrack;
            if (_dialogueTrack == null) return false;
            if (Conversation == null) return false;
            if (signalReceiver != null) return false;
            foreach (var balloon in Conversation.Balloons)
            {
                if (!balloon.useEvent) continue;
                if (signalReceiver == null) return true;
                break;
            }

            return false;
        }
#endif
    }
}