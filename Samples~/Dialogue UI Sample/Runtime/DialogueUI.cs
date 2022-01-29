using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
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
        [TitleGroup("일반 설정")] public bool AutoDialogue;
        [TitleGroup("일반 설정")] [Range(0f, 2f)][ShowIf(nameof(AutoDialogue))][Indent()] 
        public float IntervalBetweenDisplay = 0.7f;
        [TitleGroup("일반 설정")][ShowIf("@AutoDialogue && SubtitleDisplayStyle != OcDialogue.Samples.SubtitleDisplayStyle.TypeWriter")][Indent()] 
        public float DisplayTimePerCharacter = 0.2f;

        [TitleGroup("Subtitle")] public TextMeshProUGUI nameField;
        [TitleGroup("Subtitle")] public TextMeshProUGUI textField;
        [TitleGroup("Subtitle")] public Image SubtitleCompleteIcon;
        [TitleGroup("Subtitle")] public bool HideName;
        [TitleGroup("Subtitle")] public SubtitleDisplayStyle SubtitleDisplayStyle;
        [TitleGroup("Subtitle")][Range(1, 100)][ShowIf(nameof(SubtitleDisplayStyle), SubtitleDisplayStyle.TypeWriter)][Indent()] 
        public float TypeWriteSpeed = 50;
        [TitleGroup("Subtitle")] [Range(0f, 2f)][ShowIf(nameof(SubtitleDisplayStyle), SubtitleDisplayStyle.FadeInOut)][Indent()] 
        public float TextFadeInTime = 0.7f;
        [TitleGroup("Subtitle")] [Range(0f, 2f)][ShowIf(nameof(SubtitleDisplayStyle), SubtitleDisplayStyle.FadeInOut)][Indent()]
        public float TextFadeOutTime = 0.95f;
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

        public event Action OnEnd;

        TweenerCore<string, string, StringOptions> _textTween;
        TweenerCore<Color, Color, ColorOptions> _textFadeTween;
        TweenerCore<float, float, FloatOptions> _canvasGroupTween;
        Coroutine _conversationCoroutine;
#if PACKAGE_LOCALIZATION
        StringTable _table;
