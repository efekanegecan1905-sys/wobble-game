using UnityEngine;

namespace Wobble.Core
{
    public static class WobbleBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Object.FindFirstObjectByType<GameManager>() != null) return;
            var root = new GameObject("WOBBLE Runtime");
            root.AddComponent<GameManager>();
        }
    }
}
