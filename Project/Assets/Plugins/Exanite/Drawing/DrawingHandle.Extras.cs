using UnityEngine;

namespace Exanite.Drawing
{
    public partial class DrawingService
    {
        private partial class DrawingHandle
        {
            public void DrawCube(Vector3 position, Quaternion rotation, Vector3 scale, DrawType drawType = DrawType.Solid)
            {
                var matrix = Matrix4x4.TRS(position, rotation, scale);
                DrawCube(matrix, drawType);
            }

            public void DrawCube(Matrix4x4 matrix, DrawType drawType = DrawType.Solid)
            {
                if ((drawType & DrawType.Solid) == DrawType.Solid)
                {
                    DrawMesh(drawingService.CubeMesh, matrix, GetSolidMeshColor(), 0);
                }

                if ((drawType & DrawType.Wire) == DrawType.Wire)
                {
                    DrawMesh(drawingService.WireCubeMesh, matrix, 0);
                }
            }

            public void DrawSphere(Vector3 position, Quaternion rotation, Vector3 scale, DrawType drawType = DrawType.Solid)
            {
                var matrix = Matrix4x4.TRS(position, rotation, scale);
                DrawSphere(matrix, drawType);
            }

            public void DrawSphere(Matrix4x4 matrix, DrawType drawType = DrawType.Solid)
            {
                if ((drawType & DrawType.Solid) == DrawType.Solid)
                {
                    DrawMesh(drawingService.SphereMesh, matrix, GetSolidMeshColor(), 0);
                }

                if ((drawType & DrawType.Wire) == DrawType.Wire)
                {
                    DrawMesh(drawingService.WireSphereMesh, matrix, 0);
                }
            }

            public void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation, Vector3 scale, int submeshIndex = -1)
            {
                var matrix = Matrix4x4.TRS(position, rotation, scale);
                DrawMesh(mesh, matrix, submeshIndex);
            }

            public void DrawMesh(Mesh mesh, Matrix4x4 matrix, int submeshIndex = -1)
            {
                DrawMesh(mesh, matrix, Color, submeshIndex);
            }

            private void DrawMesh(Mesh mesh, Matrix4x4 matrix, Color color, int submeshIndex = -1)
            {
                if (submeshIndex == -1)
                {
                    for (var i = 0; i < mesh.subMeshCount; i++)
                    {
                        DrawMesh(mesh, matrix, i);
                    }
                }
                else
                {
                    drawingService.Enqueue(new RenderCommand
                    {
                        Mesh = mesh,
                        SubmeshIndex = submeshIndex,

                        Material = Material,
                        ShaderPass = ShaderPass,
                        Color = color,

                        LocalToWorldMatrix = LocalToWorldMatrix * matrix,
                    });
                }
            }

            private Color GetSolidMeshColor()
            {
                return Color * new Color(1, 1, 1, SolidMeshAlpha);
            }
        }
    }
}