using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue.DB
{
    public interface IOcDataSelectable
    {
#if UNITY_EDITOR
        OcData TargetData { get; set; }
        string Detail { get; }
        void UpdateExpression();
#endif
    }

    public interface IDataRowUser
    {
        DataRowContainer DataRowContainer { get; }
    }
}
