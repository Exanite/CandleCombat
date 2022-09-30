using System.Collections.Generic;
using UnityEngine;

namespace Prototype.Exanite.Pathfinding
{
    public class Path
    {
        public bool IsValid;

        public Path()
        {
            Waypoints = new List<Vector2Int>();
        }

        public Path(IEnumerable<Vector2Int> tiles)
        {
            Waypoints = new List<Vector2Int>(tiles);
        }

        public List<Vector2Int> Waypoints { get; }

        public bool HasNext()
        {
            return Waypoints.Count > 0;
        }

        public Vector2Int GetNext()
        {
            return Waypoints[Waypoints.Count - 1];
        }

        public bool TryGetNext(out Vector2Int waypoint)
        {
            waypoint = Vector2Int.zero;

            if (HasNext())
            {
                waypoint = GetNext();

                return true;
            }

            return false;
        }

        public void Pop()
        {
            Waypoints.RemoveAt(Waypoints.Count - 1);
        }
    }
}