using UnityEngine;
using UnityEngine.Rendering;

namespace Wobble.Themes
{
    // The destination skyline around the tower: four hero landmarks, a ring of
    // midground architecture, floating islands, drifting debris and one celestial
    // accent. Built from the shared ThemeWorldKit primitives so five destinations
    // read as five different places rather than one palette swap.
    // Presentation only: no colliders, no lights, nothing enters the tower corridor.
    public sealed class ThemeDistrict
    {
        private sealed class Prop
        {
            public Transform root;
            public Renderer stone, accent, dark;
            public MeshFilter stoneFilter, accentFilter, darkFilter;
            public Vector3 origin;
            public float phase, bob, sway, spin, shadowRadius;
            public float visibility = 1f;
            public readonly MaterialPropertyBlock stoneProps = new();
            public readonly MaterialPropertyBlock accentProps = new();
            public readonly MaterialPropertyBlock darkProps = new();
        }

        private const int LandmarkSlots = 4;
        private const int MidgroundSlots = 12;
        private const int IslandSlots = 8;
        private const int DebrisSlots = 5;

        private readonly Prop[] landmarks = new Prop[LandmarkSlots];
        private readonly Prop[] midground = new Prop[MidgroundSlots];
        private readonly Prop[] islands = new Prop[IslandSlots];
        private readonly Prop[] debris = new Prop[DebrisSlots];

        private readonly Transform root;
        private readonly Camera camera;
        private readonly Material stoneMaterial, darkMaterial, accentMaterial, foliageMaterial, orbMaterial, ringMaterial;
        private Material foundationMaterial, trimMaterial;
        private Color foundationTint = Color.white;
        private Transform orbRoot;
        private Renderer orb, ringA, ringB;
        private MeshFilter orbFilter;
        private Transform foundationRoot;
        private Renderer foundationStone, foundationAccent, foundationFoliage;
        private MeshFilter foundationStoneFilter, foundationAccentFilter, foundationFoliageFilter;
        private ThemeId theme = (ThemeId)(-1);
        private ThemeVisuals visuals;
        private Color orbColor = Color.white;
        private Color ringColor = Color.white;
        private float accentIntensity = .35f;
        private float orbScale = 14f;
        private float orbSpin;
        private float azimuth;
        private bool visible;

        public ThemeDistrict(Transform parent, Camera gameplayCamera)
        {
            camera = gameplayCamera;
            root = new GameObject("Destination skyline — presentation only").transform;
            root.SetParent(parent, false);

            var shader = Resources.Load<Shader>("Shaders/ArchitecturalStone");
            var spark = Resources.Load<Shader>("Shaders/Spark");
            var grain = Resources.Load<Texture2D>("Rendering/CarraraNatural");
            stoneMaterial = new Material(shader) { name = "District stone", enableInstancing = true };
            stoneMaterial.mainTexture = grain;
            darkMaterial = new Material(shader) { name = "District shadow stone", enableInstancing = true };
            darkMaterial.mainTexture = grain;
            foliageMaterial = new Material(shader) { name = "District gardens", enableInstancing = true };
            accentMaterial = new Material(spark) { name = "District glow", enableInstancing = true };
            orbMaterial = new Material(spark) { name = "District orb", enableInstancing = true };
            ringMaterial = new Material(spark) { name = "District rings", enableInstancing = true };

            foundationMaterial = new Material(shader) { name = "Foundation stone", enableInstancing = true };
            foundationMaterial.mainTexture = grain;
            trimMaterial = new Material(spark) { name = "Foundation trim", enableInstancing = true };

            BuildProps();
            BuildOrb();
            BuildFoundation();
            visuals = ThemeVisuals.Carrara;
            root.gameObject.SetActive(false);
        }

