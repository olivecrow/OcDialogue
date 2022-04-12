using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using OcDialogue;
using OcDialogue.Cutscene;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Timeline;

public class DialogueDBTest
{
    void ForeachBalloon(Action<Conversation, Balloon> action)
    {
        foreach (var conversation in DialogueAsset.Instance.Conversations)
        {
            if (conversation == null)
            {
                Debug.LogError($"conversation이 비어있음");
                continue;
            }
            if(conversation.Balloons == null)
            {
                Debug.LogError($"Conversation : {conversation.name}의 Balloons가 비어있음");
                continue;
            }
            foreach (var balloon in conversation.Balloons)
            {
                action?.Invoke(conversation, balloon);
            }
        }
    }

    [Test]
    public void EmptySubtitleTest()
    {
        Debug.Log($"Balloon에 대한 빈 말풍선 테스트 시작...");
        ForeachBalloon((conversation, balloon) =>
        {
            if (balloon.type != Balloon.Type.Dialogue) return;
            if (!string.IsNullOrWhiteSpace(balloon.text)) return;
            Debug.LogError($"[{conversation.key}/{balloon.GUID}] 빈 말풍선이 존재함");
        });
        Debug.Log($"-Balloon에 대한 빈 말풍선 테스트 종료...");
    }

    [Test]
    public void CheckerTest()
    {
        Debug.Log($"Balloon에 대한 useChecker 테스트 시작...");
        ForeachBalloon((conversation, balloon) =>
        {
            if (!balloon.useChecker) return;
            if (DataCheckerTest.HasNull(balloon.checker))
                Debug.LogError($"[{conversation.key}/{balloon.GUID}] Balloon.Checker에 빈 요소가 존재함");
            
            if (DataCheckerTest.HasUnusedCheckeables(balloon.checker))
                Debug.LogError($"[{conversation.key}/{balloon.GUID}] Balloon.Checker에 사용되지 않은 Checkable이 존재함");
        });
        Debug.Log($"-Balloon에 대한 useChecker 테스트 종료...");
    }

    [Test]
    public void SetterTest()
    {
        Debug.Log($"Balloon에 대한 useSetter 테스트 시작...");
        ForeachBalloon((conversation, balloon) =>
        {
            if (!balloon.useSetter) return;
            if (balloon.setters == null || balloon.setters.Length == 0 || balloon.setters.Any(x => x == null))
            {
                Debug.LogError($"[{conversation.key}/{balloon.GUID}] Balloon.Setter에 빈 Setter가 존재함");
                return;
            }

            if (balloon.setters.Any(x => x.TargetData == null))
                Debug.LogError($"[{conversation.key}/{balloon.GUID}] Balloon.Setter에 Data가 비어있음");
        });
        Debug.Log($"-Balloon에 대한 useSetter 테스트 종료...");
    }

    [Test]
    public void EventTest()
    {
        Debug.Log($"Balloon에 대한 useEvent 테스트 시작...");
        ForeachBalloon((conversation, balloon) =>
        {
            if (!balloon.useEvent) return;
            if (balloon.signal == null)
                Debug.LogError($"[{conversation.key}/{balloon.GUID}] Balloon.Signal이 비어있음");
        });
        Debug.Log($"-Balloon에 대한 useEvent 테스트 종료...");
    }

    [Test]
    public void ImageTest()
    {
        Debug.Log($"Balloon에 대한 Image 테스트 시작...");
        ForeachBalloon((conversation, balloon) =>
        {
            if(!balloon.useImage && balloon.displayTargetImage != null)
                Debug.LogError($"[{conversation.key}/{balloon.GUID}] 사용되지 않는 이미지가 참조됨");
            if(!balloon.useImage) return;
            if(balloon.displayTargetImage == null)
                Debug.LogError($"[{conversation.key}/{balloon.GUID}] Balloon.displayTargetImage가 비어있음");
        });    
        Debug.Log($"-Balloon에 대한 Image 테스트 종료...");
    }

    [Test]
    public void CutsceneNullTest()
    {
        Debug.Log($"DialogueTrack을 사용하는 타임라인 에셋의 Null 테스트 시작...");
        var allTimelineGUID = AssetDatabase.FindAssets("t:TimelineAsset");

        foreach (var guid in allTimelineGUID)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(path);
            var tracks = timeline.GetOutputTracks();
            var dialogueTrack = tracks.FirstOrDefault(x => x.outputs.ElementAt(0).outputTargetType == typeof(Conversation));
            if(dialogueTrack == null) continue;

            var output = dialogueTrack.outputs.ElementAt(0); 
            if(output.sourceObject == null) Debug.LogError($"타임라인 에셋 : {timeline.name} 에 Conversation 바인딩이 비어있음");
            var clips = dialogueTrack.GetClips();
            foreach (var clip in clips)
            {
                var dialogueClip = clip.asset as DialogueClip;
                if(dialogueClip.Balloon == null) Debug.LogError($"타임라인 에셋 : {timeline.name} 의 트랙 중 {dialogueClip.name}에 Balloon이 비어있음");
                if(dialogueClip.Conversation == null) Debug.LogError($"타임라인 에셋 : {timeline.name} 의 트랙 중 {dialogueClip.name}에 Conversation이 비어있음");
            }
        }
        Debug.Log($"-DialogueTrack을 사용하는 타임라인 에셋의 Null 테스트 종료...");
    }
}