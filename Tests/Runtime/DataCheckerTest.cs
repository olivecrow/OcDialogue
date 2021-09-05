using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using OcDialogue;
using OcDialogue.DB;
using UnityEngine;
using UnityEngine.TestTools;

public class DataCheckerTest
{
    static CheckFactor Factor_DataRow(DataRowType type, object value, object targetValue, CheckFactor.Operator op)
    {
        var row = ScriptableObject.CreateInstance<DataRow>();
        row.name = $"Test_{type}({value})";
        row.SetTypeAndValue(type, value);
        var factor = new CheckFactor();
        factor.SetTargetData(row);
        factor.SetTargetValue(targetValue);
        factor.op = op;
        return factor;
    }

    static CheckFactor Factor_QuestState(QuestState state, QuestState targetState, CheckFactor.Operator op)
    {
        var q = ScriptableObject.CreateInstance<Quest>();
        q.name = $"Test_Quest({state})";
        q.SetState(state);
        var factor = new CheckFactor();
        factor.SetTargetData(q);
        factor.SetTargetValue(targetState);
        factor.op = op;
        return factor;
    }

    static CheckFactor Factor_Item(ItemBase item, int targetValue, CheckFactor.Operator op)
    {
        var factor = new CheckFactor();
        factor.SetTargetData(item);
        factor.SetTargetValue(targetValue);
        factor.op = op;
        return factor;
    }

    static Binding Binding(DataChecker checker, Condition condition, params int[] indices)
    {
        var binding = new Binding();
        binding.checkables = new List<int>();
        foreach (var index in indices)
        {
            binding.checkables.Add(index);
        }

        binding.condition = condition;
        binding.Index = checker.factors?.Length ?? 0 + checker.bindings?.Count ?? 0;
        return binding;
    }

    void Result(bool expect, DataChecker checker)
    {
        var expression = checker.ToExpression();
        if (checker.bindings == null || checker.bindings.Count == 0)
        {
            expression = "";
            for (int i = 0; i < checker.factors.Length; i++)
            {
                expression += checker.factors[i].ToExpression();
                if (i < checker.factors.Length - 1) expression += "&&";
            }
        }

        if (expect != checker.IsTrue())
            Debug.LogError($"잘못된 factor.IsTrue 결과 | expect : {expect} | actual : {checker.IsTrue()}\n" +
                           $"expression : {expression}");
    }

