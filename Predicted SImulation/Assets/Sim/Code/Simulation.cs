using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;

public class Simulation : MonoBehaviour
{
    [SerializeField] private bool DEBUG_CELLS_STATES = false;
    [SerializeField] private int _predictionRange = 16;

    private int _currentTick = 0;
    private Field _field;
    private CancellationTokenSource _cslTokenSource;

    private int _unitsCountNextCycle = 0;
    private int _fieldSizeNextCycle = 0;


    private void Start()
    {
        SimPrefs.UnitsCount.Subscribe(newUnitsCount => _unitsCountNextCycle = newUnitsCount);
        SimPrefs.FieldSize.Subscribe(newFieldSize => _fieldSizeNextCycle = newFieldSize);

        _unitsCountNextCycle = SimPrefs.UnitsCount.Value;
        _fieldSizeNextCycle = SimPrefs.FieldSize.Value;

        _ = SimCycleAsync();
    }

    private void OnDestroy()
    {
        _cslTokenSource?.Cancel();
        _cslTokenSource?.Dispose();
    }


    private async UniTask SimCycleAsync()
    {
        _cslTokenSource = new();

        _field = new Field(_predictionRange);
        await _field.GenerateCellsAsync(_fieldSizeNextCycle);
        _field.CreateAndInitUnits(_unitsCountNextCycle, _currentTick);

        while (!_cslTokenSource.IsCancellationRequested)
        {
            var tasks = new List<UniTask>(_field.Units.Count);

            for (var i = 0; i < _field.Units.Count; i++)
            {
                var unit = _field.Units[i];

                if (!unit.HasPath)
                {
                    if (unit.ID == unit.Target.ID)
                    {
                        var id = unit.ID.y * _field.Size + unit.ID.x;
                        var reachedFood = _field.Food[id];
                        _field.Food.Remove(id);
                        Destroy(reachedFood.Transform.gameObject);

                        var newTargetFood = _field.CreateFood(unit);
                        var path = PathFinder.Find(unit.ID, newTargetFood.ID, _field.CellsState, _field.Size, _currentTick, _predictionRange);
                        unit.SetPath(path);
                    }
                    else
                    {
                        var path = PathFinder.Find(unit.ID, unit.Target.ID, _field.CellsState, _field.Size, _currentTick, _predictionRange);
                        unit.SetPath(path);
                    }
                }

                tasks.Add(unit.MoveOneTickAsync(SimPrefs.SimulationSpeed.Value, _cslTokenSource.Token));
            }

            if (DEBUG_CELLS_STATES)
            {
                _field.DebugCellStates(_currentTick);
            }

            await UniTask.WhenAll(tasks);
            await UniTask.Yield(_cslTokenSource.Token);

            _currentTick++;

            if (_currentTick == _predictionRange)
            {
                _currentTick = 0;

                if (_field.Size != _fieldSizeNextCycle)
                {
                    _field.Clear();
                    await _field.GenerateCellsAsync(_fieldSizeNextCycle);
                    _field.CreateAndInitUnits(_unitsCountNextCycle, _currentTick);
                }
                else if (_unitsCountNextCycle > _field.Units.Count)
                {
                    var delta = _unitsCountNextCycle - _field.Units.Count;
                    _field.CreateAndInitUnits(delta, _currentTick);
                }
                else if (_unitsCountNextCycle < _field.Units.Count)
                {
                    var delta = _field.Units.Count - _unitsCountNextCycle;
                    _field.RemoveUnits(delta, _currentTick);
                }

                _field.ResetFieldForNewCycle();
            }
        }
    }
}