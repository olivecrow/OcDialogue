using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue.DB;
using OcUtility;
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

        public override string ToString()
        {
            return Type switch
            {
                DataRowType.Bool => BoolValue.DRT(),
                DataRowType.Int => IntValue.ToString("N"),
                DataRowType.Float => FloatValue.ToString("N"),
                DataRowType.String => StringValue,
                DataRowType.Vector => VectorValue.ToString(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
