using UnityEngine;

namespace Prototype.Exanite.Pathfinding
{
    public static class Heuristics
    {
        public static float Default(Vector2Int a, Vector2Int b)
        {
            return Euclidean(a, b);
        }

        public static float Euclidean(Vector2Int a, Vector2Int b)
        {
            return Vector2Int.Distance(a, b);
        }

        public static float Manhattan(Vector2Int a, Vector2Int b)
        {
            var dx = Mathf.Abs(a.x - b.x);
            var dy = Mathf.Abs(a.y - b.y);

            return dx + dy;
        }
    }
}