using System;
using System.Reflection;
using OcUtility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OcDialogue.DB
{
    public class DBDebugObjectField : DBDebugBlock
    {
        public CanvasGroup canvasGroup;
        public TextMeshProUGUI objectValueField;
        public bool interactable
        {
            get => canvasGroup.interactable;
            set => canvasGroup.interactable = value;
        }        
        object _target;
        FieldInfo _fieldInfo;
        PropertyInfo _propertyInfo;
        public void Set(object target, FieldInfo fieldInfo)
        {
            _target = target;
            _fieldInfo = fieldInfo;
            _propertyInfo = null;
            nameText.text = fieldInfo.Name;
            UpdateValue();
        }

        public void Set(object target, PropertyInfo propertyInfo)
        {
            _target = target;
            _propertyInfo = propertyInfo;
            _fieldInfo = null;
            nameText.text = propertyInfo.Name;
        }

        void UpdateValue()
        {
            if(_fieldInfo != null)
            {
                var valueType = _fieldInfo.FieldType.ToString();
                var value = _fieldInfo.GetValue(_target);
                var valueName = value == null ? "null" : value.ToString();
                objectValueField.text = $"{valueName} ({valueType})";
            }
            else if (_propertyInfo != null)
            {
                var valueType = _propertyInfo.PropertyType.ToString();
                var value = _propertyInfo.GetValue(_target);
                var valueName = value == null ? "null" : value.ToString();
                objectValueField.text = $"{valueName} ({valueType})";
            }
        }
    }
}