        // The tower's heroic pedestal. Always visible: it sits below the play plane.
        private void BuildFoundation()
        {
            foundationRoot = new GameObject("Hero foundation").transform;
            foundationRoot.SetParent(root, false);
            foundationRoot.localPosition = new Vector3(0, -.22f, 0);
            // The plinth is authored at island scale, so gameplay brings it down to a
            // little over twice the tower footprint. Big enough to read as a monument,
            // small enough that the tower stays the hero of the frame.
            foundationRoot.localScale = Vector3.one * .46f;
            foundationStone = AddRenderer(foundationRoot, "Foundation stone", foundationMaterial, out foundationStoneFilter);
            foundationAccent = AddRenderer(foundationRoot, "Foundation trim", trimMaterial, out foundationAccentFilter);
            foundationFoliage = AddRenderer(foundationRoot, "Foundation garden", foliageMaterial, out foundationFoliageFilter);
            foundationStone.enabled = foundationAccent.enabled = foundationFoliage.enabled = true;
        }

        private void BuildProps()
        {
            // Compose relative to the opening camera azimuth so the skyline frames the
            // tower from the first frame. The camera then orbits, which gives the
            // background its own slow parallax.
            var forward = camera.transform.forward;
            azimuth = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;

            var landmarkOffsets = new[] { -96f, -54f, 54f, 96f };
            for (var i = 0; i < landmarks.Length; i++)
            {
                var radius = 60f + (i % 2) * 7f;
                landmarks[i] = CreateProp("Landmark " + i, root, Polar(azimuth + landmarkOffsets[i], radius, -14f + (i % 2) * 3f),
                    1.25f + (i % 2) * .32f, i * 1.37f, .16f, .05f);
                landmarks[i].shadowRadius = 10f;
            }

            // Keep the centre channel clear: the tower and the incoming block own it,
            // so the midground lives at the edges of the portrait frame.
            var midgroundOffsets = new[] { -168f, -140f, -112f, -82f, -56f, -34f, 34f, 56f, 82f, 112f, 140f, 168f };
            for (var i = 0; i < midground.Length; i++)
            {
                var radius = 32f + (i % 4) * 6f;
                midground[i] = CreateProp("Midground " + i, root, Polar(azimuth + midgroundOffsets[i], radius, -9f + (i % 3) * 2f),
                    .56f + (i % 3) * .12f, i * .93f + 2f, .22f, .07f);
                midground[i].shadowRadius = 6f;
            }

            var islandOffsets = new[] { -154f, -116f, -78f, -44f, 44f, 78f, 116f, 154f };
            for (var i = 0; i < islands.Length; i++)
            {
                var radius = 30f + (i % 3) * 5.5f;
                islands[i] = CreateProp("Floating island " + i, root, Polar(azimuth + islandOffsets[i], radius, -1.5f + (i % 4) * 1.5f),
                    1.05f + (i % 3) * .24f, i * 1.21f + 4f, .30f, .10f);
                islands[i].shadowRadius = 3.4f;
            }

            var debrisOffsets = new[] { -168f, -140f, -92f, -56f, 56f };
            for (var i = 0; i < debris.Length; i++)
            {
                var radius = 23f + (i % 2) * 8f;
                debris[i] = CreateProp("Floating debris " + i, root, Polar(azimuth + debrisOffsets[i], radius, 5f + (i % 3) * 2.2f),
                    .22f + (i % 2) * .06f, i * 1.9f, .32f, .18f);
                debris[i].spin = 4f + i * 1.7f;
                debris[i].shadowRadius = 2.2f;
            }
        }

        private static Vector3 Polar(float degrees, float radius, float y)
        {
            var radians = degrees * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(radians) * radius, y, Mathf.Cos(radians) * radius);
        }

        private Prop CreateProp(string name, Transform parent, Vector3 position, float scale, float phase, float bob, float sway)
        {
            var go = new GameObject(name).transform;
            go.SetParent(parent, false);
            go.localPosition = position;
            go.localScale = Vector3.one * scale;
            go.localRotation = Quaternion.Euler(0, Mathf.Atan2(position.x, position.z) * Mathf.Rad2Deg, 0);
            return new Prop
            {
                root = go,
                stone = AddRenderer(go, "Stone", stoneMaterial, out var stoneFilter),
                stoneFilter = stoneFilter,
                accent = AddRenderer(go, "Accent", accentMaterial, out var accentFilter),
                accentFilter = accentFilter,
                dark = AddRenderer(go, "Shadow", darkMaterial, out var darkFilter),
                darkFilter = darkFilter,
                origin = position, phase = phase, bob = bob, sway = sway
            };
        }

