using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyDB
{
    public class ImportantItem : ItemBase
    {
        public override ItemType type => ItemType.Important;
        public ImportantItemType subtype;
        public override string SubTypeString => subtype.ToString();

        protected override ItemBase CreateInstance()
        {
            return CreateInstance<ImportantItem>();
        }

        protected override void ApplyTypeProperty(ItemBase baseCopy)
        {
            var copy = baseCopy as ImportantItem;
            copy.subtype = subtype;
        }

        protected override void AppendAdditionalSaveData(CommonSaveData saveData)
        {
            
        }

        protected override void OverwriteAdditionalSaveData(CommonSaveData saveData)
        {
            
        }

#if UNITY_EDITOR
        public override void SetSubTypeFromString(string subtypeName)
        {
            subtype = (ImportantItemType) Enum.Parse(typeof(ImportantItemType), subtypeName);
        }
#endif
    }
}
