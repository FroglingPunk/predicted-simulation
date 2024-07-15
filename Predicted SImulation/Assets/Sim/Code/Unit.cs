using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Unit : IFieldPlaceable
{
    public Transform Transform { get; private set; }
    public Vector2Int ID { get; private set; }

    private Vector2Int[] _path;
    private int _currentPathId;

    public bool HasPath => _path != null && _currentPathId < _path.Length;
    public Food Target { get; private set; }


    public Unit(Vector2Int id, Transform transform)
    {
        Transform = transform;
        ID = id;

        Transform.position = new Vector3(ID.x, 1f, ID.y);
    }

    public void SetPath(Vector2Int[] path)
    {
        _path = path;
        _currentPathId = 0;
    }

    public void SetTargetFood(Food food)
    {
        Target = food;
    }


    public async UniTask MoveOneTickAsync(float speed, CancellationToken cslToken)
    {
        if (!HasPath)
        {
            await UniTask.WaitForSeconds(1f / speed);
            return;
        }

        await UniTask.Yield(cslToken);

        var startPosition = Transform.position;
        var endPosition = new Vector3(_path[_currentPathId].x, 1f, _path[_currentPathId].y);

        for (var lerp = 0f; lerp < 1f; lerp += Time.deltaTime * speed)
        {
            if (cslToken.IsCancellationRequested)
            {
                return;
            }

            Transform.position = Vector3.Lerp(startPosition, endPosition, lerp);
            await UniTask.Yield(cslToken);
        }

        Transform.position = endPosition;

        ID = _path[_currentPathId];

        _currentPathId++;

        await UniTask.Yield(cslToken);
    }
}