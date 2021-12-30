using System;
using UnityEngine;

namespace MyDB
{
    public interface IUsableItem
    {
        ItemBase ItemBase { get; }
        int MaxStack { get; }
        int CurrentStack { get; }
        bool IsUsableItem { get; }
        bool IsNowUsable();
        ItemEliminationResult Use();
    }

    public enum ItemEliminationResult
    {
        None,
        Success,
        Empty
    }
}