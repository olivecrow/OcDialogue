using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue.Editor;
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
        public ComparableData targetData;
        
        void OpenSelectWindow()
        {
            var window = DataSelectWindow.Open();
            window.Target = this;
        }
        
        
        /// <summary> 현재의 targetData에서 유효한 비교값 타입을 반환함. </summary>
        public CompareFactor GetValidFactor()
        {
            if (targetData == null) return CompareFactor.Boolean;
            return targetData switch    
            {
                DataRow dataRow => dataRow.type switch
                {
                    DataRow.Type.Boolean => CompareFactor.Boolean,
                    DataRow.Type.String => CompareFactor.String,
                    DataRow.Type.Int => CompareFactor.Int,
                    DataRow.Type.Float => CompareFactor.Float,
                    _ => throw new ArgumentOutOfRangeException()
                },
                Quest quest => CompareFactor.QuestState,
                ItemBase item => CompareFactor.ItemCount,
                NPC npc => CompareFactor.NpcEncounter,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        /// <summary> 전달된 ConparableData에서 유효한 비교값 타입을 반환함. </summary>
        public static CompareFactor GetValidFactor(ComparableData data)
        {
            if (data == null) return CompareFactor.Boolean;
            return data switch    
            {
                DataRow dataRow => dataRow.type switch
                {
                    DataRow.Type.Boolean => CompareFactor.Boolean,
                    DataRow.Type.String => CompareFactor.String,
                    DataRow.Type.Int => CompareFactor.Int,
                    DataRow.Type.Float => CompareFactor.Float,
                    _ => throw new ArgumentOutOfRangeException()
                },
                Quest quest => CompareFactor.QuestState,
                ItemBase item => CompareFactor.ItemCount,
                NPC npc => CompareFactor.NpcEncounter,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

    }
}
