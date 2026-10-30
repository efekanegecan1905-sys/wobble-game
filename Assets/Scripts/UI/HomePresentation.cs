using System.Collections.Generic;
using UnityEngine;
using Wobble.Gameplay;
using Wobble.Themes;
using Wobble.CameraSystem;

namespace Wobble.UI
{
    // The animated title stage: a floating destination pedestal, a wobbling hero
    // stack, layered islands and the destination landmark. Built from the same
    // code-native kit as gameplay, so the menu and the run share one art direction.
    public sealed class HomePresentation : MonoBehaviour
    {
        private sealed class FloatItem
        {
            public Transform root;
            public Vector3 home;
            public float phase;
            public float bob;
            public float spin;
        }

        private Transform stage, sculpture, logo, themeLayer;
        private readonly Transform[] letters = new Transform[6];
        private readonly Transform[] introFragments = new Transform[8];
        private readonly Renderer[] letterSurfaces = new Renderer[6];
        private readonly List<FloatItem> floats = new();
        private ParticleSystem signatureDust;
        private Material dustMaterial;
        private MaterialPropertyBlock highlight;
        private Camera homeCamera, gameplayCamera;
        private Material stoneMaterial, darkMaterial, foundationMaterial, foliageMaterial, crownMaterial;
        private Material trimMaterial, accentMaterial;
        private ThemeManager themesRef;
        private ThemeId builtTheme = (ThemeId)(-1);
        private Renderer cloudSea, heroGem;
        private Transform heroCrownRing;
        private Vector3 heroGemScale;
        private Texture2D cloudTexture;
        private Material cloudMaterial;
        private Material[] skyMaterials;
        private float section, sectionTarget, entry, clock;
        public Camera Camera => homeCamera;
        public float IntroProgress { get; private set; } = 1;

        public void Build(ThemeManager themes, Camera gameplayCamera)
        {
            highlight = new MaterialPropertyBlock();
            this.gameplayCamera = gameplayCamera;
            themesRef = themes;
            stage = new GameObject("Carrara gallery").transform;
            stage.SetParent(transform, false);
            stage.position = new Vector3(1000, 0, 1000);

            BuildMaterials();
            BuildHeroStack();
            BuildClouds();
            BuildLogo();
            BuildCamera();
            BuildKeyLight();

            BuildThemeLayer(true);
            foreach (var child in stage.GetComponentsInChildren<Transform>()) child.gameObject.layer = 30;
            SetIntro(1);
        }

        // --- Construction ---------------------------------------------------------

        private void BuildMaterials()
        {
            var stone = Resources.Load<Shader>("Shaders/ArchitecturalStone");
            var spark = Resources.Load<Shader>("Shaders/Spark");
            var grain = Resources.Load<Texture2D>("Rendering/CarraraNatural");

            stoneMaterial = new Material(stone) { name = "Home destination stone", enableInstancing = true };
            darkMaterial = new Material(stone) { name = "Home shadow stone", enableInstancing = true };
            foundationMaterial = new Material(stone) { name = "Home pedestal", enableInstancing = true };
            foliageMaterial = new Material(stone) { name = "Home gardens", enableInstancing = true };
            crownMaterial = new Material(stone) { name = "Home gold", enableInstancing = true };
            trimMaterial = new Material(spark) { name = "Home rim light", enableInstancing = true };
            accentMaterial = new Material(spark) { name = "Home accents", enableInstancing = true };
            stoneMaterial.mainTexture = darkMaterial.mainTexture = grain;
            foundationMaterial.mainTexture = foliageMaterial.mainTexture = crownMaterial.mainTexture = grain;
            skyMaterials = new[] { stoneMaterial, darkMaterial, foundationMaterial, foliageMaterial, crownMaterial };
        }

