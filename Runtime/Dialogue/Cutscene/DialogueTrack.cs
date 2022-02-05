using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace OcDialogue.Cutscene
{
    [TrackColor(0.5f, 0.8f, 1f)]
    [TrackClipType(typeof(DialogueClip))]
    [TrackBindingType(typeof(Conversation))]
    public class DialogueTrack : TrackAsset
    {
        public Conversation Conversation => conversation;
        public Conversation conversation;
        public int ClipCount => m_Clips.Count;
        public CutsceneBehaviour CutsceneBehaviour => _behaviour;
        public PlayableDirector Director => _director;
        public IEnumerable<DialogueClip> Clips => _clips;
        public IEnumerable<DialogueClipBehaviour> ClipBehaviours => _clipBehaviours;

        event Action<DialogueClipBehaviour> _onClipStart;
        event Action<DialogueClipBehaviour> _onClipFadeOut;
        event Action<DialogueClipBehaviour> _onClipEnd;
        
        List<DialogueClip> _clips;
        List<DialogueClipBehaviour> _clipBehaviours;

        CutsceneBehaviour _behaviour;
        PlayableDirector _director;
        SignalReceiver _signalReceiver;

        protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
        {
            if (!Application.isPlaying) return base.CreatePlayable(graph, gameObject, clip);
            var c = (DialogueClip)clip.asset;
            c.Init(CutsceneBehaviour, this, clip);
            c.OnCreatePlayable += behaviour =>
            {
                behaviour.AssignCallbacks(_onClipStart, _onClipFadeOut, _onClipEnd);
                _clipBehaviours.Add(behaviour);
            }; 
            if(_signalReceiver == null) _signalReceiver = gameObject.GetComponent<SignalReceiver>();
            
            // playable이 생성되기 전에 초기화해야 각 변수가 null 없이 할당됨.
            var playable = base.CreatePlayable(graph, gameObject, clip);
            return playable;
        }

        public void Init(CutsceneBehaviour behaviour, 
            Action<DialogueClipBehaviour> onClipStart,
            Action<DialogueClipBehaviour> onClipFadeOut,
            Action<DialogueClipBehaviour> onClipEnd)
        {
            _behaviour = behaviour;
            _director = behaviour.director;
            _clips = new List<DialogueClip>();
            _clipBehaviours = new List<DialogueClipBehaviour>();

            _onClipStart = onClipStart;
            _onClipFadeOut = onClipFadeOut;
            _onClipEnd = onClipEnd;
        }

        public void Release()
        {
            _clips.Clear();
            _clipBehaviours.Clear();
        }

        public int GetCurrentClipIndex()
        {
            for (int i = 0; i < _clipBehaviours.Count; i++)
            {
                if (_clipBehaviours[i].PlayState == ClipPlayState.IsPlaying) return i;
            }

            return -1;
        }
        
        public double GetClipStartTime(int index)
        {
            return m_Clips[index].start;
        }

#if UNITY_EDITOR
        [ReadOnly]public string e_directorSceneName;
        [ReadOnly]public string e_gaoName;
        void OnDestroy()
        {
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                if (conversation != null) conversation.e_CutsceneReference.Remove(this);
            }
        }
        
        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                e_directorSceneName = director.gameObject.scene.name;
                e_gaoName = director.gameObject.name;
                conversation = director.GetGenericBinding(this) as Conversation;
                if(conversation != null)
                {
                    if (!conversation.e_CutsceneReference.Contains(this))
                    {
                        conversation.e_CutsceneReference.Add(this);
                        UnityEditor.EditorUtility.SetDirty(conversation);
                        UnityEditor.AssetDatabase.SaveAssetIfDirty(conversation);
                    }
                }
            }
            base.GatherProperties(director, driver);
        }
#endif
        
    }
}