    [UnityTest]
    public IEnumerator OneFactor()
    {
        var checker = new DataChecker();
        yield return null;

        // = bool =
        // Equal
        checker.factors = new[] { Factor_DataRow(DataRowType.Bool, false, false, CheckFactor.Operator.Equal) };
        Result(true, checker);

        checker.factors = new[] { Factor_DataRow(DataRowType.Bool, false, true, CheckFactor.Operator.Equal) };
        Result(false, checker);

        // Not Equal
        checker.factors = new[] { Factor_DataRow(DataRowType.Bool, false, true, CheckFactor.Operator.NotEqual) };
        Result(true, checker);

        checker.factors = new[] { Factor_DataRow(DataRowType.Bool, false, false, CheckFactor.Operator.NotEqual) };
        Result(false, checker);

        // = string =
        // Equal
        checker.factors = new[] { Factor_DataRow(DataRowType.String, "false", "false", CheckFactor.Operator.Equal) };
        Result(true, checker);

        checker.factors = new[] { Factor_DataRow(DataRowType.String, "false", "true", CheckFactor.Operator.Equal) };
        Result(false, checker);

        // Not Equal
        checker.factors = new[] { Factor_DataRow(DataRowType.String, "false", "true", CheckFactor.Operator.NotEqual) };
        Result(true, checker);

        checker.factors = new[] { Factor_DataRow(DataRowType.String, "false", "false", CheckFactor.Operator.NotEqual) };
        Result(false, checker);
        
        
        // = QuestState =
        // Equal
        checker.factors = new[] { Factor_QuestState(QuestState.None, QuestState.None,  CheckFactor.Operator.Equal) };
        Result(true, checker);

        checker.factors = new[] { Factor_QuestState(QuestState.None, QuestState.WorkingOn, CheckFactor.Operator.Equal) };
        Result(false, checker);

        // Not Equal
        checker.factors = new[] { Factor_QuestState(QuestState.None, QuestState.WorkingOn, CheckFactor.Operator.NotEqual) };
        Result(true, checker);

        checker.factors = new[] { Factor_QuestState(QuestState.None, QuestState.None,  CheckFactor.Operator.NotEqual) };
        Result(false, checker);


        // = int =
        // -Equal
        checker.factors = new[] { Factor_DataRow(DataRowType.Int, 100, 100, CheckFactor.Operator.Equal) };
        Result(true, checker);

        checker.factors = new[] { Factor_DataRow(DataRowType.Int, 100, 101, CheckFactor.Operator.Equal) };
        Result(false, checker);

        // -Not Equal
        checker.factors = new[] { Factor_DataRow(DataRowType.Int, 100, 101, CheckFactor.Operator.NotEqual) };
        Result(true, checker);

        checker.factors = new[] { Factor_DataRow(DataRowType.Int, 100, 100, CheckFactor.Operator.NotEqual) };
        Result(false, checker);

        // -Greater
        checker.factors = new[] { Factor_DataRow(DataRowType.Int, 100, 99, CheckFactor.Operator.Greater) };
        Result(true, checker);

        checker.factors = new[] { Factor_DataRow(DataRowType.Int, 100, 100, CheckFactor.Operator.Greater) };
        Result(false, checker);

        // -Greater Equal
        checker.factors = new[] { Factor_DataRow(DataRowType.Int, 100, 100, CheckFactor.Operator.GreaterEqual) };
        Result(true, checker);

        checker.factors = new[] { Factor_DataRow(DataRowType.Int, 100, 101, CheckFactor.Operator.GreaterEqual) };
        Result(false, checker);

        // -Less
        checker.factors = new[] { Factor_DataRow(DataRowType.Int, 100, 101, CheckFactor.Operator.Less) };
        Result(true, checker);

        checker.factors = new[] { Factor_DataRow(DataRowType.Int, 100, 100, CheckFactor.Operator.Less) };
        Result(false, checker);

        // -Less Equal
        checker.factors = new[] { Factor_DataRow(DataRowType.Int, 100, 100, CheckFactor.Operator.LessEqual) };
        Result(true, checker);

        checker.factors = new[] { Factor_DataRow(DataRowType.Int, 100, 99, CheckFactor.Operator.LessEqual) };
        Result(false, checker);


        // = float =
        // -Equal
        checker.factors = new[] { Factor_DataRow(DataRowType.Float, 10f, 10f, CheckFactor.Operator.Equal) };
        Result(true, checker);

        checker.factors = new[] { Factor_DataRow(DataRowType.Float, 10f, 10.01f, CheckFactor.Operator.Equal) };
        Result(false, checker);

        // -Not Equal
        checker.factors = new[] { Factor_DataRow(DataRowType.Float, 10f, 10.01f, CheckFactor.Operator.NotEqual) };
        Result(true, checker);

        checker.factors = new[] { Factor_DataRow(DataRowType.Float, 10f, 10f, CheckFactor.Operator.NotEqual) };
        Result(false, checker);

        // -Greater
        checker.factors = new[] { Factor_DataRow(DataRowType.Float, 10f, 9.99f, CheckFactor.Operator.Greater) };
        Result(true, checker);

        checker.factors = new[] { Factor_DataRow(DataRowType.Float, 10f, 10f, CheckFactor.Operator.Greater) };
        Result(false, checker);

        // -Greater Equal
        checker.factors = new[] { Factor_DataRow(DataRowType.Float, 10f, 10f, CheckFactor.Operator.GreaterEqual) };
        Result(true, checker);

        checker.factors = new[] { Factor_DataRow(DataRowType.Float, 10f, 10.01f, CheckFactor.Operator.GreaterEqual) };
        Result(false, checker);

        // -Less
        checker.factors = new[] { Factor_DataRow(DataRowType.Float, 10f, 10.01f, CheckFactor.Operator.Less) };
        Result(true, checker);

        checker.factors = new[] { Factor_DataRow(DataRowType.Float, 10f, 10f, CheckFactor.Operator.Less) };
        Result(false, checker);

        // -Less Equal
        checker.factors = new[] { Factor_DataRow(DataRowType.Float, 10f, 10f, CheckFactor.Operator.LessEqual) };
        Result(true, checker);

        checker.factors = new[] { Factor_DataRow(DataRowType.Float, 10f, 9.99f, CheckFactor.Operator.LessEqual) };
        Result(false, checker);
        
        
        // = item =
        var inv = new Inventory();
        Inventory.PlayerInventory = inv;
        var stackable = ItemDatabase.Instance.AddItem(ItemType.Generic, GenericType.Consumable.ToString());
        yield return null;
        stackable.itemName = "TEST_Stackable";
        stackable.isStackable = true;
        stackable.maxStackCount = 999;
        inv.AddItem(stackable, 20);
        
        checker.factors = new[] { Factor_Item(stackable, 20, CheckFactor.Operator.Equal) };
        Result(true, checker);
        checker.factors = new[] { Factor_Item(stackable, 19, CheckFactor.Operator.Equal) };
        Result(false, checker);

        checker.factors = new[] { Factor_Item(stackable, 19, CheckFactor.Operator.NotEqual) };
        Result(true, checker);
        checker.factors = new[] { Factor_Item(stackable, 20, CheckFactor.Operator.NotEqual) };
        Result(false, checker);
        
        checker.factors = new[] { Factor_Item(stackable, 19, CheckFactor.Operator.Greater) };
        Result(true, checker);
        checker.factors = new[] { Factor_Item(stackable, 20, CheckFactor.Operator.Greater) };
        Result(false, checker);
        
        checker.factors = new[] { Factor_Item(stackable, 20, CheckFactor.Operator.GreaterEqual) };
        Result(true, checker);
        checker.factors = new[] { Factor_Item(stackable, 21, CheckFactor.Operator.GreaterEqual) };
        Result(false, checker);
        
        checker.factors = new[] { Factor_Item(stackable, 21, CheckFactor.Operator.Less) };
        Result(true, checker);
        checker.factors = new[] { Factor_Item(stackable, 20, CheckFactor.Operator.Less) };
        Result(false, checker);
        
        checker.factors = new[] { Factor_Item(stackable, 20, CheckFactor.Operator.LessEqual) };
        Result(true, checker);
        checker.factors = new[] { Factor_Item(stackable, 19, CheckFactor.Operator.LessEqual) };
        Result(false, checker);

        var unStackable = ItemDatabase.Instance.AddItem(ItemType.Generic, GenericType.Consumable.ToString());
        yield return null;
        unStackable.isStackable = false;
        unStackable.itemName = "TEST_UnStackable";
        inv.AddItem(unStackable, 20);
        
        checker.factors = new[] { Factor_Item(unStackable, 20, CheckFactor.Operator.Equal) };
        Result(true, checker);
        checker.factors = new[] { Factor_Item(unStackable, 19, CheckFactor.Operator.Equal) };
        Result(false, checker);

        checker.factors = new[] { Factor_Item(unStackable, 19, CheckFactor.Operator.NotEqual) };
        Result(true, checker);
        checker.factors = new[] { Factor_Item(unStackable, 20, CheckFactor.Operator.NotEqual) };
        Result(false, checker);
        
        checker.factors = new[] { Factor_Item(unStackable, 19, CheckFactor.Operator.Greater) };
        Result(true, checker);
        checker.factors = new[] { Factor_Item(unStackable, 20, CheckFactor.Operator.Greater) };
        Result(false, checker);
        
        checker.factors = new[] { Factor_Item(unStackable, 20, CheckFactor.Operator.GreaterEqual) };
        Result(true, checker);
        checker.factors = new[] { Factor_Item(unStackable, 21, CheckFactor.Operator.GreaterEqual) };
        Result(false, checker);
        
        checker.factors = new[] { Factor_Item(unStackable, 21, CheckFactor.Operator.Less) };
        Result(true, checker);
        checker.factors = new[] { Factor_Item(unStackable, 20, CheckFactor.Operator.Less) };
        Result(false, checker);
        
        checker.factors = new[] { Factor_Item(unStackable, 20, CheckFactor.Operator.LessEqual) };
        Result(true, checker);
        checker.factors = new[] { Factor_Item(unStackable, 19, CheckFactor.Operator.LessEqual) };
        Result(false, checker);
        
        ItemDatabase.Instance.DeleteItem(stackable);
        ItemDatabase.Instance.DeleteItem(unStackable);
        Inventory.PlayerInventory = null;
    }

