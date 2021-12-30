using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using OcDialogue.DB;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace MyDB.Test.Editor
{
    public class GameProcessDBTest
    {
        [Test]
        public void ContainerTest()
        {
            foreach (var gameProcessData in GameProcessDB.Instance.Data)
            {
                DataRowContainerTest.DoAllTest(gameProcessData);   
            }
        }
    }
}