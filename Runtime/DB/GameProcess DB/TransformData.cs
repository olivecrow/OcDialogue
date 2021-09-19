using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public struct TransformData
    {
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;

        public static TransformData zero => new TransformData() { position = Vector3.zero, rotation = Quaternion.identity, scale = Vector3.one };

        public TransformData(Transform transform)
        {
            position = transform.position;
            scale = transform.localScale;
            rotation = transform.rotation;
        }
    }
}
