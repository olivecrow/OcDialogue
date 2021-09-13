using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcDialogue.DB;
using OcUtility;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Timeline;

namespace OcDialogue
{
    public class Balloon : ScriptableObject
    {
        public enum Type
        {
            Entry,
            Dialogue,
            Choice,
            Action
        }

        [ReadOnly] public string GUID;
        [ReadOnly] public Type type;

        [ShowIf("type", Type.Dialogue)] [ValueDropdown("GetNPCList")]
        public NPC actor;

        [InfoBox("타입이 Action일땐 텍스트가 참고용 설명으로만 쓰이고 대화에서 나타나지 않음\n오로지 Checker, Setter, Event, Image용도로만 쓰임", 
            VisibleIf = "@type == Balloon.Type.Action")]
        [HideIf("type", Type.Entry), TextArea]
        public string text;

        
        public Vector2 position
        {
            get => _position;
            set => _position = value;
        }
        public Vector2 _position;

        [ValueDropdown("GetBalloonText")]
        public List<Balloon> linkedBalloons;
        
        [HideIf("type", Type.Entry)] public bool useChecker;

        [InfoBox("Check Factor가 하나도 없음", InfoMessageType.Error, "@checker != null && checker.factors != null && checker.factors.Length == 0")]
        [ShowIf("useChecker"), HideLabel, BoxGroup("Checker"), GUIColor(1f, 2f, 1f)]
        public DataChecker checker;
        
        [HideIf("type", Type.Entry)] public bool useSetter;

        [InfoBox("Setter가 없음.", InfoMessageType.Warning, "@useSetter && (setters == null || setters.Length == 0)")]
        [ShowIf("useSetter"), HideLabel, BoxGroup("Setter"), GUIColor(2.8f, 1.8f, 1.5f)]
        public DataSetter[] setters;

        public bool useEvent;
        
        [ShowIf("useEvent")] [InfoBox("Signal 에셋이 필요함.", InfoMessageType.Warning, "@useEvent && signal == null")]
        [ValueDropdown("GetSignalAssetList")]
        [Indent()]
        public SignalAsset signal;
        
        [InfoBox("이미지를 사용하지 않으면 참조를 해제하셈", InfoMessageType.Warning, "@!useImage && displayTargetImage != null")]
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
        public Texture2D displayTargetImage;
        
        [Indent()][ShowIf("@useImage && imageViewerSize == ImageViewerSize.FloatingSize")] 
        public Vector2 imageSizeOverride;

#if UNITY_EDITOR
        public string description;
        /// <summary> actor필드에서 NPC이름을 드롭다운으로 보여주기위한 리스트를 반환함. (Odin Inspector용) </summary>
        ValueDropdownList<NPC> GetNPCList()
        {
            var list = new ValueDropdownList<NPC>();
            foreach (var npc in NPCDB.Instance.NPCs)
            {
                list.Add(npc.name, npc);
            }
        
            return list;
        }

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
            switch (imageViewerSize)
            {
                case ImageViewerSize.FullSize:
                    if (displayTargetImage.height < 800 || displayTargetImage.width < 1000) return true;
                    if (displayTargetImage.height > displayTargetImage.width * 0.7f) return true;
                    break;
                case ImageViewerSize.FloatingSize:
                    if (displayTargetImage.height >= 1080 || displayTargetImage.width >= 1920) return true;
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
                    if (factor.Data == null) return true;
                }
            }
            if (useSetter)
            {
                if(setters == null || setters.Length == 0) return true;
                foreach (var setter in setters)
                {
                    if (setter.Data == null) return true;
                }
            }
            if (useEvent)
            {
                if(signal == null) return true;
            }

            if (useImage)
            {
                if (IsImageSizeMismatching() || displayTargetImage == null) return true;
            }
            else
            {
                if (displayTargetImage != null) return true;
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
                Debug.Log($"[Balloon] <{balloon.text}> 라는 Balloon에서 <{displayTargetImage.name}>을 가져옴");
                return;
            }
            
            Debug.Log($"[Balloon] 상위 Balloon중, 이미지를 사용하는 Balloon을 찾지 못 함.");
        }
#endif

    }
}
