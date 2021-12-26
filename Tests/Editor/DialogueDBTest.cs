using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using OcDialogue;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class DialogueDBTest
{
    void ForeachBalloon(Action<Conversation, Balloon> action)
    {
        foreach (var conversation in DialogueAsset.Instance.Conversations)
        {
            foreach (var balloon in conversation.Balloons)
            {
                action?.Invoke(conversation, balloon);
            }
        }
    }

    [Test]
    public void EmptySubtitleTest()
    {
        ForeachBalloon((conversation, balloon) =>
        {
            if (balloon.type != Balloon.Type.Dialogue) return;
            if (!string.IsNullOrWhiteSpace(balloon.text)) return;
            Debug.LogError($"[{conversation.key}/{balloon.GUID}] 빈 말풍선이 존재함");
        });
    }

    [Test]
    public void CheckerTest()
    {
        ForeachBalloon((conversation, balloon) =>
        {
            if (!balloon.useChecker) return;
            if (DataCheckerTest.HasNull(balloon.checker))
                Debug.LogError($"[{conversation.key}/{balloon.GUID}] Balloon.Checker에 빈 요소가 존재함");
            
            if (DataCheckerTest.HasUnusedCheckeables(balloon.checker))
                Debug.LogError($"[{conversation.key}/{balloon.GUID}] Balloon.Checker에 사용되지 않은 Checkable이 존재함");
        });
    }

    [Test]
    public void SetterTest()
    {
        ForeachBalloon((conversation, balloon) =>
        {
            if (!balloon.useSetter) return;
            if (balloon.setters == null || balloon.setters.Length == 0 || balloon.setters.Any(x => x == null))
            {
                Debug.LogError($"[{conversation.key}/{balloon.GUID}] Balloon.Setter에 빈 Setter가 존재함");
                return;
            }

            if (balloon.setters.Any(x => x.targetData == null))
                Debug.LogError($"[{conversation.key}/{balloon.GUID}] Balloon.Setter에 Data가 비어있음");
        });
    }

    [Test]
    public void EventTest()
    {
        ForeachBalloon((conversation, balloon) =>
        {
            if (!balloon.useEvent) return;
            if (balloon.signal == null)
                Debug.LogError($"[{conversation.key}/{balloon.GUID}] Balloon.Signal이 비어있음");
        });
    }

    [Test]
    public void ImageTest()
    {
        ForeachBalloon((conversation, balloon) =>
        {
            if(!balloon.useImage && balloon.displayTargetImage != null)
                Debug.LogError($"[{conversation.key}/{balloon.GUID}] 사용되지 않는 이미지가 참조됨");
            if(!balloon.useImage) return;
            if(balloon.displayTargetImage == null)
                Debug.LogError($"[{conversation.key}/{balloon.GUID}] Balloon.displayTargetImage가 비어있음");
        });    
    }

}