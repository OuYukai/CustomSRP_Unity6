using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace LiteRP
{
    public class LiteRenderPipeline : RenderPipeline
    {
        // Old version
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            // 不實現
        }
        
        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            // Start Render Context
            BeginContextRendering(context, cameras);
            
            // Render Camera
            for (int i = 0; i < cameras.Count; i++)
            {
                Camera camera = cameras[i];
                RenderCamera(context, camera);
            }
            
            // End Render Context
            EndContextRendering(context, cameras);
        }

        private void RenderCamera(ScriptableRenderContext context, Camera camera)
        {
            // Start Render Camera
            BeginCameraRendering(context, camera);
            
            // 獲取相機剔除參數，並進行剔除
            ScriptableCullingParameters cullingParameters;
            if (!camera.TryGetCullingParameters(out cullingParameters))
                return;
            CullingResults cullingResults = context.Cull(ref cullingParameters);
            
            // 為相機創建CommandBuffer
            CommandBuffer cmd = CommandBufferPool.Get(camera.name);
            
            // 設置相機屬性參數
            context.SetupCameraProperties(camera);
            
            // 清理渲染目標
            // 指定渲染排序設置SortSettings
            // 指定渲染狀態設置DrawSettings
            // 指定渲染過濾設置FilterSettings
            // 創建渲染列表
            // 繪製渲染列表
            
            // 提交命令緩衝區
            context.ExecuteCommandBuffer(cmd);
            
            // 釋放命令緩衝區
            cmd.Clear();
            CommandBufferPool.Release(cmd);
            
            // 提交渲染上下文
            context.Submit();
            
            // End Render Camera
            EndCameraRendering(context, camera);
        }
    }
}