using System.Collections.Generic;
using UnityEngine;

namespace Sandbox.PolyLabel
{
    public static class EdgeHelpers
    {
        public struct Edge
        {
            public readonly int V1;
            public readonly int V2;
            public int TriangleIndex;

            public Edge(int aV1, int aV2, int aIndex)
            {
                V1 = aV1;
                V2 = aV2;
                TriangleIndex = aIndex;
            }
        }

        public static IEnumerable<Edge> GetEdges(int[] aIndices)
        {
            var result = new List<Edge>();

            for (var i = 0; i < aIndices.Length; i += 3)
            {
                var v1 = aIndices[i];
                var v2 = aIndices[i + 1];
                var v3 = aIndices[i + 2];
                result.Add(new Edge(v1, v2, i));
                result.Add(new Edge(v2, v3, i));
                result.Add(new Edge(v3, v1, i));
            }

            return result;
        }

        public static IEnumerable<Edge> FindBoundary(this IEnumerable<Edge> aEdges)
        {
            var result = new List<Edge>(aEdges);

            for (var i = result.Count - 1; i > 0; i--)
            {
                for (var n = i - 1; n >= 0; n--)
                {
                    if (result[i].V1 == result[n].V2 && result[i].V2 == result[n].V1)
                    {
                        result.RemoveAt(i);
                        result.RemoveAt(n);
                        i--;
                        break;
                    }
                }
            }

            return result;
        }

        public static List<Vector2Int> Convert(this List<Edge> aEdges)
        {
            var result = new List<Vector2Int>();

            for (var i = 0; i < aEdges.Count; i++)
            {
                result.Add(new Vector2Int(aEdges[i].V1, aEdges[i].V2));
            }

            return result;
        }

        public static List<Edge> SortEdges(this IEnumerable<Edge> aEdges)
        {
            var result = new List<Edge>(aEdges);

            for (var i = 0; i < result.Count - 2; i++)
            {
                var edge = result[i];

                for (var n = i + 1; n < result.Count; n++)
                {
                    var a = result[n];

                    if (edge.V2 == a.V1)
                    {
                        if (n == i + 1)
                        {
                            break;
                        }

                        result[n] = result[i + 1];
                        result[i + 1] = a;
                        break;
                    }
                }
            }

            return result;
        }
    }
}