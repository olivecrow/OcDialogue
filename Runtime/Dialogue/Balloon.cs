using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using OcDialogue.DB;
using OcUtility;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Timeline;
using Random = UnityEngine.Random;

namespace OcDialogue
{
    public class Balloon : ScriptableObject, IOcDataSelectable
    {
        public enum Type
        {
            Entry,
            Dialogue,
            Choice,
            Action
        }

        public enum ChoiceCheckerDisplayMode
        {
            Disable,
            Hide
        }

        public enum ActionType
        {
            None,
            SubEntry,
            RandomSelection,
            CycleSelection,
            SequenceSelection
        }

        public enum SubEntryDataType
        {
            String,
            OcData
        }
        public OcData TargetData
        {
            get => subEntryTriggerData;
            set => subEntryTriggerData = value;
        }

        public string Detail { get; }
        
        [ReadOnly] public string GUID;
        [ReadOnly] public Type type;

        [ShowIf("type", Type.Dialogue)] [ValueDropdown("GetNPCList")]
        public OcData actor;

        [InfoBox("타입이 Action일땐 텍스트가 참고용 설명으로만 쓰이고 대화에서 나타나지 않음\n오로지 Checker, Setter, Event, Image용도로만 쓰임", 
            VisibleIf = "@type == Balloon.Type.Action")]
        [HideIf("type", Type.Entry), TextArea(3, 5)]
        public string text;

        
        public Vector2 position
        {
            get => _position;
            set => _position = value;
        }
        public Vector2 _position;

        [ValueDropdown("GetBalloonText")]
        public List<Balloon> linkedBalloons = new List<Balloon>();
        [ShowIf("@type == Type.Choice && useChecker")]
        public ChoiceCheckerDisplayMode choiceCheckerDisplayMode;
        [HideIf("type", Type.Entry)] public bool useChecker;

        [InfoBox("Check Factor가 하나도 없음", InfoMessageType.Error, "@checker != null && checker.factors != null && checker.factors.Length == 0")]
        [ShowIf("useChecker"), HideLabel, BoxGroup("Checker"), GUIColor(1f, 2f, 1f)]
        public DataChecker checker;

        [HideIf("type", Type.Entry)] public bool useHighlight;
        [InfoBox("Check Factor가 하나도 없음", InfoMessageType.Error, "@highlightCondition != null && highlightCondition.factors != null && highlightCondition.factors.Length == 0")]
        [ShowIf("useHighlight"), HideLabel, BoxGroup("Highlight"), GUIColor(1f, 1.8f, 2f)]public DataChecker highlightCondition;
        
        [HideIf("type", Type.Entry)] public bool useSetter;

        [InfoBox("Setter가 없음.", InfoMessageType.Warning, "@useSetter && (setters == null || setters.Length == 0)")]
        [ShowIf("useSetter"), HideLabel, BoxGroup("Setter"), GUIColor(2.8f, 1.8f, 1.5f)]
        public DataSetter[] setters;

        public bool useEvent;
        
        [ShowIf("useEvent")] [InfoBox("Signal 에셋이 필요함.", InfoMessageType.Warning, "@useEvent && signal == null")]
        [ValueDropdown("GetSignalAssetList")]
        [Indent()]
        public SignalAsset signal;
        
        [InfoBox("이미지를 사용하지 않으면 참조를 해제하셈", InfoMessageType.Warning, "@!useImage && displayTargetImage.editorAsset != null")]
        [InlineButton("ReleaseUnusedImage", "참조 해제", ShowIf = "@!useImage && displayTargetImage != null")]
        public bool useImage;
        
        [Indent()]
        [InfoBox("이미지 사이즈가 이상함", InfoMessageType.Warning, "IsImageSizeMismatching")]
        [ShowIf("useImage")] 
        public ImageViewerSize imageViewerSize;
        
        [Indent()]
        [InfoBox("이미지가 없음", InfoMessageType.Error, "@displayTargetImage == null")]
        [InlineButton("QueryImageFromPreviousBalloon", "앞에꺼 사용")]
        [ShowIf("useImage")] 
        public AssetReferenceSprite displayTargetImage;
        
        [Indent()][ShowIf("@useImage && imageViewerSize == ImageViewerSize.FloatingSize")] 
        public Vector2 imageSizeOverride;
        
        [TextArea]public string description;
#if UNITY_EDITOR
        [TextArea] public string localizationComment;  
#endif

        [Indent()][ShowIf("type", Type.Action)]public ActionType actionType;
        [Indent()][ShowIf("type", Type.Action)]public SubEntryDataType subEntryDataType;

