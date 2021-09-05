using System.Collections;
using NUnit.Framework;
using OcDialogue.DB;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using OcDialogue;

public static class DataRowContainerTest
{
    public static void DoAllTest(IDataRowUser user)
    {
        ParentTest(user);
        OverlapTest(user);
    }

    public static void ParentTest(IDataRowUser user)
    {
        var ocData = user as OcData;
        var isValidParent = user.DataRowContainer.Parent == ocData;
        if (!isValidParent)
        {
            Debug.LogWarning($"[{ocData.Address}]유효하지 않은 Parent를 수정힘");
            user.DataRowContainer.Parent = ocData;
            EditorUtility.SetDirty(ocData);
        }

        foreach (var dataRow in user.DataRowContainer.DataRows)
        {
            isValidParent = dataRow.Parent == ocData;

            if (!isValidParent)
            {
                Debug.LogWarning($"[{dataRow.Address}]유효하지 않은 Parent를 수정힘");
                dataRow.SetParent(ocData);
                EditorUtility.SetDirty(dataRow);
            }
        }
    }

    public static void OverlapTest(IDataRowUser user)
    {
        foreach (var dataRow in user.DataRowContainer.DataRows)
        {
            var count = user.DataRowContainer.DataRows.Count(x => x.Name == dataRow.Name);
            if (count > 1) Debug.LogError($"[{dataRow.Address}] 중복된 이름이 존재함");
        }
    }
}