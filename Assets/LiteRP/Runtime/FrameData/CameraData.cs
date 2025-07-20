using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.FrameData
{
    public class CameraData : ContextItem
    {
        public Camera camera;
        public CullingResults CullingResults;

        public override void Reset()
        {
            camera = null;
            CullingResults = default;
        }
    }
}