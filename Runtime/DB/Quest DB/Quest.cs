using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public class Quest : ScriptableObject
    {
        public enum State
        {
            None,
            WorkingOn,
            Finished
        }
        public string key;
        public List<DataRow> DataList;
    }
}
