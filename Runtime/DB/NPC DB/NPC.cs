using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue.DB;
using OcUtility;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.DB
{
    public class NPC : OcData, IDataRowUser
    {
        public enum Gender
        {
            None,
            Male,
            Female
        }
        public override string Address => $"{Category}/{name}";
        public DataRowContainer DataRowContainer => dataRowContainer;
        [ValueDropdown("GetCategory")][PropertyOrder(-2)]public string Category;
        [ShowInInspector, PropertyOrder(-1)][DelayedProperty]
        public string Name
        {
            get => name;
            set
            {
                name = value;
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(this), name);
            }
        }
        [HorizontalGroup("2")]public Gender gender;

        /// <summary> Dialogue Editor 등에서 한 눈에 알아보기 쉽도록 지정하는 고유색. </summary>
        [HorizontalGroup("2"), HideLabel, ColorUsage(false)]
        public Color color;
        [TextArea]
        public string Description;
        public DataRowContainer dataRowContainer;
        
#if UNITY_EDITOR
        
        public RuntimeValue EditorPreset => _editorPreset;
        [SerializeField]
        [DisableIf("@UnityEditor.EditorApplication.isPlaying")]
        [BoxGroup("Debug")]
        [HorizontalGroup("Debug/1")]
        RuntimeValue _editorPreset;
#endif
        [ShowInInspector]
        [HorizontalGroup("Debug/1")]
        [EnableIf("@UnityEngine.Application.isPlaying")]
        public RuntimeValue Runtime
        {
            get => _runtime;
            set => _runtime = value;
        }
        RuntimeValue _runtime;
        
        
        public void GenerateRuntimeData()
        {
            _runtime = new RuntimeValue();
        }

        public bool IsTrue(CheckFactor.Operator op, bool isEncountered)
        {
            switch (op)
            {
                case CheckFactor.Operator.Equal:
#if UNITY_EDITOR
                    if (!EditorApplication.isPlaying) return _editorPreset.IsEncountered == isEncountered;     
#endif
                    return _runtime.IsEncountered == isEncountered;
                case CheckFactor.Operator.NotEqual:
#if UNITY_EDITOR
                    if (!EditorApplication.isPlaying) return _editorPreset.IsEncountered != isEncountered;     
#endif
                    return _runtime.IsEncountered != isEncountered;
            }

            return false;
        }

        public void SetEncountered(bool encountered)
        {
            _runtime.IsEncountered = encountered;
        }
        
        
        [Serializable]
        public struct RuntimeValue
        {
            public bool IsEncountered;
        }
#if UNITY_EDITOR
        void Reset()
        {
            if (DataRowContainer == null) dataRowContainer = new DataRowContainer();
            DataRowContainer.Parent = this;
        }

        public void Resolve()
        {
            if(DataRowContainer.Parent != this) Printer.Print($"[NPC]{name}) DataRowContainer의 Parent를 재설정");
            DataRowContainer.Parent = this;
            DataRowContainer.MatchParent();
        }

        ValueDropdownList<string> GetCategory()
        {
            var list = new ValueDropdownList<string>();
            foreach (var category in NPCDB.Instance.Category)
            {
                list.Add(category);
            }

            return list;
        }
#endif
    }
}