#endif
        
        bool _isNextTriggered;
        float _doubleClickTimer;

        const float Default_CutsceneTextInterval = 0.2f;
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

            if (_canvasGroupTween != null)
            {
                _canvasGroupTween.Complete();
                _canvasGroupTween.Kill();
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
                    Instance.OnEnd += onEnd;
                    Instance.StartConversation(conversation);
                    IsLoadAsyncOn = false;
                };
            }
            else
            {
                Instance.StartConversation(conversation);
            }
        }

        public void Next()
        {
            if(AutoDialogue) return;
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
        }

        void StartConversation(Conversation conversation)
        {
            if(_conversationCoroutine != null) StopCoroutine(_conversationCoroutine);
            Conversation = conversation;
            _conversationCoroutine = StartCoroutine(Process());
        }

        void Choice(Balloon choice)
        {
            Balloon = choice;
            _isNextTriggered = true;
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
            
            _doubleClickTimer = 0f;
            _isNextTriggered = false;
            Balloon = Conversation.Balloons.FirstOrDefault(x => x.type == Balloon.Type.Entry);
            if (Balloon == null)
            {
                Debug.LogWarning($"엔트리 노드가 없음 ㅅㅂ | Conversation : {Conversation.key}");
                yield break;
            }

            if(_canvasGroupTween == null) _canvasGroupTween = canvasGroup.DOFade(1f, 0.2f).SetAutoKill(false);
            else
            {
                _canvasGroupTween.ChangeValues(canvasGroup.alpha, 1f, 0.2f);
                _canvasGroupTween.Restart();
            }
            
            while (true)
            {
                choiceArea.SetActive(false);
                Choices?.Clear();
                SubtitleCompleteIcon.gameObject.SetActive(false);

                var children = Balloon.linkedBalloons;
                if(children.Count == 0) break;

                var linkedBalloons = children.Where(x => x.type == Balloon.Type.Dialogue || x.type == Balloon.Type.Action).ToList();
                var shouldStop = true;
                foreach (var balloon in linkedBalloons)
                {
                    if(balloon.useChecker && !balloon.checker.IsTrue()) continue;
                    Balloon = balloon;
                    shouldStop = false;
                    break;
                }
                if(shouldStop) break;
                Choices = Balloon.linkedBalloons.Where(x => x.type == Balloon.Type.Choice && (!x.useChecker || (x.useChecker && x.checker.IsTrue()))).ToList();
                yield return StartCoroutine(PrintContents());
            }
            OnEnd?.Invoke();
            SceneManager.UnloadSceneAsync(sceneName);
            _conversationCoroutine = null;
        }


        IEnumerator PrintContents()
        {
            if (Balloon.useEvent)
            {
                if (User != null)
                {
                    if(Balloon.signal != null) User.SignalReceiver.GetReaction(Balloon.signal).Invoke();
                    else Debug.LogError($"[Dialogue UI] 이벤트를 실행하려 했으나 Balloon에 에셋이 없음. Conversation : {Conversation.key} | Balloon Text : {Balloon.text}"
                        .ToRichText(Color.cyan));
                }else Debug.LogError($"[Dialogue UI] 이벤트를 실행하려 했으나 리시버가 없음. Conversation : {Conversation.key} | Balloon Text : {Balloon.text}"
                    .ToRichText(Color.cyan));
            }
            
            // setter실행
            if (Balloon.useSetter)
            {
                foreach (var setter in Balloon.setters)
                {
                    setter.Execute();
                }
            }

            if (Balloon.useImage)
            {
                if (Balloon.displayTargetImage == null)
                {
                    Debug.LogError($"[Dialogue UI] 표시하려는 이미지가 비어있음".ToRichText(Color.cyan));
                }
                else
                {
                    switch (Balloon.imageViewerSize)
                    {
                        case ImageViewerSize.FullSize:
                            fullSizeImage.texture = Balloon.displayTargetImage;
                            fullSizeImage.gameObject.SetActive(true);
                            break;
                        case ImageViewerSize.FloatingSize:
                            floatingImage.texture = Balloon.displayTargetImage;
                            floatingImageRoot.gameObject.SetActive(true);
                            
                            if(Balloon.imageSizeOverride.sqrMagnitude < 1)
                            {
                                floatingImageRoot.offsetMin = -new Vector2(floatingImage.texture.width * 0.5f,
                                    floatingImage.texture.height * 0.5f);
                                floatingImageRoot.offsetMax = new Vector2(floatingImage.texture.width * 0.5f,
                                    floatingImage.texture.height * 0.5f);
                            }
                            else
                            {
                                floatingImageRoot.offsetMin = -new Vector2(Balloon.imageSizeOverride.x * 0.5f,
                                    floatingImage.texture.height * 0.5f);
                                floatingImageRoot.offsetMax = new Vector2(Balloon.imageSizeOverride.y * 0.5f,
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
            
            if(Balloon.type == Balloon.Type.Action) yield break;

            // TODO: 여기서 로컬라이징.
#if PACKAGE_LOCALIZATION
            var targetText = _table == null ? Balloon.text : GetLocalizedString(Balloon);
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
                var actorName = Balloon.actor == null || Balloon.actor.name == "System" ? "" : Balloon.actor.name;
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
                    if(_textTween == null) _textTween = textField.DOText(targetText, duration).SetEase(Ease.Linear).SetAutoKill(false);
                    else
                    {
                        _textTween.ChangeValues("", targetText, duration);
                        _textTween.Restart();
                    }
                    yield return _textTween;
                    break;
                case SubtitleDisplayStyle.FadeInOut:
                    textField.alpha = 0f;
                    textField.text = targetText;
                    if (_textFadeTween == null) _textFadeTween = textField.DOFade(1, TextFadeInTime).SetAutoKill(false).SetEase(Ease.OutCubic);
                    else
                    {
                        _textFadeTween.ChangeValues(textField.color, textField.color.SetA(1), TextFadeInTime);
                        _textFadeTween.Restart();
                    }
                    yield return _textFadeTween.WaitForCompletion();
                    
                    if (DisplayTimePerCharacter <= 0 || DisplayTimePerCharacter > 0.5f)
                    {
                        Debug.LogWarning($"[Dialogue UI] DisplayTimePerCharacter가 유효하지 않은 값이라 기본값으로 변경함.");
                        DisplayTimePerCharacter = Default_CutsceneTextInterval;
                    }

                    if(AutoDialogue)
                    {
                        // 일단 글자수 만큼의 시간을 기다리는데, 도중에 Next가 트리거되면 페이드 아웃으로 넘어감. 아직은 컷씬 도중에 자동으로 Next를 트리거하는게 없지만 성우녹음 등을 하면 필요해질듯.
                        for (var f = 0f; f < targetText.Length * DisplayTimePerCharacter; f += Time.deltaTime)
                        {
                            yield return null;
                            if (_isNextTriggered) break;
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _isNextTriggered = false;

            
            // 선택지.
            yield return new WaitForSeconds(ChoiceWaitTime);
            if(Choices == null || Choices.Count == 0)
            {
                // 선택지가 없는 경우. 자동 넘김이 아닐 경우에만 완료 아이콘을 활성화하고 Next호출을 대기함.
                if (!AutoDialogue)
                {
                    SubtitleCompleteIcon.gameObject.SetActive(true);
                    while (!_isNextTriggered) yield return null;
                    _isNextTriggered = false;
                }
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
                _textFadeTween.ChangeValues(textField.color, textField.color.SetA(0f), TextFadeOutTime);
                _textFadeTween.Restart();
                yield return _textFadeTween.WaitForCompletion();
                yield return new WaitForSeconds(IntervalBetweenDisplay);
            }
        }

#if PACKAGE_LOCALIZATION
        public string GetLocalizedString(Balloon balloon)
        {
            return _table == null ? balloon.text : _table.GetEntry($"{Conversation.Category}/{Conversation.key}/{balloon.GUID}").GetLocalizedString();
        }
#endif
    }
}