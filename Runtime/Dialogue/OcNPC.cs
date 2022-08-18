using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    [Obsolete("이 클래스는 더이상 사용되지 않음. IOcNPC 인터페이스를 대신 사용할 것.")]
    public abstract class OcNPC : OcData
    {
        public override string Address { get; }
        [HorizontalGroup("2"), HideLabel, ColorUsage(false)]
        public Color color;
    }
}