using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class CategoryEditWindow : OdinEditorWindow
    {
        [TableList]public List<Transition> transitions;
        Action<List<Transition>> applyAction;
        public static CategoryEditWindow Open(string[] category, Action<List<Transition>> transition)
        {
            var window = GetWindow<CategoryEditWindow>();
            window.transitions = new List<Transition>();
            foreach (var s in category)
            {
                var t = new Transition(s);
                window.transitions.Add(t);
            }

            window.applyAction = transition;
            return window;
        }

        [Button]
        public void Apply()
        {
            
            var sb = new StringBuilder();
            foreach (var transition in transitions)
            {
                if (transitions.Any(x =>
                    !string.IsNullOrWhiteSpace(x.after) &&
                    x.after == transition.after && 
                    x != transition))
                {
                    Debug.LogWarning($"중복된 카테고리를 지정할 수 없음");
                    return;
                }
                sb.Append(transition);
                sb.Append('\n');
            }
            if(!EditorUtility.DisplayDialog(
                "변경된 사항을 저장하고 콜백을 실행하겠습니까?", sb.ToString(),
                "OK", "Cancel")) return;
            applyAction?.Invoke(transitions);
            Close();
        }

        [Serializable]
        public class Transition
        {
            [ReadOnly]
            public string before;
            public string after;

            public Transition(string s)
            {
                before = s;
                after = s;
            }

            public override string ToString()
            {
                return $"{before} => {(string.IsNullOrWhiteSpace(after) ? "[삭제됨]" : after)}";
            }
        }
    }
}