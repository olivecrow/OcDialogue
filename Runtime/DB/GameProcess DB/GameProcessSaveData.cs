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
        public List<DynamicData> DynamicDataList;
    }
}
