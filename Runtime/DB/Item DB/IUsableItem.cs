using System;

namespace OcDialogue
{
    public interface IUsableItem
    {
        bool IsUsableItem { get; }
        bool IsNowUsable();
        bool Use(Action onEmpty);
    }
}