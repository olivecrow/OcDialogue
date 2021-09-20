using System;
using OcDialogue.DB;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
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
            if (_target.DataRow == null)
            {
                EditorGUILayout.LabelField("연결된 DataRow가 없음. 새로 생성하거나 직접 연결할 것.");
                EditorGUILayout.BeginHorizontal();
                _creationDataKeyInput = EditorGUILayout.TextField("Key", _creationDataKeyInput);
                if (GUILayout.Button("생성"))
                {
                    if (GameProcessDB.Instance.DataRowContainer.HasKey(_creationDataKeyInput))
                    {
                        EditorUtility.DisplayDialog("데이터 생성 불가", $"이미 해당 키가 존재함 : {_creationDataKeyInput}", "OK");
                        return;
                    }

                    _target.DataRow = GameProcessDB.Instance.DataRowContainer.AddData(_creationDataKeyInput);
                }
                EditorGUILayout.EndHorizontal();
            }
            base.OnInspectorGUI();
            // EditorGUILayout.PropertyField(serializedObject.FindProperty("DataRow"));
            // if (_target.DataRow != null)
            // {
            //     EditorGUILayout.BeginHorizontal();
            //     EditorGUI.BeginChangeCheck();
            //     _target.DataRow.Name = EditorGUILayout.DelayedTextField(_target.DataRow.Name);
            //     _target.DataRow.Type = (DataRowType)SirenixEditorFields.EnumDropdown(EditorGUILayout.GetControlRect(), _target.DataRow.Type);
            //     switch (_target.DataRow.Type)
            //     {
            //         case DataRowType.Bool:
            //             _target.DataRow.InitialValue.BoolValue = SirenixEditorFields.
            //             break;
            //         case DataRowType.Int:
            //             break;
            //         case DataRowType.Float:
            //             break;
            //         case DataRowType.String:
            //             break;
            //     }
            //     if (EditorGUI.EndChangeCheck())
            //     {
            //         EditorUtility.SetDirty(_target.DataRow);
            //         AssetDatabase.SaveAssets();
            //     }
            //     EditorGUILayout.EndHorizontal();
            // }
            serializedObject.ApplyModifiedProperties();
        }
    }
}