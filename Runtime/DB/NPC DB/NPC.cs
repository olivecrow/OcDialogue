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

        [ValueDropdown("GetCategory"), LabelWidth(100)]
        public string Category;

        [TableColumnWidth(150, false), VerticalGroup("NPC"), HideLabel]
        public string NPCName;

        [VerticalGroup("NPC"), HideLabel] public Gender gender;

        /// <summary> Dialogue Editor 등에서 한 눈에 알아보기 쉽도록 지정하는 고유색. </summary>
        [VerticalGroup("NPC"), HideLabel, ColorUsage(false)]
        public Color color;

        /// <summary> 게임 내의 도감에서 보여지는 설명 </summary>
        [Multiline] public string description;

        [HideLabel, BoxGroup, PropertyOrder(10)]public DataRowContainer DataRowContainer;

        public bool IsEncounter { get; set; }

        public override bool IsTrue(CompareFactor factor, Operator op, object value)
        {
            if (factor != CompareFactor.NpcEncounter) return false;

            return op switch
            {
                Operator.Equal => IsEncounter == (bool) value,
                Operator.NotEqual => IsEncounter != (bool) value,
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
            DataRowContainer.owner = this;
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