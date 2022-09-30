using System.Collections.Generic;
using UnityEngine;

namespace Prototype.Exanite.Pathfinding
{
    public class PathSolver
    {
        private NodeData[,] nodeData;

        private readonly List<Vector2Int> open;
        private readonly HashSet<Vector2Int> closed;

        public PathSolver(float[,] grid)
        {
            Grid = grid;

            Path = new Path();

            open = new List<Vector2Int>();
            closed = new HashSet<Vector2Int>();
        }

        public float[,] Grid { get; }
        public Path Path { get; }

        public bool FindPath(Vector2Int start, Vector2Int destination)
        {
            Prepare(Grid);

            open.Add(start);
            nodeData[start.x, start.y].FCost = 0;
            nodeData[start.x, start.y].GCost = 0;
            nodeData[start.x, start.y].IsOpen = true;

            Vector2Int current;
            var isSuccess = false;

            while (open.Count > 0)
            {
                current = open[0];

                for (var i = 1; i < open.Count; i++)
                {
                    if (nodeData[open[i].x, open[i].y].FCost < nodeData[current.x, current.y].FCost)
                    {
                        current = open[i];
                    }
                }

                open.Remove(current);
                closed.Add(current);
                nodeData[current.x, current.y].IsOpen = false;

                if (current == destination)
                {
                    isSuccess = true;

                    break;
                }

                void ProcessNeighbor(Vector2Int neighbor)
                {
                    if (neighbor.x < 0 || neighbor.y < 0 || neighbor.x >= Grid.GetLength(0) || neighbor.y >= Grid.GetLength(1))
                    {
                        return;
                    }

                    if (closed.Contains(neighbor))
                    {
                        return;
                    }

                    if (!nodeData[neighbor.x, neighbor.y].IsOpen)
                    {
                        open.Add(neighbor);
                        nodeData[neighbor.x, neighbor.y].IsOpen = true;

                        nodeData[neighbor.x, neighbor.y].GCost = float.PositiveInfinity;
                        nodeData[neighbor.x, neighbor.y].Parent = current;
                    }

                    var newGCost = nodeData[current.x, current.y].GCost + Heuristics.Manhattan(current, neighbor);

                    if (newGCost < nodeData[neighbor.x, neighbor.y].GCost)
                    {
                        nodeData[neighbor.x, neighbor.y].GCost = newGCost;
                        nodeData[neighbor.x, neighbor.y].Parent = current;
                    }

                    nodeData[neighbor.x, neighbor.y].FCost = nodeData[neighbor.x, neighbor.y].GCost + Heuristics.Manhattan(neighbor, destination);
                }

                ProcessNeighbor(new Vector2Int(current.x - 1, current.y - 1));
                ProcessNeighbor(new Vector2Int(current.x - 1, current.y));
                ProcessNeighbor(new Vector2Int(current.x - 1, current.y + 1));
                ProcessNeighbor(new Vector2Int(current.x, current.y + 1));
                ProcessNeighbor(new Vector2Int(current.x + 1, current.y + 1));
                ProcessNeighbor(new Vector2Int(current.x + 1, current.y));
                ProcessNeighbor(new Vector2Int(current.x + 1, current.y - 1));
                ProcessNeighbor(new Vector2Int(current.x, current.y - 1));
            }

            Path.IsValid = isSuccess;

            if (isSuccess)
            {
                RetracePath(start, destination, Path);
            }

            return Path.IsValid;
        }

        private void RetracePath(Vector2Int start, Vector2Int destination, Path path)
        {
            path.Waypoints.Clear();

            var current = destination;
            while (current != start)
            {
                path.Waypoints.Add(current);

                current = nodeData[current.x, current.y].Parent;
            }
        }

        private void Prepare(float[,] grid)
        {
            open.Clear();
            closed.Clear();

            if (nodeData == null
                || nodeData.GetLength(0) != grid.GetLength(0)
                || nodeData.GetLength(1) != grid.GetLength(1))
            {
                nodeData = new NodeData[grid.GetLength(0), grid.GetLength(1)];
            }

            for (var y = 0; y < nodeData.GetLength(1); y++)
            {
                for (var x = 0; x < nodeData.GetLength(0); x++)
                {
                    nodeData[x, y] = new NodeData();
                }
            }
        }

        private struct NodeData
        {
            public Vector2Int Parent;
            public float FCost;
            public float GCost;

            /// <summary>
            ///     Is the node in the open list. <br/>
            ///     Doesn't replace the use of the list, but optimizes
            ///     the contains check that is otherwise needed
            /// </summary>
            public bool IsOpen; // Change to HasOpened?
        }
    }
}