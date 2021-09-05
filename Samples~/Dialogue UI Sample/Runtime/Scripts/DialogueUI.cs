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
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OcDialogue.Samples
{
    public enum DialogueDisplayMode
    {
        Default,
        Cutscene
    }
    public class DialogueUI : MonoBehaviour
    {
        public static DialogueUI Instance => _instance;
        static DialogueUI _instance;
        public static bool IsLoadAsyncOn { get; private set; }
        
        public CanvasGroup canvasGroup;
        
        [BoxGroup("Subtitle")]public TextMeshProUGUI nameField;
        [BoxGroup("Subtitle")]public TextMeshProUGUI textField;
        [BoxGroup("Subtitle")]public GameObject choiceArea;
        [BoxGroup("Subtitle")]public ChoiceButton choiceButtonTemplate;
        [BoxGroup("Subtitle")]public Image nextIcon;
        [BoxGroup("Subtitle")][Range(1, 100)] public float TypeWriteSpeed = 50;
        [BoxGroup("Subtitle")][Range(0f, 1f)] public float ChoiceWaitTime = 0.25f;
        [BoxGroup("Subtitle")][Range(0f, 5f)] public float DoubleClickBlockTime = 0.15f;

        [BoxGroup("Subtitle/Cutscene")] public bool HideName;
        [BoxGroup("Subtitle/Cutscene")] [Range(0f, 2f)] public float TextFadeInTime = 0.7f;
        [BoxGroup("Subtitle/Cutscene")] [Range(0f, 2f)] public float TextFadeOutTime = 0.95f;
        [BoxGroup("Subtitle/Cutscene")] [Range(0f, 2f)] public float CutsceneSubtitleInterval = 0.7f;
        [BoxGroup("Subtitle/Cutscene")] public float CutsceneTextSpeed = 0.2f; 

        [BoxGroup("Image Viewer")] public RawImage fullSizeImage;
        [BoxGroup("Image Viewer")] public RawImage floatingImage;
        [BoxGroup("Image Viewer")] public RectTransform floatingImageRoot;
        
        public Conversation Conversation { get; private set; }
        public Balloon Balloon { get; private set; }
        public IDialogueUser User { get; private set; }
        public List<Balloon> Choices { get; private set; }
        public List<ChoiceButton> ChoiceButtons { get; private set; }
        public DialogueDisplayMode DisplayMode => _displayMode;
        public string sceneName { get; private set; }
        
        TweenerCore<string, string, StringOptions> _textTween;
        TweenerCore<Color, Color, ColorOptions> _textFadeTween;
        DialogueDisplayMode _displayMode;
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
            textField.text = "";
        }

        void OnDestroy()
        {
            if (_instance == this) _instance = null;
            if(_textTween != null)
            {
                _textTween.Kill();
                _textTween = null;
            }

            if (_textFadeTween != null)
            {
                _textFadeTween.Kill();
                _textFadeTween = null;
            }
        }

        void Update()
        {
            _doubleClickTimer += Time.deltaTime;
        }

        public static void StartConversation(string sceneName, Conversation conversation, IDialogueUser user, DialogueDisplayMode mode)
        {
            // 현재 열린 DialogueUI가 없고, 로딩중도 아니면 로딩을 시작함.
            if (_instance == null && !IsLoadAsyncOn)
            {
                IsLoadAsyncOn = true;
                var async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                
                // 로딩이 완료되면 대화를 시작함.
                async.completed += operation =>
                {
                    Instance.sceneName = sceneName;
                    Instance.User = user;
                    Instance._displayMode = mode;
                    Instance.StartConversation(conversation);
                    IsLoadAsyncOn = false;
                };
            }
        }

        public void Next()
        {
            if(_displayMode == DialogueDisplayMode.Cutscene) return;
            if(_doubleClickTimer < DoubleClickBlockTime) return;
            if (!_textTween.IsComplete())
            {
                _textTween.Complete();
                return;
            }
            if(Choices != null && Choices.Count > 0) return;
            _isNextTriggered = true;
            _doubleClickTimer = 0f;
        }

        void StartConversation(Conversation conversation)
        {
            Conversation = conversation;
            StartCoroutine(Process());
        }

        void Choice(Balloon choice)
        {
            Balloon = choice;
            _isNextTriggered = true;
        }

        IEnumerator Process()
        {
            _doubleClickTimer = 0f;
            _isNextTriggered = false;
            Balloon = Conversation.Balloons.FirstOrDefault(x => x.type == Balloon.Type.Entry);
            if (Balloon == null)
            {
                Debug.LogWarning($"엔트리 노드가 없음 ㅅㅂ | Conversation : {Conversation.key}");
                yield break;
            }

            while (true)
            {
                choiceArea.SetActive(false);
                Choices?.Clear();
                nextIcon.gameObject.SetActive(false);

                var children = GetChildren(Balloon);
                if(children.Count == 0) break;

                var linkedBalloons = children.Where(x => x.type == Balloon.Type.Dialogue || x.type == Balloon.Type.Action).ToList();
                foreach (var balloon in linkedBalloons)
                {
                    if(balloon.useChecker && !balloon.checker.IsTrue()) continue;
                    Balloon = balloon;
                    break;
                }
                Choices = Balloon.linkedBalloons.Where(x => x.type == Balloon.Type.Choice && (!x.useChecker || (x.useChecker && x.checker.IsTrue()))).ToList();
                yield return StartCoroutine(PrintContents());
            }

            SceneManager.UnloadSceneAsync(sceneName);
        }

        List<Balloon> GetChildren(Balloon balloon)
        {
            return balloon.linkedBalloons;
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

            if (Balloon.actor != null)
            {
                var npc = NPCDB.Instance.NPCs.Find(x => x.name == Balloon.actor.name);
                if (!npc.Runtime.IsEncountered) npc.SetEncountered(true);
            }
            
            
            // TODO: 여기서 로컬라이징.
            var targetText = Balloon.text;
            var actorName = Balloon.actor == null || Balloon.actor.name == "System" ? "" : Balloon.actor.name;
            
            // 실제 출력되는 부분.
            switch (_displayMode)
            {
                case DialogueDisplayMode.Default:
                    if(nameField != null) nameField.text = actorName;
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
                case DialogueDisplayMode.Cutscene:
                    if(nameField != null) nameField.gameObject.SetActive(false);
                    if (HideName)
                    {
                        textField.text = targetText;
                    }
                    else
                    {
                        var withColon = string.IsNullOrWhiteSpace(actorName) ? "" : $"{actorName} :  ";
                        textField.text = $"{withColon}{targetText}";
                    }
                    textField.alpha = 0f;

                    if (_textFadeTween == null) _textFadeTween = textField.DOFade(1, TextFadeInTime).SetAutoKill(false).SetEase(Ease.OutCubic);
                    else
                    {
                        _textFadeTween.ChangeValues(textField.color, textField.color.SetA(1), TextFadeInTime);
                        _textFadeTween.Restart();
                    }

                    yield return _textFadeTween.WaitForCompletion();
                    if (CutsceneTextSpeed <= 0 || CutsceneTextSpeed > 0.5f)
                    {
                        Debug.LogWarning($"[Dialogue UI] CutsceneTextSpeed가 유효하지 않은 값이라 기본값으로 변경함.");
                        CutsceneTextSpeed = Default_CutsceneTextInterval;
                    }

                    // 일단 글자수 만큼의 시간을 기다리는데, 도중에 Next가 트리거되면 페이드 아웃으로 넘어감. 아직은 컷씬 도중에 Next를 트리거하는게 없지만 성우녹음 등을 하면 필요해질듯.
                    for (var f = 0f; f < targetText.Length * CutsceneTextSpeed; f += Time.deltaTime)
                    {
                        yield return null;
                        if(_isNextTriggered) break;
                    }
                    break;
                default: goto case DialogueDisplayMode.Default;
            }
            
            _isNextTriggered = false;

            
            // 선택지.
            yield return new WaitForSeconds(ChoiceWaitTime);
            if(Choices == null || Choices.Count == 0)
            {
                if(_displayMode != DialogueDisplayMode.Cutscene)
                {
                    nextIcon.gameObject.SetActive(true);
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
            
            if(_displayMode == DialogueDisplayMode.Cutscene)
            {
                _textFadeTween.ChangeValues(textField.color, textField.color.SetA(0f), TextFadeOutTime);
                _textFadeTween.Restart();
                yield return _textFadeTween.WaitForCompletion();
                yield return new WaitForSeconds(CutsceneSubtitleInterval);
            }
        }
    }
}