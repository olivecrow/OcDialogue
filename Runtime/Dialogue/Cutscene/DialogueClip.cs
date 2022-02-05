using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace OcDialogue.Cutscene
{
    [Serializable]
    public class DialogueClip : PlayableAsset, ITimelineClipAsset
    {
        public Conversation Conversation;
        public Balloon Balloon;
        public DialogueClipBehaviour template;
        public ClipCaps clipCaps => ClipCaps.None;
        public CutsceneBehaviour CutsceneBehaviour { get; private set; }
        public DialogueTrack DialogueTrack { get; private set; }
        public PlayableDirector Director { get; private set; }
        public DialogueClipBehaviour ClipBehaviour { get; private set; }
        public TimelineClip TimelineClip { get; private set; }
        public event Action<DialogueClipBehaviour> OnCreatePlayable;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            if(!Application.isPlaying) return ScriptPlayable<DialogueClipBehaviour>.Create(graph, template);
            var playable = ScriptPlayable<DialogueClipBehaviour>.Create(graph, template);
            ClipBehaviour = playable.GetBehaviour();
            ClipBehaviour.Init(CutsceneBehaviour, this, DialogueTrack);
            OnCreatePlayable?.Invoke(ClipBehaviour);
            return playable;
        }

        public void Init(CutsceneBehaviour cutscene, DialogueTrack track, TimelineClip clip)
        {
            CutsceneBehaviour = cutscene;
            Director = CutsceneBehaviour.director;
            DialogueTrack = track;
            TimelineClip = clip;
        }
    }
}