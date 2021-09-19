using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue.DB;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace OcDialogue
{
    public class DynamicDataBehaviour : MonoBehaviour
    {
        [PropertyOrder(-3)][ReadOnly]public string Key;
        [InfoBox("TransformData를 사용하면, OnEnable에서 GameProcessDB에 저장된 데이터를 사용하여 트랜스폼을 이동함.", InfoMessageType.Info, VisibleIf = nameof(UseTransformData))]
        [PropertyOrder(-2)]public bool UseTransformData;

        [HideLabel]
        [ShowInInspector]
        [PropertyOrder(-1)]
        [HorizontalGroup("1", MaxWidth = 100)]
        public DataRowType Type
        {
            get => PrimitiveData.Type;
            set => PrimitiveData.Type = value;
        }
        [HideLabel][HorizontalGroup("1")]public PrimitiveValue PrimitiveData;
        [HideLabel][HorizontalGroup("1")]public string description = "Description Here";
        
        [ShowIf(nameof(Type), DataRowType.Bool)] public UnityEvent<bool> OnEnable_Bool;
        [ShowIf(nameof(Type), DataRowType.Int)] public UnityEvent<int> OnEnable_Int;
        [ShowIf(nameof(Type), DataRowType.Float)] public UnityEvent<float> OnEnable_Float;
        [ShowIf(nameof(Type), DataRowType.String)] public UnityEvent<string> OnEnable_String;
        void Reset()
        {
            Key = Guid.NewGuid().ToString();
        }

        void OnEnable()
        {
            GameProcessDB.Instance.ReadDynamicDataOrRegister(this);
            switch (Type)
            {
                case DataRowType.Bool:
                    OnEnable_Bool.Invoke(PrimitiveData.BoolValue);
                    break;
                case DataRowType.Int:
                    OnEnable_Int.Invoke(PrimitiveData.IntValue);
                    break;
                case DataRowType.Float:
                    OnEnable_Float.Invoke(PrimitiveData.FloatValue);
                    break;
                case DataRowType.String:
                    OnEnable_String.Invoke(PrimitiveData.StringValue);
                    break;
            }
        }

        public void SetBool(bool value)
        {
            PrimitiveData.BoolValue = value;
            GameProcessDB.Instance.UpdateDynamicData(this);
        }

        public void SetInt(int value)
        {
            PrimitiveData.IntValue = value;
            GameProcessDB.Instance.UpdateDynamicData(this);
        }

        public void SetFloat(float value)
        {
            PrimitiveData.FloatValue = value;
            GameProcessDB.Instance.UpdateDynamicData(this);
        }

        public void SetString(string value)
        {
            PrimitiveData.StringValue = value;
            GameProcessDB.Instance.UpdateDynamicData(this);
        }

        public void UpdateTransformData()
        {
            GameProcessDB.Instance.UpdateDynamicData(this);
        }
    }
}
