using UnityEngine;

namespace OcDialogue.DB
{
    public interface INPCCharacter
    {
        GameObject gameObject { get; }
        Transform transform { get; }
        NPC NPC { get; }
    }
}