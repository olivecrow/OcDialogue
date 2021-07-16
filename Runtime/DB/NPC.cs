using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    public class NPC : ComparableData
    {
        public enum Gender
        {
            None,
            Male,
            Female
        }
        public override string Key => NPCName;

        [TableColumnWidth(150, false), VerticalGroup("NPC"), HideLabel]public string NPCName;
        [VerticalGroup("NPC"), HideLabel]public Gender gender;
        /// <summary> Dialogue Editor 등에서 한 눈에 알아보기 쉽도록 지정하는 고유색. </summary>
        [VerticalGroup("NPC"), HideLabel, ColorUsage(false)]public Color color;
        
        /// <summary> 게임 내의 도감에서 보여지는 설명 </summary>
        [Multiline]public string description;

        public bool IsEncounter { get; set; }
        
        public override bool IsTrue(CompareFactor factor, Operator op, object value)
        {
            if (factor != CompareFactor.NpcEncounter) return false;

            return op switch
            {
                Operator.Equal => IsEncounter == (bool) value,
                Operator.NotEqual => IsEncounter != (bool) value,
                _ => false
            };
        }
    }
}
