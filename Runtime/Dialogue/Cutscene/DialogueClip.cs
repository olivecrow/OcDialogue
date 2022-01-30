using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace OcDialogue.Cutscene
{
    [Serializable]
    public class DialogueClip : PlayableAsset, ITimelineClipAsset
    {
        public DialogueClipBehaviour template;
        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<DialogueClipBehaviour>.Create(graph, template);
            return playable;
        }
    }
}