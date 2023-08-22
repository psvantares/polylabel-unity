using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sandbox.PolyLabel
{
    public abstract class PolyLabelNet
    {
        public static (float radius, Vector2 pole) FindPoleOfIsolation(List<Vector2> vertices, float precision = 1)
        {
            var minX = float.MaxValue;
            var minY = float.MaxValue;
            var maxX = float.MinValue;
            var maxY = float.MinValue;

            foreach (var edgePoint in vertices)
            {
                minX = minX > edgePoint.x ? edgePoint.x : minX;
                minY = minY > edgePoint.y ? edgePoint.y : minY;
                maxX = maxX < edgePoint.x ? edgePoint.x : maxX;
                maxY = maxY < edgePoint.y ? edgePoint.y : maxY;
            }

            var width = maxX - minX;
            var height = maxY - minY;
            var cellSize = Mathf.Min(width, height);
            var h = cellSize / 2;

            var bestCell = GetCentroidCell(vertices);

            var bboxCell = new Cell(minX + width / 2, minY + height / 2, 0, vertices);
            bestCell = bboxCell.Distance > bestCell.Distance ? bboxCell : bestCell;

            var cellQueue = new PriorityQueue();

            for (var x = minX; x < maxX; x += cellSize)
            {
                for (var y = minY; y < maxY; y += cellSize)
                {
                    var cell = new Cell(x + h, y + h, h, vertices);
                    cellQueue.Enqueue(cell, cell.MaxDistance);
                }
            }

            while (cellQueue.Count > 0)
            {
                var cell = cellQueue.Dequeue();

                if (cell.Distance > bestCell.Distance)
                {
                    bestCell = cell;
                }

                if (cell.MaxDistance - bestCell.Distance <= precision)
                {
                    continue;
                }

                h = cell.HalfSize / 2;

                var cell1 = new Cell(cell.Center.x - h, cell.Center.y - h, h, vertices);
                cellQueue.Enqueue(cell1, cell1.MaxDistance);
                var cell2 = new Cell(cell.Center.x + h, cell.Center.y - h, h, vertices);
                cellQueue.Enqueue(cell2, cell2.MaxDistance);
                var cell3 = new Cell(cell.Center.x - h, cell.Center.y + h, h, vertices);
                cellQueue.Enqueue(cell3, cell3.MaxDistance);
                var cell4 = new Cell(cell.Center.x + h, cell.Center.y + h, h, vertices);
                cellQueue.Enqueue(cell4, cell4.MaxDistance);
            }

            return (bestCell.Distance, bestCell.Center);
        }

        private static Cell GetCentroidCell(List<Vector2> vertices)
        {
            var centroid = Centroid(vertices);
            return new Cell(centroid.x, centroid.y, 0, vertices);
        }

        private static Vector2 Centroid(IReadOnlyList<Vector2> vertices)
        {
            var pointCount = vertices.Count;
            float totalArea = 0;
            float centroidX = 0;
            float centroidY = 0;

            for (var i = 0; i < pointCount; i++)
            {
                var currentPoint = vertices[i];
                var nextPoint = vertices[(i + 1) % pointCount];

                var area = currentPoint.x * nextPoint.y - nextPoint.x * currentPoint.y;
                totalArea += area;

                centroidX += (currentPoint.x + nextPoint.x) * area;
                centroidY += (currentPoint.y + nextPoint.y) * area;
            }

            totalArea *= 0.5f;
            centroidX /= 6 * totalArea;
            centroidY /= 6 * totalArea;

            return new Vector2(centroidX, centroidY);
        }

        private class Cell
        {
            public readonly Vector2 Center;
            public readonly float HalfSize;
            public readonly float Distance;
            public readonly float MaxDistance;

            public Cell(float x, float y, float h, List<Vector2> vertices)
            {
                Center = new Vector2(x, y);
                HalfSize = h;
                Distance = PointToPolygonDist(Center, vertices);
                MaxDistance = Distance + HalfSize * (float)Math.Sqrt(2);
            }

            private float PointToPolygonDist(Vector2 point, List<Vector2> vertices)
            {
                var minDist = float.PositiveInfinity;

                for (var i = 0; i < vertices.Count; i++)
                {
                    minDist = Math.Min(minDist, DistancePointSegment(point, vertices[i], vertices[(i + 1) % vertices.Count]));
                }

                if (!IsPointInside(point, vertices))
                {
                    minDist *= -1;
                }

                return minDist;
            }

            private float DistancePointSegment(Vector2 point, Vector2 segStart, Vector2 segEnd)
            {
                var squaredLength = Mathf.Pow(Vector2.Distance(segEnd, segStart), 2);
                if (squaredLength == 0)
                {
                    return Vector2.Distance(point, segStart);
                }

                var t = Mathf.Clamp01(Vector2.Dot(point - segStart, segEnd - segStart) / squaredLength);
                var projection = segStart + t * (segEnd - segStart);
                return Vector2.Distance(point, projection);
            }

            private static bool IsPointInside(Vector2 point, IReadOnlyList<Vector2> vertices)
            {
                var count = vertices.Count;
                var isInside = false;

                for (int i = 0, j = count - 1; i < count; j = i++)
                {
                    if (vertices[i].y > point.y != vertices[j].y > point.y &&
                        point.x < vertices[j].x + (point.y - vertices[j].y) * (vertices[i].x - vertices[j].x) / (vertices[i].y - vertices[j].y))
                    {
                        isInside = !isInside;
                    }
                }

                return isInside;
            }
        }

        private class PriorityQueue
        {
            private readonly List<(Cell c, float p)> list;

            public int Count => list.Count;

            public PriorityQueue()
            {
                list = new List<(Cell e, float p)>();
            }

            public void Enqueue(Cell cell, float priority)
            {
                list.Add((cell, priority));
            }

            public Cell Dequeue()
            {
                var maxPriority = float.MinValue;
                var indexMax = 0;
                for (var i = 0; i < list.Count; i++)
                {
                    var (_, p) = list[i];

                    if (!(maxPriority < p))
                    {
                        continue;
                    }

                    maxPriority = p;
                    indexMax = i;
                }

                var elem = list[indexMax].c;
                list.RemoveAt(indexMax);
                return elem;
            }
        }
    }
}