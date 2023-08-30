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
        public ClipCaps clipCaps => ClipCaps.None;
        public CutsceneBehaviour CutsceneBehaviour;
        public DialogueTrack DialogueTrack;
        public PlayableDirector Director;
        public DialogueClipBehaviour ClipBehaviour;
        public TimelineClip TimelineClip;
        public event Action<DialogueClipBehaviour> OnCreatePlayable;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<DialogueClipBehaviour>.Create(graph);
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