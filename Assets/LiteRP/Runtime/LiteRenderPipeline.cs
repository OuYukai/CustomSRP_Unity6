using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using LiteRP.FrameData;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public class LiteRenderPipeline : RenderPipeline
    {
        // SRPDefaultUnlit : shader 的 Pass 沒有指定 Name 時預設使用
        private readonly static ShaderTagId _shaderTagId = new ShaderTagId("SRPDefaultUnlit");

        // 渲染圖
        private RenderGraph m_RenderGraph = null;
        // 渲染圖紀錄器
        private LiteRenderGraphRecorder m_LiteRenderGraphRecorder = null;
        // 上下文容器
        private ContextContainer m_ContextContainer = null;

        public LiteRenderPipeline()
        {
            InitializeRenderGraph();
        }
        
        protected override void Dispose(bool disposing)
        {
            CleanupRenderGraph();
            base.Dispose(disposing);
        }
        
        // 初始化渲染圖
        private void InitializeRenderGraph()
        {
            m_RenderGraph = new RenderGraph("LiteRPRenderGraph");
            m_LiteRenderGraphRecorder = new LiteRenderGraphRecorder();
            m_ContextContainer = new ContextContainer();
        }
        
        // 清理渲染圖
        private void CleanupRenderGraph()
        {
            m_RenderGraph?.Cleanup();
            m_RenderGraph = null;
            m_ContextContainer?.Dispose();
            m_ContextContainer = null;
            m_LiteRenderGraphRecorder = null;
        }
        
        // 老版本
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            // 不實現
        }
        
        // 新版本
        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            // 開始渲染上下文
            BeginContextRendering(context, cameras);
            
            // 渲染相機
            for (int i = 0; i < cameras.Count; i++)
            {
                Camera camera = cameras[i];
                RenderCamera(context, camera);
            }
            
            // 結束渲染圖
            m_RenderGraph.EndFrame();
            
            // 結束渲染上下文
            EndContextRendering(context, cameras);
        }

        private void RenderCamera(ScriptableRenderContext context, Camera camera)
        {
            // 開始渲染相機
            BeginCameraRendering(context, camera);
            
            // 準備 FrameData
            if(!PrepareFrameData(context, camera))
                return;
            
            
            
            // 為相機創建CommandBuffer
            CommandBuffer cmd = CommandBufferPool.Get(camera.name);
            
            // 設置相機屬性參數
            context.SetupCameraProperties(camera);

            // 紀錄並執行渲染圖
            RecordAndExecuteRenderGraph(context, camera, cmd);
            
            // 提交命令緩衝區
            context.ExecuteCommandBuffer(cmd);
            
            // 釋放命令緩衝區
            cmd.Clear();
            CommandBufferPool.Release(cmd);
            
            // 提交渲染上下文
            context.Submit();
            
            // 結束渲染相機
            EndCameraRendering(context, camera);
        }

        private bool PrepareFrameData(ScriptableRenderContext context, Camera camera)
        {
            // 獲取相機剔除參數，並進行剔除
            ScriptableCullingParameters cullingParameters;
            if (!camera.TryGetCullingParameters(out cullingParameters))
                return false;
            CullingResults cullingResults = context.Cull(ref cullingParameters);
            
            // Camera Data
            CameraData cameraData = m_ContextContainer.GetOrCreate<CameraData>();
            cameraData.camera = camera;
            cameraData.CullingResults = cullingResults;
            
            return true;
        }

        private void RecordAndExecuteRenderGraph(ScriptableRenderContext context, Camera camera, CommandBuffer cmd)
        {
            RenderGraphParameters renderGraphParameters = new RenderGraphParameters()
            {
                executionName = camera.name,
                commandBuffer = cmd,
                scriptableRenderContext = context,
                currentFrameIndex = Time.frameCount
            };
            m_RenderGraph.BeginRecording(renderGraphParameters);
            
            // 開啟紀錄線
            m_LiteRenderGraphRecorder.RecordRenderGraph(m_RenderGraph, m_ContextContainer);
            
            m_RenderGraph.EndRecordingAndExecute();
        }
    }
}