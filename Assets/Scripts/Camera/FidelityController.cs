using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Wobble.CameraSystem
{
    // Rendering-only settings; never controls simulation, input or progression.
    public sealed class FidelityController : MonoBehaviour
    {
        private void Awake()
        {
            var camera = GetComponent<UnityEngine.Camera>();
            camera.allowHDR = true;
            camera.allowMSAA = true;
            camera.allowDynamicResolution = false;
            var data = camera.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.None;
            data.dithering = false;
            var volume = gameObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = Resources.Load<VolumeProfile>("Rendering/CarraraPost");
        }
    }
}
