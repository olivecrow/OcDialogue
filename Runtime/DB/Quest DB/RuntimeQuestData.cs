using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public class RuntimeQuestData
    {
        List<Quest> _quests;
        public RuntimeQuestData(IEnumerable<Quest> original)
        {
            var list = new List<Quest>();
            foreach (var quest in original)
            {
                list.Add(quest.GetCopy());
            }

            _quests = list;
        }
    }
}
