#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Wobble.Editor
{
    // Run only in an isolated build copy; never reconfigure scenes or art assets.
    public static class IOSDeviceBuilder
    {
        public static void Build()
        {
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS,BuildTarget.iOS))
                throw new System.Exception("Unity cannot locate installed iOS Build Support.");
            // Platform imports must finish before executeMethod starts; switching here can
            // leave cached resource/shader references unresolved in a batch build.
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
                throw new System.Exception("Start this build with -buildTarget iOS so platform imports finish first.");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (var name in Wobble.Audio.AudioManager.PlacementClipNames)
                if (Resources.Load<AudioClip>("Audio/Placement/" + name) == null)
                    throw new System.Exception("iOS build is missing required placement audio: " + name);
            var shaders=GraphicsSettings.GetRenderPipelineSettings<UniversalRenderPipelineRuntimeShaders>();
            if(shaders==null || shaders.coreBlitPS==null || shaders.coreBlitColorAndDepthPS==null)
                throw new System.Exception("iOS build has unresolved URP Blitter shaders; rebuild the isolated import cache before exporting.");
            AddressableAssetSettings.BuildPlayerContent(out var contentResult);
            if (!string.IsNullOrEmpty(contentResult.Error))
                throw new System.Exception("iOS localization bundles failed: " + contentResult.Error);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS,ScriptingImplementation.IL2CPP);
            PlayerSettings.iOS.sdkVersion=iOSSdkVersion.DeviceSDK;
            // This isolated device build targets the minimum supported by current Xcode.
            PlayerSettings.iOS.targetOSVersionString="15.0";
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{"Assets/Scenes/Wobble.unity"},
                locationPathName="/private/tmp/wobble-ios-device/Xcode",
                target=BuildTarget.iOS,options=BuildOptions.Development | BuildOptions.CleanBuildCache
            });
            if(result.summary.result!=BuildResult.Succeeded)throw new System.Exception("iOS export failed: "+result.summary.result);
            Debug.Log("WOBBLE iOS Xcode export ready: /private/tmp/wobble-ios-device/Xcode");
        }
    }
}
#endif
