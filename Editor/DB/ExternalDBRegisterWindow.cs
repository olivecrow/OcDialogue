using OcDialogue.DB;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class ExternalDBRegisterWindow : OdinEditorWindow
    {
        [Button]
        void Register(OcData db)
        {
            DBManager.Instance.ExternalDBs.Add(db);
            Debug.Log($"DBManager에 외부 DB가 등록됨 : [{db.GetType().Name}]");
            Close();
        }
    }
}