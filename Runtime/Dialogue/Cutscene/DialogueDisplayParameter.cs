using System;
using OcUtility;
using UnityEngine;

namespace OcDialogue.Cutscene
{
    public class DialogueDisplayParameter : ScriptableObject
    {
        public static DialogueDisplayParameter Instance
        { 
            get
            {
                if (_instance == null) _instance = Resources.Load<DialogueDisplayParameter>("Default Dialogue Display Parameter");
                return _instance;
            }
        }
        static DialogueDisplayParameter _instance;

        public string DialogueSceneName = "Dialogue UI";
        public float canvasFadeInDuration = 0.7f;
        public float canvasFadeOutDuration = 0.95f;
        public float textFadeInDuration = 0.1f;
        public float textFadeOutDuration = 0.2f;
        public float minimumDuration = 2f;
        public float durationPerChar = 0.17f;
        [Range(0.1f, 0.999f)] public float autoPauseTime = 0.95f;
    }
}