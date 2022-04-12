using System;
using System.Collections.Generic;
using System.Linq;
using OcDialogue.Cutscene;
using UnityEditor;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace OcDialogue.Editor.Cutscene
{
    [MenuEntry("Custom Actions/Query Conversation")]
    public class ConversationQueryAction : TimelineAction
    {
        const double durationPerChar = 0.15;
        const double minimumDuration = 2;
        public override ActionValidity Validate(ActionContext context)
        {
            var dialogueTrack = context.tracks.FirstOrDefault(x => x is DialogueTrack);

            if (dialogueTrack == null)
            {
                return ActionValidity.NotApplicable;
            }

            if (dialogueTrack.outputs.All(x => x.sourceObject == null)) return ActionValidity.Invalid;
            
            return ActionValidity.Valid;
        }
        
        public override bool Execute(ActionContext context)
        {
            var dialogueTrack = context.tracks.FirstOrDefault(x => x is DialogueTrack) as DialogueTrack;

            var conversation = context.director.GetGenericBinding(dialogueTrack) as Conversation;
            if (conversation == null)
            {
                Debug.Log($"Conversation 바인딩이 비어있음");
                return false;
            }
            
            double time = 0;
            Balloon balloon = conversation.Balloons[0];
            while (true)
            {
                switch (balloon.type)
                {
                    case Balloon.Type.Entry:
                        break;
                    case Balloon.Type.Dialogue:
                        var clip = dialogueTrack.CreateClip<DialogueClip>();
                        var dialogueClip = clip.asset as DialogueClip;

                        var actorName = balloon.actor == null ? "" : $"{balloon.actor.name} :";

                        dialogueClip.Balloon = balloon;
                        dialogueClip.Conversation = conversation;
                        clip.displayName = $"{actorName} {balloon.text}";
                        clip.start = time;
                        // TODO : 나중에 더빙 추가하면 음성 파일을 기준으로 한 길이를 넣기.
                        var duration = minimumDuration + balloon.text.Length * durationPerChar;
                        clip.duration = duration;
                        time += duration;
                        break;
                    case Balloon.Type.Choice:
                        // TODO : 마커 추가하기, 타임라인 일시정지 시키기.
                        break;
                    case Balloon.Type.Action:
                        time += 1;
                        break;
                }

                if (balloon.linkedBalloons.Count == 0) break;
                else balloon = balloon.linkedBalloons[0];
            }

            return true;
        }
    }
}