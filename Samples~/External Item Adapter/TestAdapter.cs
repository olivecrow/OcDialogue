using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcDialogue;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Samples
{
    [CreateAssetMenu(fileName = "TestAdapter", menuName = "Oc Dialogue/Sample/TestAdapter")]
    public class TestAdapter : ScriptableObject
    {
        [InlineButton("CreateTestItem", "Create")]
        public TestItem template;

        /*
         * 이 메서드를 복사해서 사용하면 됨.
         * 요점은 외부에서 만든 후 아이템베이스에 집어넣는것.
         */
        void CreateTestItem()
        {
            var asset = CreateInstance<TestItem>();
            internalProcess(asset);
        }

        void internalProcess(ItemBase asset)
        {
            asset.GUID = OcDataUtility.CalcItemGUID();
            asset.type = template.type;

            asset.name = OcDataUtility.CalculateDataName($"New {asset.GetType()}", ItemDatabase.Instance.Items.Select(x => x.itemName));
            asset.itemName = asset.name;

            ItemDatabase.Instance.Items.Add(asset);
            AssetDatabase.AddObjectToAsset(asset, ItemDatabase.Instance);
            EditorUtility.SetDirty(ItemDatabase.Instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

}