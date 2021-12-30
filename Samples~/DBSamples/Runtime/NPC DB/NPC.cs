using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue;
using OcDialogue.DB;
using OcUtility;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MyDB
{
    public sealed class NPC : OcNPC, IDataRowUser
    {
        public enum Gender
        {
            None,
            Male,
            Female
        }
        public override string Address => $"{Category}/{name}";

        public override string Category
        {
            get => category;
            set => category = value;
        }
        public DataRowContainer DataRowContainer => dataRowContainer;
        [ValueDropdown("GetCategory")][PropertyOrder(-2)]public string category;
        [ShowInInspector, PropertyOrder(-1)][DelayedProperty]
        public string Name
        {
            get => name;
            set
            {
                name = value;
#if UNITY_EDITOR
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(this), name);          
#endif
            }
        }
        [HorizontalGroup("2")]public Gender gender;
        [TextArea]
        public string Description;
        public DataRowContainer dataRowContainer;
        public event Action<NPC> OnRuntimeValueChanged;
#if UNITY_EDITOR
        
        public RuntimeValue EditorPreset => _editorPreset;
        [SerializeField]
        [DisableIf("@UnityEditor.EditorApplication.isPlaying")]
        [BoxGroup("Debug")]
        [HorizontalGroup("Debug/1")]
        RuntimeValue _editorPreset;
#endif
        public const string fieldName_isEncountered = "IsEncountered";
        
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
            dataRowContainer.GenerateRuntimeData();
            dataRowContainer.OnRuntimeValueChanged += row => OnRuntimeValueChanged?.Invoke(this);
        }

        public override bool IsTrue(string fieldName, CheckFactor.Operator op, object checkValue)
        {
            if (checkValue is bool boolValue) return ((bool)GetValue(fieldName)).IsTrue(op, boolValue);
            Debug.LogWarning($"[NPC] IsTrue | 잘못된 데이터 타입 : {checkValue.GetType()} | 필요한 데이터 타입 : bool");
            return false;
        }

        public override object GetValue(string fieldName)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying) return _editorPreset.IsEncountered;
#endif
            return _runtime.IsEncountered;
        }

        public override string[] GetFieldNames()
        {
            return new[] { fieldName_isEncountered };
        }

        public override void SetValue(string fieldName, DataSetter.Operator op, object value)
        {
            if (fieldName == fieldName_isEncountered) _runtime.IsEncountered = (bool)value;
        }

        public override DataRowType GetValueType(string fieldName)
        {
            return DataRowType.Bool;
        }

        public void SetEncountered(bool encountered, bool withoutNotify = false)
        {
            var isNew = _runtime.IsEncountered != encountered;
            _runtime.IsEncountered = encountered;
            if(!withoutNotify && isNew) OnRuntimeValueChanged?.Invoke(this);
        }

        public void SetCharacter(INPCCharacter character)
        {
            _runtime.Character = character;
        }
        
        public CommonSaveData GetSaveData()
        {
            var data = new CommonSaveData
            {
                Key = Name,
                DataRowContainerDict = dataRowContainer.GetSaveData(),
                Data = new Dictionary<string, string>
                {
                    [fieldName_isEncountered] = _runtime.IsEncountered.ToString()
                }
            };
            return data;
        }

        public void Overwrite(CommonSaveData data)
        {
            dataRowContainer.Overwrite(data.DataRowContainerDict);
            if (data.Data.ContainsKey(fieldName_isEncountered)) 
                bool.TryParse(data.Data[fieldName_isEncountered], out _runtime.IsEncountered);

        }
        
        
        [Serializable]
        public struct RuntimeValue
        {
            public bool IsEncountered;
            public INPCCharacter Character;
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
            foreach (var category in NPCDB.Instance.Categories)
            {
                list.Add(category);
            }

            return list;
        }
#endif
    }
}
