using System.Collections.Generic;
using Exanite.Drawing;
using Prototype.Exanite.Pathfinding;
using UnityEngine;

namespace Prototype.Exanite.ProcGen
{
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Dependencies")]
        public DrawingService DrawingService;

        [Header("Settings")]
        public Vector2Int Size = Vector2Int.one * 10;

        // Walk
        [Space]
        public float WalkPerlinScale = 1;
        public Vector2 WalkPerlinOffset = Vector2.one;
        public float WalkPerlinMultiplier = 1;
        public float WalkRandomMultiplier = 1;

        // RoomSize
        [Space]
        public float RoomSizePerlinScale = 1;
        public Vector2 RoomSizePerlinOffset = Vector2.one;
        public float RoomSizePerlinMultiplier = 1;
        public float RoomSizeRandomMultiplier = 1;

        [Header("Seed")]
        public bool UseSeed;
        public string Seed;

        [Header("Visualization")]
        public Color WalkCostMin = Color.green;
        public Color WalkCostMax = Color.red;

        [Space]
        public Color RoomSizeCostMin = Color.yellow;
        public Color RoomSizeCostMax = Color.magenta;

        [Space]
        public Color RoomAnchorColor = Color.blue;
        public Color RoomConnectionColor = Color.cyan;
        public Color RoomCandidateColor = Color.cyan;

        [Header("Nodes")]
        public List<RoomConnection> RoomConnections = new List<RoomConnection>();

        [Header("Generation")]
        public bool GenerateContinuously;
        public float GenerationDelay = 0.25f;

        private float[,] walkCosts;
        private float[,] roomSizeCosts;
        private bool[,] roomCandidates;

        private float generationTimer;

        private void Start()
        {
            Generate();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Generate();
            }

