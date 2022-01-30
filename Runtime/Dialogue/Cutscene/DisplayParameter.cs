using System;
using OcUtility;

namespace OcDialogue.Cutscene
{
    [Serializable]
    public class DisplayParameter
    {
        public float canvasFadeInDuration = 0.7f;
        public float canvasFadeOutDuration = 0.95f;
        public float textFadeInDuration = 0.1f;
        public float textFadeOutDuration = 0.2f;
        public float minimumDuration = 2f;
        public float durationPerChar = 0.17f;
    }
}