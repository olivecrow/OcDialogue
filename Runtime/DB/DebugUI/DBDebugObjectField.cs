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
        public void Set(object target, FieldInfo fieldInfo)
        {
            _target = target;
            _fieldInfo = fieldInfo;
            nameText.text = fieldInfo.Name;
            UpdateValue();
        }

        void UpdateValue()
        {
            var valueType = _fieldInfo.FieldType.ToString();
            var valueName = _fieldInfo.GetValue(_target).ToString();
            objectValueField.text = $"{valueName} ({valueType})";
        }
    }
}