    [UnityTest]
    public IEnumerator TwoFactors()
    {
        yield return null;

        var checker = new DataChecker();
        // No Binding
        // A && B
        checker.factors = new[]
        {
            Factor_DataRow(DataRowType.Int, 100, 101, CheckFactor.Operator.Equal),
            Factor_DataRow(DataRowType.Bool, true, true, CheckFactor.Operator.Equal)
        };
        Result(false, checker);
        
        // Binding
        // A && B
        {
            checker.bindings = new List<Binding>();
            var binding = Binding(checker, Condition.And, 0, 1);
            checker.bindings.Add(binding);
            Result(false, checker);
        }
        
        // A || B
        {
            checker.bindings = new List<Binding>();
            var binding = Binding(checker, Condition.Or, 0, 1);
            checker.bindings.Add(binding);
            Result(true, checker);
        }
    }

    [UnityTest]
    public IEnumerator ThreeFactors()
    {
        yield return null;
        var checker = new DataChecker();
        
        // No Binding
        // A && B && C
        checker.factors = new[]
        {
            Factor_DataRow(DataRowType.Bool, true, false, CheckFactor.Operator.NotEqual), // true
            Factor_DataRow(DataRowType.Int, 100, 100, CheckFactor.Operator.Equal), // true
            Factor_QuestState(QuestState.WorkingOn, QuestState.Done, CheckFactor.Operator.Equal) // false
        };
        Result(false, checker);
        
        // Bindings
        // (A && B) && C
        {
            checker.bindings = new List<Binding>();
            var binding1 = Binding(checker, Condition.And, 0, 1);
            var binding2 = Binding(checker, Condition.And, binding1.Index, 2);
            checker.bindings.Add(binding1);
            checker.bindings.Add(binding2);
            Result(false, checker);
        }
        // A && (B && C)
        {
            checker.bindings = new List<Binding>();
            var binding1 = Binding(checker, Condition.And, 1, 2);
            var binding2 = Binding(checker, Condition.And, binding1.Index, 0);
            checker.bindings.Add(binding1);
            checker.bindings.Add(binding2);
            Result(false, checker);
        }
        // A && (B || C)
        {
            checker.bindings = new List<Binding>();
            var binding1 = Binding(checker, Condition.Or, 1, 2);
            var binding2 = Binding(checker, Condition.And, binding1.Index, 0);
            checker.bindings.Add(binding1);
            checker.bindings.Add(binding2);
            Result(true, checker);
        }
        // A || (B && C)
        {
            checker.bindings = new List<Binding>();
            var binding1 = Binding(checker, Condition.And, 1, 2);
            var binding2 = Binding(checker, Condition.Or, binding1.Index, 0);
            checker.bindings.Add(binding1);
            checker.bindings.Add(binding2);
            Result(true, checker);
        }
        // A || B || C
        {
            checker.bindings = new List<Binding>();
            var binding1 = Binding(checker, Condition.Or, 0, 1, 2);
            checker.bindings.Add(binding1);
            Result(true, checker);
        }
    }

}