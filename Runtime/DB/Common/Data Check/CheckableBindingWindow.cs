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

namespace OcDialogue
{
    public class CheckableBindingWindow : OdinEditorWindow
    {
        public static CheckableBindingWindow Open()
        {
            var wnd = GetWindow<CheckableBindingWindow>(true);
            wnd.minSize = new Vector2(720, 480);
            wnd.maxSize = new Vector2(720, 480);
            return wnd;
        }
        
        public DataChecker Checker { get; private set; }

        [HorizontalGroup("1"), HideLabel][ValueDropdown("GetIndexDropDown", DropdownWidth = 800)]public int[] cIndex;
        [HorizontalGroup("1"), HideLabel]public Condition condition;
            
        [ReadOnly]public string expression;
        Binding _tempBinding;
        public void SetChecker(DataChecker checker)
        {
            Checker = checker;
            expression = checker.ToExpression();
        }
        [Button]
        void UpdateExpression()
        {
            _tempBinding = new Binding();
            _tempBinding.Index = Checker.factors.Length + Checker.bindings.Count;
            _tempBinding.condition = condition;
            _tempBinding.checkables = cIndex.ToList();

            var allBindings = new List<Binding>();
            for (int i = 0; i < Checker.bindings.Count; i++)
            {
                Checker.bindings[i].Index = Checker.factors.Length + i;
            }
            allBindings.AddRange(Checker.bindings);
            allBindings.Add(_tempBinding);

            var tmpGroups = DataChecker.CreateCheckGroups(Checker.factors.ToList(), allBindings);

            expression = tmpGroups[tmpGroups.Count - 1].ToExpression(false);
        }
        [Button]
        void Bind()
        {
            if(_tempBinding == null) return;
            Checker.bindings.Add(_tempBinding);
        }

        ValueDropdownList<int> GetIndexDropDown()
        {
            var list = new ValueDropdownList<int>();

            for (int i = 0; i < Checker.factors.Length; i++)
            {
                list.Add($"[{i}] {Checker.factors[i].ToExpression(false)}", i);
            }

            var tmpGroup = Checker.CreateCheckGroups();
            for (int i = 0; i < Checker.bindings.Count; i++)
            {
                var targetGroup = tmpGroup.Find(x => x.Index == Checker.bindings[i].Index);
                list.Add($"[{Checker.bindings[i].Index}] {targetGroup.ToExpression(false)}", Checker.bindings[i].Index);   
            }

            return list;
        }
    }
}
#endif