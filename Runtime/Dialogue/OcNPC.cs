using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    public abstract class OcNPC : OcData
    {
        public override string Address { get; }
        [HorizontalGroup("2"), HideLabel, ColorUsage(false)]
        public Color color;
    }
}