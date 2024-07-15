using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

public class Field
{
    private GameObject _cellPrefab;
    private GameObject _unitGoPrefab;
    private GameObject _foodGoPrefab;
    private int _predictionRange = 16;
    private Transform _fieldTransform;

    public int Size { get; private set; }

    // 0 = free cell
    // 1 = occupied cell
    // 2 = moving y+
    // 3 = moving x+
    // 4 = moving y-
    // 5 = moving x-
    public byte[,,] CellsState;
    public Transform[,] CellsTransform;

    public List<Unit> Units = new();
    public Dictionary<int, Food> Food = new();


    public Field(int predictionRange)
    {
        _predictionRange = predictionRange;
    }


    public void DebugCellStates(int tick)
    {
        for (var h = 0; h < CellsTransform.GetLength(1); h++)
        {
            for (var w = 0; w < CellsTransform.GetLength(0); w++)
            {
                var color = Color.white;

                switch (CellsState[w, h, tick])
                {
                    case 0:
                        color = Color.green;
                        break;

                    case 1:
                        color = Color.red;
                        break;

                    case 2:
                        color = Color.blue;
                        break;

                    case 3:
                        color = Color.black;
                        break;

                    case 4:
                        color = Color.yellow;
                        break;

                    case 5:
                        color = Color.white;
                        break;
                }

                Debug.DrawRay(CellsTransform[w, h].position, Vector3.up * 10f, color, 1f / SimPrefs.SimulationSpeed.Value);
            }
        }
    }

    public async UniTask GenerateCellsAsync(int size)
    {
        if (_cellPrefab == null)
        {
            await LoadPrefabsAsync();
        }

        if (_fieldTransform != null)
        {
            Clear();
        }

        Size = size;

        _fieldTransform = new GameObject($"Field {size}X{size}").transform;

        CellsState = new byte[size, size, _predictionRange + 1];

        CellsTransform = new Transform[size, size];

        var cellLocalPosition = Vector3.zero;

        for (var h = 0; h < size; h++)
        {
            cellLocalPosition.z = h;

            for (var w = 0; w < size; w++)
            {
                cellLocalPosition.x = w;
                var cellTransform = Object.Instantiate(_cellPrefab, _fieldTransform).transform;
                cellTransform.localPosition = cellLocalPosition;

                CellsTransform[w, h] = cellTransform;
            }
        }
    }

    public void Clear()
    {
        RemoveUnits(Units.Count, 0);

        Object.Destroy(_fieldTransform.gameObject);
        CellsTransform = null;
        CellsState = null;

        Units.Clear();
        Food.Clear();
    }

    public void CreateAndInitUnits(int count, int currentTick)
    {
        for (var i = 0; i < count; i++)
        {
            CreateUnit(currentTick);
        }

        for (var i = 0; i < Units.Count; i++)
        {
            var unit = Units[i];
            var food = CreateFood(unit);

            var path = PathFinder.Find(unit.ID, food.ID, CellsState, Size, currentTick, _predictionRange);
            unit.SetPath(path);
        }
    }

    public void RemoveUnits(int count, int currentTick)
    {
        count = Mathf.Clamp(count, 0, Units.Count);

        for (var i = 0; i < count; i++)
        {
            var unit = Units[0];

            for (var p = currentTick; p < _predictionRange + 1; p++)
            {
                CellsState[unit.ID.x, unit.ID.y, p] = 0;
            }

            var unitFood = unit.Target;

            Food.Remove(unitFood.ID.y * Size + unitFood.ID.x);
            Object.Destroy(unitFood.Transform.gameObject);

            Object.Destroy(unit.Transform.gameObject);
            Units.RemoveAt(0);
        }
    }

    public void ResetFieldForNewCycle()
    {
        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                for (var p = 0; p < _predictionRange + 1; p++)
                {
                    CellsState[x, y, p] = 0;
                }
            }
        }

        for (var i = 0; i < Units.Count; i++)
        {
            var unit = Units[i];

            for (var p = 0; p < _predictionRange + 1; p++)
            {
                CellsState[unit.ID.x, unit.ID.y, p] = 1;
            }
        }
    }

    private Unit CreateUnit(int currentTick)
    {
        var unitPoint = GetFreePointForUnit(currentTick);
        var unitGo = Object.Instantiate(_unitGoPrefab);
        var unit = new Unit(unitPoint, unitGo.transform);
        Units.Add(unit);

        for (var p = currentTick; p < _predictionRange + 1; p++)
        {
            CellsState[unit.ID.x, unit.ID.y, p] = 1;
        }

        return unit;
    }

    public Food CreateFood(Unit unit)
    {
        var foodPoint = GetFreePointForFood(unit);
        var foodGo = Object.Instantiate(_foodGoPrefab);
        var food = new Food(foodPoint, foodGo.transform);
        Food.Add(foodPoint.y * Size + foodPoint.x, food);
        unit.SetTargetFood(food);
        return food;
    }

    private Vector2Int GetFreePointForUnit(int currentTick)
    {
        while (true)
        {
            var x = Random.Range(0, Size);
            var y = Random.Range(0, Size);

            if (CellsState[x, y, currentTick] == 0 && CellsState[x, y, currentTick + 1] == 0)
            {
                return new Vector2Int(x, y);
            }
        }
    }

    private Vector2Int GetFreePointForFood(Unit unit)
    {
        var simSpeed = SimPrefs.SimulationSpeed.Value;

        while (true)
        {
            var x = Random.Range(Mathf.Clamp(unit.ID.x - (int)(simSpeed * 5), 0, Size), Mathf.Clamp(unit.ID.x + (int)(simSpeed * 5), 0, Size));
            var y = Random.Range(Mathf.Clamp(unit.ID.y - (int)(simSpeed * 5), 0, Size), Mathf.Clamp(unit.ID.y + (int)(simSpeed * 5), 0, Size));

            if (unit.ID.x != x && unit.ID.y != y && !Food.ContainsKey(y * Size + x))
            {
                return new Vector2Int(x, y);
            }
        }
    }

    private async UniTask LoadPrefabsAsync()
    {
        var loadTasks = new[]
            {
                Addressables.LoadAssetAsync<GameObject>("Cell").Task,
                Addressables.LoadAssetAsync<GameObject>("Unit").Task,
                Addressables.LoadAssetAsync<GameObject>("Food").Task
            };

        await Task.WhenAll(loadTasks);

        _cellPrefab = loadTasks[0].Result;
        _unitGoPrefab = loadTasks[1].Result;
        _foodGoPrefab = loadTasks[2].Result;
    }
}