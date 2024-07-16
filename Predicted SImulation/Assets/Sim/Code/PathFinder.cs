using System.Collections.Generic;
using UnityEngine;

public class PathFinder
{
    public static Vector2Int[] Find(Vector2Int from, Vector2Int to, byte[,,] field, int fieldSize, int currentTick, int predictionRange)
    {
        if (!NeighborCellIsFree(from, field, fieldSize, currentTick + 1))
        {
            field[from.x, from.y, currentTick + 1] = 1;
            return new Vector2Int[] { from };
        }

        if (currentTick == 0)
        {
            for (var p = 0; p < predictionRange + 1; p++)
            {
                field[from.x, from.y, p] = 0;
            }
        }

        var moveCost = new ushort[fieldSize, fieldSize];
        var ncl = new List<Vector2Int> { from };

        moveCost[from.x, from.y] = ushort.MaxValue;

        ushort cycle = 1;

        var lastCyclePoints = new List<Vector2Int>(ncl);

        while (ncl.Count > 0 && moveCost[to.x, to.y] == 0 && cycle + currentTick < predictionRange + 1)
        {
            lastCyclePoints.Clear();
            lastCyclePoints.AddRange(ncl);
            ncl.Clear();

            for (var i = 0; i < lastCyclePoints.Count; i++)
            {
                var p = lastCyclePoints[i];

                if (p.x - 1 >= 0 && moveCost[p.x - 1, p.y] == 0)
                {
                    if (field[p.x - 1, p.y, currentTick + cycle] == 0 && field[p.x - 1, p.y, currentTick + cycle - 1] != 3)
                    {
                        moveCost[p.x - 1, p.y] = cycle;
                        ncl.Add(new Vector2Int(p.x - 1, p.y));
                    }
                }

                if (p.x + 1 < fieldSize && moveCost[p.x + 1, p.y] == 0)
                {
                    if (field[p.x + 1, p.y, currentTick + cycle] == 0 && field[p.x + 1, p.y, currentTick + cycle - 1] != 5)
                    {
                        moveCost[p.x + 1, p.y] = cycle;
                        ncl.Add(new Vector2Int(p.x + 1, p.y));
                    }
                }

                if (p.y - 1 >= 0 && moveCost[p.x, p.y - 1] == 0)
                {
                    if (field[p.x, p.y - 1, currentTick + cycle] == 0 && field[p.x, p.y - 1, currentTick + cycle - 1] != 2)
                    {
                        moveCost[p.x, p.y - 1] = cycle;
                        ncl.Add(new Vector2Int(p.x, p.y - 1));
                    }
                }

                if (p.y + 1 < fieldSize && moveCost[p.x, p.y + 1] == 0)
                {
                    if (field[p.x, p.y + 1, currentTick + cycle] == 0 && field[p.x, p.y + 1, currentTick + cycle - 1] != 4)
                    {
                        moveCost[p.x, p.y + 1] = cycle;
                        ncl.Add(new Vector2Int(p.x, p.y + 1));
                    }
                }
            }

            cycle++;
        }

        if (moveCost[to.x, to.y] == 0)
        {
            // путь заблокирован
            if (ncl.Count == 0)
            {
                to = FindClosestPoint(to, lastCyclePoints);
            }
            // не успевает до конца цикла добраться до цели
            else if (cycle + currentTick >= predictionRange + 1)
            {
                to = FindClosestPoint(to, ncl);
            }

            return Find(from, to, field, fieldSize, currentTick, predictionRange);
        }

        cycle--;

        var path = new Vector2Int[cycle];
        path[cycle - 1] = to;
        field[to.x, to.y, currentTick + cycle] = 1;

        var cPoint = to;
        for (var i = cycle - 1; i > 0; i--)
        {
            if (cPoint.x - 1 >= 0)
            {
                if (moveCost[cPoint.x - 1, cPoint.y] == i)
                {
                    cPoint.x--;
                    path[i - 1] = cPoint;
                    field[cPoint.x, cPoint.y, currentTick + i] = 3;
                    continue;
                }
            }

            if (cPoint.x + 1 < fieldSize)
            {
                if (moveCost[cPoint.x + 1, cPoint.y] == i)
                {
                    cPoint.x++;
                    path[i - 1] = cPoint;
                    field[cPoint.x, cPoint.y, currentTick + i] = 5;
                    continue;
                }
            }

            if (cPoint.y - 1 >= 0)
            {
                if (moveCost[cPoint.x, cPoint.y - 1] == i)
                {
                    cPoint.y--;
                    path[i - 1] = cPoint;
                    field[cPoint.x, cPoint.y, currentTick + i] = 2;
                    continue;
                }
            }

            if (cPoint.y + 1 < fieldSize)
            {
                if (moveCost[cPoint.x, cPoint.y + 1] == i)
                {
                    cPoint.y++;
                    path[i - 1] = cPoint;
                    field[cPoint.x, cPoint.y, currentTick + i] = 4;
                    continue;
                }
            }
        }

        var firstPoint = path[0];

        if (firstPoint.x > from.x)
        {
            field[from.x, from.y, currentTick] = 3;
        }
        else if (firstPoint.x < from.x)
        {
            field[from.x, from.y, currentTick] = 5;
        }
        else if (firstPoint.y > from.y)
        {
            field[from.x, from.y, currentTick] = 2;
        }
        else if (firstPoint.y < from.y)
        {
            field[from.x, from.y, currentTick] = 4;
        }

        return path;
    }


    private static bool NeighborCellIsFree(Vector2Int point, byte[,,] field, int fieldSize, int tick)
    {
        if (point.x - 1 >= 0)
        {
            if (field[point.x - 1, point.y, tick] == 0 && field[point.x - 1, point.y, tick - 1] != 3)
            {
                return true;
            }
        }

        if (point.x + 1 < fieldSize)
        {
            if (field[point.x + 1, point.y, tick] == 0 && field[point.x + 1, point.y, tick - 1] != 5)
            {
                return true;
            }
        }

        if (point.y - 1 >= 0)
        {
            if (field[point.x, point.y - 1, tick] == 0 && field[point.x, point.y - 1, tick - 1] != 2)
            {
                return true;
            }
        }

        if (point.y + 1 < fieldSize)
        {
            if (field[point.x, point.y + 1, tick] == 0 && field[point.x, point.y + 1, tick - 1] != 4)
            {
                return true;
            }
        }

        return false;
    }

    private static Vector2Int FindClosestPoint(Vector2Int goal, List<Vector2Int> points)
    {
        var minDistance = Mathf.Abs(points[0].x - goal.x) + Mathf.Abs(points[0].y - goal.y);
        var closestPoint = points[0];

        for (var i = 1; i < points.Count; i++)
        {
            var distance = Mathf.Abs(points[i].x - goal.x) + Mathf.Abs(points[i].y - goal.y);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = points[i];
            }
        }

        return closestPoint;
    }
}