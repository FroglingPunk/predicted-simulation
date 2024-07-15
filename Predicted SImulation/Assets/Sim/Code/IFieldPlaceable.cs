using UnityEngine;

public interface IFieldPlaceable
{
    public Transform Transform { get; }
    public Vector2Int ID { get; }
}