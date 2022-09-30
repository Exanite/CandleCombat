#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Exanite.Drawing
{
    [ExecuteAlways]
    public partial class DrawingService : MonoBehaviour
    {
        public static readonly Color DefaultColor = Color.white;

        public Material Material;
        public int ShaderPass = -1;
        public string ColorProperty = "_BaseColor";

        private CommandBuffer commandBuffer;
        private MaterialPropertyBlock propertyBlock;

        private readonly List<RenderCommand> renderQueue = new List<RenderCommand>();
        private readonly List<DrawingHandle> pooledHandles = new List<DrawingHandle>();
        private int currentHandleIndex;

        public event Action Rendering;

#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public int PooledHandleCount => pooledHandles.Count;

#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public int RenderQueueCount => renderQueue.Count;

        private void OnEnable()
        {
            propertyBlock = new MaterialPropertyBlock();
            commandBuffer = new CommandBuffer();
            commandBuffer.name = name;

            DrawingServiceRenderFeature.Rendering += OnRendering;
        }

        private void OnDisable()
        {
            DrawingServiceRenderFeature.Rendering -= OnRendering;

            renderQueue.Clear();
            commandBuffer.Release();
        }

        private void OnDestroy()
        {
            ClearPooledHandles();
        }

        public IDrawingHandle BeginDrawing()
        {
            if (!isActiveAndEnabled)
            {
                currentHandleIndex = 0;
            }

            if (currentHandleIndex >= pooledHandles.Count)
            {
                pooledHandles.Add(new DrawingHandle(this));
            }

            var handle = pooledHandles[currentHandleIndex];
            handle.Reset();

            currentHandleIndex++;

            return handle;
        }

        private void Enqueue(in RenderCommand command)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            renderQueue.Add(command);
        }

        private void ClearPooledHandles()
        {
            foreach (var handle in pooledHandles)
            {
                handle.Destroy();
            }

            pooledHandles.Clear();
            currentHandleIndex = 0;
        }

        private void OnRendering(ScriptableRenderContext context, Camera camera)
        {
            Rendering?.Invoke();

            commandBuffer.Clear();
            commandBuffer.SetProjectionMatrix(camera.projectionMatrix);
            commandBuffer.SetViewMatrix(camera.worldToCameraMatrix);

            foreach (var command in renderQueue)
            {
                command.ApplyDefaultsIfNeeded(this);
                if (command.IsValid())
                {
                    propertyBlock.SetColor(ColorProperty, command.Color);
                    commandBuffer.DrawMesh(command.Mesh, command.LocalToWorldMatrix, command.Material, command.SubmeshIndex, command.ShaderPass,
                        propertyBlock);
                }
            }

            context.ExecuteCommandBuffer(commandBuffer);

            currentHandleIndex = 0;
            renderQueue.Clear();
        }
    }
}