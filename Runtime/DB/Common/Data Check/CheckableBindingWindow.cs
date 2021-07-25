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
            var wnd = GetWindow<CheckableBindingWindow>();
            wnd.ShowUtility();
            wnd.minSize = new Vector2(720, 480);
            wnd.maxSize = new Vector2(720, 480);
            return wnd;
        }
        
        public DataChecker Checker { get; private set; }

        [HorizontalGroup("1"), HideLabel]public int[] cIndex;
        [HorizontalGroup("1"), HideLabel]public DataChecker.Condition condition;
            
        [ReadOnly]public string expression;
        DataChecker.Binding _tempBinding;
        public void SetChecker(DataChecker checker)
        {
            Checker = checker;
            expression = OcDataUtility.ToStringFromBindings(Checker.factors, Checker.bindings.ToArray());
        }
        [Button]
        void UpdateExpression()
        {
            _tempBinding = new DataChecker.Binding();
            _tempBinding.Index = Checker.factors.Length + Checker.bindings.Count;
            _tempBinding.condition = condition;
            _tempBinding.checkables = cIndex.ToList();

            var allBindings = new List<DataChecker.Binding>();
            for (int i = 0; i < Checker.bindings.Count; i++)
            {
                Checker.bindings[i].Index = Checker.factors.Length + i;
            }
            allBindings.AddRange(Checker.bindings);
            allBindings.Add(_tempBinding);
            
            expression = OcDataUtility.ToStringFromBindings(Checker.factors, allBindings.ToArray());
        }
        [Button]
        void Bind()
        {
            if(_tempBinding == null) return;
            Checker.bindings.Add(_tempBinding);
        }

        
    }
}
#endif