        private void BuildHeroStack()
        {
            sculpture = new GameObject("Wobbling hero stack").transform;
            sculpture.SetParent(stage, false);

            // A tall, slightly drunken stack. Ten slabs read instantly as the game at
            // menu scale; offsets stay small so the silhouette remains one object.
            var widths = new[] { 2.18f, 2.18f, 2.16f, 2.13f, 2.11f, 2.08f, 2.04f, 1.99f, 1.93f, 1.85f };
            var offsets = new[] { 0f, .055f, -.045f, .075f, -.06f, .045f, -.035f, .055f, -.02f, .04f };
            var depths = new[] { 0f, -.035f, .04f, -.03f, .045f, -.02f, .03f, -.04f, .02f, -.025f };
            var yaws = new[] { 0f, .8f, -1.1f, 1.4f, -1.2f, .9f, -1.6f, 1.2f, -.7f, 1f };
            for (var i = 0; i < widths.Length; i++)
            {
                var go = new GameObject("Hero slab " + i);
                go.transform.SetParent(sculpture, false);
                go.transform.localPosition = new Vector3(offsets[i], .2f + i * StackManager.BlockHeight, depths[i]);
                go.transform.localRotation = Quaternion.Euler(0, yaws[i], 0);
                var view = go.AddComponent<BlockView>();
                view.Initialize(new Vector3(widths[i], StackManager.BlockHeight, widths[i]), themesRef.BlockMaterial, themesRef.TopMaterial);
                var collider = go.GetComponent<Collider>();
                if (collider != null) collider.enabled = false;
                foreach (var renderer in go.GetComponentsInChildren<MeshRenderer>())
                {
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }
            }

            // Gold crown: a thin cap, a floating crystal and a slowly turning halo.
            // Gold inlay rather than a solid cap: a cream marble border stays visible,
            // which reads as craftsmanship instead of a recoloured roof.
            var crown = AddPart(ThemeWorldKit.BoxMesh, crownMaterial, sculpture, new Vector3(.02f, 4.045f, -.01f),
                new Vector3(1.52f, .05f, 1.52f), Quaternion.Euler(0, 3, 0));
            crown.name = "Hero crown";
            heroGem = AddPart(ThemeWorldKit.CrystalMesh, accentMaterial, sculpture, new Vector3(.02f, 4.52f, -.01f),
                Vector3.one * .34f, Quaternion.Euler(0, 24, 8));
            heroGem.name = "Hero crystal";
            heroGemScale = heroGem.transform.localScale;
            var ring = AddPart(ThemeWorldKit.TrimMesh, trimMaterial, sculpture, new Vector3(.02f, 4.40f, -.01f),
                new Vector3(.55f, 1f, .55f), Quaternion.identity);
            ring.name = "Hero halo";
            heroCrownRing = ring.transform;
        }

        private void BuildClouds()
        {
            cloudTexture = CarraraWorld.CreateCloudTexture(128);
            cloudMaterial = new Material(Resources.Load<Shader>("Shaders/CloudSea")) { name = "Home cloud sea" };
            cloudMaterial.SetTexture("_MainTex", cloudTexture);
            cloudMaterial.SetFloat("_Opacity", .42f);
            cloudMaterial.SetFloat("_Tiling", 4.6f);
            var sea = new GameObject("Home cloud sea", typeof(MeshFilter), typeof(MeshRenderer));
            sea.GetComponent<MeshFilter>().sharedMesh = ThemeWorldKit.QuadMesh;
            sea.transform.SetParent(stage, false);
            sea.transform.localPosition = new Vector3(0, -4.6f, 12f);
            sea.transform.localRotation = Quaternion.Euler(90, 0, 0);
            sea.transform.localScale = new Vector3(220, 220, 1);
            cloudSea = sea.GetComponent<Renderer>();
            cloudSea.sharedMaterial = cloudMaterial;
            cloudSea.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            cloudSea.receiveShadows = false;
        }

