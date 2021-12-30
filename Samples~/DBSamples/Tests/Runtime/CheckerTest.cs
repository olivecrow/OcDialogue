using System;
using System.Collections;
using NUnit.Framework;
using OcDialogue;
using OcDialogue.DB;
using UnityEngine;
using UnityEngine.TestTools;
using Random = UnityEngine.Random;

namespace MyDB.Test
{
    public class CheckerTest
    {
        static CheckFactor Factor_QuestState(QuestState state, QuestState targetState, CheckFactor.Operator op)
        {
            var q = ScriptableObject.CreateInstance<Quest>();
            q.name = $"Test_Quest({state})";
            q.SetState(state);
            var factor = new CheckFactor();
            factor.TargetData = q;
            factor.detail = Quest.fieldName_QuestState;
            factor.EnmValue = targetState.ToString();
            factor.op = op;
            return factor;
        }

        static CheckFactor Factor_Item(ItemBase item, int targetValue, CheckFactor.Operator op)
        {
            var factor = new CheckFactor();
            factor.TargetData = item;
            factor.IntValue = targetValue;
            factor.op = op;
            return factor;
        }

        [UnityTest]
        public IEnumerator OneFactor()
        {
            var checker = new DataChecker();
            yield return null;
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


            
        // = item =
        var inv = new Inventory();
        Inventory.PlayerInventory = inv;
        var stackable = ScriptableObject.CreateInstance<GenericItem>();
        stackable.GUID = Random.Range(int.MinValue, int.MaxValue);
        stackable.subtype = GenericType.Consumable;
        yield return null;
        stackable.itemName = "TEST_Stackable";
        stackable.name = stackable.itemName;
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

        var unStackable = ScriptableObject.CreateInstance<GenericItem>();
        unStackable.GUID = Random.Range(int.MinValue, int.MaxValue);
        unStackable.subtype = GenericType.Consumable;
        yield return null;
        unStackable.isStackable = false;
        unStackable.itemName = "TEST_UnStackable";
        unStackable.name = unStackable.itemName;
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
        
        Inventory.PlayerInventory = null;
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
                Factor_QuestState(QuestState.WorkingOn, QuestState.Done, CheckFactor.Operator.Equal), // false
                Factor_QuestState(QuestState.Done, QuestState.Done, CheckFactor.Operator.Equal), // true
                Factor_QuestState(QuestState.WorkingOn, QuestState.Done, CheckFactor.Operator.Equal) // false
            };
            Result(false, checker);
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
    }
}