            if (GenerateContinuously)
            {
                generationTimer += Time.deltaTime;

                if (generationTimer > GenerationDelay)
                {
                    Generate();

                    generationTimer = 0;
                }
            }
        }

        private void Generate()
        {
            if (UseSeed)
            {
                Random.InitState(Seed.GetHashCode());
            }

            walkCosts = new float[Size.x, Size.y];
            roomSizeCosts = new float[Size.x, Size.y];
            roomCandidates = new bool[Size.x, Size.y];

            // Walk costs
            var walkPerlinOffsetFromSeed = new Vector2(Random.Range(0f, 10000f), Random.Range(0f, 10000f));

            for (var y = 0; y < walkCosts.GetLength(1); y++)
            {
                for (var x = 0; x < walkCosts.GetLength(0); x++)
                {
                    var perlin = Mathf.PerlinNoise(
                        x * WalkPerlinScale + WalkPerlinOffset.x + walkPerlinOffsetFromSeed.x,
                        y * WalkPerlinScale + WalkPerlinOffset.y + walkPerlinOffsetFromSeed.y);

                    perlin *= WalkPerlinMultiplier;

                    walkCosts[x, y] += perlin;
                }
            }

            for (var y = 0; y < walkCosts.GetLength(1); y++)
            {
                for (var x = 0; x < walkCosts.GetLength(0); x++)
                {
                    walkCosts[x, y] += Random.Range(0f, WalkRandomMultiplier);
                }
            }

            // Room size costs
            var roomSizePerlinOffsetFromSeed = new Vector2(Random.Range(0f, 10000f), Random.Range(0f, 10000f));

            for (var y = 0; y < roomSizeCosts.GetLength(1); y++)
            {
                for (var x = 0; x < roomSizeCosts.GetLength(0); x++)
                {
                    var perlin = Mathf.PerlinNoise(
                        x * RoomSizePerlinScale + RoomSizePerlinOffset.x + roomSizePerlinOffsetFromSeed.x,
                        y * RoomSizePerlinScale + RoomSizePerlinOffset.y + roomSizePerlinOffsetFromSeed.y);

                    perlin *= RoomSizePerlinMultiplier;

                    roomSizeCosts[x, y] += perlin;
                }
            }

            for (var y = 0; y < roomSizeCosts.GetLength(1); y++)
            {
                for (var x = 0; x < roomSizeCosts.GetLength(0); x++)
                {
                    roomSizeCosts[x, y] += Random.Range(0f, RoomSizeRandomMultiplier);
                }
            }

            // Room candidate selection
            var solver = new PathSolver(walkCosts);
            var path = solver.Path;

            foreach (var connection in RoomConnections)
            {
                var start = GetClosestPointInGrid(connection.A.position);
                var end = GetClosestPointInGrid(connection.B.position);

                solver.FindPath(start, end);

                if (path.IsValid)
                {
                    foreach (var waypoint in path.Waypoints)
                    {
                        roomCandidates[waypoint.x, waypoint.y] = true;
                    }
                }
            }
        }

        private Vector2Int GetClosestPointInGrid(Vector3 worldPosition)
        {
            var localPosition = transform.InverseTransformPoint(worldPosition);
            var localPositionInt = new Vector2Int(Mathf.RoundToInt(localPosition.x), Mathf.RoundToInt(localPosition.z));

            localPositionInt.x = Mathf.Clamp(localPositionInt.x, 0, Size.x - 1);
            localPositionInt.y = Mathf.Clamp(localPositionInt.y, 0, Size.y - 1);

            return localPositionInt;
        }

        private readonly HashSet<Transform> roomAnchorSet = new HashSet<Transform>();

        private void OnRenderObject()
        {
            using (var handle = DrawingService.BeginDrawing())
            {
                // Draw walk costs
                var maxWalkCost = 0.001f;
                for (var y = 0; y < walkCosts.GetLength(1); y++)
                {
                    for (var x = 0; x < walkCosts.GetLength(0); x++)
                    {
                        maxWalkCost = Mathf.Max(maxWalkCost, walkCosts[x, y]);
                    }
                }

                for (var y = 0; y < walkCosts.GetLength(1); y++)
                {
                    for (var x = 0; x < walkCosts.GetLength(0); x++)
                    {
                        handle.Color = Color.Lerp(WalkCostMin, WalkCostMax, walkCosts[x, y] / maxWalkCost);
                        handle.DrawCube(Vector3.right * x + Vector3.forward * y, Quaternion.identity, new Vector3(0.9f, 0.1f, 0.9f));
                    }
                }

                // Draw room size costs
                var maxRoomSizeCost = 0.001f;
                for (var y = 0; y < roomSizeCosts.GetLength(1); y++)
                {
                    for (var x = 0; x < roomSizeCosts.GetLength(0); x++)
                    {
                        maxRoomSizeCost = Mathf.Max(maxRoomSizeCost, roomSizeCosts[x, y]);
                    }
                }

                for (var y = 0; y < roomSizeCosts.GetLength(1); y++)
                {
                    for (var x = 0; x < roomSizeCosts.GetLength(0); x++)
                    {
                        handle.Color = Color.Lerp(RoomSizeCostMin, RoomSizeCostMax, roomSizeCosts[x, y] / maxRoomSizeCost);
                        handle.DrawCube(Vector3.right * x + Vector3.forward * y + Vector3.down, Quaternion.identity, new Vector3(0.9f, 0.1f, 0.9f));
                    }
                }

                // Draw room anchors and connections
                roomAnchorSet.Clear();
                foreach (var roomConnection in RoomConnections)
                {
                    roomAnchorSet.Add(roomConnection.A);
                    roomAnchorSet.Add(roomConnection.B);
                }

                handle.Color = RoomAnchorColor;
                foreach (var roomAnchor in roomAnchorSet)
                {
                    handle.DrawSphere(roomAnchor.position, Quaternion.identity, Vector3.one, DrawType.WireAndSolid);
                }

                handle.Color = RoomConnectionColor;
                handle.Topology = MeshTopology.Lines;
                foreach (var roomConnection in RoomConnections)
                {
                    handle.AddVertex(roomConnection.A.position);
                    handle.AddVertex(roomConnection.B.position);
                }

                // Draw room candidates
                handle.Color = RoomCandidateColor;
                for (var y = 0; y < roomCandidates.GetLength(1); y++)
                {
                    for (var x = 0; x < roomCandidates.GetLength(0); x++)
                    {
                        if (roomCandidates[x, y])
                        {
                            handle.DrawCube(new Vector3(x, 0, y), Quaternion.identity, Vector3.one * 0.5f);
                        }
                    }
                }
            }
        }
    }
}