        private void BuildLogo()
        {
            logo = new GameObject("WOBBLE stone identity").transform;
            logo.SetParent(stage, false);
            for (var i = 0; i < 6; i++)
            {
                var go = new GameObject("WOBBLE letter " + i);
                go.transform.SetParent(logo, false);
                letters[i] = go.transform;
                go.AddComponent<MeshFilter>().sharedMesh = Resources.Load<Mesh>("Presentation/Letter" + i);
                letterSurfaces[i] = go.AddComponent<MeshRenderer>();
                letterSurfaces[i].sharedMaterial = themesRef.TopMaterial;
            }
            for (var i = 0; i < introFragments.Length; i++)
            {
                var go = new GameObject("Intro marble fragment " + i);
                go.transform.SetParent(logo, false);
                introFragments[i] = go.transform;
                go.AddComponent<MeshFilter>().sharedMesh = Resources.Load<Mesh>("Presentation/Monolith");
                var renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = themesRef.TopMaterial;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            var dustObject = new GameObject("Identity stone dust");
            dustObject.transform.SetParent(stage, false);
            signatureDust = dustObject.AddComponent<ParticleSystem>();
            signatureDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = signatureDust.main;
            main.playOnAwake = false; main.loop = false; main.startLifetime = 1.1f; main.startSpeed = .12f;
            main.startSize = .018f; main.startColor = new Color(.94f, .86f, .72f, .20f); main.maxParticles = 32;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = signatureDust.emission; emission.enabled = false;
            var shape = signatureDust.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(4, .3f, .2f);
            dustMaterial = new Material(Resources.Load<Shader>("Shaders/StoneDust"));
            dustObject.GetComponent<ParticleSystemRenderer>().sharedMaterial = dustMaterial;
        }

        private void BuildCamera()
        {
            var cameraObject = new GameObject("Home Camera");
            cameraObject.transform.SetParent(stage.transform, false);
            homeCamera = cameraObject.AddComponent<Camera>();
            homeCamera.cullingMask = (1 << 30) | (1 << 5) | 1;
            homeCamera.clearFlags = CameraClearFlags.Skybox;
            homeCamera.fieldOfView = 40;
            homeCamera.nearClipPlane = .1f;
            homeCamera.farClipPlane = 120;
            cameraObject.AddComponent<FidelityController>();
        }

        // Menus face the camera; the world sun sits behind that axis in most themes,
        // which would leave the palace fronts in shade. One shadowless directional
        // light restricted to the presentation layer fixes that without touching the
        // gameplay world or its light budget.
        private void BuildKeyLight()
        {
            var go = new GameObject("Home key light");
            go.transform.SetParent(stage, false);
            go.transform.rotation = Quaternion.Euler(34, -24, 0);
            var key = go.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(1f, .97f, .90f);
            key.intensity = 1.05f;
            key.shadows = LightShadows.None;
            key.cullingMask = 1 << 30;
            key.renderMode = LightRenderMode.ForceVertex;
        }

        // The destination layer is rebuilt only when the selected world changes. All
        // meshes are cached in ThemeWorldKit, so this is a handful of GameObjects, not
        // a texture or model load.
        private void BuildThemeLayer(bool force)
        {
            var id = themesRef.Current.id;
            if (!force && id == builtTheme) return;
            builtTheme = id;
            if (themeLayer != null)
            {
                themeLayer.gameObject.SetActive(false);
                Destroy(themeLayer.gameObject);
            }
            floats.Clear();
            themeLayer = new GameObject("Home destination — " + id).transform;
            themeLayer.SetParent(stage, false);

            // Compose in camera-aligned space so the framing survives the opening yaw.
            var basis = Quaternion.Euler(0, 14f, 0);
            var right = basis * Vector3.right;
            var forward = basis * Vector3.forward;
            Vector3 P(float x, float y, float z) => right * x + forward * z + Vector3.up * y;

            AddStructure(ThemeWorldKit.Foundation(id), themeLayer, new Vector3(0, -.22f, 0), .34f,
                foundationMaterial, trimMaterial, foliageMaterial, Quaternion.identity);

            AddIsland(id, 0, P(-4.9f, -.20f, 18.5f), .84f, 24f);
            AddIsland(id, 1, P(5.0f, .35f, 19.5f), .76f, -32f);
            AddIsland(id, 2, P(-6.0f, 2.55f, 32.0f), .52f, 48f);
            AddIsland(id, 3, P(6.2f, 3.00f, 34.0f), .48f, -18f);

            AddMonument(Landmark(id), P(4.4f, 1.10f, 24.5f), .18f, -24f);
            AddMonument(Secondary(id), P(-4.7f, 1.30f, 25.5f), .14f, 22f);
        }

        private void AddIsland(ThemeId id, int variant, Vector3 position, float scale, float yaw)
        {
            var structure = id switch
            {
                ThemeId.Obsidian => ThemeWorldKit.ObsidianIsland(variant),
                ThemeId.AncientStone => ThemeWorldKit.SandstoneGarden(variant),
                ThemeId.Neon => ThemeWorldKit.NeonIsland(variant),
                ThemeId.Space => ThemeWorldKit.SpaceIsland(variant),
                _ => ThemeWorldKit.PalaceGarden(variant)
            };
            var root = new GameObject("Floating island").transform;
            root.SetParent(themeLayer, false);
            root.localPosition = position;
            root.localRotation = Quaternion.Euler(0, yaw, 0);
            root.localScale = Vector3.one * scale;
            // Palace and sandstone islands carry gardens in their second channel;
            // basalt, neon and crystal islands carry glow there instead.
            var dressing = id == ThemeId.Carrara || id == ThemeId.AncientStone ? foliageMaterial : accentMaterial;
            AddStructure(structure, root, Vector3.zero, 1f, stoneMaterial, dressing, darkMaterial, Quaternion.identity);
            floats.Add(new FloatItem { root = root, home = position, phase = variant * 1.37f + 1.1f, bob = .10f + (variant % 2) * .05f, spin = variant % 2 == 0 ? 1.6f : -1.2f });
        }

        private void AddMonument(ThemeWorldKit.Structure structure, Vector3 position, float scale, float yaw)
        {
            var root = new GameObject("Floating landmark").transform;
            root.SetParent(themeLayer, false);
            root.localPosition = position;
            root.localRotation = Quaternion.Euler(0, yaw, 0);
            AddPart(ThemeWorldKit.CylinderMesh, stoneMaterial, root, new Vector3(0, -.18f, 0), new Vector3(2.3f, .38f, 2.3f), Quaternion.identity);
            AddPart(ThemeWorldKit.ConeMesh, stoneMaterial, root, new Vector3(0, -1.35f, 0), new Vector3(2.0f, 2.3f, 2.0f), Quaternion.Euler(180, 0, 0));
            AddPart(ThemeWorldKit.BoxMesh, stoneMaterial, root, new Vector3(.55f, -1.0f, -.35f), new Vector3(.9f, .8f, .8f), Quaternion.Euler(6, 22, 9));
            AddStructure(structure, root, new Vector3(0, .02f, 0), scale, stoneMaterial, accentMaterial, darkMaterial, Quaternion.Euler(0, -12f, 0));
            floats.Add(new FloatItem { root = root, home = position, phase = yaw * .07f + 2.4f, bob = .13f, spin = -.8f });
        }

        private void AddShard(float x, float y, float z, float scale, float yaw)
        {
            var root = new GameObject("Foreground shard").transform;
            root.SetParent(themeLayer, false);
            var position = new Vector3(x, y, z);
            root.localPosition = position;
            root.localScale = Vector3.one * scale;
            root.localRotation = Quaternion.Euler(0, yaw, 0);
            AddPart(ThemeWorldKit.SphereMesh, stoneMaterial, root, Vector3.zero, new Vector3(1.7f, 1.1f, 1.5f), Quaternion.Euler(8, 30, 5));
            AddPart(ThemeWorldKit.ConeMesh, stoneMaterial, root, new Vector3(0, -.9f, 0), new Vector3(1.3f, 1.8f, 1.2f), Quaternion.Euler(180, 8, 0));
            floats.Add(new FloatItem { root = root, home = position, phase = yaw * .11f + 4.1f, bob = .07f, spin = 2.2f });
        }

        private ThemeWorldKit.Structure Landmark(ThemeId id) => id switch
        {
            ThemeId.Obsidian => ThemeWorldKit.FortressKeep(),
            ThemeId.AncientStone => ThemeWorldKit.Ziggurat(),
            ThemeId.Neon => ThemeWorldKit.NeonTowerA(),
            ThemeId.Space => ThemeWorldKit.SpaceMonolith(),
            _ => ThemeWorldKit.PalaceTower()
        };

        private ThemeWorldKit.Structure Secondary(ThemeId id) => id switch
        {
            ThemeId.Obsidian => ThemeWorldKit.BasaltSpire(),
            ThemeId.AncientStone => ThemeWorldKit.Obelisk(),
            ThemeId.Neon => ThemeWorldKit.NeonBillboard(),
            ThemeId.Space => ThemeWorldKit.CrystalCluster(),
            _ => ThemeWorldKit.PalaceArch()
        };

        private void AddStructure(ThemeWorldKit.Structure structure, Transform parent, Vector3 position, float scale,
            Material stone, Material accent, Material dark, Quaternion rotation)
        {
            if (structure.Stone != null) AddPart(structure.Stone, stone, parent, position, Vector3.one * scale, rotation);
            if (structure.Accent != null) AddPart(structure.Accent, accent, parent, position, Vector3.one * scale, rotation);
            if (structure.Dark != null) AddPart(structure.Dark, dark != null ? dark : stone, parent, position, Vector3.one * scale, rotation);
        }

        private static Renderer AddPart(Mesh mesh, Material material, Transform parent, Vector3 position, Vector3 scale, Quaternion rotation)
        {
            var go = new GameObject(mesh != null ? mesh.name : "Home part");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation;
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return renderer;
        }

        // --- Public animation contract -------------------------------------------

        public void SetSection(MenuScreen screen) => sectionTarget = screen == MenuScreen.Shop ? 1 : screen == MenuScreen.Themes ? -1 : screen == MenuScreen.Daily ? .5f : screen == MenuScreen.Settings ? .2f : 0;
        public void SetEntry(float progress) => entry = progress;

        public void SignatureDust()
        {
            signatureDust.transform.position = logo.position;
            signatureDust.Emit(12);
        }

        public void AssemblyDust(int index)
        {
            signatureDust.transform.position = index < 0 ? logo.position : letters[index].position;
            signatureDust.Emit(index < 0 ? 6 : 2);
        }

        public void SetIntro(float progress)
        {
            IntroProgress = Mathf.Clamp01(progress);
            logo.gameObject.SetActive(progress < 1);
            var seconds = IntroProgress * 5.8f;
            sculpture.localScale = Vector3.one * (.96f * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(4.65f, 5.3f, seconds)));
            if (progress >= 1) signatureDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void Show(bool show)
        {
            stage.gameObject.SetActive(show);
            gameplayCamera.enabled = !show;
            if (show) entry = 0;
        }

        private void LateUpdate()
        {
            if (stage == null || !stage.gameObject.activeSelf) return;
            clock += Time.unscaledDeltaTime;

            if (themesRef != null && themesRef.Current.id != builtTheme) BuildThemeLayer(false);

            var sky = RenderSettings.skybox;
            if (sky != null)
            {
                if (skyMaterials != null)
                    foreach (var material in skyMaterials) PushSky(material, sky);
                cloudMaterial.SetColor("_Color", themesRef.Visuals.cloudSeaTop);
                cloudMaterial.SetColor("_DeepColor", themesRef.Visuals.cloudSeaDeep);
                cloudMaterial.SetColor("_Accent", themesRef.Visuals.accent);
                cloudMaterial.SetVector("_SunDir", themesRef.Visuals.sunDirection);
            }
            cloudMaterial.mainTextureOffset = new Vector2(clock * .004f, clock * .002f);

            if (themesRef != null)
            {
                var v = themesRef.Visuals;
                // Cooler destination stone separates the floating structures from the
                // warm marble tower; the darker channel gives cliffs real underside depth.
                stoneMaterial.color = Color.Lerp(v.ruinColor, v.propLight, .58f);
                darkMaterial.color = Color.Lerp(v.propDark, v.ruinColor, .18f);
                foliageMaterial.color = v.foliage;
                foundationMaterial.color = builtTheme switch
                {
                    ThemeId.Obsidian => new Color(.19f, .15f, .18f),
                    ThemeId.AncientStone => new Color(.72f, .53f, .33f),
                    ThemeId.Neon => new Color(.16f, .08f, .28f),
                    ThemeId.Space => new Color(.20f, .19f, .48f),
                    _ => new Color(.90f, .83f, .72f)
                };
                crownMaterial.color = Color.Lerp(v.accent, Color.white, .18f);
                accentMaterial.SetColor("_Color", v.accent);
                accentMaterial.SetFloat("_Intensity", .55f);
                trimMaterial.SetColor("_Color", v.edgeColor);
                trimMaterial.SetFloat("_Intensity", .34f);
            }

            if (heroGem != null)
            {
                var breath = .5f + .5f * Mathf.Sin(clock * 1.7f);
                heroGem.transform.localScale = heroGemScale * (1f + breath * .10f);
                heroGem.transform.localRotation = Quaternion.Euler(8, clock * 26f, 12);
            }
            if (heroCrownRing != null) heroCrownRing.localRotation = Quaternion.Euler(0, clock * 18f, 0);

            foreach (var item in floats)
            {
                if (item.root == null) continue;
                var bob = Mathf.Sin(clock * .34f + item.phase) * item.bob;
                item.root.localPosition = item.home + Vector3.up * bob;
                item.root.localRotation = Quaternion.Euler(0, item.phase * 16f + Mathf.Sin(clock * .12f + item.phase) * 2.6f, 0);
            }

            section = Mathf.Lerp(section, sectionTarget, 1 - Mathf.Exp(-Time.unscaledDeltaTime * 5));
            sculpture.localPosition = new Vector3(0, Mathf.Sin(clock * .5f) * .035f, 0);
            sculpture.localRotation = Quaternion.Euler(0, Mathf.Sin(clock * .21f) * 1.8f + section * 6f, 0);

            var rotation = Quaternion.Euler(20 + Mathf.Sin(clock * .16f) * .5f, 14 + section * 4f, 0);
            var introSeconds = IntroProgress * 5.8f;
            var hero = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(3f, 4f, introSeconds));
            var exit = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(4.8f, 5.8f, introSeconds));
            if (IntroProgress < 1) rotation *= Quaternion.Euler(-2 * (1 - exit), Mathf.Lerp(-5, 3, hero) * (1 - exit), 0);
            var distance = Mathf.Max(16.2f, 7.6f / Mathf.Max(.4f, homeCamera.aspect));
            var focus = stage.transform.position + new Vector3(section * .38f, 1.16f, 0);
            homeCamera.transform.SetPositionAndRotation(focus - rotation * Vector3.forward * (distance - entry * 2), rotation);

