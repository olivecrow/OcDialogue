using UnityEngine;

namespace MyDB
{
    public interface IGamePlayer
    {
        GameObject gameObject { get; }
        Transform transform { get; }
        TransformData LastSafeTransformData { get; }
        public float HP { get; }
    }
}