using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public abstract class AddressableData : ScriptableObject
    {
        public virtual AddressableData Parent => _parent;
        public abstract string Address { get; }
        public string TotalAddress => $"{(Parent == null ? "" : Parent.TotalAddress + "/")}{Address}";
        [HideInInspector][SerializeField] protected AddressableData _parent;

#if UNITY_EDITOR
        /// <summary> Editor Only. </summary>
        public void SetParent(AddressableData parent)
        {
            _parent = parent;
        }  
#endif
    }
}
