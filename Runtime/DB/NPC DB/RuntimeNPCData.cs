using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public class RuntimeNPCData
    {
        readonly List<NPC> _npcs;
        public RuntimeNPCData(IEnumerable<NPC> npcs)
        {
            _npcs = new List<NPC>();
            foreach (var npc in npcs)
            {
                _npcs.Add(npc.GetCopy());
            }
        }

        public NPC FindNPC(string key)
        {
            var npc = _npcs.Find(x => x.Key == key);
            if (npc == null)
            {
                Debug.LogWarning($"해당 키값의 NPC가 없음 | key : {key}");
                return null;
            }

            return npc;
        }

        /// <summary> 오리지날 데이터 중, 해당 dataRow를 가진 NPC의 오버라이드를 반환함. </summary>
        public NPC FindNPC(DataRow dataRow)
        {
            var originalNPC = NPCDatabase.Instance.NPCs.Find(x => x.DataRowContainer.Contains(dataRow));
            if (originalNPC == null)
            {
                Debug.LogWarning($"해당 dataRow를 가진 NPC가 없음 | dataRow.key : {dataRow.key}");
                return null;
            }

            return FindNPC(originalNPC.Key);
        }

        public bool IsTrue(string key, Operator op, object value)
        {
            var npc = FindNPC(key);
            if (npc == null) return false;

            return npc.IsTrue(CompareFactor.NpcEncounter, op, value);
        }
    }
}
