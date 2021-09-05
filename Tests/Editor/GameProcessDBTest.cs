using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using OcDialogue.DB;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
public class GameProcessDBTest
{
    [Test]
    public void ContainerTest()
    {
        DataRowContainerTest.DoAllTest(GameProcessDB.Instance);
    }
}