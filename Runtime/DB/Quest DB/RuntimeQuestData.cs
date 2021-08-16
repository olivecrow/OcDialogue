using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    public class RuntimeQuestData
    {
        public event Action<Quest> OnQuestDataChanged;
        
        [ShowInInspector][InlineEditor()] public List<Quest> Quests { get; }

        public RuntimeQuestData(IEnumerable<Quest> original)
        {
            Quests = new List<Quest>();
            foreach (var quest in original)
            {
                var q = quest.GetCopy();
                q.OnStateChanged += state => OnQuestDataChanged?.Invoke(q);
                Quests.Add(q);
            }
        }

        public Quest FindQuest(string key)
        {
            var q = Quests.Find(x => x.key == key);
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


        public bool IsTrue(string key, CompareFactor factor, Operator op, object value)
        {
            var q = FindQuest(key);
            if (q == null) return false;

            return q.IsTrue(factor, op, value);
        }
    }
}
