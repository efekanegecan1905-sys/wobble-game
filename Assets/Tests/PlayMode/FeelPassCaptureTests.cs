using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
using Wobble.Core;
using Wobble.Gameplay;
using Wobble.Save;
using Wobble.Themes;
using Wobble.UI;

namespace Wobble.Tests
{
    // Visual proof for the premium-feel pass: FLOW at x5, FEVER + WOBBLE at x8,
    // the stabilization beat, and a new-best result card. Captures are written to
    // Artifacts/Feel/Pass and are also checked for clipped or missing text.
    public sealed class FeelPassCaptureTests
    {
        private GameManager game;
        private UIManager ui;
        private StackManager stack;
        private string saved;
        private bool hadSave;
        private static readonly Vector2Int[] Sizes = { new(1080, 1920), new(1170, 2532), new(1080, 2400) };

        private T Field<T>(string name) =>
            (T)typeof(GameManager).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game);

        [UnitySetUp]
        public IEnumerator Setup()
        {
            hadSave = PlayerPrefs.HasKey("wobble.save.v1");
            saved = PlayerPrefs.GetString("wobble.save.v1");
            var old = Object.FindFirstObjectByType<GameManager>();
            if (old != null) Object.Destroy(old.gameObject);
            yield return null;
            game = new GameObject("Feel pass capture").AddComponent<GameManager>();
            ui = game.GetComponent<UIManager>();
            yield return new WaitUntil(() => ui.Localization.Ready);
            ui.SkipIntro();
            // Keep the record low enough that this run genuinely beats it; set it
            // before StartGame so runStartBest captures it.
            Field<SaveManager>("saves").Data.bestScore = 12;
            typeof(GameManager).GetMethod("StartGame", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(game, null);
            stack = Field<StackManager>("stack");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            Object.Destroy(game.gameObject);
            yield return null;
            if (hadSave) PlayerPrefs.SetString("wobble.save.v1", saved);
            else PlayerPrefs.DeleteKey("wobble.save.v1");
            PlayerPrefs.Save();
        }

        [UnityTest]
        public IEnumerator PremiumFeelMomentsAreReadableAndColliderSafe()
        {
            var combo = Field<ComboManager>("combo");
            var energy = Field<ComboEnergy>("comboEnergy");

            for (var i = 0; i < 4; i++) yield return PlacePerfect();
            Assert.That(combo.DisplayMultiplier, Is.EqualTo(5));
            Assert.That(energy.Tier, Is.EqualTo(1), "x5 must enter FLOW.");
            yield return new WaitForSeconds(.22f);
            yield return Capture("flow-x5");
            Assert.That(MilestoneAlpha(), Is.GreaterThan(.15f), "FLOW badge must be visible.");

            for (var i = 0; i < 3; i++) yield return PlacePerfect();
            Assert.That(combo.DisplayMultiplier, Is.EqualTo(8));
            Assert.That(energy.Tier, Is.EqualTo(2), "x8 must enter FEVER.");
            Assert.That(stack.IsTense, Is.True, "FEVER entry must start WOBBLE tension.");
            Assert.That(stack.Top.transform.localScale, Is.EqualTo(stack.Top.Size), "WOBBLE must not touch the gameplay footprint.");
            Assert.That(stack.Top.GetComponent<BoxCollider>().size, Is.EqualTo(Vector3.one));
            yield return new WaitForSeconds(.26f);
            yield return Capture("fever-wobble-x8");
            Assert.That(TensionAlpha(), Is.GreaterThan(.15f), "WOBBLE cue must be visible.");

            // The next Perfect resolves tension; it is presentation only, so the
            // score still gains exactly one placement's worth.
            var before = game.Score;
            yield return PlacePerfect();
            Assert.That(stack.IsTense, Is.False, "A Perfect during WOBBLE must stabilize.");
            Assert.That(game.Score - before, Is.EqualTo(6), "Stabilizing must only add the normal 8th-perfect award.");
            Assert.That(stack.Top.GetComponent<BoxCollider>().size, Is.EqualTo(Vector3.one));
            yield return new WaitForSeconds(.16f);
            yield return Capture("wobble-stabilized");

            // Force a miss, then wait for the full 0.72 s fall plus the card reveal.
            stack.ActiveBlock.transform.position = stack.Top.transform.position + Vector3.right * 30f;
            stack.PlaceCurrent();
            Assert.That(game.State, Is.EqualTo(GameState.GameOver));
            yield return new WaitForSeconds(1.42f);
            Assert.That(Field<bool>("newBest"), Is.True, "The run must beat the stored record.");
            var plate = (UnityEngine.UI.Image)typeof(UIManager).GetField("newBestPlate", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ui);
            Assert.That(plate.gameObject.activeSelf, Is.True, "New-best plate must be shown.");
            foreach (var size in Sizes) yield return Capture("result-newbest", size);
        }

        [UnityTest]
        public IEnumerator ThemeFeverReactionsStayReadable()
        {
            // Each destination gets a fresh run and its own x8 FEVER beat, so the
            // capture shows the organic sky/rim/district reaction, not a stylised
            // overlay.
            var themes = Field<ThemeManager>("themes");
            foreach (var id in new[] { "Obsidian", "AncientStone", "Neon", "Space" })
            {
                typeof(GameManager).GetMethod("StartGame", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(game, null);
                themes.SelectTheme(id, true);
                themes.ApplyEnvironment(1, true);
                stack.SetAccents(themes.Accent, themes.AccentSecondary);
                yield return new WaitForSeconds(.12f);
                for (var i = 0; i < 7; i++) yield return PlacePerfect();
                Assert.That(Field<ComboEnergy>("comboEnergy").Tier, Is.EqualTo(2), id + " must reach FEVER.");
                Assert.That(stack.IsTense, Is.True, id + " must enter WOBBLE.");
                yield return new WaitForSeconds(.24f);
                yield return Capture("fever-" + id, new Vector2Int(1080, 1920));
                var visuals = themes.Visuals;
                Assert.That(visuals.accent, Is.Not.EqualTo(default(Color)), id + " must expose an accent.");
                Assert.That(stack.Top.GetComponentsInChildren<Renderer>().Length, Is.GreaterThan(0));
            }
        }

        private IEnumerator PlacePerfect()
        {
            stack.ActiveBlock.transform.position = stack.Top.transform.position + Vector3.up * StackManager.BlockHeight;
            stack.PlaceCurrent();
            yield return new WaitForSeconds(.14f);
        }

        private float MilestoneAlpha()
        {
            var label = (TextMeshProUGUI)typeof(UIManager).GetField("comboBanner", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ui);
            return label.canvasRenderer.GetAlpha();
        }

        private float TensionAlpha()
        {
            var label = (TextMeshProUGUI)typeof(UIManager).GetField("tensionLabel", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ui);
            return label.canvasRenderer.GetAlpha();
        }

        private IEnumerator Capture(string label, Vector2Int? only = null)
        {
            var camera = game.GetComponentsInChildren<UnityEngine.Camera>().First(c => c.enabled);
            var canvas = game.GetComponentInChildren<Canvas>();
            foreach (var size in only.HasValue ? new[] { only.Value } : Sizes)
            {
                var target = new RenderTexture(size.x, size.y, 24) { antiAliasing = 4 };
                camera.targetTexture = target;
                camera.aspect = size.x / (float)size.y;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1f;
                yield return null;
                Canvas.ForceUpdateCanvases();
                RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                foreach (var text in canvas.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    text.ForceMeshUpdate();
                    Assert.That(text.isTextOverflowing, Is.False, label + ": clipped text " + text.text);
                    Assert.That(text.text, Does.Not.Contain("No translation found"));
                }
                var previous = RenderTexture.active;
                RenderTexture.active = target;
                var image = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
                image.Apply();
                var directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Artifacts/Feel/Pass"));
                Directory.CreateDirectory(directory);
                File.WriteAllBytes(Path.Combine(directory, label + "-" + size.x + "x" + size.y + ".png"), image.EncodeToPNG());
                RenderTexture.active = previous;
                camera.targetTexture = null;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                Object.Destroy(image);
                target.Release();
                Object.Destroy(target);
            }
        }
    }
}
