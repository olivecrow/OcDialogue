using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public class Enemy : ComparableData
    {
        public override string Key { get; }
        public override bool IsTrue(CompareFactor factor, Operator op, object value1)
        {
            throw new System.NotImplementedException();
        }
    }
}
