using System;
using System.Collections.Generic;
using System.Linq;
using OcUtility;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace OcDialogue.Cutscene
{
    public class CutsceneBehaviour : MonoBehaviour, IDialogueUser
    {
        public static bool IsCutscenePlaying { get; private set; }

        public static bool IsCutscenePaused { get; private set; }

        public virtual bool IsSkipToEndAvailable
        {
            get
            {
                if (director.state != PlayState.Playing) return false;
                if (!IsCutscenePlaying) return false;
                if (ActiveCutscene.director.time > 10)
                {
                    if (director.time > director.duration - 3) return false;
                }
                else
                {
                    if (director.time > director.duration * 0.95) return false;
                }

                return true;
            }
        }
        public static CutsceneBehaviour ActiveCutscene { get; protected set; }
        public static event Action<CutsceneBehaviour> OnCutsceneStart;
        public static event Action<CutsceneBehaviour> OnCutsceneEnd;
        
        public static event Action<DialogueClipBehaviour> OnClipStart;
        public static event Action<DialogueClipBehaviour> OnClipFadeOut;
        public static event Action<DialogueClipBehaviour> OnClipEnd;

        public string DialogueSceneName => dialogueSceneName;
        public SignalReceiver SignalReceiver => signalReceiver;
        public Conversation Conversation => DialogueTrack.Conversation;
        [FoldoutGroup("Dialogue")]public string dialogueSceneName = "Dialogue UI";
        [FoldoutGroup("Dialogue")][Range(0.5f, 0.99f)]public double autoPauseNormalizedTime = 0.95;
        [FoldoutGroup("Dialogue")][Range(0, 5)]public double balloonFadeOutDuration = 1;
        [InfoBox("이 대화에는 SignalReceiver가 필요함", InfoMessageType.Error, VisibleIf = "IsSignalReceiverRequired")]
        public SignalReceiver signalReceiver;

        public PlayableDirector director;
        
        [FoldoutGroup("OnAwake")] public UnityEvent OnAwake;
        [FoldoutGroup("OnPlay")] public UnityEvent OnPlay;
        [FoldoutGroup("OnEnd")] public UnityEvent OnEnd;

        public DialogueTrack DialogueTrack { get; private set; }

        protected virtual void Awake()
        {
            OnAwake?.Invoke();
            
            DialogueTrack = director.playableAsset.outputs
                .FirstOrDefault(x => x.outputTargetType == typeof(Conversation))
                .sourceObject as DialogueTrack;
            if(DialogueTrack != null) DialogueTrack.Init(this,
                dialogueClipBehaviour => OnClipStart?.Invoke(dialogueClipBehaviour), 
                dialogueClipBehaviour => OnClipFadeOut?.Invoke(dialogueClipBehaviour), 
                dialogueClipBehaviour => OnClipEnd?.Invoke(dialogueClipBehaviour));

            director.stopped += playableDirector => InternalEnd();
        }
        

        [EnableIf("@EditorApplication.isPlaying")][Button]
        public virtual void Play()
        {
            if (IsCutscenePlaying)
            {
                if(ActiveCutscene != null) 
                    Debug.LogWarning($"이미 재생중인 컷씬이 있음 : {ActiveCutscene.name} |=> return");
                else 
                    Debug.LogWarning($"현재 재생중인 컷씬은 없으나 isCutscenePlaying == true 상태임. |=> return");
                
                return;
            }
            PrePlay();
            IsCutscenePlaying = true;
            ActiveCutscene = this;
            director.Play();
            OnPlay.Invoke();
            OnCutsceneStart?.Invoke(this);
            PostPlay();
        }

        public void Pause()
        {
            if(!IsCutscenePlaying) return;
            // ActiveCutscene.director.Pause() 말고 Speed 를 0으로 해서 쓰기.
            director.playableGraph.GetRootPlayable(0).SetSpeed(0);
            IsCutscenePaused = true;
        }

        public void Resume()
        {
            if(!IsCutscenePlaying) return;
            // ActiveCutscene.director.Resume() 말고 Speed를 1로 해서 쓰기.
            director.playableGraph.GetRootPlayable(0).SetSpeed(1);
            IsCutscenePaused = false;
        }

        [Button]
        public void SkipToNextClip()
        {
            if(DialogueTrack == null)
            {
                Debug.Log($"현재 컷씬에는 DialogueClip이 없음");
                return;
            }

            var currentClipIndex = DialogueTrack.GetCurrentClipIndex();

            if (currentClipIndex + 1 > DialogueTrack.ClipCount - 1)
            {
                return;
            }
            else
            {
                director.time = DialogueTrack.GetClipStartTime(currentClipIndex + 1);
                Resume();
            }
        }

        [Button]
        public void SkipToEnd()
        {
            if(!IsSkipToEndAvailable) return;

            if (director.time > 10)
            {
                director.time = director.duration - 3;
            }
            else
            {
                director.time = director.duration * 0.95;
            }
            
        }

        [Button]
        public void Stop()
        {
            director.Stop();
        }

        protected virtual void PrePlay(){}
        protected virtual void PostPlay(){}
        protected virtual void PreEnd(){}
        protected virtual void PostEnd(){}
        
        void InternalEnd()
        {
            PreEnd();
            if(DialogueTrack != null)DialogueTrack.Release();
            OnEnd.Invoke();
            IsCutscenePlaying = false;
            IsCutscenePaused = false;
            if(ActiveCutscene == this) ActiveCutscene = null;
            OnCutsceneEnd?.Invoke(this);
            PostEnd();
        }


#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        static void Init()
        {
            EditorApplication.playModeStateChanged += Release;
        }

        static void Release(PlayModeStateChange change)
        {
            if(change != PlayModeStateChange.EnteredEditMode) return;
            OnCutsceneStart = null;
            OnCutsceneEnd = null;
            
            OnClipStart = null;
            OnClipFadeOut = null;
            OnClipEnd = null;
            IsCutscenePlaying = false;
            ActiveCutscene = null;
            

            EditorApplication.playModeStateChanged -= Release;
        }

        void Reset()
        {
            director = GetComponent<PlayableDirector>();
        }

        bool IsSignalReceiverRequired()
        {
            if (director == null) return false;
            if (director.playableAsset == null) return false;
            if (director.playableAsset.outputs == null) return false;
            if (director.playableAsset.outputs.All(x => x.outputTargetType != typeof(Conversation))) return false;
            
            if(DialogueTrack == null) DialogueTrack = director.playableAsset.outputs
                .FirstOrDefault(x => x.outputTargetType == typeof(Conversation))
                .sourceObject as DialogueTrack;
            if (DialogueTrack == null) return false;
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