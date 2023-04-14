using System;
using System.Reflection;
using OcUtility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OcDialogue.DB
{
    public class DBDebugStandardField : DBDebugBlock
    {
        public CanvasGroup canvasGroup;
        public TMP_InputField valueInputField;

        public bool interactable
        {
            get => canvasGroup.interactable;
            set => canvasGroup.interactable = value;
        }

        object _target;
        FieldInfo _fieldInfo;
        PropertyInfo _propertyInfo;
        MemberInfo _memberInfo;

        public void Set(object target, FieldInfo fieldInfo)
        {
            _propertyInfo = null;
            _target = target;
            _fieldInfo = fieldInfo;
            _memberInfo = fieldInfo;
            nameText.text = fieldInfo.Name;

            valueInputField.onEndEdit.RemoveAllListeners();
            UpdateValue();

            valueInputField.onEndEdit.AddListener(TrySetValue);
        }

        public void Set(object target, PropertyInfo propertyInfo)
        {
            _fieldInfo = null;
            _target = target;
            _propertyInfo = propertyInfo;
            _memberInfo = propertyInfo;
            nameText.text = propertyInfo.Name;
            valueInputField.onEndEdit.RemoveAllListeners();
            UpdateValue();

            valueInputField.onEndEdit.AddListener(TrySetValue);
        }

        void UpdateValue()
        {
            try
            {
                if (_fieldInfo != null)
                {
                    if (_fieldInfo.FieldType == typeof(bool))
                        valueInputField.text = ((bool)_fieldInfo.GetValue(_target)).DRT();
                    else
                        valueInputField.text = _fieldInfo.GetValue(_target).ToString();
                }
                else if (_propertyInfo != null)
                {
                    if (_propertyInfo.PropertyType == typeof(bool))
                        valueInputField.text = ((bool)_propertyInfo.GetValue(_target)).DRT();
                    else
                        valueInputField.text = _propertyInfo.GetValue(_target).ToString();
                }
            }
            catch (Exception e)
            {
                valueInputField.text = "cannot read";
            }
            
        }

        void TrySetValue(string text)
        {
            if (_fieldInfo != null)
            {
                var t = _fieldInfo.FieldType;
                if (t == typeof(bool))
                {
                    if (bool.TryParse(text, out var result)) _fieldInfo.SetValue(_target, result);
                }
                else if (t == typeof(float))
                {
                    if (float.TryParse(text, out var result)) _fieldInfo.SetValue(_target, result);
                }
                else if (t == typeof(int) || t == typeof(Enum))
                {
                    if (int.TryParse(text, out var result)) _fieldInfo.SetValue(_target, result);
                }
                else if (t == typeof(string))
                {
                    _fieldInfo.SetValue(_target, text);
                }
                else if (t == typeof(Vector2))
                {
                    var split = text.Substring(1, text.Length - 2).Split(',');
                    var xString = split[0];
                    var yString = split[1];
                    if (float.TryParse(xString, out var x) && float.TryParse(yString, out var y))
                        _fieldInfo.SetValue(_target, new Vector2(x, y));
                }
                else if (t == typeof(Vector3))
                {
                    var split = text.Substring(1, text.Length - 2).Split(',');
                    var xString = split[0];
                    var yString = split[1];
                    var zString = split[2];
                    if (float.TryParse(xString, out var x) && float.TryParse(yString, out var y) &&
                        float.TryParse(zString, out var z))
                        _fieldInfo.SetValue(_target, new Vector3(x, y, z));
                }
                else if (t == typeof(Vector4))
                {
                    var split = text.Substring(1, text.Length - 2).Split(',');
                    var xString = split[0];
                    var yString = split[1];
                    var zString = split[2];
                    var wString = split[3];
                    if (float.TryParse(xString, out var x) && float.TryParse(yString, out var y) &&
                        float.TryParse(zString, out var z) && float.TryParse(wString, out var w))
                        _fieldInfo.SetValue(_target, new Vector4(x, y, z, w));
                }
            }
            else
            {
                var t = _propertyInfo.PropertyType;
                if (t == typeof(bool))
                {
                    if (bool.TryParse(text, out var result)) _propertyInfo.SetValue(_target, result);
                }
                else if (t == typeof(float))
                {
                    if (float.TryParse(text, out var result)) _propertyInfo.SetValue(_target, result);
                }
                else if (t == typeof(int) || t == typeof(Enum))
                {
                    if (int.TryParse(text, out var result)) _propertyInfo.SetValue(_target, result);
                }
                else if (t == typeof(string))
                {
                    _propertyInfo.SetValue(_target, text);
                }
                else if (t == typeof(Vector2))
                {
                    var split = text.Substring(1, text.Length - 2).Split(',');
                    var xString = split[0];
                    var yString = split[1];
                    if (float.TryParse(xString, out var x) && float.TryParse(yString, out var y))
                        _propertyInfo.SetValue(_target, new Vector2(x, y));
                }
                else if (t == typeof(Vector3))
                {
                    var split = text.Substring(1, text.Length - 2).Split(',');
                    var xString = split[0];
                    var yString = split[1];
                    var zString = split[2];
                    if (float.TryParse(xString, out var x) && float.TryParse(yString, out var y) &&
                        float.TryParse(zString, out var z))
                        _propertyInfo.SetValue(_target, new Vector3(x, y, z));
                }
                else if (t == typeof(Vector4))
                {
                    var split = text.Substring(1, text.Length - 2).Split(',');
                    var xString = split[0];
                    var yString = split[1];
                    var zString = split[2];
                    var wString = split[3];
                    if (float.TryParse(xString, out var x) && float.TryParse(yString, out var y) &&
                        float.TryParse(zString, out var z) && float.TryParse(wString, out var w))
                        _propertyInfo.SetValue(_target, new Vector4(x, y, z, w));
                }
            }

            UpdateValue();
        }
    }
}