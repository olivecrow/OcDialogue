using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue.DB;
using UnityEngine;

namespace OcDialogue
{
    public class DummyDataSelector : IOcDataSelectable
    {
        public OcData Data { get; set; }
        public string Detail { get; set; }

        public event Action OnDataSelected;
        public void UpdateAddress()
        {
        }

        public void SetTargetData(OcData data)
        {
            Data = data;
            switch (data)
            {
                case DataRow dataRow:
                    Detail = null;
                    break;
                case Quest quest:
                    Detail = DataChecker.QUEST_STATE;
                    break;
                case NPC npc:
                    Detail = DataChecker.NPC_ENCOUNTERED;
                    break;
                case Enemy enemy:
                    Detail = DataChecker.ENEMY_KILLCOUNT;
                    break;
            }
            OnDataSelected?.Invoke();
        }
    }
}
