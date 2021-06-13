using UnityEngine;

public interface IHierarchyIconDrawable
{
    /// <summary>
    /// Editor Default Resources 폴더에 접근함. 확장자를 꼭 적어줄것.
    /// </summary>
    string IconPath { get; }

    /// <summary>
    /// Hierarchy 창의 오른쪽으로부터의 거리.
    /// Default = 15;
    /// </summary>
    int DistanceToText { get; }
}