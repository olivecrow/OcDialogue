using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace OcDialogue
{
    public class NPC : ComparableData
    {
        public enum Gender
        {
            None,
            Male,
            Female
        }

        public override string Key => NPCName;

        [HorizontalGroup("1")]
        public string NPCName;
        [ValueDropdown("GetCategory"), HorizontalGroup("1"), LabelWidth(100)]
        public string Category;

        [HorizontalGroup("2")]public Gender gender;

        /// <summary> Dialogue Editor 등에서 한 눈에 알아보기 쉽도록 지정하는 고유색. </summary>
        [HorizontalGroup("2"), HideLabel, ColorUsage(false)]
        public Color color;

        /// <summary> 게임 내의 도감에서 보여지는 설명 </summary>
        [Multiline] public string description;

        [HideLabel, BoxGroup, PropertyOrder(10)]public DataRowContainer DataRowContainer;

        public bool IsEncounter { get; set; }

        public NPC GetCopy()
        {
            var npc = CreateInstance<NPC>();
            npc.NPCName = NPCName;
            npc.name = name;
            npc.Category = Category;
            npc.gender = gender;
            npc.color = color;
            npc.description = description;
            var dataRowCopy = DataRowContainer.GetAllCopies();
            npc.DataRowContainer = new DataRowContainer(npc, dataRowCopy);

            return npc;
        }
        
        public override bool IsTrue(CompareFactor factor, Operator op, object value1)
        {
            if (factor != CompareFactor.NpcEncounter) return false;

            return op switch
            {
                Operator.Equal => IsEncounter == (bool) value1,
                Operator.NotEqual => IsEncounter != (bool) value1,
                _ => false
            };
        }

#if UNITY_EDITOR
        ValueDropdownList<string> GetCategory()
        {
            var list = new ValueDropdownList<string>();
            foreach (var s in NPCDatabase.Instance.Category)
            {
                list.Add(s);
            }

            return list;
        }
        
        void MatchName()
        {
            var path = AssetDatabase.GetAssetPath(this);
            AssetDatabase.RenameAsset(path, NPCName);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        
        void Reset()
        {
            if(DataRowContainer != null) DataRowContainer.owner = this;
        }

        void OnValidate()
        {
            DataRowContainer.CheckNames();
        }

        [Button, HorizontalGroup("Row"), PropertyOrder(9), GUIColor(0,1,1)]
        void AddData()
        {
            DataRowContainer.owner = this;
            DataRowContainer.AddData(DBType.Quest, DataStorageType.Embeded);
        }
        
        [Button, HorizontalGroup("Row"), PropertyOrder(9), GUIColor(1,0,0)]
        void DeleteData(string k)
        {
            DataRowContainer.DeleteRow(k, DataStorageType.Embeded);
        }

        [Button, HorizontalGroup("Row"), PropertyOrder(9)]
        void MatchNames()
        {
            DataRowContainer.MatchDataRowNames();
        }

#endif
    }
}