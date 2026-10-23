using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Wobble.CameraSystem
{
    // Observes URP's actual camera descriptor without adding a rendering pass.
    public sealed class FidelityRenderProbe : ScriptableRendererFeature
    {
        public static RenderTextureDescriptor LastDescriptor { get; private set; }
        public static int LastCameraId { get; private set; }
        public override void Create() { }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.camera.cameraType != CameraType.Game) return;
            LastDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            LastCameraId = renderingData.cameraData.camera.GetInstanceID();
        }
    }
}
