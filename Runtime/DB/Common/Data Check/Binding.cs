using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public enum Condition
    {
        And,
        Or
    }
    [Serializable]
    public class Binding
    {
        public int Index { get; set; }
        public List<int> checkables;
        public Condition condition;
    }
}
