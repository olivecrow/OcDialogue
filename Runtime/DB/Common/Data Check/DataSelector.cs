using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue.Editor;
using OcUtility;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    [Serializable][HideLabel]
    public class DataSelector
    {
        [ReadOnly][HorizontalGroup("1", Width = 250), LabelWidth(50)]
        public string path;
        [HideInInspector] public DBType DBType;
        [InfoBox("선택된 데이터가 없음", InfoMessageType.Error, "@targetData == null")]
        [HideLabel, HorizontalGroup("1", Width = 250, LabelWidth = 200), InlineButton("OpenSelectWindow", "선택")]
        [OnValueChanged("OnDataChanged")]
        public ComparableData targetData;
        
        [HideLabel, HorizontalGroup("Value")][ValueDropdown("GetDetailTargetProperty")][ShowIf("UseDetailTargetProperty")]
        public string DetailTargetProperty;

        public const string PROPERTY_QUESTSTATE = "Quest State";
        public const string PROPERTY_QUESTCLEARAVAILABILITY = "Quest ClearAvailability";
        public const string PROPERTY_NPCENCOUNTERED = "NPC IsEncounter";
        public const string PROPERTY_ENEMYKILLCOUNT = "Enemy KillCount";
        void OpenSelectWindow()
        {
            var window = DataSelectWindow.Open();
            window.Target = this;
        }
        
        
        /// <summary> 현재의 targetData에서 유효한 비교값 타입을 반환함. </summary>
        public CompareFactor GetValidFactor()
        {
            if (targetData == null) return CompareFactor.Boolean;

            switch (targetData)
            {
                case Quest _:
                    switch (DetailTargetProperty)
                    {
                        case PROPERTY_QUESTSTATE:
                            return CompareFactor.QuestState;
                        case PROPERTY_QUESTCLEARAVAILABILITY:
                            return CompareFactor.QuestClearAvailability;
                        default:
                            DetailTargetProperty = PROPERTY_QUESTSTATE;
                            return CompareFactor.QuestState;
                    }

                    break;
                case Enemy _:
                    switch (DetailTargetProperty)
                    {
                        case PROPERTY_ENEMYKILLCOUNT:
                            return CompareFactor.EnemyKillCount;
                        default:
                            DetailTargetProperty = PROPERTY_ENEMYKILLCOUNT;
                            return CompareFactor.EnemyKillCount;
                    }

                    break;
                case NPC _:
                    switch (DetailTargetProperty)
                    {
                        case PROPERTY_NPCENCOUNTERED:
                            return CompareFactor.NpcEncounter;
                        default:
                            DetailTargetProperty = PROPERTY_NPCENCOUNTERED;
                            return CompareFactor.NpcEncounter;
                    }
                    break;
                
                default:
                    return targetData switch    
                    {
                        DataRow dataRow => dataRow.type switch
                        {
                            DataRow.Type.Boolean => CompareFactor.Boolean,
                            DataRow.Type.String => CompareFactor.String,
                            DataRow.Type.Int => CompareFactor.Int,
                            DataRow.Type.Float => CompareFactor.Float,
                            _ => throw new ArgumentOutOfRangeException()
                        }
                    };
            }
        }
        
#if UNITY_EDITOR
        bool UseDetailTargetProperty()
        {
            return targetData is Quest|| targetData is NPC || targetData is Enemy;
        }

        ValueDropdownList<string> GetDetailTargetProperty()
        {
            var list = new ValueDropdownList<string>();
            switch (targetData)
            {
                case Quest quest:
                    list.Add(PROPERTY_QUESTSTATE);
                    list.Add(PROPERTY_QUESTCLEARAVAILABILITY);
                    break;
                case NPC npc:
                    list.Add(PROPERTY_NPCENCOUNTERED);
                    break;
                case Enemy enemy:
                    list.Add(PROPERTY_ENEMYKILLCOUNT);
                    break;
            }

            return list;
        }

        void OnDataChanged()
        {
            switch (targetData)
            {
                case Quest quest:
                    DetailTargetProperty = PROPERTY_QUESTSTATE;
                    break;
                case NPC npc:
                    DetailTargetProperty = PROPERTY_NPCENCOUNTERED;
                    break;
                case Enemy enemy:
                    DetailTargetProperty = PROPERTY_ENEMYKILLCOUNT;
                    break;
            }
        }
#endif

    }
}
