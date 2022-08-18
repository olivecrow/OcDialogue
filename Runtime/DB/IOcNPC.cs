using UnityEngine;

namespace OcDialogue.DB
{
    public enum Gender
    {
        None,
        Male,
        Female
    }
    public interface IOcNPC
    {
        OcData OcData { get; }
        Gender gender { get; }
        Color color { get; }
        string job { get; }
        string tribe { get; }
        string group { get; }
    }
}