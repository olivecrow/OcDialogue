using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public class GameProcessDataUser : DataUser
    {
        public List<DataRow> DataRows;
        public override void Load()
        {
#if UNITY_EDITOR
            if (GameProcessDataPreset.Instance.usePreset)
            {
                DataRows = GameProcessDataPreset.Instance.GetAllCopies();
                return;
            }
#endif
            
            DataRows = GameProcessDatabase.Instance.GetAllCopies();
            // TODO : 데이터 로드.
        }

        public override void Save()
        {
        }
    }
}
