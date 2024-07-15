using UnityEngine;

public class Food : IFieldPlaceable
{
    public Transform Transform { get; private set; }
    public Vector2Int ID { get; private set; }



    public Food(Vector2Int id, Transform transform)
    {
        Transform = transform;
        ID = id;

        transform.position = new Vector3(ID.x, 1f, ID.y);
    }
}