using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public interface ICategoryUser
    {
        public string[] CategoryRef { get; set; }
    }
    public class DataCategoryDrawer
    {
        public bool IsEditMode;
        public string CurrentCategory;
        ICategoryUser _user;
        public DataCategoryDrawer(ICategoryUser user)
        {
            _user = user;
        }

        public void Draw(Color textColor, Color buttonColor, in SerializedObject serializedObject, out bool isSelectionChanged)
        {
            GUI.contentColor = textColor;
            GUI.backgroundColor = buttonColor;
            isSelectionChanged = false;
            if(_user.CategoryRef.Length > 0 && !IsEditMode)
            {
                var categoryList = _user.CategoryRef.ToList();
                var currentIdx = categoryList.Contains(CurrentCategory) ? categoryList.IndexOf(CurrentCategory) : 0;

                var idx = GUILayout.Toolbar(currentIdx, _user.CategoryRef, GUILayoutOptions.Height(25));

                if (CurrentCategory != _user.CategoryRef[idx]) isSelectionChanged = true;
                CurrentCategory = _user.CategoryRef[idx];
            }

            if (IsEditMode)
            {
                // _user.EditCategory();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Category"));
                serializedObject.ApplyModifiedProperties();
            }
            GUI.contentColor = Color.white;
            GUI.backgroundColor = Color.white;
        }
    }
}
