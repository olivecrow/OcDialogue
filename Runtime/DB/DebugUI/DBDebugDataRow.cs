using System;
using OcUtility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OcDialogue.DB
{
    public class DBDebugDataRow : DBDebugBlock
    {
        public TextMeshProUGUI typeText;
        public TextMeshProUGUI description;
        public TextMeshProUGUI initialValue;
        
        public Toggle boolToggle;
        public TextMeshProUGUI boolValueText;

        public TMP_InputField valueInputField;

        DataRow _dataRow;
        public void Set(DataRow dataRow)
        {
            _dataRow = dataRow;

            nameText.text = dataRow.Name;
            typeText.text = dataRow.Type.ToString();
            description.text = dataRow.description;

            UpdateValue();

            boolToggle.onValueChanged.RemoveAllListeners();
            boolToggle.onValueChanged.AddListener(TrySetValue);
            
            valueInputField.onEndEdit.RemoveAllListeners();
            valueInputField.onEndEdit.AddListener(TrySetValue);
        }

        void UpdateValue()
        {
            switch (_dataRow.Type)
            {
                case DataRowType.Bool:
                    initialValue.text = _dataRow.InitialValue.BoolValue.DRT();
                    break;
                case DataRowType.Int:
                    initialValue.text = _dataRow.InitialValue.IntValue.ToString();
                    break;
                case DataRowType.Float:
                    initialValue.text = _dataRow.InitialValue.FloatValue.ToString();
                    break;
                case DataRowType.String:
                    initialValue.text = _dataRow.InitialValue.StringValue;
                    break;
                case DataRowType.Vector:
                    initialValue.text = _dataRow.InitialValue.VectorValue.ToString();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            switch (_dataRow.Type)
            {
                case DataRowType.Bool:
                    boolToggle.gameObject.SetActive(true);
                    valueInputField.gameObject.SetActive(false);

                    boolToggle.isOn = _dataRow.RuntimeValue.BoolValue;
                    boolValueText.text = boolToggle.isOn ? "true".Rich(Color.green.Darken(0.2f)) : "false".Rich(Color.red);
                    break;
                case DataRowType.Int:
                case DataRowType.Float:
                case DataRowType.String:
                case DataRowType.Vector:
                    boolToggle.gameObject.SetActive(false);
                    valueInputField.gameObject.SetActive(true);

                    valueInputField.text = _dataRow.TargetValue.ToString();

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void TrySetValue(bool isTrue)
        {
            _dataRow.SetValue(isTrue);
            UpdateValue();
        }
        
        void TrySetValue(string text)
        {
            switch (_dataRow.Type)
            {
                case DataRowType.Int:
                {
                    if (int.TryParse(text, out var result)) _dataRow.SetValue(result);
                    break;
                }
                case DataRowType.Float:
                {
                    if (float.TryParse(text, out var result)) _dataRow.SetValue(result);
                    break;
                }
                case DataRowType.String:
                {
                    _dataRow.SetValue(text);
                    break;
                }
                case DataRowType.Vector:
                {
                    var split = text.Substring(1, text.Length - 2).Split(',');
                    var xString = split[0];
                    var yString = split[1];
                    var zString = split[2];
                    var wString = split[3];
                    if(float.TryParse(xString, out var x) && float.TryParse(yString, out var y) && 
                       float.TryParse(zString, out var z) && float.TryParse(wString, out var w)) 
                        _dataRow.SetValue(new Vector4(x, y, z, w));
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            UpdateValue();
        }
    }
}