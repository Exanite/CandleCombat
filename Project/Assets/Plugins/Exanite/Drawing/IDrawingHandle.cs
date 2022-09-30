using System;
using UnityEngine;

namespace Exanite.Drawing
{
    public interface IDrawingHandle : IDisposable
    {
        public Color Color { get; set; }
        public float SolidMeshAlpha { get; set; }
        public Material Material { get; set; }
        public int ShaderPass { get; set; }
        public MeshTopology Topology { get; set; }
        public Matrix4x4 LocalToWorldMatrix { get; set; }

        public void AddVertex(Vector3 vertex);
        public void PushCurrent();

        public void DrawCube(Vector3 position, Quaternion rotation, Vector3 scale, DrawType drawType = DrawType.Solid);
        public void DrawCube(Matrix4x4 matrix, DrawType drawType = DrawType.Solid);

        public void DrawSphere(Vector3 position, Quaternion rotation, Vector3 scale, DrawType drawType = DrawType.Solid);
        public void DrawSphere(Matrix4x4 matrix, DrawType drawType = DrawType.Solid);

        public void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation, Vector3 scale, int submeshIndex = -1);
        public void DrawMesh(Mesh mesh, Matrix4x4 matrix, int submeshIndex = -1);
    }
}