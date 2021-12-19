using System;
using UnityEngine;

namespace OcDialogue
{
    public interface IUsableItem
    {
        ItemBase ItemBase { get; }
        int MaxStack { get; }
        int CurrentStack { get; }
        bool IsUsableItem { get; }
        bool IsNowUsable();
        ItemUsageResult Use();
    }

    public enum ItemUsageResult
    {
        None,
        Success,
        Empty
    }
}