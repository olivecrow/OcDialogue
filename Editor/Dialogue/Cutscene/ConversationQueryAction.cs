using System;
using System.Collections.Generic;
using System.Linq;
using OcDialogue.Cutscene;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace OcDialogue.Editor.Cutscene
{
    [MenuEntry("OcDialogue/대화 가져오기", 1)]
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
            var director = context.director;
            if (director == null)
            {
                Debug.LogWarning($"에셋이 아닌 PlayableDirector가 있는 게임 오브젝트를 직접 선택해야함");
                return false;
            }
            var dialogueTrack = context.tracks.FirstOrDefault(x => x is DialogueTrack) as DialogueTrack;

            var conversation = context.director.GetGenericBinding(dialogueTrack) as Conversation;
            if (conversation == null)
            {
                Debug.Log($"Conversation 바인딩이 비어있음");
                return false;
            }

            var clips = dialogueTrack.GetClips().ToList();
            var count = dialogueTrack.ClipCount;
            for (int i = 0; i < count; i++)
            {
                dialogueTrack.DeleteClip(clips[0]);
                Object.DestroyImmediate(clips[0].asset);
                clips.RemoveAt(0);
            }

            var wnd = TimelineEditorWindow.focusedWindow as TimelineEditorWindow;
            wnd.SetTimeline(dialogueTrack.timelineAsset);

            
            Balloon balloon = conversation.Balloons[0];
            double time = balloon.waitTime;
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
                        var duration = DialogueAsset.Instance.minTime + balloon.text.Length * DialogueAsset.Instance.timePerChar;
                        var audiDuration = balloon.audioClip.editorAsset == null ? 0 : balloon.audioClip.editorAsset.length;
                        duration = Mathf.Max(duration, audiDuration);
                        clip.duration = duration;
                        time += duration;
                        time += balloon.waitTime;
                        break;
                    case Balloon.Type.Choice:
                        // TODO : 마커 추가하기, 타임라인 일시정지 시키기.
                        break;
                    case Balloon.Type.Action:
                        time += balloon.waitTime;
                        break;
                }

                if (balloon.linkedBalloons.Count == 0) break;
                else balloon = balloon.linkedBalloons[0];
            }
            wnd.SetTimeline(director);
            return true;
        }
    }
}