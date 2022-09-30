using System;
using System.Collections.Generic;
using UnityEngine;

namespace Exanite.Drawing
{
    public partial class DrawingService
    {
        private partial class DrawingHandle : IDrawingHandle
        {
            public const float DefaultSolidMeshAlpha = 0.5f;

            private Color color;
            private Material material;
            private int shaderPass;
            private MeshTopology topology;
            private Matrix4x4 localToWorldMatrix;

            private int lastPushIndex;
            private int currentSubmeshIndex;

            private readonly List<Vector3> vertices = new List<Vector3>();
            private readonly List<ushort> indices = new List<ushort>();
            private readonly Mesh mesh;

            private readonly DrawingService drawingService;

            public DrawingHandle(DrawingService drawingService)
            {
                this.drawingService = drawingService;

                mesh = new Mesh();
                Reset();
            }

            public Color Color
            {
                get => color;
                set
                {
                    if (color != value)
                    {
                        PushCurrent();
                        color = value;
                    }
                }
            }

            public float SolidMeshAlpha { get; set; }

            public Material Material
            {
                get => material;
                set
                {
                    if (material != value)
                    {
                        PushCurrent();
                        material = value;
                    }
                }
            }

            public int ShaderPass
            {
                get => shaderPass;
                set
                {
                    if (shaderPass != value)
                    {
                        PushCurrent();
                        shaderPass = value;
                    }
                }
            }

            public MeshTopology Topology
            {
                get => topology;
                set
                {
                    if (topology != value)
                    {
                        PushCurrent();
                        topology = value;
                    }
                }
            }

            public Matrix4x4 LocalToWorldMatrix
            {
                get => localToWorldMatrix;
                set
                {
                    if (localToWorldMatrix != value)
                    {
                        PushCurrent();
                        localToWorldMatrix = value;
                    }
                }
            }

            public void AddVertex(Vector3 vertex)
            {
                vertices.Add(vertex);
            }

            public void PushCurrent()
            {
                if (vertices.Count == lastPushIndex)
                {
                    return;
                }

                while (indices.Count < vertices.Count)
                {
                    indices.Add((ushort)indices.Count);
                }

                mesh.subMeshCount = currentSubmeshIndex + 1;
                mesh.SetVertices(vertices);
                mesh.SetIndices(
                    indices,
                    lastPushIndex,
                    vertices.Count - lastPushIndex,
                    Topology,
                    currentSubmeshIndex);

                drawingService.Enqueue(new RenderCommand
                {
                    Mesh = mesh,
                    SubmeshIndex = currentSubmeshIndex,

                    Material = Material,
                    ShaderPass = ShaderPass,
                    Color = Color,

                    LocalToWorldMatrix = LocalToWorldMatrix,
                });

                lastPushIndex = vertices.Count;
                currentSubmeshIndex++;
            }

            public void Reset()
            {
                lastPushIndex = 0;
                currentSubmeshIndex = 0;

                color = DefaultColor;
                SolidMeshAlpha = DefaultSolidMeshAlpha;
                material = null;
                shaderPass = -1;
                topology = MeshTopology.Triangles;
                localToWorldMatrix = Matrix4x4.identity;

                mesh.Clear();
                vertices.Clear();
            }

            public void Destroy()
            {
                DestroyImmediate(mesh);
            }

            void IDisposable.Dispose()
            {
                PushCurrent();
            }
        }
    }
}