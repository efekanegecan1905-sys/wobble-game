#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Wobble.Core;
using Wobble.Gameplay;

namespace Wobble.CameraSystem
{
    // Opt-in development-player verification only. Absent from release builds.
    public sealed class FidelityPlayerReview : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--wobble-visual-check") >= 0)
            {
                Application.runInBackground = true;
                new GameObject("Development visual review").AddComponent<FidelityPlayerReview>();
            }
        }

        private IEnumerator Start()
        {
            var directory = "/private/tmp/wobble-fidelity-player/review";
            Directory.CreateDirectory(directory);
            var hadSave = PlayerPrefs.HasKey("wobble.save.v1");
            var saved = PlayerPrefs.GetString("wobble.save.v1", "");
            yield return null;
            var game = FindFirstObjectByType<GameManager>();
            typeof(GameManager).GetMethod("StartGame", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(game, null);
            var stack = (StackManager)typeof(GameManager).GetField("stack", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(game);
            for (var i = 0; i < 12; i++)
            {
                stack.ActiveBlock.transform.position = stack.Top.transform.position + Vector3.up * StackManager.BlockHeight;
                stack.PlaceCurrent();
                yield return new WaitForSeconds(.2f);
            }
            stack.ActiveBlock.Stop();
            stack.ActiveBlock.transform.position = stack.Top.transform.position + new Vector3(-2.5f, StackManager.BlockHeight, 0);
            var camera = game.GetComponentInChildren<UnityEngine.Camera>();
            var canvas = game.GetComponentInChildren<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1;
            var report = new List<string> { SystemInfo.deviceModel, SystemInfo.graphicsDeviceName, "Development player; desktop measurements are NOT mobile FPS claims." };
            Application.targetFrameRate = -1;
            foreach (var size in new[] { new Vector2Int(1080,1920), new Vector2Int(1170,2532), new Vector2Int(1080,2400) })
            {
                var target = new RenderTexture(size.x, size.y, 24) { antiAliasing = 4 };
                camera.targetTexture = target;
                camera.aspect = size.x / (float)size.y;
                for (var frame = 0; frame < 40; frame++) yield return null;
                var samples = new List<float>();
                for (var frame = 0; frame < 120; frame++) { yield return null; samples.Add(Time.unscaledDeltaTime * 1000); }
                samples.Sort();
                var sum = 0f;
                foreach (var sample in samples) sum += sample;
                Canvas.ForceUpdateCanvases();
                RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                var descriptor = FidelityRenderProbe.LastDescriptor;
                var line = $"{size.x}x{size.y}: actual={descriptor.width}x{descriptor.height}, MSAA={descriptor.msaaSamples}, mean={sum / samples.Count:F2}ms, p95={samples[(int)(samples.Count*.95f)]:F2}ms";
                report.Add(line);
                Debug.Log(line);
                if (descriptor.width != size.x || descriptor.height != size.y || descriptor.msaaSamples != 4)
                    report.Add("FAIL: native resolution / MSAA mismatch");
                var previous = RenderTexture.active;
                RenderTexture.active = target;
                var image = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0,0,size.x,size.y),0,0);
                image.Apply();
                File.WriteAllBytes(Path.Combine(directory, $"player-{size.x}x{size.y}.png"), image.EncodeToPNG());
                RenderTexture.active = previous;
                camera.targetTexture = null;
                target.Release();
                Destroy(target);
                Destroy(image);
            }
            File.WriteAllLines(Path.Combine(directory, "render-verification.txt"), report);
            if (hadSave) PlayerPrefs.SetString("wobble.save.v1", saved); else PlayerPrefs.DeleteKey("wobble.save.v1");
            PlayerPrefs.Save();
            Application.Quit();
        }
    }
}
#endif
