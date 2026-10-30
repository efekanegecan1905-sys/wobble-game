using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Wobble.Audio;
using Wobble.CameraSystem;
using Wobble.Economy;
using Wobble.Gameplay;
using Wobble.Save;
using Wobble.Themes;
using Wobble.UI;

namespace Wobble.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        public GameState State { get; private set; } = GameState.Home;
        public int Score { get; private set; }

        private SaveManager saves;
        private EconomyManager economy;
        private ThemeManager themes;
        private AudioManager audio;
        private HapticManager haptics;
        private StackManager stack;
        private ComboManager combo;
        private ComboEnergy comboEnergy;
        private LevelManager levels;
        private CameraController cameraController;
        private UIManager ui;
        private Light keyLight;
        private CarraraWorld world;
        private Material plinthMaterial;
        private Coroutine gameOverRoutine;
        private int inputReadyFrame;
        private int awardedGems;
        private int runStartBest;
        private bool newBest, feverTensionShown;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            BuildCamera();
            // A dedicated demo build carries this Resources marker; the regular app does not.
            var demo = Resources.Load<TextAsset>("Demo/AllThemes") != null;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            demo |= System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "--wobble-theme-demo") >= 0;
#endif
            saves = new SaveManager(demo ? new DemoSaveStore() : null);
            if (demo)
            {
                saves.Data.ownedSkins = new System.Collections.Generic.List<string> { "Carrara", "Obsidian", "AncientStone", "Neon", "Space" };
                saves.Data.selectedTheme = "Carrara";
            }
            QualitySettings.SetQualityLevel(saves.Data.graphicsQuality, true);
            economy = new EconomyManager(saves, demo);
            themes = new ThemeManager();
            themes.SelectTheme(saves.Data.selectedTheme, true);
            audio = new AudioManager(gameObject, themes.Current);
            audio.SetEnabled(saves.Data.audioEnabled);
            audio.SetMusicEnabled(saves.Data.musicEnabled);
            haptics = new HapticManager(saves.Data.hapticsEnabled);
            combo = new ComboManager();
            comboEnergy = new ComboEnergy();
            levels = new LevelManager();
            BuildWorld();

            stack = new StackManager(transform, themes, audio, haptics, this);
            stack.Placed += OnPlaced;
            stack.Missed += OnMissed;
            stack.SetAccents(themes.Accent, themes.AccentSecondary);
            levels.LevelUp += OnLevelUp;

            ui = gameObject.AddComponent<UIManager>();
            ui.Build(saves, economy, audio, haptics, themes);
            ui.PlayPressed += StartGame;
            ui.RetryPressed += StartGame;
            ui.HomePressed += GoHome;
            ui.ContinuePressed += ContinueWithLife;
        }

        private void BuildCamera()
        {
            var go = new GameObject("Main Camera", typeof(UnityEngine.Camera), typeof(AudioListener), typeof(CameraController));
            go.tag = "MainCamera";
            go.transform.SetParent(transform);
            go.transform.position = new Vector3(6.6f, 6.1f, 6.6f);
            cameraController = go.GetComponent<CameraController>();
            cameraController.Initialize();
            go.AddComponent<FidelityController>();
        }

        private void BuildWorld()
        {
            var keyGo = new GameObject("Soft key light", typeof(Light));
            keyGo.transform.SetParent(transform);
            keyGo.transform.rotation = Quaternion.Euler(46f, -32f, 0);
            keyLight = keyGo.GetComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.15f;
            keyLight.color = new Color(1f, .97f, .91f);
            keyLight.shadows = LightShadows.Soft;
            keyLight.shadowStrength = .55f;
            keyLight.shadowBias = .35f;
            keyLight.shadowNormalBias = .3f;

            var rimGo = new GameObject("Soft neutral fill", typeof(Light));
            rimGo.transform.SetParent(transform);
            rimGo.transform.rotation = Quaternion.Euler(35f, 145f, 0);
            var rim = rimGo.GetComponent<Light>();
            rim.type = LightType.Directional; rim.intensity = .12f; rim.color = new Color(.97f, .97f, .96f);
            rim.shadows = LightShadows.None;

            // Built from the shared kit instead of GameObject.CreatePrimitive: IL2CPP
            // strips collider types the player never references, so CreatePrimitive
            // logs a missing-component error for every call on device.
            var floor = new GameObject("Architectural plinth", typeof(MeshFilter), typeof(MeshRenderer));
            floor.transform.SetParent(transform);
            floor.transform.position = new Vector3(0, -.43f, 0);
            floor.transform.localScale = new Vector3(4.5f, .28f, 4.5f);
            floor.GetComponent<MeshFilter>().sharedMesh = ThemeWorldKit.CylinderMesh;
            // A Resources reference survives player shader stripping.
            var shader = Resources.Load<Shader>("Shaders/Carrara");
            plinthMaterial = new Material(shader) { color = WobblePalette.StoneMid };
            plinthMaterial.SetFloat("_Glossiness", .16f);
            floor.GetComponent<Renderer>().sharedMaterial = plinthMaterial;
            world = gameObject.AddComponent<CarraraWorld>();
            world.Build(GetComponentInChildren<UnityEngine.Camera>(),keyLight);
            world.SetThemes(themes);
        }

        private void Update()
        {
            themes.Tick(Time.deltaTime);
            // One smooth presentation value drives every FLOW/FEVER reaction. It is
            // read after the tick so audio, camera and world share the same frame.
            comboEnergy.Tick(Time.deltaTime);
            stack.SetComboEnergy(comboEnergy.Intensity, comboEnergy.Tier);
            themes.SetCombo(comboEnergy.Intensity, comboEnergy.Tier);
            world.SetCombo(comboEnergy.Intensity, comboEnergy.Tier);
            stack.TickEffects(Time.deltaTime);
            keyLight.color = themes.Environment.keyLightTint;
            plinthMaterial.color = Color.Lerp(themes.Visuals.propDark, themes.Visuals.propLight, .45f);
            if (stack.Top != null) world.Follow(stack.Top.transform.position.y);
            if (State != GameState.Playing || !stack.CanPlace) return;
            if (Time.frameCount <= inputReadyFrame) return;
            var tapped = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
            if (tapped && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;
            if (!Application.isMobilePlatform) tapped |= Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
            if (Input.GetMouseButtonDown(0) && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            if (tapped) stack.PlaceCurrent();
        }

        private void StartGame()
        {
            if (gameOverRoutine != null) StopCoroutine(gameOverRoutine);
            inputReadyFrame = Time.frameCount;
            State = GameState.Playing;
            Score = 0;
            awardedGems = 0;
            runStartBest = saves.Data.bestScore;
            newBest = false;
            feverTensionShown = false;
            comboEnergy.Reset();
            themes.SetCombo(0f, 0);
            world.SetCombo(0f, 0);
            combo.ResetRun();
            audio.ResetPlacementCombo();
            levels.ResetRun();
            themes.SelectTheme(saves.Data.selectedTheme, true);
            themes.ApplyEnvironment(1, true);
            stack.SetAccents(themes.Accent, themes.AccentSecondary);
            world.Show(true);
            cameraController.RotateForLevel(1);
            cameraController.FollowHeight(0);
            stack.StartRun(levels.Speed);
            ui.ShowGame(saves.Data.bestScore);
        }

        private void OnPlaced(bool perfect, Vector3 position)
        {
            Score += combo.Register(perfect);
            var multiplier = combo.DisplayMultiplier;
            var escalated = comboEnergy.Push(perfect, multiplier);
            audio.Impact(perfect, multiplier);
            saves.SubmitScore(Score);
            newBest = Score > runStartBest;
            cameraController.FollowTop(position);
            if (perfect) cameraController.PerfectImpulse();
            cameraController.ComboImpulse(multiplier);
            if (escalated > 0)
            {
                // x5 FLOW and x8 FEVER each get one clean hero beat: a camera push,
                // a world pulse and a held milestone label. No extra shake stacks.
                cameraController.FlowPulse(escalated >= 2 ? 1f : .62f);
                themes.Pulse(escalated);
                world.Pulse(escalated);
                ui.ComboMilestone(escalated);
            }
            else if (!perfect) { ui.ComboRelaxed(); ui.TenseCleared(); }
            // WOBBLE begins on the first x8 Perfect and is resolved by the next
            // Perfect. It is presentation tension only: no reward and no corruption.
            if (comboEnergy.Tier >= 2 && !feverTensionShown)
            {
                feverTensionShown = true;
                stack.BeginTension();
                ui.TenseBegin();
            }
            if (stack.StabilizedThisPlacement) ui.TenseStabilized();
            if (perfect) world.Celebrate(multiplier);
            else world.Contact();
            stack.Celebrate(perfect, multiplier);
            ui.Feedback(perfect, multiplier, position);
            levels.RegisterPlacement();
            ui.UpdateHud(Score, saves.Data.bestScore, levels.Level);
            stack.SpawnNext(levels.Speed);
        }

        private void OnLevelUp(int level)
        {
            themes.ApplyEnvironment(level);
            world.SetLevel(level);
            cameraController.RotateForLevel(level);
            audio.LevelUp();
            ui.LevelUp(level);
        }

        private void OnMissed()
        {
            State = GameState.GameOver;
            // Death always re-arms the placement bank at Wobble_Normal.
            audio.ResetPlacementCombo();
            comboEnergy.Reset();
            feverTensionShown = false;
            themes.SetCombo(0f, 0);
            world.SetCombo(0f, 0);
            ui.ComboRelaxed();
            stack.FailBurst();
            world.Fail();
            cameraController.FailImpulse();
            ui.FailFlash();
            saves.SubmitScore(Score);
            gameOverRoutine = StartCoroutine(ShowResultsAfterFall());
        }

        private IEnumerator ShowResultsAfterFall()
        {
            // 0.72 s of falling stone and camera settle before the card appears:
            // long enough to read the loss, short enough to tap again immediately.
            yield return new WaitForSeconds(.72f);
            // Additive run reward: spends no existing currency and changes no price or life rule.
            var earned = Mathf.Clamp(Score / 10, 0, 99);
            var grant = Mathf.Max(0, earned - awardedGems);
            if (grant > 0) economy.GrantReward(grant, 0);
            awardedGems = earned;
            ui.ShowGameOver(Score, saves.Data.bestScore, combo.PerfectCount, combo.MaximumCombo, stack.BlockCount, earned, newBest);
            gameOverRoutine = null;
        }

        private void GoHome()
        {
            State = GameState.Home;
            comboEnergy.Reset();
            feverTensionShown = false;
            themes.SetCombo(0f, 0);
            world.SetCombo(0f, 0);
            world.Show(false);
            themes.ApplyEnvironment(1,true);
            stack.ClearTower();
            cameraController.FollowHeight(0);
            ui.ShowHome(saves.Data.bestScore);
        }

        public void ContinueWithLife()
        {
            if (State != GameState.GameOver || gameOverRoutine != null || !economy.ConsumeLifeForContinue()) return;
            combo.BreakChain();
            comboEnergy.Reset();
            feverTensionShown = false;
            audio.ResetPlacementCombo();
            inputReadyFrame = Time.frameCount;
            State = GameState.Playing;
            stack.ResumeAfterMiss(levels.Speed);
            ui.ShowContinuedGame(Score, saves.Data.bestScore, levels.Level);
        }

        private void OnDestroy()
        {
            themes?.Dispose();
            audio?.Dispose();
            stack?.Dispose();
            Destroy(plinthMaterial);
        }

        public void ContinueWithRewardedAd() => economy.ContinueWithRewardedAd();
    }
}
