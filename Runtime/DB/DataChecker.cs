using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    [Serializable]
    public class DataChecker
    {
#if UNITY_EDITOR
        public DataSelector[] selectors;
#endif
        public DataRow[] targetData;
    }
}
