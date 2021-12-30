using OcDialogue;
using OcDialogue.DB;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyDB
{
    public class GameProcessData : OcData, IDataRowUser
    {
        public DataRowContainer DataRowContainer => dataRowContainer;
        public override string Address => category;

        public override string Category
        {
            get => category;
            set => category = value;
        }
        public string category;
        public DataRowContainer dataRowContainer;

        public void Overwrite(CommonSaveData saveData)
        {
            dataRowContainer.Overwrite(saveData.DataRowContainerDict);
        }
        
        public override bool IsTrue(string fieldName, CheckFactor.Operator op, object checkValue)
        {
            throw new System.NotImplementedException();
        }

        public override object GetValue(string fieldName)
        {
            throw new System.NotImplementedException();
        }

        public override string[] GetFieldNames()
        {
            throw new System.NotImplementedException();
        }

        public override void SetValue(string fieldName, DataSetter.Operator op, object value)
        {
            throw new System.NotImplementedException();
        }

        public override DataRowType GetValueType(string fieldName)
        {
            return DataRowType.String;
        }
    }
}