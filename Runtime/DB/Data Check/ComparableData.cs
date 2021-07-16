using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public abstract class ComparableData : ScriptableObject
    {
        public abstract string Key { get; }
        public abstract bool IsTrue(CompareFactor factor, Operator op, object value);
    }
}
