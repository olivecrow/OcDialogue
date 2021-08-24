using System.Collections;
using System.Collections.Generic;
using OcDialogue;
using UnityEngine;

namespace OcDialogue.Samples
{
    [CreateAssetMenu(fileName = "Test Item", menuName = "Oc Dialogue/Sample/TestItem")]
    public class TestItem : GenericItem
    {
        [Range(0, 100)] public int testValue = 10;

        public TestItem()
        {
            subtype = GenericType.Consumable;
        }
    }

}