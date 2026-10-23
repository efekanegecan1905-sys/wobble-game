#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Wobble.Editor
{
    // Run only in an isolated build copy; never reconfigure scenes or art assets.
    public static class IOSDeviceBuilder
    {
        public static void Build()
        {
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS,BuildTarget.iOS))
                throw new System.Exception("Unity cannot locate installed iOS Build Support.");
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS))
                throw new System.Exception("Could not switch the active build target to iOS.");
            AddressableAssetSettings.BuildPlayerContent(out var contentResult);
            if (!string.IsNullOrEmpty(contentResult.Error))
                throw new System.Exception("iOS localization bundles failed: " + contentResult.Error);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS,ScriptingImplementation.IL2CPP);
            PlayerSettings.iOS.sdkVersion=iOSSdkVersion.DeviceSDK;
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{"Assets/Scenes/Wobble.unity"},
                locationPathName="/private/tmp/wobble-ios-device/Xcode",
                target=BuildTarget.iOS,options=BuildOptions.Development
            });
            if(result.summary.result!=BuildResult.Succeeded)throw new System.Exception("iOS export failed: "+result.summary.result);
            Debug.Log("WOBBLE iOS Xcode export ready: /private/tmp/wobble-ios-device/Xcode");
        }
    }
}
#endif
