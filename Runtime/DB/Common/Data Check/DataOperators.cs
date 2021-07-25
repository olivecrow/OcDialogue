using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public enum Operator
    {
        Equal,
        NotEqual,
        Greater,
        GreaterEqual,
        Less,
        LessEqual
    }

    public enum CompareFactor
    {
        Float,
        Int,
        String,
        Boolean,
        QuestState,
        ItemCount,
        NpcEncounter
    }
}
