using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public interface IComparableData
    {
        public string Key { get; }
        public bool IsTrue(CompareFactor factor, Operator op, object value);
    }
}
