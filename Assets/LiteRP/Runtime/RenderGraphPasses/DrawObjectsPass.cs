using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_DrawObjectsProfilingSampler = new ProfilingSampler("Draw Objects");
        private static readonly ShaderTagId s_ShaderTagID = new ShaderTagId("SRPDefaultUnlit");
        
        internal class DrawObjectsPassData
        {
            internal RendererListHandle opaqueRendererListHandle;
            internal RendererListHandle transpatretRendererListHandle;
        }

        private void AddDrawObjectsPass(RenderGraph renderGraph, CameraData cameraData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<DrawObjectsPassData>("Draw Objects Pass",
                       out var passData, s_DrawObjectsProfilingSampler))
            {
                // 聲明創建 或 引用的資源
                
                // 創建不透明對象渲染列表
                RendererListDesc opaqueRendererListDesc =
                    new RendererListDesc(s_ShaderTagID, cameraData.CullingResults, cameraData.camera);
                opaqueRendererListDesc.sortingCriteria = SortingCriteria.CommonOpaque;
                opaqueRendererListDesc.renderQueueRange = RenderQueueRange.opaque;
                passData.opaqueRendererListHandle = renderGraph.CreateRendererList(opaqueRendererListDesc);
                
                // RenderGraph 引用不透明渲染列表
                builder.UseRendererList(passData.opaqueRendererListHandle);
                
                // 創建半透明對象渲染列表
                RendererListDesc transparentRendererListDesc =
                    new RendererListDesc(s_ShaderTagID, cameraData.CullingResults, cameraData.camera);
                transparentRendererListDesc.sortingCriteria = SortingCriteria.CommonTransparent;
                transparentRendererListDesc.renderQueueRange = RenderQueueRange.transparent;
                passData.transpatretRendererListHandle = renderGraph.CreateRendererList(transparentRendererListDesc);
                
                // RenderGraph 引用半透明渲染列表
                builder.UseRendererList(passData.transpatretRendererListHandle);
                
                builder.SetRenderAttachment(m_BackbufferColorHandle, 0, AccessFlags.Write);
                
                // 設置渲染全局狀態
                builder.AllowPassCulling(false);
                
                builder.SetRenderFunc((DrawObjectsPassData passData, RasterGraphContext context) =>
                {
                    // 調用渲染指令繪製
                    context.cmd.DrawRendererList(passData.opaqueRendererListHandle);
                    context.cmd.DrawRendererList(passData.transpatretRendererListHandle);
                });
            }
        }
    }
}