using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public abstract class ComparableData : ScriptableObject, IComparableData
    {
        public abstract string Key { get; }
        public abstract bool IsTrue(CompareFactor factor, Operator op, object value1);
    }

    public interface IComparableData
    {
        public string Key { get; }
        public bool IsTrue(CompareFactor factor, Operator op, object value1);
    }
}
