using UnityEngine;

namespace MyDB
{
    public interface INPCCharacter
    {
        GameObject gameObject { get; }
        Transform transform { get; }
        NPC NPC { get; }
    }
}