        [Indent(2)] [ShowIf("@type == Type.Action && subEntryDataType == SubEntryDataType.String")]
        public string subEntryTrigger;

        [Indent(2)] [ShowIf("@type == Type.Action && subEntryDataType == SubEntryDataType.OcData")][InlineButton("OpenDataSelectWindow", "선택")]
        public OcData subEntryTriggerData;

        public float waitTime;

        public AssetReferenceT<AudioClip> audioClip;

        public bool IsAvailable => !useChecker || checker.IsTrue();
        public void OnDataApplied() { }

        public Balloon GetNext(List<Balloon> choices, ref int cycleIndex)
        {
            choices?.Clear();
            if (linkedBalloons.Count == 0) return null;

            
            if (type == Type.Action)
            {
                switch (actionType)
                {
                    case ActionType.None:
                    case ActionType.SubEntry:
                        return get_next();
                    case ActionType.RandomSelection:
                        return get_random();
                    case ActionType.CycleSelection:
                        return get_cycle(ref cycleIndex);
                    case ActionType.SequenceSelection:
                        return get_sequence(ref cycleIndex);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return get_next();

            Balloon get_next()
            {
                query_choices(this);
                for (int i = 0; i < linkedBalloons.Count; i++)
                {
                    var b = linkedBalloons[i];
                    if (is_available(b))
                    {
                        return b;
                    }
                }
                return null;
            }

            Balloon get_random()
            {
                var random = new System.Random();
                // 중복되지 않은 인덱스로 반복을 돌려야해서 linkedBalloon의 개수 내에서 순서를 섞어서 사용함.
                var randomOrderedIndex = Enumerable.Range(0, linkedBalloons.Count).OrderBy(x => random.Next());

                for (int i = 0; i < linkedBalloons.Count; i++)
                {
                    var b = linkedBalloons[randomOrderedIndex.ElementAt(i)];
                    if (is_available(b))
                    {
                        query_choices(b);
                        return b;
                    }
                }

                return null;
            }
            
            Balloon get_cycle(ref int cycleIndex)
            {
                var iteration = 0;
                while (iteration < linkedBalloons.Count)
                {
                    cycleIndex = (int)Mathf.Repeat(cycleIndex, linkedBalloons.Count);
                
                    var b = linkedBalloons[cycleIndex];
                    cycleIndex++;
                    if (is_available(b))
                    {
                        query_choices(b);
                        return b;
                    }
                    iteration++;
                }

                return null;
            }

            Balloon get_sequence(ref int cycleIndex)
            {
                var iteration = 0;
                while (iteration < linkedBalloons.Count)
                {
                    cycleIndex = Mathf.Clamp(cycleIndex, 0, linkedBalloons.Count - 1);
                
                    var b = linkedBalloons[cycleIndex];
                    cycleIndex++;
                    if (is_available(b))
                    {
                        query_choices(b);
                        return b;
                    }
                    iteration++;
                }

                return null;
            }

            bool is_available(Balloon b) => b.type != Type.Choice && b.IsAvailable;

            void query_choices(Balloon b)
            {
                for (int i = 0; i < b.linkedBalloons.Count; i++)
                {
                    var linked = b.linkedBalloons[i];
                    if (linked.type == Type.Choice && linked.IsAvailable) choices.Add(linked);   
                }

            }
        }



        public void UseBalloon(SignalReceiver signalReceiver)
        {
            if(useEvent)
            {
                if (signal == null)
                {
                    Debug.LogWarning($"Balloon({type}) : {text}|에서 이벤트를 사용하지만 SignalAsset이 설정되지 않음");
                }
                else
                {
                    if(signalReceiver == null) Debug.LogError($"Balloon({type}) : {text}|에서 이벤트를 사용하지만 SignalReceiver가 없음 | signal : {signal.name}");
                    else
                    {
                        var reaction =signalReceiver.GetReaction(signal);
                        
                        if(reaction == null)
                            Debug.LogWarning($"Balloon({type}) : {text}|에서 이벤트를 사용하지만 SignalReceiver에 이벤트가 설정되지 않음 | signal : {signal.name}");
                    }
                }
                
            }
            if(useSetter)
                foreach (var setter in setters) setter.Execute();
        }

        public override string ToString()
        {
            return $"type : {type} | {GUID} \n{(actor == null ? "NoActor" : actor.name)}:{text}";
        }

#if UNITY_EDITOR

        /// <summary> actor필드에서 NPC이름을 드롭다운으로 보여주기위한 리스트를 반환함. (Odin Inspector용) </summary>
        ValueDropdownList<OcData> GetNPCList() => DialogueAsset.Instance.GetNPCDropDown();

        ValueDropdownList<Balloon> GetBalloonText()
        {
            var list = new ValueDropdownList<Balloon>();
            if (linkedBalloons == null) return null;
            foreach (var linkedBalloon in linkedBalloons)
            {
                list.Add($"{linkedBalloon.text}   |   {linkedBalloon.GUID}", linkedBalloon);
            }

            return list;   
        }

        ValueDropdownList<SignalAsset> GetSignalAssetList()
        {
            var list = new ValueDropdownList<SignalAsset>();

            var assetGUIDs = AssetDatabase.FindAssets("t:SignalAsset");
            foreach (var guid in assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<SignalAsset>(path);
                list.Add(asset.name, asset);
            }

            return list;
        }

        bool IsImageSizeMismatching()
        {
            if (!useImage) return false;
            if (displayTargetImage == null) return false;
            if (displayTargetImage.Asset == null) return false;
            var image = displayTargetImage.Asset as Sprite;
            switch (imageViewerSize)
            {
                case ImageViewerSize.FullSize:
                    if (image.bounds.size.y < 800 || image.bounds.size.x < 1000) return true;
                    if (image.bounds.size.y > image.bounds.size.x * 0.7f) return true;
                    break;
                case ImageViewerSize.FloatingSize:
                    if (image.bounds.size.y >= 1080 || image.bounds.size.x >= 1920) return true;
                    break;
            }

            return false;
        }

        /// <summary> Editor Only. </summary>
        public void ReleaseUnusedImage()
        {
            if(useImage) return;
            if(displayTargetImage == null) return;
            displayTargetImage = null;
            Debug.Log($"[Balloon] 이미지 참조 해제 | balloon : {GUID} \n text : {text}");
        }
        
        /// <summary> Editor Only. </summary>
        public bool IsWarningOn()
        {
            if (useChecker)
            {
                if (checker.factors == null) return false;
                if(checker.factors.Length == 0) return true;
                foreach (var factor in checker.factors)
                {
                    if (factor.TargetData == null) return true;
                }
            }
            if (useSetter)
            {
                if(setters == null || setters.Length == 0) return true;
                foreach (var setter in setters)
                {
                    if (setter.TargetData == null) return true;
                }
            }

            if (useHighlight)
            {
                if (highlightCondition.factors == null) return false;
                if(highlightCondition.factors.Length == 0) return true;
                foreach (var factor in highlightCondition.factors)
                {
                    if (factor.TargetData == null) return true;
                }
            }
            if (useEvent)
            {
                if(signal == null) return true;
            }

            if (useImage)
            {
                if (IsImageSizeMismatching() || displayTargetImage.editorAsset == null) return true;
            }
            else
            {
                if (displayTargetImage != null && displayTargetImage.editorAsset != null) return true;
            }
            return false;
        }

        void QueryImageFromPreviousBalloon()
        {
            var parentConversation = DialogueAsset.Instance.Conversations.Find(x => x.Balloons.Contains(this));
            if (parentConversation == null)
            {
                Debug.LogWarning("[Balloon] 자신을 포함한 Conversation을 DialogueAsset.Instance에서 찾지 못 함.");
                return;
            }

            var parentBalloonsGUID = parentConversation.LinkData.Where(x => x.to == GUID).Select(x => x.@from);
            var parentBalloons = parentBalloonsGUID.Select(x => parentConversation.Balloons.Find(y => y.GUID == x));

            foreach (var balloon in parentBalloons)
            {
                if(!balloon.useImage || balloon.displayTargetImage == null) return;
                displayTargetImage = balloon.displayTargetImage;
#if UNITY_EDITOR
                Debug.Log($"[Balloon] <{balloon.text}> 라는 Balloon에서 <{displayTargetImage.editorAsset.name}>을 가져옴");          
#endif
                return;
            }
            
            Debug.Log($"[Balloon] 상위 Balloon중, 이미지를 사용하는 Balloon을 찾지 못 함.");
        }

        [Button]
        void AppendTMPSprite()
        {
            TMPSpriteSearchWindow.Open(InsertTMP_SpriteAtlas_Keyword);
        }
        void InsertTMP_SpriteAtlas_Keyword(TMP_SpriteAsset spriteAsset, Sprite sprite)
        {
            text += $"<sprite=\"{spriteAsset.name}\" name=\"{sprite.name}\">";
        }

        void OpenDataSelectWindow()
        {
            DataSelectWindow.Open(this);
        }
#endif
    }
}
