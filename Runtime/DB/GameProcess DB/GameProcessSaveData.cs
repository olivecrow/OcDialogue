using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    [Serializable]
    public class GameProcessSaveData
    {
        public Dictionary<string, string> DataRowContainerDict;
        public TransformData PlayerTransform;
        public float PlayerHP;
        public DateTime GameTime;
        public TimeSpan PlayTime;
    }
}
