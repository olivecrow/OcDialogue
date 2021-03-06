using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyDB
{
    [Serializable]
    public struct BattleStat
    {
        [ShowInInspector][PropertyOrder(-3), HorizontalGroup("phy")][LabelWidth(100)]public float TotalSum => PhysicalSum + ElementalSum;
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

        public void PhysicalAdd(float value)
        {
            Strike += value;
            Slice += value;
            Thrust += value;
        }
        
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

        /// <summary>
        /// ?????? ????????? ????????? ?????? ??????.
        /// </summary>
        /// <param name="uniformValue"></param>
        public void Multiply(float uniformValue)
        {
            PhysicalMultiply(uniformValue);
            ElementalMultiply(uniformValue);
        }

        /// <summary>
        /// ?????? ????????? ???????????? ??? ????????? ??????
        /// </summary>
        /// <param name="multiplier"></param>
        public void Multiply(BattleStat multiplier)
        {
            PhysicalMultiply(multiplier.Strike, multiplier.Slice, multiplier.Thrust);
            ElementalMultiply(multiplier.Fire, multiplier.Ice, multiplier.Lightening, multiplier.Dark);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(BattleStat other)
        {
            return Strike.Equals(other.Strike) && Slice.Equals(other.Slice) && Thrust.Equals(other.Thrust) &&
                   Fire.Equals(other.Fire) && Ice.Equals(other.Ice) && Lightening.Equals(other.Lightening) &&
                   Dark.Equals(other.Dark);
        }
        public override bool Equals(object obj)
        {
            return obj is BattleStat other && Equals(other);
        }

        public static bool operator ==(BattleStat a, BattleStat b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(BattleStat a, BattleStat b)
        {
            return !(a == b);
        }

        /// <summary>
        /// ????????? ?????? ?????? ????????? ?????????. ????????? ???????????? ??????.
        /// </summary>
        public static BattleStat operator * (BattleStat a, float mult)
        {
            a.Multiply(mult);
            return a;
        }
        /// <summary>
        /// ????????? ????????? ?????? ????????? ?????????. ????????? ???????????? ??????.
        /// </summary>
        public static BattleStat operator * (BattleStat a, BattleStat b)
        {
            a.Multiply(b);
            return a;
        }
        
        public static BattleStat operator +(BattleStat a, BattleStat b)
        {
            a.Slice += b.Slice;
            a.Strike += b.Strike;
            a.Thrust += b.Thrust;

            a.Fire += b.Fire;
            a.Ice += b.Ice;
            a.Lightening += b.Lightening;
            a.Dark += b.Dark;

            return a;
        }

        public static BattleStat operator -(BattleStat a, BattleStat b)
        {
            a.Slice -= b.Slice;
            a.Strike -= b.Strike;
            a.Thrust -= b.Thrust;

            a.Fire -= b.Fire;
            a.Ice -= b.Ice;
            a.Lightening -= b.Lightening;
            a.Dark -= b.Dark;

            return a;
        }

        public override string ToString()
        {
            return
                $"Strike : {Strike} | Slice : {Slice} | Thrust : {Thrust} => [Sum : {PhysicalSum}] [Avg : {PhysicalAvg}]\n" +
                $"Fire : {Fire} | Ice : {Ice} | Lightening : {Lightening} | Dark : {Dark} => [Sum : {ElementalSum}] [Avg : {ElementalAvg}]";
        }
    }
}
