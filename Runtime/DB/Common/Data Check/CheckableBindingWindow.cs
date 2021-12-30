#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.DB
{
    public class CheckableBindingWindow : OdinEditorWindow
    {
        public DataChecker Checker { get; private set; }

        [HideInInspector]public int[] cIndex;
        [ReadOnly]public List<Binding> tempBindings;
        [InlineButton(nameof(AddBindingSize), "+")]
        [InlineButton(nameof(SubtractBindingSize), "-")]
        public int bindingSize = 1;
        [ShowInInspector][ValueDropdown(nameof(GetIndexDropDown))]
        [HorizontalGroup("2"), HideLabel][PropertyOrder(1)]
        public int Index0
        {
            get => cIndex[0];
            set => cIndex[0] = value;
        }
        [HorizontalGroup("2", MaxWidth = 50), HideLabel][PropertyOrder(2)] 
        public Condition condition1;
        
        [ShowInInspector][ValueDropdown(nameof(GetIndexDropDown))]
        [HorizontalGroup("2"), HideLabel][PropertyOrder(3)] 
        public int Index1
        {
            get => cIndex[1];
            set => cIndex[1] = value;
        }
        [HorizontalGroup("2", MaxWidth = 50), HideLabel] [ShowInInspector][ShowIf("@bindingSize > 1")] [PropertyOrder(4)]
        public Condition condition2 => condition1;
        
        [ShowInInspector][ValueDropdown(nameof(GetIndexDropDown))]
        [HorizontalGroup("2"), HideLabel] [ShowIf("@bindingSize > 1")][PropertyOrder(5)]
        public int Index2
        {
            get => cIndex[2];
            set => cIndex[2] = value;
        }

        [HorizontalGroup("2", MaxWidth = 50), HideLabel] [ShowInInspector] [ShowIf("@bindingSize > 2")][PropertyOrder(6)]
        public Condition condition3 => condition1;
        
        [ShowInInspector][ValueDropdown(nameof(GetIndexDropDown))]
        [HorizontalGroup("2"), HideLabel] [ShowIf("@bindingSize > 2")][PropertyOrder(7)]
        public int Index3
        {
            get => cIndex[3];
            set => cIndex[3] = value;
        }
            
        [ReadOnly]public string expression;
        Binding _tempBinding;
        bool _previewApplied;
        public static CheckableBindingWindow Open()
        {
            var wnd = GetWindow<CheckableBindingWindow>(true);
            wnd.minSize = new Vector2(720, 480);
            return wnd;
        }
        public void SetChecker(DataChecker checker)
        {
            Checker = checker;
            expression = checker.ToExpression();
            tempBindings = new List<Binding>();
            tempBindings.AddRange(checker.bindings);
            cIndex = new[] {-1, -1, -1, -1};
            bindingSize = 1;
        }
        [HorizontalGroup("button1")]
        [Button(ButtonSizes.Medium)][PropertyOrder(100)][DisableIf("@Index0 == -1 || Index1 == -1")]
        void PreviewExpression()
        {
            _tempBinding = new Binding();
            _tempBinding.Index = Checker.factors.Length + tempBindings.Count;
            _tempBinding.condition = condition1;
            _tempBinding.checkables = new List<int>();
            foreach (var i in cIndex)
            {
                if(i < 0) continue;
                _tempBinding.checkables.Add(i);
            }

            var allBindings = new List<Binding>();
            allBindings.AddRange(tempBindings);
            allBindings.Add(_tempBinding);

            var tmpGroups = DataChecker.CreateCheckGroups(Checker.factors.ToList(), allBindings);

            expression = tmpGroups[tmpGroups.Count - 1].ToExpression(false);
            _previewApplied = false;
        }
        [HorizontalGroup("button1")]
        [Button(ButtonSizes.Medium)][PropertyOrder(100)][EnableIf("@!_previewApplied && _tempBinding != null")]
        void Bind()
        {
            if(_tempBinding == null) return;

            foreach (var binding in tempBindings)
            {
                if (binding.HasSameIndex(_tempBinding))
                {
                    Debug.LogWarning($"중복된 바인딩이 감지됨");
                    return;
                }
            }
            tempBindings.Add(_tempBinding);
            cIndex = new []{-1,-1,-1,-1};
            bindingSize = 1;
            _tempBinding = null;
            _previewApplied = true;
        }
        [Button(ButtonSizes.Large)][PropertyOrder(100), GUIColor(1,1,0)]
        void Apply()
        {
            if (!_previewApplied && _tempBinding != null)
            {
                if (!EditorUtility.DisplayDialog("현재 진행중인 바인딩이 적용되지 않았습니다",
                    $"현재 진행중인 바인딩을 적용하시겠습니까?",
                    "적용", "취소"))return;
                Bind();
            }

            if (HasUnusedCheckables())
            {
                EditorUtility.DisplayDialog("적용 실패", e_bindingErrMsg, "OK");
                return;
            }
            
            Checker.bindings = new List<Binding>();
            Checker.bindings.AddRange(tempBindings);
            Checker.UpdateExpression();
            Close();
        }

        void AddBindingSize()
        {
            bindingSize++;
            bindingSize = Mathf.Clamp(bindingSize, 0, 4);
        }

        void SubtractBindingSize()
        {
            bindingSize--;
            bindingSize = Mathf.Clamp(bindingSize, 0, 4);
        }

        string e_bindingErrMsg;
        bool HasUnusedCheckables()
        {
            if (tempBindings == null || tempBindings.Count == 0) return false;
            var maxIndex = Checker.factors.Length + tempBindings.Count - 1;
            var indices = new List<int>();
            for (int i = 0; i < maxIndex; i++)
            {
                indices.Add(i);
            }

            foreach (var binding in tempBindings)
            {
                foreach (var checkable in binding.checkables)
                {
                    if (indices.Contains(checkable)) indices.Remove(checkable);
                }
            }

            if (indices.Count > 0)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < indices.Count; i++)
                {
                    sb.Append(indices[i]);
                    if(i < indices.Count - 1)sb.Append(", ");
                }

                e_bindingErrMsg = $"바인딩에 포함되지 않은 요소가 있음 | index : {sb}";
                return true;
            }

            return false;
        }
        
        ValueDropdownList<int> GetIndexDropDown()
        {
            var list = new ValueDropdownList<int>();

            for (int i = 0; i < Checker.factors.Length; i++)
            {
                if(cIndex.Contains(i)) continue;
                list.Add($"[{i}] {Checker.factors[i].ToExpression(false)}", i);
            }

            var tmpGroups = DataChecker.CreateCheckGroups(Checker.factors.ToList(), tempBindings);
            for (int i = 0; i < tempBindings.Count; i++)
            {
                if(cIndex.Contains(i)) continue;
                var targetGroup = tmpGroups.Find(x => x.Index == tempBindings[i].Index);
                list.Add($"[{tempBindings[i].Index}] {targetGroup.ToExpression(false)}", tempBindings[i].Index);   
            }

            return list;
        }

        void OnValidate()
        {
            bindingSize = Mathf.Clamp(bindingSize, 0, 4);
        }
    }
}
#endif