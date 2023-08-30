using System;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace OcDialogue.Cutscene
{
    [Serializable]
    public class DialogueClipBehaviour : PlayableBehaviour
    {
        public Conversation Conversation;
        public Balloon Balloon;
        public CutsceneBehaviour Cutscene;
        public PlayableDirector Director;
        public DialogueTrack Track;
        public DialogueClip Clip;
        public ClipPlayState PlayState => _playState;
        public double LeftTime => __duration - __time;
        public double NormalizedTime => __duration == 0 ? 1 : __time / __duration;
        
        public bool hasToPause;
        
        ClipPlayState _playState;
        bool _pauseScheduled;
        bool _fadeOutInvoked;
        double __duration;
        double __time;

        event Action<DialogueClipBehaviour> OnStart;
        event Action<DialogueClipBehaviour> OnFadeOut;
        event Action<DialogueClipBehaviour> OnEnd;

#if UNITY_EDITOR
        static Canvas _editorPreviewCanvas;
        static TextMeshProUGUI _editorPreviewText;  
#endif
        public void Init(CutsceneBehaviour cutscene, DialogueClip clip, DialogueTrack track)
        {
            Cutscene = cutscene;
            Director = cutscene.director;
            Clip = clip;
            Track = track;

            Conversation = clip.Conversation;
            Balloon = clip.Balloon;
        }

        public void AssignCallbacks(
            Action<DialogueClipBehaviour> onClipStart,
            Action<DialogueClipBehaviour> onClipFadeOut,
            Action<DialogueClipBehaviour> onClipEnd)
        {
            OnStart = onClipStart;
            OnFadeOut = onClipFadeOut;
            OnEnd = onClipEnd;
        }
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {

            if(_playState == ClipPlayState.None && info.weight > 0f)
            {
                if(hasToPause) _pauseScheduled = true;
                _playState = ClipPlayState.IsPlaying;
                __duration = playable.GetDuration();
                OnStart?.Invoke(this);
            }
            
            if(_playState == ClipPlayState.IsPlaying)
            {
                __time = playable.GetTime();
                
                if (_pauseScheduled)
                {
                    // 일시정지가 이뤄지는 대화에선 마지막에 도달할때 즈음에 일시정지함.
                    if(NormalizedTime >= Cutscene.autoPauseNormalizedTime)
                    {
                        Cutscene.Pause();
                    }
                }
                else
                {
                    // 일시정지가 이뤄지는 대화에선 페이드아웃이 실행되면 안됨.
                    // 남은 시간이 페이드아웃 시간 이하가 되면 페이드아웃을 시작함.
                    if (!_fadeOutInvoked && LeftTime < Cutscene.balloonFadeOutDuration)
                    {
                        _fadeOutInvoked = true;
                        OnFadeOut?.Invoke(this);
                    }    
                }
                
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            ClearPreview();
#endif
            // 이름은 일시정지때 호출될 것 같지만, 실제로는 클립이 끝날때도 호출됨.
            // 물론 director.Pause를 통해 일시정지하면 일시정지떄때도 호출하지만,
            // CutsceneBehaviour에서 일시정지를 Speed = 0으로 하는 식으로 하기때문에 그 일시정지로는 호출되지 않음.
            if(!CutsceneBehaviour.IsCutscenePaused && _playState == ClipPlayState.IsPlaying)
            {
                _pauseScheduled = false;
                _playState = ClipPlayState.Played;
                OnEnd?.Invoke(this);
            }
        }
        

        public override void OnPlayableDestroy(Playable playable)
        {
            _playState = ClipPlayState.None;
        }

#if UNITY_EDITOR
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            InitializePreview();
            UpdatePreviewText();
        }
        public override void OnGraphStop(Playable playable)
        {
            ClearPreview();
        }
        void InitializePreview()
        {
            if(Application.isPlaying) return;
            if (_editorPreviewCanvas == null)
            {
                _editorPreviewCanvas = new GameObject("_DialogueClip Preview").AddComponent<Canvas>();
                _editorPreviewCanvas.gameObject.hideFlags = HideFlags.HideAndDontSave;
                _editorPreviewCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = _editorPreviewCanvas.gameObject.AddComponent<CanvasScaler>();
            }

            if (_editorPreviewText == null)
            {
                _editorPreviewText = new GameObject("TXT").AddComponent<TextMeshProUGUI>();
                _editorPreviewText.font = DialogueAsset.Instance.cutscenePreviewFont;
                _editorPreviewText.transform.SetParent(_editorPreviewCanvas.transform);
                _editorPreviewText.rectTransform.anchorMin = Vector2.zero;
                _editorPreviewText.rectTransform.anchorMax = Vector2.one;
                _editorPreviewText.rectTransform.anchoredPosition = Vector2.zero;
                _editorPreviewText.rectTransform.sizeDelta = Vector2.zero;
                _editorPreviewText.alignment = TextAlignmentOptions.Bottom;
            }
        }

        void UpdatePreviewText()
        {
            if(Application.isPlaying) return;
            if (_editorPreviewText != null) _editorPreviewText.text = Balloon.text;
        }

        void ClearPreview()
        {
            if(_editorPreviewCanvas != null) Object.DestroyImmediate(_editorPreviewCanvas.gameObject);
        }
#endif

    }

    public enum ClipPlayState
    {
        None,
        IsPlaying,
        Played
    }
}