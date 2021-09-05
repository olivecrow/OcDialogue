using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using OcDialogue;
using OcDialogue.DB;
using UnityEngine;
using UnityEngine.TestTools;

public class DataSetterTest
{
    static void Result(object expect, DataRow dataRow, string prefix = "")
    {
        if(!dataRow.IsTrue(CheckFactor.Operator.Equal, expect)) 
            Debug.LogError($"[{dataRow.Name}] {prefix}) 잘못된 IsTrue 결과 | expect : {expect} | actual : {dataRow.TargetValue}");
    }

    static void Result(int expect, ItemBase item)
    {
        if(!Inventory.PlayerInventory.Count(item).IsTrue(CheckFactor.Operator.Equal, expect))
            Debug.LogError($"[{item.itemName}] 잘못된 IsTrue 결과 | expect : {expect} | actual : {Inventory.PlayerInventory.Count(item)}");
    }
    [UnityTest]
    public IEnumerator GameProcessDBTest()
    {
        if (GameProcessDB.Instance == null)
        {
            Debug.LogWarning($"[DataSetterTest] GameProcessDB가 없음");
            yield break;
        }
        yield return null;

        var setter = new DataSetter();
        var dataRow = GameProcessDB.Instance.DataRowContainer.AddData();
        var initialValue = new PrimitiveValue()
        {
            BoolValue = true,
            IntValue = 100,
            FloatValue = 10f,
            StringValue = "Hello"
        };

        void resetValue()
        {
            dataRow.GetType()
                .GetField("_initialValue", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(dataRow, initialValue);
            dataRow.GetType()
                .GetField("_runtimeValue", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(dataRow, initialValue);
        }
        
        setter.SetTargetData(dataRow);
        resetValue();
        // Equal
        // bool
        setter.SetTargetValue(false);
        setter.Execute();
        Result(false, dataRow);
        
        // int
        // set
        dataRow.Type = DataRowType.Int;
        setter.SetTargetValue(99);
        setter.Execute();
        Result(99, dataRow);
        resetValue();
        
        // add
        dataRow.Type = DataRowType.Int;
        setter.SetTargetValue(10);
        setter.op = DataSetter.Operator.Add;
        setter.Execute();
        Result(110, dataRow, "Add");
        resetValue();
        
        // multiply
        dataRow.Type = DataRowType.Int;
        setter.SetTargetValue(10);
        setter.op = DataSetter.Operator.Multiply;
        setter.Execute();
        Result(1000, dataRow, "Multiply");
        resetValue();
        
        // divide
        dataRow.Type = DataRowType.Int;
        setter.SetTargetValue(10);
        setter.op = DataSetter.Operator.Divide;
        setter.Execute();
        Result(10, dataRow, "Divide");
        resetValue();
        
        
        // float
        dataRow.Type = DataRowType.Float;
        setter.op = DataSetter.Operator.Set;
        setter.SetTargetValue(9.9f);
        setter.Execute();
        Result(9.9f, dataRow);

        // string
        dataRow.Type = DataRowType.String;
        setter.SetTargetValue("Bye");
        setter.Execute();
        Result("Bye", dataRow);

        
        GameProcessDB.Instance.DataRowContainer.DeleteRow(dataRow.Name);
    }

    [UnityTest]
    public IEnumerator ItemDBTest()
    {
        if (ItemDatabase.Instance == null)
        {
            Debug.LogWarning($"[DataSetterTest] ItemDatabase가 없음");
            yield break;
        }
        
        var setter = new DataSetter();
        var inv = new Inventory();
        Inventory.PlayerInventory = inv;
        yield return null;

        var stackable = ItemDatabase.Instance.AddItem(ItemType.Generic, GenericType.Consumable.ToString());
        yield return null;
        stackable.itemName = "TEST_Stackable";
        stackable.isStackable = true;
        stackable.maxStackCount = 999;
        
        setter.SetTargetData(stackable);
        setter.op = DataSetter.Operator.Add;
        setter.IntValue = 10;
        setter.Execute();
        Result(10, stackable);

        setter.op = DataSetter.Operator.Add;
        setter.IntValue = -2;
        setter.Execute();
        Result(8, stackable);
        
        
        var unStackable = ItemDatabase.Instance.AddItem(ItemType.Generic, GenericType.Consumable.ToString());
        yield return null;
        unStackable.itemName = "TEST_UnStackable";
        unStackable.isStackable = false;

        setter.SetTargetData(unStackable);
        setter.op = DataSetter.Operator.Add;
        setter.IntValue = 10;
        setter.Execute();
        Result(10, unStackable);

        setter.op = DataSetter.Operator.Add;
        setter.IntValue = -2;
        setter.Execute();
        Result(8, unStackable);
        
        ItemDatabase.Instance.DeleteItem(stackable);
        ItemDatabase.Instance.DeleteItem(unStackable);
    }
}
