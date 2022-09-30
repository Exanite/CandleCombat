using System.Collections.Generic;
using Exanite.Core.Utilities;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Exanite.Drawing
{
    public partial class DrawingService
    {
        public Mesh CubeMesh;
        public Mesh SphereMesh;

        public Mesh WireCubeMesh;
        public Mesh WireSphereMesh;

        public int SphereLoopCount = 8;

        private bool hasGeneratedMeshes;

        [Button]
        public void GenerateMeshes()
        {
            WireCubeMesh = GenerateWireCubeMesh();
            WireSphereMesh = GenerateWireSphereMesh();
        }

#if UNITY_EDITOR
        [Button]
        private void SaveMeshes()
        {
            var meshes = new Mesh[]
            {
                CubeMesh,
                SphereMesh,
                WireCubeMesh,
                WireSphereMesh,
            };

            foreach (var mesh in meshes)
            {
                if (!AssetDatabase.Contains(mesh))
                {
                    var absolutePath = EditorUtility.SaveFilePanel("Save Mesh", "Assets", mesh.name, "asset");
                    var relativePath = FileUtility.GetAssetsRelativePath(absolutePath);
                    AssetDatabase.CreateAsset(mesh, relativePath);
                }
            }
        }
#endif

        private Mesh GenerateWireCubeMesh()
        {
            var mesh = new Mesh();
            mesh.name = "WireCube";

            var vertices = new List<Vector3>
            {
                new Vector3(-1, -1, -1),
                new Vector3(-1, -1, 1),
                new Vector3(1, -1, 1),
                new Vector3(1, -1, -1),

                new Vector3(-1, 1, -1),
                new Vector3(-1, 1, 1),
                new Vector3(1, 1, 1),
                new Vector3(1, 1, -1),
            };

            for (var i = 0; i < vertices.Count; i++)
            {
                vertices[i] *= 0.5f;
            }

            var indices = new List<ushort>
            {
                0, 1,
                1, 2,
                2, 3,
                3, 0,

                4, 5,
                5, 6,
                6, 7,
                7, 4,

                0, 4,
                1, 5,
                2, 6,
                3, 7,
            };

            mesh.SetVertices(vertices);
            mesh.SetIndices(indices, MeshTopology.Lines, 0);

            return mesh;
        }

        private Mesh GenerateWireSphereMesh()
        {
            var mesh = new Mesh();
            mesh.name = "WireSphere";

            var vertices = new List<Vector3>();
            var indices = new List<ushort>();

            for (var x = 0; x < SphereLoopCount; x++)
            {
                var startIndex = 0;
                var angle = 360 * x / SphereLoopCount;
                var position = Vector3.right;
                position = Quaternion.Euler(Vector3.forward * angle) * position;

                vertices.Add(position);
                indices.Add((ushort)(startIndex + x));
                indices.Add((ushort)(startIndex + (x + 1) % SphereLoopCount));
            }

            for (var y = 0; y < SphereLoopCount; y++)
            {
                var startIndex = SphereLoopCount;
                var angle = 360 * y / SphereLoopCount;
                var position = Vector3.up;
                position = Quaternion.Euler(Vector3.right * angle) * position;

                vertices.Add(position);
                indices.Add((ushort)(startIndex + y));
                indices.Add((ushort)(startIndex + (y + 1) % SphereLoopCount));
            }

            for (var z = 0; z < SphereLoopCount; z++)
            {
                var startIndex = SphereLoopCount * 2;
                var angle = 360 * z / SphereLoopCount;
                var position = Vector3.forward;
                position = Quaternion.Euler(Vector3.up * angle) * position;

                vertices.Add(position);
                indices.Add((ushort)(startIndex + z));
                indices.Add((ushort)(startIndex + (z + 1) % SphereLoopCount));
            }

            for (var i = 0; i < vertices.Count; i++)
            {
                vertices[i] *= 0.5f;
            }

            mesh.SetVertices(vertices);
            mesh.SetIndices(indices, MeshTopology.Lines, 0);

            return mesh;
        }
    }
}