using System.Collections;
using System.Reflection;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Wobble.Core;
using Wobble.Gameplay;
using Wobble.CameraSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Wobble.Tests
{
    public class WobblePlayModeTests
    {
        private GameManager game;
        private string savedData;
        private bool hadSave;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            hadSave = PlayerPrefs.HasKey("wobble.save.v1");
            savedData = PlayerPrefs.GetString("wobble.save.v1", "");
            var oldGame = Object.FindFirstObjectByType<GameManager>();
            if (oldGame != null) Object.Destroy(oldGame.gameObject);
            yield return null;
            game = new GameObject("WOBBLE test runtime").AddComponent<GameManager>();
            yield return null;
            yield return new WaitUntil(() => game.GetComponent<Wobble.UI.UIManager>().Localization.Ready);
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (game != null) Object.Destroy(game.gameObject);
            yield return null;
            if (hadSave) PlayerPrefs.SetString("wobble.save.v1", savedData);
            else PlayerPrefs.DeleteKey("wobble.save.v1");
            PlayerPrefs.Save();
        }

        [UnityTest]
        public IEnumerator HomeToPerfectPlacement_CreatesNextMovingBlock()
        {
            yield return null;
            Assert.That(game, Is.Not.Null);
            Assert.That(game.State, Is.EqualTo(GameState.Home));

            typeof(GameManager).GetMethod("StartGame", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(game, null);
            yield return null;
            Assert.That(game.State, Is.EqualTo(GameState.Playing));

            var stackField = typeof(GameManager).GetField("stack", BindingFlags.Instance | BindingFlags.NonPublic);
            var stack = stackField?.GetValue(game) as StackManager;
            Assert.That(stack, Is.Not.Null);
            var moving = stack.ActiveBlock;
            Assert.That(moving, Is.Not.Null);
            var identity = moving.GetInstanceID();
            moving.Advance(12f);
            Assert.That(moving.GetInstanceID(), Is.EqualTo(identity));
            // Presentation and physics pools also own BlockViews; target this run's actual top.
            var previous = stack.Top;
            Assert.That(previous, Is.Not.Null);

            moving.transform.position = previous.transform.position + Vector3.up * StackManager.BlockHeight;
            stack.PlaceCurrent();
            yield return null;

            Assert.That(game.Score, Is.EqualTo(2));
            Assert.That(Object.FindFirstObjectByType<MovingBlock>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator MissShowsLifeContinueAndPreservesRun()
        {
            typeof(GameManager).GetMethod("StartGame", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(game, null);
            var stack = (StackManager)typeof(GameManager).GetField("stack", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game);
            var economy = (Wobble.Economy.EconomyManager)typeof(GameManager).GetField("economy", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game);
            if (economy.Lives == 0) economy.GrantReward(0, 1);
            var startingLives = economy.Lives;
            var moving = stack.ActiveBlock;
            moving.transform.position = stack.Top.transform.position + Vector3.up * StackManager.BlockHeight + Vector3.right * 8f;
            stack.PlaceCurrent();
            Assert.That(game.State, Is.EqualTo(GameState.GameOver));
            yield return new WaitForSeconds(.95f);
            var panel = GameObject.Find("Game Over");
            Assert.That(panel, Is.Not.Null);
            var button = panel.transform.Find("gameover.continue");
            Assert.That(button, Is.Not.Null);
            Assert.That(button.gameObject.activeSelf, Is.True);
            game.ContinueWithLife();
            Assert.That(game.State, Is.EqualTo(GameState.Playing));
            Assert.That(economy.Lives, Is.EqualTo(startingLives - 1));
            Assert.That(stack.ActiveBlock, Is.Not.Null);
            Assert.That(stack.Top, Is.Not.Null);
            Assert.That(game.Score, Is.Zero);
            Assert.That(panel.activeSelf, Is.False);
            game.ContinueWithLife();
            Assert.That(economy.Lives, Is.EqualTo(startingLives - 1));
        }

        [UnityTest]
        public IEnumerator ThirtyPlacements_PreserveDirections_Levels_AndPortraitFraming()
        {
            typeof(GameManager).GetMethod("StartGame", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(game, null);
            var stack = (StackManager)typeof(GameManager).GetField("stack", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game);
            var levels = (LevelManager)typeof(GameManager).GetField("levels", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game);
            var camera = game.GetComponentInChildren<UnityEngine.Camera>();
            var controller = camera.GetComponent<CameraController>();
            var sequence = new[] { MoveDirection.PositiveX, MoveDirection.NegativeZ, MoveDirection.NegativeX, MoveDirection.PositiveZ };
            Assert.That(levels.Speed, Is.EqualTo(4f));
            for (var i = 0; i < 30; i++)
            {
                var moving = stack.ActiveBlock;
                Assert.That(moving.Direction, Is.EqualTo(sequence[i % 4]));
                moving.transform.position = stack.Top.transform.position + Vector3.up * StackManager.BlockHeight;
                stack.PlaceCurrent();
                Assert.That(levels.Level, Is.EqualTo(1 + (i + 1) / 10));
                yield return new WaitForSeconds(.2f);
            }
            Assert.That(game.State, Is.EqualTo(GameState.Playing));
            Assert.That(levels.Speed, Is.InRange(4f, 6.6f));
            foreach (var aspect in new[] { 9f / 16f, 9f / 19.5f, 9f / 21f })
            {
                camera.aspect = aspect;
                for (var level = 1; level <= 5; level++)
                {
                    controller.RotateForLevel(level);
                    for (var frame = 0; frame < 70; frame++)
                    {
                        yield return null;
                        var top = stack.Top.transform.position;
                        var viewport = camera.WorldToViewportPoint(top);
                        Assert.That(viewport.x, Is.InRange(.42f, .58f));
                        Assert.That(viewport.y, Is.InRange(.36f, .56f));
                        AssertInside(camera, top - Vector3.up * StackManager.BlockHeight * 4);
                        foreach (var axis in new[] { Vector3.right, Vector3.left, Vector3.forward, Vector3.back })
                        foreach (var x in new[] { -1.65f, 1.65f })
                        foreach (var z in new[] { -1.65f, 1.65f })
                            AssertInside(camera, top + axis * StackManager.TravelBoundary + new Vector3(x, StackManager.BlockHeight, z));
                    }
                }
            }
        }

        [UnityTest]
        public IEnumerator AllThemeEnvironmentsAndCloserPortraitCamera()
        {
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var themes = (Wobble.Themes.ThemeManager)typeof(GameManager).GetField("themes", flags).GetValue(game);
            var saves = (Wobble.Save.SaveManager)typeof(GameManager).GetField("saves", flags).GetValue(game);
            var stack = (StackManager)typeof(GameManager).GetField("stack", flags).GetValue(game);
            var camera = game.GetComponentInChildren<UnityEngine.Camera>();
            var controller = camera.GetComponent<CameraController>();
            var canvas = game.GetComponentInChildren<Canvas>();
            foreach (var id in new[] { "Carrara", "Obsidian", "AncientStone", "Neon", "Space" })
            {
                var previousSky = themes.Environment.skyTop;
                themes.SelectTheme(id);
                Assert.That(themes.Environment.skyTop, Is.EqualTo(previousSky));
                themes.Tick(1.2f);
                Assert.That(themes.Environment.skyTop, Is.EqualTo(Color.Lerp(previousSky, themes.Current.environment.skyTop, .5f)));
                saves.Data.selectedTheme = id;
                typeof(GameManager).GetMethod("StartGame", flags).Invoke(game, null);
                Assert.That(themes.Current.name, Is.EqualTo(id));
                for (var i = 0; i < 12; i++)
                {
                    stack.ActiveBlock.transform.position = stack.Top.transform.position + Vector3.up * StackManager.BlockHeight;
                    stack.PlaceCurrent();
                    yield return null;
                }
                stack.ActiveBlock.Stop();
                stack.ActiveBlock.transform.position = stack.Top.transform.position + new Vector3(-2, StackManager.BlockHeight, 0);
                camera.aspect = 9f / 19.5f;
                yield return new WaitForSeconds(2.6f);
                Assert.That(themes.Environment.skyTop, Is.EqualTo(themes.Current.environment.skyTop));
                var catalog = Resources.Load<Wobble.Economy.MenuCatalog>("Menu/MenuCatalog");
                var approvedTint = id == "Carrara" ? themes.Current.blockTop : System.Array.Find(catalog.themes, item => item.id == id).previewTint;
                Assert.That(Vector4.Distance(themes.TopMaterial.color, approvedTint), Is.LessThan(.0001f));
                Assert.That(RenderSettings.ambientSkyColor, Is.EqualTo(themes.Current.environment.ambientTint));
                Assert.That(game.GetComponent<Wobble.Themes.CarraraWorld>().GeometryCount, Is.EqualTo(24));
                var oldRadius = Mathf.Sqrt(Mathf.Pow(StackManager.TravelBoundary + 1.65f, 2) + 1.65f * 1.65f);
                var tangent = Mathf.Tan(20 * Mathf.Deg2Rad) * camera.aspect * .91f;
                var oldDistance = oldRadius * Mathf.Sqrt(1 + 1 / (tangent * tangent)) + .3f;
                Assert.That(controller.FollowDistance, Is.LessThan(oldDistance * .95f));
                Debug.Log($"Theme framing {id}: distance {controller.FollowDistance:F2}, previous {oldDistance:F2}, apparent scale gain {oldDistance / controller.FollowDistance:P1}");
                var rt = new RenderTexture(540, 1170, 24) { antiAliasing = 4 };
                camera.targetTexture = rt;
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
                game.GetComponent<Wobble.UI.UIManager>().Feedback(true, 3, stack.Top.transform.position);
                yield return new WaitForSeconds(.15f);
                Canvas.ForceUpdateCanvases();
                RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = rt });
                var previous = RenderTexture.active; RenderTexture.active = rt;
                var image = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); image.Apply();
                var directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Artifacts/ThemeEnvironment"));
                Directory.CreateDirectory(directory); File.WriteAllBytes(Path.Combine(directory, id + ".png"), image.EncodeToPNG());
                RenderTexture.active = previous; camera.targetTexture = null;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                rt.Release(); Object.Destroy(rt); Object.Destroy(image);
            }
        }

        private static void AssertInside(UnityEngine.Camera camera, Vector3 point)
        {
            var viewport = camera.WorldToViewportPoint(point);
            Assert.That(viewport.z, Is.GreaterThan(0));
            Assert.That(viewport.x, Is.InRange(.02f, .98f), "The complete slab must stay inside the screen at both edges.");
            Assert.That(viewport.y, Is.InRange(.20f, .78f), "Movement must stay clear of the HUD and bottom edge.");
        }

        [UnityTest]
        public IEnumerator FidelitySettings_AndCutTextureCoordinates_ArePreserved()
        {
            foreach (var name in new[] { "Low", "Medium", "High" })
            {
                var asset = Resources.Load<UniversalRenderPipelineAsset>("Rendering/" + name);
                Assert.That(asset, Is.Not.Null);
                Assert.That(asset.renderScale, Is.EqualTo(1));
                Assert.That(asset.msaaSampleCount, Is.EqualTo(name == "Low" ? 2 : 4));
                Assert.That(asset.shadowDistance, Is.EqualTo(48));
            }
            var cam = game.GetComponentInChildren<UnityEngine.Camera>();
            Assert.That(cam.fieldOfView, Is.EqualTo(40));
            Assert.That(cam.allowDynamicResolution, Is.False);
            Assert.That(cam.allowMSAA, Is.True);
            Assert.That(cam.allowHDR, Is.True);
            Assert.That(cam.GetUniversalAdditionalCameraData().renderPostProcessing, Is.True);
            var scaler = game.GetComponentInChildren<UnityEngine.UI.CanvasScaler>();
            Assert.That(scaler.referenceResolution, Is.EqualTo(new Vector2(1080,1920)));
            Assert.That(scaler.matchWidthOrHeight, Is.EqualTo(.5f));
            Assert.That(game.GetComponentsInChildren<UnityEngine.UI.Text>(true), Is.Empty);
            Assert.That(game.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true).Length, Is.GreaterThan(10));
            var texture = Resources.Load<Texture2D>("Rendering/CarraraNatural");
            Assert.That(texture.width, Is.GreaterThanOrEqualTo(1024));
            Assert.That(texture.filterMode, Is.EqualTo(FilterMode.Trilinear));
            Assert.That(texture.mipmapCount, Is.GreaterThan(1));
            Assert.That(texture.anisoLevel, Is.EqualTo(8));

            typeof(GameManager).GetMethod("StartGame", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(game, null);
            var stack = (StackManager)typeof(GameManager).GetField("stack", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game);
            for (var i = 0; i < 4; i++)
            {
                var moving = stack.ActiveBlock;
                var view = moving.GetComponent<BlockView>();
                var offset = moving.Axis;
                moving.transform.position = stack.Top.transform.position + Vector3.up * StackManager.BlockHeight + offset * .45f;
                var sourcePosition = moving.transform.position;
                var sourceOrigin = view.StoneOrigin;
                stack.PlaceCurrent();
                Assert.That(Vector3.Distance(view.StoneOrigin, sourceOrigin + view.transform.position - sourcePosition), Is.LessThan(.0001f));
                Assert.That(view.Size.y, Is.EqualTo(.4f), "Approved collider / gameplay height must not change.");
                var properties = new MaterialPropertyBlock();
                view.GetComponentInChildren<Renderer>().GetPropertyBlock(properties);
                Assert.That((Vector3)properties.GetVector("_StoneSize"), Is.EqualTo(view.Size));
                foreach (var body in game.GetComponentsInChildren<Rigidbody>())
                {
                    var piece = body.GetComponent<BlockView>();
                    if (!piece.gameObject.activeSelf) continue;
                    // Check the new offcut before physics advances this frame.
                    if (Mathf.Abs(piece.transform.position.y - view.transform.position.y) > .01f) continue;
                    Assert.That(Vector3.Distance(piece.StoneOrigin, sourceOrigin + piece.transform.position - sourcePosition), Is.LessThan(.0001f));
                    Assert.That(piece.GetComponentInChildren<Renderer>().sharedMaterials, Is.EqualTo(view.GetComponentInChildren<Renderer>().sharedMaterials));
                }
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator RenderHomeGameplayAndResultsForReview()
        {
            yield return Capture("home", 1080, 1920);
            typeof(GameManager).GetMethod("StartGame", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(game, null);
            var stack = (StackManager)typeof(GameManager).GetField("stack", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game);
            for (var i = 0; i < 12; i++)
            {
                stack.ActiveBlock.transform.position = stack.Top.transform.position + Vector3.up * StackManager.BlockHeight;
                stack.PlaceCurrent();
                if (i == 9)
                {
                    yield return new WaitForSeconds(.08f);
                    yield return Capture("perfect-level-transition", 1170, 2532);
                }
                yield return new WaitForSeconds(.18f);
            }
            yield return new WaitForSeconds(.9f);
            stack.ActiveBlock.transform.position = stack.Top.transform.position + new Vector3(-2.5f, StackManager.BlockHeight, 0);
            stack.ActiveBlock.Stop();
            yield return Capture("carrara-1080x1920", 1080, 1920);
            yield return Capture("carrara-1170x2532", 1170, 2532);
            yield return Capture("carrara-1080x2400", 1080, 2400);
            var cutting = stack.ActiveBlock;
            cutting.Begin(cutting.Direction, stack.Top.transform.position, StackManager.TravelBoundary, 0);
            cutting.transform.position = stack.Top.transform.position + Vector3.up * StackManager.BlockHeight + cutting.Axis * .65f;
            stack.PlaceCurrent();
            stack.ActiveBlock.Stop();
            yield return new WaitForSeconds(.07f);
            yield return Capture("cut-piece", 1080, 1920);
            // Restart just this block's motion to place a genuine complete miss.
            var moving = stack.ActiveBlock;
            moving.Begin(moving.Direction, stack.Top.transform.position, StackManager.TravelBoundary, 0);
            stack.PlaceCurrent();
            Assert.That(game.State, Is.EqualTo(GameState.GameOver));
            yield return new WaitForSeconds(.7f);
            yield return Capture("results", 1080, 1920);
        }

        private IEnumerator Capture(string name, int width, int height)
        {
            var camera = game.GetComponentInChildren<UnityEngine.Camera>();
            if (game.State == GameState.Home) camera = game.GetComponent<Wobble.UI.UIManager>().Presentation.Camera;
            var canvas = game.GetComponentInChildren<Canvas>();
            var target = new RenderTexture(width, height, 24) { antiAliasing = 4 };
            camera.targetTexture = target;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
            yield return null;
            if (name == "perfect-level-transition")
            {
                var stack = (StackManager)typeof(GameManager).GetField("stack", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game);
                game.GetComponent<Wobble.UI.UIManager>().Feedback(true, 5, stack.Top.transform.position);
                stack.Top.Punch();
                yield return new WaitForSeconds(.10f);
                var ui = game.GetComponent<Wobble.UI.UIManager>();
                var feedback = (TMPro.TextMeshProUGUI)typeof(Wobble.UI.UIManager).GetField("feedbackText", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ui);
                Assert.That(feedback.canvasRenderer.GetAlpha(), Is.GreaterThan(.1f));
                Assert.That(Mathf.Abs(feedback.rectTransform.anchoredPosition.x), Is.LessThan(20f), "Centered tower feedback must not drift sideways in portrait render targets.");
            }
            Canvas.ForceUpdateCanvases();
            RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
            Assert.That(FidelityRenderProbe.LastCameraId, Is.EqualTo(camera.GetInstanceID()));
            Assert.That(FidelityRenderProbe.LastDescriptor.width, Is.EqualTo(width));
            Assert.That(FidelityRenderProbe.LastDescriptor.height, Is.EqualTo(height));
            Assert.That(FidelityRenderProbe.LastDescriptor.msaaSamples, Is.EqualTo(4), "URP must actually request 4x MSAA.");
            var previousTarget = RenderTexture.active;
            RenderTexture.active = target;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();
            var directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Artifacts/Previews"));
            Directory.CreateDirectory(directory);
            File.WriteAllBytes(Path.Combine(directory, name + ".png"), image.EncodeToPNG());
            RenderTexture.active = previousTarget;
            camera.targetTexture = null;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Object.Destroy(image);
            target.Release();
            Object.Destroy(target);
        }
    }
}