        private static Renderer AddRenderer(Transform parent, string name, Material material, out MeshFilter filter)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            filter = go.AddComponent<MeshFilter>();
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.enabled = false;
            return renderer;
        }

        private void BuildOrb()
        {
            orbRoot = new GameObject("Celestial accent").transform;
            orbRoot.SetParent(root, false);
            var angle = 232f * Mathf.Deg2Rad;
            orbRoot.localPosition = new Vector3(Mathf.Cos(angle) * 92f, 2f, Mathf.Sin(angle) * 92f);
            orb = AddRenderer(orbRoot, "Orb", orbMaterial, out orbFilter);
            orb.enabled = true;
            ringA = AddRenderer(orbRoot, "Ring A", ringMaterial, out var filterA);
            filterA.sharedMesh = ThemeWorldKit.RingArcA;
            ringA.enabled = true;
            ringB = AddRenderer(orbRoot, "Ring B", ringMaterial, out var filterB);
            filterB.sharedMesh = ThemeWorldKit.RingArcB;
            ringB.enabled = true;
        }

        public void SetVisible(bool value)
        {
            visible = value;
            if (root != null) root.gameObject.SetActive(value);
        }

        public void SetTheme(ThemeId id)
        {
            if (theme == id) return;
            theme = id;
            visuals = ThemeVisuals.Get(id);
            Assign(landmarks, LandmarkRecipe(id));
            Assign(midground, MidgroundRecipe(id));
            Assign(islands, IslandRecipe(id));
            Assign(debris, DebrisRecipe(id));

            var gardenIslands = id == ThemeId.Carrara || id == ThemeId.AncientStone;
            foreach (var island in islands)
                island.accent.sharedMaterial = gardenIslands ? foliageMaterial : accentMaterial;

            var foundation = ThemeWorldKit.Foundation(id);
            foundationStoneFilter.sharedMesh = foundation.Stone;
            foundationAccentFilter.sharedMesh = foundation.Accent;
            foundationFoliageFilter.sharedMesh = foundation.Dark;
            foundationAccent.sharedMaterial = accentMaterial;
            foundationFoliage.sharedMaterial = foliageMaterial;

            accentIntensity = id switch
            {
                ThemeId.Neon => .92f,
                ThemeId.Space => .78f,
                ThemeId.Obsidian => .58f,
                ThemeId.AncientStone => .30f,
                _ => .26f
            };
            // The pedestal is a solid object, so it gets its own albedo instead of the
            // atmospheric stone tint: dark worlds need a dark plinth, sand worlds a
            // plinth that separates from the sand sky.
            foundationTint = id switch
            {
                ThemeId.Obsidian => new Color(.20f, .16f, .19f),
                ThemeId.AncientStone => new Color(.70f, .52f, .33f),
                ThemeId.Neon => new Color(.15f, .08f, .26f),
                ThemeId.Space => new Color(.20f, .19f, .47f),
                _ => new Color(.86f, .79f, .68f)
            };
            var v = visuals;
            orbColor = id switch
            {
                ThemeId.Obsidian => Color.Lerp(v.accent, new Color(1f, .42f, .18f), .5f),
                ThemeId.Neon => Color.Lerp(v.accentSecondary, v.accent, .35f),
                ThemeId.Space => Color.Lerp(v.accent, v.accentSecondary, .45f),
                ThemeId.AncientStone => Color.Lerp(v.sunColor, v.accent, .35f),
                _ => v.sunColor
            };
            ringColor = Color.Lerp(v.accent, v.edgeColor, .4f);
            orbFilter.sharedMesh = id == ThemeId.Space ? ThemeWorldKit.SpacePlanet().Stone : ThemeWorldKit.SphereMesh;
            orbScale = id == ThemeId.Space ? 2.4f : 15f;
            orb.transform.localScale = Vector3.one * orbScale;
            var ringRadius = orbScale * .5f * 1.45f;
            ringA.transform.localScale = new Vector3(ringRadius, 1f, ringRadius);
            ringB.transform.localScale = new Vector3(ringRadius * .92f, 1f, ringRadius * .92f);
            var rings = id == ThemeId.Space || id == ThemeId.Neon;
            ringA.enabled = rings;
            ringB.enabled = rings;
        }

        private static void Assign(Prop[] props, ThemeWorldKit.Structure[] recipe)
        {
            for (var i = 0; i < props.Length; i++)
            {
                var p = props[i];
                var structure = recipe[i % recipe.Length];
                p.stoneFilter.sharedMesh = structure.Stone;
                p.accentFilter.sharedMesh = structure.Accent;
                p.darkFilter.sharedMesh = structure.Dark;
                p.stone.enabled = structure.Stone != null;
                p.accent.enabled = structure.Accent != null;
                p.dark.enabled = structure.Dark != null;
            }
        }

        private static ThemeWorldKit.Structure[] LandmarkRecipe(ThemeId id)
        {
            switch (id)
            {
                case ThemeId.Obsidian:
                    return new[] { ThemeWorldKit.FortressKeep(), ThemeWorldKit.BasaltSpire(), ThemeWorldKit.FortressWall(), ThemeWorldKit.BasaltSpire() };
                case ThemeId.AncientStone:
                    return new[] { ThemeWorldKit.Ziggurat(), ThemeWorldKit.Obelisk(), ThemeWorldKit.SandstoneArch(), ThemeWorldKit.BrokenColonnade() };
                case ThemeId.Neon:
                    return new[] { ThemeWorldKit.NeonTowerA(), ThemeWorldKit.NeonBillboard(), ThemeWorldKit.NeonTowerB(), ThemeWorldKit.NeonArch() };
                case ThemeId.Space:
                    return new[] { ThemeWorldKit.SpaceMonolith(), ThemeWorldKit.CrystalCluster(), ThemeWorldKit.SpaceMonolith(), ThemeWorldKit.Asteroid() };
                default:
                    return new[] { ThemeWorldKit.PalacePortico(), ThemeWorldKit.PalaceTower(), ThemeWorldKit.PalaceArch(), ThemeWorldKit.PalaceTower() };
            }
        }

        private static ThemeWorldKit.Structure[] MidgroundRecipe(ThemeId id)
        {
            switch (id)
            {
                case ThemeId.Obsidian:
                    return new[] { ThemeWorldKit.FortressWall(), ThemeWorldKit.BasaltSpire(), ThemeWorldKit.FortressWall(), ThemeWorldKit.FortressKeep() };
                case ThemeId.AncientStone:
                    return new[] { ThemeWorldKit.BrokenColonnade(), ThemeWorldKit.Obelisk(), ThemeWorldKit.Ziggurat(), ThemeWorldKit.Obelisk() };
                case ThemeId.Neon:
                    return new[] { ThemeWorldKit.NeonTowerB(), ThemeWorldKit.NeonTowerA(), ThemeWorldKit.NeonTowerB(), ThemeWorldKit.NeonTowerA() };
                case ThemeId.Space:
                    return new[] { ThemeWorldKit.Satellite(), ThemeWorldKit.CrystalCluster(), ThemeWorldKit.SpaceMonolith(), ThemeWorldKit.Asteroid() };
                default:
                    return new[] { ThemeWorldKit.PalaceTower(), ThemeWorldKit.PalacePortico(), ThemeWorldKit.PalaceTower(), ThemeWorldKit.PalacePortico() };
            }
        }

        private static ThemeWorldKit.Structure[] IslandRecipe(ThemeId id)
        {
            switch (id)
            {
                case ThemeId.Obsidian: return new[] { ThemeWorldKit.ObsidianIsland(0), ThemeWorldKit.ObsidianIsland(1), ThemeWorldKit.ObsidianIsland(2) };
                case ThemeId.AncientStone: return new[] { ThemeWorldKit.SandstoneGarden(0), ThemeWorldKit.SandstoneGarden(1), ThemeWorldKit.SandstoneGarden(2) };
                case ThemeId.Neon: return new[] { ThemeWorldKit.NeonIsland(0), ThemeWorldKit.NeonIsland(1) };
                case ThemeId.Space: return new[] { ThemeWorldKit.SpaceIsland(0), ThemeWorldKit.SpaceIsland(1), ThemeWorldKit.SpaceIsland(2) };
                default: return new[] { ThemeWorldKit.PalaceGarden(0), ThemeWorldKit.PalaceGarden(1), ThemeWorldKit.PalaceGarden(2) };
            }
        }

        private static ThemeWorldKit.Structure[] DebrisRecipe(ThemeId id)
        {
            switch (id)
            {
                case ThemeId.Obsidian: return new[] { ThemeWorldKit.BasaltSpire(), ThemeWorldKit.ObsidianIsland(3) };
                case ThemeId.AncientStone: return new[] { ThemeWorldKit.BrokenColonnade(), ThemeWorldKit.Obelisk() };
                case ThemeId.Neon: return new[] { ThemeWorldKit.NeonBillboard(), ThemeWorldKit.NeonArch() };
                case ThemeId.Space: return new[] { ThemeWorldKit.CrystalCluster(), ThemeWorldKit.Asteroid() };
                default: return new[] { ThemeWorldKit.PalaceArch(), ThemeWorldKit.PalaceTower() };
            }
        }

        // Atmosphere follows the same recipe as the gameplay camera: sky, fog and
        // accent are pushed into the stone shader so distant masses dissolve into the
        // horizon instead of reading as cut-out props.
        public void ApplyVisuals(ThemeVisuals v, float celebrate, float fail)
        {
            visuals = v;
            var tint = Color.Lerp(v.ruinColor, v.propLight, .52f);
            stoneMaterial.color = Color.Lerp(tint, v.accent, celebrate * .10f);
            darkMaterial.color = Color.Lerp(v.propDark, v.ruinColor, .30f);
            foliageMaterial.color = Color.Lerp(v.foliage, v.accent, celebrate * .12f);
            foundationMaterial.color = Color.Lerp(foundationTint, v.accent, celebrate * .08f);
            accentMaterial.SetColor("_Color", Color.Lerp(v.accent, v.edgeColor, .30f));
            accentMaterial.SetFloat("_Intensity", accentIntensity * (1f + celebrate * .65f));
            orbMaterial.SetColor("_Color", orbColor);
            orbMaterial.SetFloat("_Intensity", .38f + celebrate * .22f);
            ringMaterial.SetColor("_Color", ringColor);
            ringMaterial.SetFloat("_Intensity", .22f + celebrate * .25f);
            trimMaterial.SetColor("_Color", Color.Lerp(v.edgeColor, v.accent, .35f));
            trimMaterial.SetFloat("_Intensity", .24f + celebrate * .20f + fail * .26f);
            if (fail > 0f)
            {
                accentMaterial.SetColor("_Color", Color.Lerp(v.accent, new Color(1f, .34f, .22f), fail));
                accentMaterial.SetFloat("_Intensity", accentIntensity * (1f + fail * .8f));
            }
            PushSky(stoneMaterial, v);
            PushSky(darkMaterial, v);
            PushSky(foliageMaterial, v);
            PushSky(foundationMaterial, v);
        }

        private static void PushSky(Material material, ThemeVisuals v)
        {
            var sky = RenderSettings.skybox;
            material.SetColor("_SkyTop", sky != null ? sky.GetColor("_Top") : v.skyTop);
            material.SetColor("_SkyHorizon", sky != null ? sky.GetColor("_Horizon") : v.skyHorizon);
            material.SetColor("_SkyBottom", sky != null ? sky.GetColor("_Bottom") : v.skyBottom);
            material.SetColor("_Accent", v.accent);
            material.SetFloat("_AccentStrength", .05f);
            material.SetColor("_SunColor", v.sunColor);
            material.SetVector("_SunDir", v.sunDirection);
            material.SetFloat("_SunIntensity", v.sunIntensity);
            material.SetColor("_CloudTint", v.cloudTint);
            material.SetFloat("_CloudStrength", v.cloudStrength);
            material.SetFloat("_Stars", v.starStrength);
            material.SetFloat("_FogNear", RenderSettings.fogStartDistance);
            material.SetFloat("_FogFar", RenderSettings.fogEndDistance);
            material.SetFloat("_Visibility", 1f);
        }

        public void Tick(float dt, float towerHeight, float stage, float celebrate)
        {
            if (!visible) return;
            var time = Time.time;
            var towerDepth = camera.WorldToViewportPoint(new Vector3(0, towerHeight, 0)).z;
            TickGroup(landmarks, time, towerHeight, towerDepth, stage);
            TickGroup(midground, time, towerHeight, towerDepth, stage);
            TickGroup(islands, time, towerHeight, towerDepth, stage);
            TickGroup(debris, time, towerHeight, towerDepth, stage);

            orbSpin += dt * 1.4f;
            orbRoot.localRotation = Quaternion.Euler(0, orbSpin, 0);
            orbRoot.localPosition = new Vector3(orbRoot.localPosition.x, 2f + towerHeight * .12f, orbRoot.localPosition.z);
            var pulse = 1f + Mathf.Sin(time * .5f) * .02f + celebrate * .04f;
            orb.transform.localScale = Vector3.one * orbScale * pulse;
            ringA.transform.localRotation = Quaternion.Euler(18 + Mathf.Sin(time * .18f) * 4f, orbSpin * 1.6f, -14);
            ringB.transform.localRotation = Quaternion.Euler(18 + Mathf.Cos(time * .22f) * 4f, -orbSpin * 1.2f, -14);
        }

        private void TickGroup(Prop[] props, float time, float towerHeight, float towerDepth, float stage)
        {
            foreach (var p in props)
            {
                var bob = Mathf.Sin(time * .19f + p.phase) * p.bob;
                var sway = Mathf.Cos(time * .12f + p.phase * 1.4f) * p.sway;
                p.root.localPosition = p.origin + new Vector3(sway, bob + towerHeight * .18f - stage * .9f, sway * .6f);
                if (p.spin != 0f)
                    p.root.localRotation = Quaternion.Euler(p.spin * 3f,
                        Mathf.Atan2(p.origin.x, p.origin.z) * Mathf.Rad2Deg + time * p.spin, p.spin * 2f);

                var visibility = Visibility(p, towerHeight, towerDepth);
                p.visibility = visibility;
                if (visibility <= .01f)
                {
                    p.stone.enabled = p.accent.enabled = p.dark.enabled = false;
                    continue;
                }
                p.stoneProps.SetFloat("_Visibility", visibility);
                p.stone.SetPropertyBlock(p.stoneProps);
                p.accentProps.SetFloat("_Visibility", 1f);
                p.accent.SetPropertyBlock(p.accentProps);
                p.darkProps.SetFloat("_Visibility", visibility * .9f);
                p.dark.SetPropertyBlock(p.darkProps);
                p.stone.enabled = p.stoneFilter.sharedMesh != null;
                p.accent.enabled = p.accentFilter.sharedMesh != null;
                p.dark.enabled = p.darkFilter.sharedMesh != null;
            }
        }

        // Ninth-of-screen precision is not needed; a viewport test is cheap and keeps
        // every silhouette out of the stack corridor while still framing the edges.
        private float Visibility(Prop p, float towerHeight, float towerDepth)
        {
            var position = p.root.position;
            var vp = camera.WorldToViewportPoint(position);
            if (vp.z <= 0f) return 0f;
            var edge = Mathf.SmoothStep(0, 1, (vp.x + .34f) / .22f)
                       * Mathf.SmoothStep(0, 1, (1.34f - vp.x) / .22f)
                       * Mathf.SmoothStep(0, 1, (vp.y + .40f) / .24f)
                       * Mathf.SmoothStep(0, 1, (1.48f - vp.y) / .24f);
            if (edge <= 0f) return 0f;
            var beyondTower = vp.z - towerDepth;
            if (beyondTower > p.shadowRadius) return edge;
            // Near the tower: only survive when it sits well outside the play column.
            return edge * Mathf.SmoothStep(0, 1, (Mathf.Abs(vp.x - .5f) - .20f) / .16f);
        }

        public void Dispose()
        {
            if (root != null) Object.Destroy(root.gameObject);
            Object.Destroy(stoneMaterial);
            Object.Destroy(darkMaterial);
            Object.Destroy(foliageMaterial);
            Object.Destroy(accentMaterial);
            Object.Destroy(orbMaterial);
            Object.Destroy(ringMaterial);
            Object.Destroy(foundationMaterial);
            Object.Destroy(trimMaterial);
        }
    }
}
