using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue.DB
{
    public enum Condition
    {
        And,
        Or
    }
    [Serializable]
    public class Binding
    {
        public int Index;
        public List<int> checkables;
        public Condition condition;

        public bool HasSameIndex(Binding binding)
        {
            for (int i = 0; i < checkables.Count; i++)
            {
                if (checkables[i] != binding.checkables[i]) return false;
            }

            return true;
        }
    }
}
