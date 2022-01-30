using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using OcDialogue.Cutscene;
using OcDialogue.DB;
using OcUtility;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
/*
 * PACKAGE_LOCALIZATION 은 정의된 어셈블리를 기준으로 작동함.
 * 따라서, OcDialoguePackage에 있는 스크립트들은 이 심볼이 패키지에따라 자동으로 활성화/비활성화되지만
 * Sample의 스크립트들을 본래의 어셈블리 외부로 가져와서 사용하는 경우, 심볼이 비활성화됨
 * 이 경우엔 해당되는 스크립트들이 속한 어셈블리의 Version Defines를 직접 정의해야함.
 */
#if PACKAGE_LOCALIZATION
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
#endif
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OcDialogue.Samples
{
    public enum SubtitleDisplayStyle
    {
        None,
        TypeWriter,
        FadeInOut
    }
    public class DialogueUI : MonoBehaviour
    {
        public static DialogueUI Instance => _instance;
        static DialogueUI _instance;
        public static bool IsLoadAsyncOn { get; private set; }
        
        [TitleGroup("일반 설정")] public CanvasGroup canvasGroup;
#if PACKAGE_LOCALIZATION
        [TitleGroup("일반 설정")] public LocalizedStringTable LocalizedStringTable;
#endif
        [TitleGroup("일반 설정")] public float CanvasFadeInDuration = 0.7f;
        [TitleGroup("일반 설정")] public float CanvasFadeOutDuration = 0.9f;

        [TitleGroup("Subtitle")] public TextMeshProUGUI nameField;
        [TitleGroup("Subtitle")] public TextMeshProUGUI textField;
        [TitleGroup("Subtitle")] public Image SubtitleCompleteIcon;
        [TitleGroup("Subtitle")] public bool HideName;
        
        [TitleGroup("Subtitle")] public SubtitleDisplayStyle SubtitleDisplayStyle;
        [TitleGroup("Subtitle")][Range(1, 100)][ShowIf(nameof(SubtitleDisplayStyle), SubtitleDisplayStyle.TypeWriter)][Indent()] 
        public float TypeWriteSpeed = 50;
        
        [TitleGroup("Subtitle")] [Range(0f, 2f)][ShowIf(nameof(SubtitleDisplayStyle), SubtitleDisplayStyle.FadeInOut)][Indent()] 
        public float TextFadeInDuration = 0.1f;
        [TitleGroup("Subtitle")] [Range(0f, 2f)][ShowIf(nameof(SubtitleDisplayStyle), SubtitleDisplayStyle.FadeInOut)][Indent()]
        public float TextFadeOutDuration = 0.2f;
        [TitleGroup("Subtitle")] [Range(0f, 5f)][ShowIf(nameof(SubtitleDisplayStyle), SubtitleDisplayStyle.FadeInOut)][Indent()]
        public float MinimumDisplayDuration = 2f;
        [TitleGroup("Subtitle")] [Range(0f, 0.5f)][ShowIf(nameof(SubtitleDisplayStyle), SubtitleDisplayStyle.FadeInOut)][Indent()]
        public float DisplayTimePerCharacter = 0.17f;

        
        [TitleGroup("Subtitle")][Range(0f, 5f)] public float DoubleClickBlockTime = 0.15f;
        
        [TitleGroup("선택지")]public GameObject choiceArea;
        [TitleGroup("선택지")]public ChoiceButton choiceButtonTemplate;
        [TitleGroup("선택지")][Range(0f, 1f)] public float ChoiceWaitTime = 0.25f;

        [TitleGroup("이미지 뷰어")] public RawImage fullSizeImage;
        [TitleGroup("이미지 뷰어")] public RawImage floatingImage;
        [TitleGroup("이미지 뷰어")] public RectTransform floatingImageRoot;
        
        public Conversation Conversation { get; private set; }
        public Balloon Balloon { get; private set; }
        public IDialogueUser User { get; private set; }
        public List<Balloon> Choices { get; private set; }
        public List<ChoiceButton> ChoiceButtons { get; private set; }
        public string sceneName { get; private set; }

        public event Action OnConversationEnd;
        public event Action OnBalloonEnd;

        TweenerCore<string, string, StringOptions> _textTween;
        TweenerCore<Color, Color, ColorOptions> _textFadeTween;
        TweenerCore<float, float, FloatOptions> _canvasGroupFadeTween;
        Coroutine _conversationCoroutine;
        Coroutine _UIFadeOutCoroutine;
#if PACKAGE_LOCALIZATION
        StringTable _table;
#endif
        bool _isNextTriggered;
        float _doubleClickTimer;

        const float Default_CutsceneTextInterval = 0.2f;

        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            DialogueClipBehaviour.OnDialogueClipStart += (behaviour, conversation, director) =>
            {
                var track = director.playableAsset.outputs
                    .FirstOrDefault(x => x.sourceObject is DialogueTrack).sourceObject as DialogueTrack;

                DisplayBalloon("Dialogue UI", conversation, behaviour.balloon, 
                    director.GetComponent<IDialogueUser>(), track.param);
            };
            DialogueClipBehaviour.OnDialogueClipFadeOut += (behaviour, conversation, director) => Stop();
            DialogueClipBehaviour.OnDialogueClipEnd += (behaviour, conversation, director) =>
            {
                if (_instance._UIFadeOutCoroutine != null) return;
                Stop();
            };
        }
        
        void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        void Awake()
        {
            _instance = this;
            fullSizeImage.transform.localPosition = Vector3.zero;
            fullSizeImage.gameObject.SetActive(false);
            floatingImageRoot.gameObject.SetActive(false);
            canvasGroup.alpha = 0;
            textField.text = "";

#if PACKAGE_LOCALIZATION
            if(LocalizedStringTable != null && !LocalizedStringTable.IsEmpty)
            {
                var async = LocalizedStringTable.GetTableAsync();
                async.Completed += handle => _table = handle.Result;
            }
#endif
        }

        void OnDestroy()
        {
            if (_instance == this) _instance = null;
            if(_textTween != null)
            {
                _textTween.Complete();
                _textTween.Kill();
            }

            if (_textFadeTween != null)
            {
                _textFadeTween.Complete();
                _textFadeTween.Kill();
            }

            if (_canvasGroupFadeTween != null)
            {
                _canvasGroupFadeTween.Complete();
                _canvasGroupFadeTween.Kill();
            }
        }

        void Update()
        {
            _doubleClickTimer += Time.deltaTime;
        }

        public static void StartConversation(string sceneName, Conversation conversation, IDialogueUser user, Action onEnd = null)
        {
            // 현재 열린 DialogueUI가 없고, 로딩중도 아니면 로딩을 시작함.
            if (_instance == null && !IsLoadAsyncOn)
            {
                IsLoadAsyncOn = true;
                var async = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                // 로딩이 완료되면 대화를 시작함.
                async.Completed += handle =>
                {
                    Instance.sceneName = sceneName;
                    Instance.User = user;
                    Instance.StartConversation(conversation);
                    Instance.OnConversationEnd = onEnd;
                    IsLoadAsyncOn = false;
                };
            }
            else
            {
                Instance.StartConversation(conversation);
                Instance.OnConversationEnd = onEnd;
            }
        }

        public static void DisplayBalloon(string sceneName, Conversation conversation, Balloon balloon, 
            IDialogueUser user, DisplayParameter param, Action onEnd = null)
        {
            if (_instance == null && !IsLoadAsyncOn)
            {
                IsLoadAsyncOn = true;
                var async = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                // 로딩이 완료되면 대화를 시작함.
                async.Completed += handle =>
                {
                    Instance.sceneName = sceneName;
                    Instance.User = user;
                    Instance.DisplayBalloon(conversation, balloon);
                    Instance.OnBalloonEnd = onEnd;
                    applyParam();
                    IsLoadAsyncOn = false;
                };
            }
            else
            {
                Instance.DisplayBalloon(conversation, balloon);
                Instance.OnBalloonEnd = onEnd;
                applyParam();
            }

            void applyParam()
            {
                if(param == null) return;
                Instance.CanvasFadeInDuration = param.canvasFadeInDuration;
                Instance.CanvasFadeOutDuration = param.canvasFadeOutDuration;
                Instance.TextFadeInDuration = param.textFadeInDuration;
                Instance.TextFadeOutDuration = param.textFadeOutDuration;
                Instance.DisplayTimePerCharacter = param.durationPerChar;
                Instance.MinimumDisplayDuration = param.minimumDuration;
            }
        }

        public static void Stop()
        {
            Instance.DelayedStop();
        }
        

        public void Next()
        {
            if(_doubleClickTimer < DoubleClickBlockTime) return;
            if(SubtitleDisplayStyle == SubtitleDisplayStyle.TypeWriter)
            {
                if (!_textTween.IsComplete())
                {
                    _textTween.Complete();
                    return;
                }
            }

            if(Choices != null && Choices.Count > 0) return;
            _isNextTriggered = true;
            _doubleClickTimer = 0f;
            
            SubtitleCompleteIcon.gameObject.SetActive(false);
        }
        
        void StartConversation(Conversation conversation)
        {
            OnConversationEnd = null;
            if(_conversationCoroutine != null) StopCoroutine(_conversationCoroutine);
            Conversation = conversation;
            PreProcess();
            _conversationCoroutine = StartCoroutine(Process());
        }

        void DisplayBalloon(Conversation conversation, Balloon balloon)
        {
            OnBalloonEnd = null;
            if(_conversationCoroutine != null) StopCoroutine(_conversationCoroutine);
            Conversation = conversation;
            PreProcess();
            _conversationCoroutine = StartCoroutine(PrintContents(balloon));
        }

        void DelayedStop()
        {
            if(_UIFadeOutCoroutine != null) StopCoroutine(_UIFadeOutCoroutine);
            _UIFadeOutCoroutine = StartCoroutine(StopProcess());
        }

        void Choice(Balloon choice)
        {
            Balloon = choice;
            _isNextTriggered = true;
            
            choiceArea.SetActive(false);
        }

        void PreProcess()
        {
            if(_UIFadeOutCoroutine != null) StopCoroutine(_UIFadeOutCoroutine);
            
            choiceArea.SetActive(false);
            SubtitleCompleteIcon.gameObject.SetActive(false);
            
            _doubleClickTimer = 0f;
            _isNextTriggered = false;
            
            if(_canvasGroupFadeTween == null) 
                _canvasGroupFadeTween = canvasGroup.DOFade(1f, CanvasFadeInDuration).SetAutoKill(false);
            else
            {
                _canvasGroupFadeTween.ChangeValues(canvasGroup.alpha, 1f, 0.2f);
                _canvasGroupFadeTween.Restart();
            }
        }

        IEnumerator Process()
        {
#if PACKAGE_LOCALIZATION
            if(LocalizedStringTable != null && !LocalizedStringTable.IsEmpty)
            {
                while (_table == null)
                {
                    yield return null;
                }
            }
#endif

            Balloon = Conversation.Balloons.FirstOrDefault(x => x.type == Balloon.Type.Entry);
            if (Balloon == null)
            {
                Debug.LogWarning($"엔트리 노드가 없음 | Conversation : {Conversation.key}");
                yield break;
            }

            
            while (true)
            {
                var children = Balloon.linkedBalloons;
                if(children.Count == 0) break;

                var linkedBalloons = 
                    children.Where(x => x.type == Balloon.Type.Dialogue || x.type == Balloon.Type.Action)
                        .ToList();
                var shouldStop = true;
                Balloon nextBalloon = null;
                foreach (var balloon in linkedBalloons)
                {
                    if(balloon.useChecker && !balloon.checker.IsTrue()) continue;
                    nextBalloon = balloon;
                    shouldStop = false;
                    break;
                }
                if(shouldStop) break;
                yield return StartCoroutine(PrintContents(nextBalloon));
            }
            OnConversationEnd?.Invoke();
            SceneManager.UnloadSceneAsync(sceneName);
            _conversationCoroutine = null;
        }


        IEnumerator PrintContents(Balloon balloon)
        {
            Balloon = balloon;
            if (balloon.useEvent)
            {
                if (User != null)
                {
                    if(balloon.signal != null) User.SignalReceiver.GetReaction(balloon.signal).Invoke();
                    else
                    {
                        Debug.LogError($"[Dialogue UI] 이벤트를 실행하려 했으나 Balloon에 에셋이 없음. " +
                                       $"Conversation : {Conversation.key} | Balloon Text : {balloon.text}"
                            .ToRichText(Color.cyan));
                    }
                }else Debug.LogError($"[Dialogue UI] 이벤트를 실행하려 했으나 리시버가 없음. " +
                                     $"Conversation : {Conversation.key} | Balloon Text : {balloon.text}"
                    .ToRichText(Color.cyan));
            }
            
            // setter실행
            if (balloon.useSetter)
            {
                foreach (var setter in balloon.setters)
                {
                    setter.Execute();
                }
            }

            if (balloon.useImage)
            {
                if (balloon.displayTargetImage == null)
                {
                    Debug.LogError($"[Dialogue UI] 표시하려는 이미지가 비어있음".ToRichText(Color.cyan));
                }
                else
                {
                    switch (balloon.imageViewerSize)
                    {
                        case ImageViewerSize.FullSize:
                            fullSizeImage.texture = balloon.displayTargetImage;
                            fullSizeImage.gameObject.SetActive(true);
                            break;
                        case ImageViewerSize.FloatingSize:
                            floatingImage.texture = balloon.displayTargetImage;
                            floatingImageRoot.gameObject.SetActive(true);
                            
                            if(balloon.imageSizeOverride.sqrMagnitude < 1)
                            {
                                floatingImageRoot.offsetMin = -new Vector2(floatingImage.texture.width * 0.5f,
                                    floatingImage.texture.height * 0.5f);
                                floatingImageRoot.offsetMax = new Vector2(floatingImage.texture.width * 0.5f,
                                    floatingImage.texture.height * 0.5f);
                            }
                            else
                            {
                                floatingImageRoot.offsetMin = -new Vector2(balloon.imageSizeOverride.x * 0.5f,
                                    floatingImage.texture.height * 0.5f);
                                floatingImageRoot.offsetMax = new Vector2(balloon.imageSizeOverride.y * 0.5f,
                                    floatingImage.texture.height * 0.5f);
                            }
                            break;
                        default: goto case ImageViewerSize.FloatingSize;
                    }
                }
            }
            else
            {
                fullSizeImage.gameObject.SetActive(false);
                floatingImageRoot.gameObject.SetActive(false);
            }
            
            if(balloon.type == Balloon.Type.Action)
            {
                OnBalloonEnd?.Invoke();
                yield break;
            }

            // TODO: 여기서 로컬라이징.
#if PACKAGE_LOCALIZATION
            var targetText = _table == null ? balloon.text : GetLocalizedString(balloon);
            if (_table == null)
            {
                Debug.LogWarning("Localization 패키지가 감지되었으나, LocalizationTable _table 이 없음");
            }
#else
            var targetText = Balloon.text;
#endif
            
            // 실제 출력되는 부분.
            
            if (!HideName)
            {
                var actorName = balloon.actor == null || balloon.actor.name == "System" ? "" : balloon.actor.name;
                nameField.text = actorName;
            }

            switch (SubtitleDisplayStyle)
            {
                case SubtitleDisplayStyle.None:
                    textField.text = targetText;
                    break;
                case SubtitleDisplayStyle.TypeWriter:
                    textField.text = "";
                    var duration = targetText.Length / TypeWriteSpeed;
                    if(_textTween == null) 
                        _textTween = textField.DOText(targetText, duration).SetEase(Ease.Linear).SetAutoKill(false);
                    else
                    {
                        _textTween.ChangeValues("", targetText, duration);
                        _textTween.Restart();
                    }
                    yield return _textTween.WaitForCompletion();
                    break;
                case SubtitleDisplayStyle.FadeInOut:
                    textField.alpha = 0f;
                    textField.text = targetText;
                    if (_textFadeTween == null) 
                        _textFadeTween = textField.DOFade(1, TextFadeInDuration).SetAutoKill(false).SetEase(Ease.OutCubic);
                    else
                    {
                        _textFadeTween.ChangeValues(textField.color, textField.color.SetA(1), TextFadeInDuration);
                        _textFadeTween.Restart();
                    }
                    yield return _textFadeTween.WaitForCompletion();
                    
                    if (DisplayTimePerCharacter <= 0 || DisplayTimePerCharacter > 0.5f)
                    {
                        Debug.LogWarning($"[Dialogue UI] DisplayTimePerCharacter가 유효하지 않은 값이라 기본값으로 변경함.");
                        DisplayTimePerCharacter = Default_CutsceneTextInterval;
                    }

                    var waitTime = MinimumDisplayDuration + targetText.Length * DisplayTimePerCharacter;
                    
                    // 일단 글자수 만큼의 시간을 기다리는데, 도중에 Next가 트리거되면 페이드 아웃으로 넘어감.
                    // 아직은 컷씬 도중에 자동으로 Next를 트리거하는게 없지만 성우녹음 등을 하면 필요해질듯.
                    for (var f = 0f; f < waitTime; f += Time.deltaTime)
                    {
                        yield return null;
                        if (_isNextTriggered) break;
                    }
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _isNextTriggered = false;

            
            // 선택지.
            yield return new WaitForSeconds(ChoiceWaitTime);
            Choices = Balloon.linkedBalloons
                .Where(x => x.type == Balloon.Type.Choice && (!x.useChecker || (x.useChecker && x.checker.IsTrue())))
                .ToList();
            
            if(Choices == null || Choices.Count == 0)
            {
                // 선택지가 없는 경우. 완료 아이콘을 활성화하고 Next호출을 대기함.
                SubtitleCompleteIcon.gameObject.SetActive(true);
                while (!_isNextTriggered) yield return null;
                _isNextTriggered = false;
            }
            else
            {
                choiceArea.SetActive(true);
                if (ChoiceButtons == null)
                {
                    ChoiceButtons = new List<ChoiceButton>();
                    ChoiceButtons.Add(choiceButtonTemplate);
                    choiceButtonTemplate.onClick.AddListener(() => Choice(choiceButtonTemplate.ChoiceBalloon));
                }
                if (Choices.Count > ChoiceButtons.Count)
                {
                    var diff = Choices.Count - ChoiceButtons.Count;
                    for (int i = 0; i < diff; i++)
                    {
                        var btn = Instantiate(choiceButtonTemplate, choiceArea.transform);
                        ChoiceButtons.Add(btn);
                        btn.onClick.AddListener(() => Choice(btn.ChoiceBalloon));
                    }
                }

                for (int i = 0; i < ChoiceButtons.Count; i++)
                {
                    if(i < Choices.Count)
                    {
                        ChoiceButtons[i].SetChoice(Choices[i]);
                        ChoiceButtons[i].gameObject.SetActive(true);
                    }else ChoiceButtons[i].gameObject.SetActive(false);
                }
                
                while (!_isNextTriggered) yield return null;
                _isNextTriggered = false;
            }
            
            if(SubtitleDisplayStyle == SubtitleDisplayStyle.FadeInOut)
            {
                _textFadeTween.ChangeValues(textField.color, textField.color.SetA(0f), TextFadeOutDuration);
                _textFadeTween.Restart();
                yield return _textFadeTween.WaitForCompletion();
            }
            
            OnBalloonEnd?.Invoke();
        }

        IEnumerator StopProcess()
        {
            OnConversationEnd?.Invoke();
            
            _textFadeTween.ChangeValues(textField.color, textField.color.SetA(0f), TextFadeOutDuration);
            _textFadeTween.Restart();
            yield return _textFadeTween.WaitForCompletion();

            _canvasGroupFadeTween.ChangeValues(canvasGroup.alpha, 0f, CanvasFadeOutDuration);
            _canvasGroupFadeTween.Restart();
            yield return _canvasGroupFadeTween.WaitForCompletion();
            
            SceneManager.UnloadSceneAsync(Instance.sceneName);
            _conversationCoroutine = null;
            _UIFadeOutCoroutine = null;
        }

#if PACKAGE_LOCALIZATION
        public string GetLocalizedString(Balloon balloon)
        {
            return _table == null ? 
                balloon.text : 
                _table.GetEntry($"{Conversation.Category}/{Conversation.key}/{balloon.GUID}").GetLocalizedString();
        }
#endif
    }
}