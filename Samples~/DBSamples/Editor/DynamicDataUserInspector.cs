using System;
using OcDialogue.DB;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace MyDB.Editor
{
    [CustomEditor(typeof(DynamicDataUser))]
    public class DynamicDataUserInspector : OdinEditor
    {
        DynamicDataUser _target;
        string _creationDataKeyInput;

        protected override void OnEnable()
        {
            base.OnEnable();
            _target = target as DynamicDataUser;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // TODO : 나중에 다시 작성할 것.
        }
    }
}