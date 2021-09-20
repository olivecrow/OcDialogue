using UnityEngine;

namespace OcDialogue.DB.GameProcess_DB
{
    public interface IGamePlayer
    {
        GameObject gameObject { get; }
        Transform transform { get; }
        TransformData LastSafeTransformData { get; }
        public float HP { get; }
    }
}