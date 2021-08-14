using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    [Serializable]
    public class NPCEditorPreset
    {
        public List<OverridenNPC> NPCs;

        public List<NPC> GetAllCopies()
        {
            var list = new List<NPC>();

            foreach (var overridenNPC in NPCs)
            {
                var copy = overridenNPC.npc.GetCopy();
                overridenNPC.ApplyOverrides(copy);
                list.Add(copy);
            }

            return list;
        }

        [Button]
        void ToDefault()
        {
            var list = new List<OverridenNPC>();
            foreach (var npc in NPCDatabase.Instance.NPCs)
            {
                var overrideNPC = new OverridenNPC(npc);
                list.Add(overrideNPC);
            }

            NPCs = list;
        }

        [Serializable]
        public class OverridenNPC : IComparableData
        {
            [ReadOnly]public NPC npc;
            public bool IsEncounter;
            public List<OverridenRow> overrideRows;

            public OverridenNPC(NPC npc)
            {
                this.npc = npc;
                overrideRows = new List<OverridenRow>();

                foreach (var dataRow in npc.DataRowContainer.dataRows)
                {
                    var overrideRow = new OverridenRow(dataRow, dataRow.TargetValue);
                    overrideRows.Add(overrideRow);
                }
            }

            public void ApplyOverrides(NPC target)
            {
                target.IsEncounter = IsEncounter;
                for (int i = 0; i < target.DataRowContainer.dataRows.Count; i++)
                {
                    target.DataRowContainer.SetValue(overrideRows[i].Key, overrideRows[i].OverridenValue);
                }
            }

            public string Key => npc.Key;
            public bool IsTrue(CompareFactor factor, Operator op, object value1)
            {
                var tmp = npc.GetCopy();
                ApplyOverrides(tmp);
                return tmp.IsTrue(factor, op, value1);
            }
        }
    }
}
