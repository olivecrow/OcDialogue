using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    [Serializable]
    public class DynamicData
    {
        public string Key;
        public TransformData TransformData;
        public PrimitiveValue PrimitiveData;
#if DEBUG
        public string description;
        public DynamicDataBehaviour Behaviour;
#endif

        public DynamicData(){}
        
        public DynamicData(DynamicDataBehaviour behaviour)
        {
            Key = behaviour.Key;
            TransformData = new TransformData(behaviour.transform);
            PrimitiveData = behaviour.PrimitiveData;

#if DEBUG
            description = behaviour.description;
            Behaviour = behaviour;
#endif
        }
    }
}
