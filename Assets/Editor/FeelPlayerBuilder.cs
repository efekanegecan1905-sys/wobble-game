#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Wobble.Editor
{
    public static class FeelPlayerBuilder
    {
        public static void BuildReview()
        {
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes=new[]{"Assets/Scenes/Wobble.unity"},
                locationPathName="/private/tmp/wobble-feel-player/WOBBLE.app",
                target=BuildTarget.StandaloneOSX,
                options=BuildOptions.Development
            });
            if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Feel review build failed.");
        }
    }
}
#endif
