using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Exanite.Drawing
{
    public class DrawingServiceRenderFeature : ScriptableRendererFeature
    {
        public static event Action<ScriptableRenderContext, Camera> Rendering;
        public static readonly CameraType AllowedCameraTypes = CameraType.Game | CameraType.SceneView;

        private class DrawingServiceRenderPass : ScriptableRenderPass
        {
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if ((renderingData.cameraData.cameraType | AllowedCameraTypes) == AllowedCameraTypes)
                {
                    Rendering?.Invoke(context, renderingData.cameraData.camera);
                }
            }
        }

        private DrawingServiceRenderPass renderPass;

        public override void Create()
        {
            renderPass = new DrawingServiceRenderPass();
            renderPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(renderPass);
        }
    }
}