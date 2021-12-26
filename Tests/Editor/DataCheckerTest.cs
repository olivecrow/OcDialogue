using System.Collections;
using System.Linq;
using NUnit.Framework;
using OcDialogue;
using OcDialogue.DB;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;


public static class DataCheckerTest
{
    public static bool HasNull(DataChecker checker)
    {
        if (checker.factors.Any(x => x == null || x.TargetData == null)) return true;
        if (checker.factors.Length == 0) return true;
        return false;
    }

    public static bool HasUnusedCheckeables(DataChecker checker)
    {
        return checker.HasUnusedCheckables();
    }
}