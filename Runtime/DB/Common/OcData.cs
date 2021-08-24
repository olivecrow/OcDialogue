using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public abstract class OcData : ScriptableObject
    {
        public virtual OcData Parent => _parent;
        public abstract string Address { get; }
        public string TotalAddress => $"{(Parent == null ? "" : Parent.TotalAddress + "/")}{Address}";
        [HideInInspector][SerializeField] protected OcData _parent;

#if UNITY_EDITOR
        /// <summary> Editor Only. </summary>
        public void SetParent(OcData parent)
        {
            _parent = parent;
        }
#endif
    }
}
