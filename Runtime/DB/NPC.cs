using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    public class NPC : ScriptableObject
    {
        public enum Gender
        {
            None,
            Male,
            Female
        }
        [TableColumnWidth(150, false), VerticalGroup("NPC"), HideLabel]public string NPCName;
        [VerticalGroup("NPC"), HideLabel]public Gender gender;
        /// <summary> Dialogue Editor 등에서 한 눈에 알아보기 쉽도록 지정하는 고유색. </summary>
        [VerticalGroup("NPC"), HideLabel, ColorUsage(false)]public Color color;
        
        /// <summary> 게임 내의 도감에서 보여지는 설명 </summary>
        [Multiline]public string description;

    }
}
