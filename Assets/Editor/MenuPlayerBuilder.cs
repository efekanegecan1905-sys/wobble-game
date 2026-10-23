#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Wobble.Editor
{
    public static class MenuPlayerBuilder
    {
        public static void BuildReview()
        {
            MenuAssetBuilder.Configure();
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Wobble.unity" },
                locationPathName = "/private/tmp/wobble-menu-player/WOBBLE.app",
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.Development
            });
            if (result.summary.result != BuildResult.Succeeded)
                throw new System.Exception("Menu review player failed: " + result.summary.result);
        }
    }
}
#endif
