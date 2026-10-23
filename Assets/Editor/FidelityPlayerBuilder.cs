#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Wobble.Editor
{
    public static class FidelityPlayerBuilder
    {
        public static void BuildReview()
        {
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Wobble.unity" },
                locationPathName = "/private/tmp/wobble-fidelity-player/WOBBLE.app",
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.Development
            });
            if (result.summary.result != BuildResult.Succeeded)
                throw new System.Exception("Review player failed: " + result.summary.result);
            Debug.Log("WOBBLE standalone review player built.");
        }
    }
}
#endif
