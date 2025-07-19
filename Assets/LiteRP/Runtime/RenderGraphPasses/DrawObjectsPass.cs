using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_DrawObjectsProfilingSampler = new ProfilingSampler("Draw Objects");
        
        internal class DrawObjectsPassData
        {
            
        }

        private void AddDrawObjectsPass(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<DrawObjectsPassData>("Draw Objects Pass",
                       out var passData, s_DrawObjectsProfilingSampler))
            {
                // 聲明創建 或 引用的資源
                // 設置渲染全局狀態
                builder.SetRenderFunc((DrawObjectsPassData passData, RasterGraphContext context) =>
                {
                    // 調用渲染指令繪製
                });
            }
        }
    }
}