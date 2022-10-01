using UnityEngine;

namespace Exanite.Drawing
{
    public partial class DrawingService
    {
        public struct RenderCommand
        {
            public Mesh Mesh;
            public int SubmeshIndex;

            public Material Material;
            public int ShaderPass;
            public Color Color;

            public Matrix4x4 LocalToWorldMatrix;

            public bool IsValid()
            {
                return Mesh && Material;
            }

            public void ApplyDefaultsIfNeeded(DrawingService drawingService)
            {
                if (!Material)
                {
                    Material = drawingService.Material;
                    ShaderPass = drawingService.ShaderPass;
                }
            }
        }
    }
}