using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public interface IEquipment
    {
        int MaxUpgrade { get; }
        int CurrentUpgrade { get; set; }
        int MaxDurability { get; }
        float CurrentDurability { get; set; }
        float Weight { get; }
        float Equipped { get; set; }
    }
}
