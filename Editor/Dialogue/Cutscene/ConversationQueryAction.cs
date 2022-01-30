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

            var param = dialogueTrack.param;

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

                        dialogueClip.template.balloon = balloon;
                        clip.displayName = $"{actorName} {balloon.text}";
                        clip.start = time;
                        var duration = param.minimumDuration + balloon.text.Length * param.durationPerChar;
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