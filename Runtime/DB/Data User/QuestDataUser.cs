using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public class QuestDataUser : DataUser
    {
        public List<Quest> Quests;
        
        public override void Load()
        {
#if UNITY_EDITOR
            if (QuestDataPreset.Instance.usePreset)
            {
                Quests = QuestDataPreset.Instance.GetAllCopies();
                return;
            }
#endif
            
            Quests = QuestDataPreset.Instance.GetAllCopies();
            // TODO : 데이터 로드.
        }

        public override void Save()
        {
            throw new System.NotImplementedException();
        }
    }
}
