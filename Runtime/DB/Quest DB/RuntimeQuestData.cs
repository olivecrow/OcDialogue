using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public class RuntimeQuestData
    {
        public event Action<Quest> OnQuestDataChanged;
        
        readonly List<Quest> _quests;
        public RuntimeQuestData(IEnumerable<Quest> original)
        {
            _quests = new List<Quest>();
            foreach (var quest in original)
            {
                _quests.Add(quest.GetCopy());
            }

            if (Inventory.PlayerInventory == null)
            {
                Inventory.OnPlayerInventoryChanged += inv =>
                {
                    inv.OnItemAdded += x => CheckQuestCondition();
                    inv.OnItemRemoved += x => CheckQuestCondition();
                };
            }
            else
            {
                Inventory.PlayerInventory.OnItemAdded += x => CheckQuestCondition();
                Inventory.PlayerInventory.OnItemRemoved += x => CheckQuestCondition();
            }
        }

        public Quest FindQuest(string key)
        {
            var q = _quests.Find(x => x.key == key);
            if (q == null)
            {
                Debug.LogWarning($"해당 키값의 퀘스트를 찾을 수 없음 | key : {key}");
                return null;
            }

            return q;
        }

        /// <summary> 오리지널 데이터 중, 해당 dataRow를 가졌던 퀘스트의 오버라이드를 반환함. </summary>
        public Quest FindQuest(DataRow dataRow)
        {
            
            var originalQuest = QuestDatabase.Instance.Quests.Find(x => x.DataRowContainer.Contains(dataRow));
            if (originalQuest == null)
            {
                Debug.LogWarning($"해당 데이터를 가진 퀘스트를 찾을 수 없음 | dataRow.key : {dataRow.key}");
                return null;
            }

            return FindQuest(originalQuest.key);
        }

        public void SetQuestState(string key, Quest.State state)
        {
            var quest = FindQuest(key);
            if(quest == null) return;

            quest.QuestState = state;
            OnQuestDataChanged?.Invoke(quest);
        }
        

        public bool IsTrue(string key, Operator op, Quest.State questState)
        {
            var q = FindQuest(key);
            if (q == null) return false;

            return q.IsTrue(op, questState);
        }

        public void CheckQuestCondition()
        {
            foreach (var quest in _quests)
            {
                // TODO : 퀘스트 클리어 가능 여부 등 업데이트.
            }
        }
    }
}
