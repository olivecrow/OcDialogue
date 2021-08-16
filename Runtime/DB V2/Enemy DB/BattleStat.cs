using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    [Serializable]
    public struct BattleStat
    {
        [ShowInInspector][PropertyOrder(-2), HorizontalGroup("phy")][LabelWidth(100)]public float PhysicalSum => Strike + Slice + Thrust;
        [ShowInInspector][PropertyOrder(-2), HorizontalGroup("phy")][LabelWidth(100)]public float PhysicalAvg => PhysicalSum / 3f;
        
        [PropertyOrder(1)][LabelWidth(100)]public float Strike;
        [PropertyOrder(1)][LabelWidth(100)]public float Slice;
        [PropertyOrder(1)][LabelWidth(100)]public float Thrust;
        
        [ShowInInspector][PropertyOrder(2), HorizontalGroup("el")][LabelWidth(100)]public float ElementalSum => Fire + Ice + Lightening + Dark;
        [ShowInInspector][PropertyOrder(2), HorizontalGroup("el")][LabelWidth(100)]public float ElementalAvg => ElementalSum / 4f;
        [PropertyOrder(4)][LabelWidth(100)]public float Fire;
        [PropertyOrder(4)][LabelWidth(100)]public float Ice;
        [PropertyOrder(4)][LabelWidth(100)]public float Lightening;
        [PropertyOrder(4)][LabelWidth(100)]public float Dark;

        public void PhysicalMultiply(float uniformValue)
        {
            Strike *= uniformValue;
            Slice *= uniformValue;
            Thrust *= uniformValue;
        }

        public void PhysicalMultiply(float strike, float slice, float thrust)
        {
            Strike *= strike;
            Slice *= slice;
            Thrust *= thrust;
        }

        public void ElementalMultiply(float uniformValue)
        {
            Fire *= uniformValue;
            Ice *= uniformValue;
            Lightening *= uniformValue;
            Dark *= uniformValue;
        }
        public void ElementalMultiply(float fire, float ice, float lightening, float dark)
        {
            Fire *= fire;
            Ice *= ice;
            Lightening *= lightening;
            Dark *= dark;
        }

        public void Multiply(float uniformValue)
        {
            PhysicalMultiply(uniformValue);
            ElementalMultiply(uniformValue);
        }

        public void Multiply(BattleStat multiplier)
        {
            PhysicalMultiply(multiplier.Strike, multiplier.Slice, multiplier.Thrust);
            ElementalMultiply(multiplier.Fire, multiplier.Ice, multiplier.Lightening, multiplier.Dark);
        }
    }
}