            if (IntroProgress < 1)
            {
                logo.position = focus;
                logo.rotation = rotation;
                logo.position += rotation * new Vector3(0, exit * 7, -exit * 3);
                logo.localScale = Vector3.one * Mathf.Lerp(.74f, .84f, hero);
                for (var i = 0; i < 6; i++)
                {
                    var gather = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.6f, 1.8f, introSeconds));
                    var assemble = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(1.8f + i * .15f, 2.15f + i * .15f, introSeconds));
                    var spread = (1 - assemble) * Mathf.Lerp(5, 1.5f, gather);
                    letters[i].localPosition = new Vector3((i - 2.5f) * 1.02f, (i % 2 == 0 ? 1 : -1) * spread, -(1 - assemble) * Mathf.Lerp(7, 1.8f + i * .2f, gather));
                    letters[i].localRotation = Quaternion.Euler((1 - assemble) * 35, 0, (1 - assemble) * (i % 2 == 0 ? 14 : -14));
                    var sweep = Mathf.Exp(-Mathf.Pow((introSeconds - 3.12f) * 8 - i * 1.05f, 2));
                    var warmth = Color.Lerp(new Color(.70f, .74f, .77f), new Color(.88f, .83f, .74f), hero);
                    highlight.SetColor("_Color", Color.Lerp(warmth, new Color(1, .95f, .83f), sweep));
                    letterSurfaces[i].SetPropertyBlock(highlight);
                }
                var fragmentsIn = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.6f, 1.8f, introSeconds));
                var fragmentsOut = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(2.4f, 3.1f, introSeconds));
                for (var i = 0; i < introFragments.Length; i++)
                {
                    var angle = i * Mathf.PI * .25f;
                    var radius = Mathf.Lerp(8, 3.5f, fragmentsIn);
                    introFragments[i].localPosition = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius * .65f, Mathf.Lerp(-8 - i * .3f, 1 + i % 3, fragmentsIn));
                    introFragments[i].localRotation = Quaternion.Euler(i * 23, introSeconds * 12 + i * 31, i * 17);
                    introFragments[i].localScale = new Vector3(.22f, .12f, .18f) * fragmentsIn * fragmentsOut;
                }
            }
            else
            {
                // Occasional signature sheen: one soft highlight crosses the six
                // letters every few seconds. Reuses the intro property block, so it
                // costs no extra material and stays below the eye's notice.
                var cycle = Mathf.Repeat(clock, 7.4f);
                for (var i = 0; i < 6; i++)
                {
                    var phase = Mathf.Exp(-Mathf.Pow((cycle - 4.7f) * 6.2f - i * 1.1f, 2));
                    highlight.SetColor("_Color", Color.Lerp(new Color(.97f, .95f, .92f), new Color(1f, .94f, .80f), phase * .55f));
                    letterSurfaces[i].SetPropertyBlock(highlight);
                }
            }
        }

        private static void PushSky(Material material, Material sky)
        {
            if (material == null) return;
            material.SetColor("_SkyTop", sky.GetColor("_Top"));
            material.SetColor("_SkyHorizon", sky.GetColor("_Horizon"));
            material.SetColor("_SkyBottom", sky.GetColor("_Bottom"));
            material.SetColor("_Accent", sky.GetColor("_Accent"));
            material.SetFloat("_AccentStrength", sky.GetFloat("_AccentStrength"));
            material.SetColor("_SunColor", sky.GetColor("_SunColor"));
            material.SetVector("_SunDir", sky.GetVector("_SunDir"));
            material.SetFloat("_SunIntensity", sky.GetFloat("_SunIntensity"));
            material.SetColor("_CloudTint", sky.GetColor("_CloudTint"));
            material.SetFloat("_CloudStrength", sky.GetFloat("_CloudStrength"));
            material.SetFloat("_Stars", sky.GetFloat("_Stars"));
            material.SetFloat("_FogNear", 28);
            material.SetFloat("_FogFar", 95);
            material.SetFloat("_Visibility", 1);
        }

        private void OnDestroy()
        {
            if (gameplayCamera != null) gameplayCamera.enabled = true;
            Destroy(stoneMaterial); Destroy(darkMaterial); Destroy(foundationMaterial);
            Destroy(foliageMaterial); Destroy(crownMaterial); Destroy(trimMaterial); Destroy(accentMaterial);
            Destroy(dustMaterial); Destroy(cloudMaterial); Destroy(cloudTexture);
        }
    }
}
