using System;
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

        public string DialogueSceneName => EffectiveParam.DialogueSceneName;
        public SignalReceiver SignalReceiver => signalReceiver;
        public Conversation Conversation => _dialogueTrack.Conversation;
        [InfoBox("이 대화에는 SignalReceiver가 필요함", InfoMessageType.Error, VisibleIf = "IsSignalReceiverRequired")]
        public SignalReceiver signalReceiver;

        public PlayableDirector director;
        
        [FoldoutGroup("Params")][GUIColor(2,1,2)]
        [ShowInInspector][InlineEditor(Expanded = true)][HideIf(nameof(overrideParam))]
        public DialogueDisplayParameter EffectiveParam => overrideParam ? param : DialogueDisplayParameter.Instance;
        
        [HorizontalGroup("Params/Header")][ReadOnly][GUIColor(2,1,2)]
        public bool overrideParam;
        
        [FoldoutGroup("Params")][ShowIf(nameof(overrideParam))][InlineEditor(Expanded = true)][GUIColor(2,1,2)]
        public DialogueDisplayParameter param;
        
        [InlineButton(nameof(QueryBindings), "Query")]
        public OcDictionary<Object, GameObject> bindingOverride;

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
            _dialogueTrack.Init(this,
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

        /// <summary> bindingOverride를 기준으로 리바인드함. 기본적으로 재생 직전에 한 번 호출됨. </summary>
        public void Rebind()
        {
            foreach (var kv in bindingOverride)
            {
                director.SetGenericBinding(kv.Key, kv.Value);
            }
        }

        [EnableIf("@EditorApplication.isPlaying")][Button]
        public void Play()
        {
            Rebind();
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
            // ActiveCutscene.director.Pause();
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
            // ActiveCutscene.director.Resume();
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
        
        void QueryBindings()
        {
            var dict = new OcDictionary<Object, GameObject>();
            foreach (var output in director.playableAsset.outputs)
            {
                var exist = bindingOverride.ContainsKey(output.sourceObject) ? bindingOverride[output.sourceObject] : null;
                dict[output.sourceObject] = exist;
            }

            bindingOverride = dict;
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

        [Button("Override")][HorizontalGroup("Params/Header")][DisableIf(nameof(overrideParam))]
        void OverrideParameter()
        {
            overrideParam = true;
            param = ScriptableObject.CreateInstance<DialogueDisplayParameter>();
            UnityEditor.EditorUtility.CopySerialized(DialogueDisplayParameter.Instance, param);
        }
        [Button("Cancel")][HorizontalGroup("Params/Header")][EnableIf(nameof(overrideParam))]
        void CancelParameterOverride()
        {
            overrideParam = false;
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