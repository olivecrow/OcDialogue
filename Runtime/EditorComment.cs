using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcUtility
{
    public class EditorComment : MonoBehaviour, IHierarchyIconDrawable
    {
#if UNITY_EDITOR || DEBUG_BUILD
        public string IconPath => "EditorComment Icon";
        public int DistanceToText => -55;
        [HideInInspector] GameObject gizmoTarget;
        public Context[] Contexts;

        void Reset()
        {
            Contexts = new Context[1];
            gameObject.hideFlags = HideFlags.DontSaveInBuild;
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, "Comment");
        }


        [Serializable]
        public class Context
        {
            [HideLabel] public string header;

            [TextArea(minLines: 5, maxLines: 20), HideLabel]
            public string content;
        }
#endif
    }
}