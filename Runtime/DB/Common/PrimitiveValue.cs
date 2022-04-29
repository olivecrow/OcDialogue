using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue.DB;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    [Serializable]
    public struct PrimitiveValue
    {
        [HideInInspector]public DataRowType Type;
        [ShowIf("Type", DataRowType.Bool), HideLabel][ExplicitToggle()]
        public bool BoolValue;
        [ShowIf("Type", DataRowType.Int), HideLabel] 
        public int IntValue;
        [ShowIf("Type", DataRowType.Float), HideLabel] 
        public float FloatValue;
        [ShowIf("Type", DataRowType.String), HideLabel] 
        public string StringValue;
        [ShowIf("Type", DataRowType.Vector), HideLabel]
        public Vector4 VectorValue;
    }
}
