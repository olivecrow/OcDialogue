using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue.DB
{
    public interface IOcDataSelectable
    {
#if UNITY_EDITOR
      OcData Data { get; }
        string Detail { get; }
        void UpdateAddress();
        void SetTargetData(OcData data);  
#endif
    }
    
    public interface IStandardDB
    {
        string[] CategoryRef { get; }
        IEnumerable<OcData> AllData { get; }
    }

    public interface IDataRowUser
    {
        DataRowContainer DataRowContainer { get; }
    }
}
