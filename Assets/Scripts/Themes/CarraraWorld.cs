using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Wobble.CameraSystem;

namespace Wobble.Themes
{
    // Presentation only: no colliders, scoring, input, movement or scene loads.
    // The foreground theatre (24 quarried monuments) stays contract-identical; the
    // destination skyline lives in its own ThemeDistrict so gameplay depth tests
    // remain untouched.
    public sealed class CarraraWorld : MonoBehaviour
    {
        private sealed class Monument
        {
            public Transform transform;
            public Renderer renderer;
            public Vector3 origin;
            public int layer;
            public float visibility;
            public MaterialPropertyBlock properties = new();
        }

        private readonly Monument[] monuments = new Monument[24];
        private readonly Material[] materials = new Material[2];
        private readonly Mesh[] meshes = new Mesh[3];
        private Camera gameplayCamera;
        private Light key;
        private ParticleSystem dust;
        private Material dustMaterial;
        private Transform content;
        private Material cloudSeaMaterial;
        private Renderer cloudSea;
        private Texture2D cloudTexture;
        private readonly List<Mesh> generatedMeshes = new();
        private float height, progress, targetProgress;
        private bool playing;
        private ThemeManager themes;
        private ThemeDistrict district;
        private ThemeId districtTheme = (ThemeId)(-1);
        private float celebrate, failFlash;
        private Color propTint = WobblePalette.StoneLight;

        public void SetThemes(ThemeManager value) => themes = value;
        public int Stage => Mathf.Clamp(Mathf.FloorToInt(targetProgress) + 1, 1, 4);
        public int GeometryCount => monuments.Length;
        public int AmbientParticleLimit => dust == null ? 0 : dust.main.maxParticles;

        public void Build(Camera gameplayCamera, Light keyLight)
        {
            this.gameplayCamera = gameplayCamera;
            key = keyLight;
            content = new GameObject("Carrara architecture — presentation only").transform;
            content.SetParent(transform, false);
            for (var i = 0; i < 2; i++)
                materials[i] = new Material(Resources.Load<Shader>("Shaders/ArchitecturalStone"))
                {
                    name = i == 0 ? "Weathered horizon stone" : "Fog-bound distant stone",
                    enableInstancing = true
                };
            materials[0].color = WobblePalette.StoneLight;
            materials[1].color = WobblePalette.StoneMid;
            var grain = Resources.Load<Texture2D>("Rendering/CarraraNatural");
            foreach (var material in materials) material.mainTexture = grain;

            // Theme-agnostic rock silhouettes: angular ridges that read as a distant
            // coast for every destination. Identity comes from the district in front.
            for (var i = 0; i < meshes.Length; i++)
            {
                meshes[i] = BuildRidgeMesh(i);
                generatedMeshes.Add(meshes[i]);
            }
            for (var i = 0; i < monuments.Length; i++)
            {
                var distant = i >= 14;
                var a = i * Mathf.PI * 2 / (distant ? 10 : 14) + (distant ? .3f : 0);
                var radius = distant ? 62 + (i % 3) * 8 : 34 + (i % 3) * 5;
                var origin = new Vector3(Mathf.Cos(a) * radius, distant ? -24 : -20, Mathf.Sin(a) * radius);
                var go = new GameObject(distant ? "Fog-bound ridge" : "Quarried horizon monument");
                go.transform.SetParent(content, false);
                go.transform.localPosition = origin;
                go.transform.localRotation = Quaternion.Euler(0, i * 37, 0);
                go.transform.localScale = distant ? new Vector3(11, 6 + i % 4 * 2, 9) : new Vector3(6 + i % 3, 5 + i % 4 * 2, 5 + i % 2);
                go.AddComponent<MeshFilter>().sharedMesh = meshes[distant ? 0 : 1 + i % 2];
                var renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = materials[distant ? 1 : 0];
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                monuments[i] = new Monument { transform = go.transform, renderer = renderer, origin = origin, layer = distant ? 1 : 0 };
            }
            BuildDust();
            BuildBackdrop();
            Show(false);
        }

        private void BuildDust()
        {
            var go = new GameObject("Suspended marble motes");
            go.transform.SetParent(content, false);
            dust = go.AddComponent<ParticleSystem>();
            dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = dust.main;
            main.playOnAwake = false;
            main.startLifetime = 9;
            main.startSpeed = .025f;
            main.startSize = new ParticleSystem.MinMaxCurve(.008f, .019f);
            main.startColor = new Color(.68f, .66f, .6f, .1f);
            main.maxParticles = QualitySettings.GetQualityLevel() == 0 ? 12 : 36;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var shape = dust.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(18, 12, 18);
            var emission = dust.emission;
            emission.rateOverTime = 1;
            dustMaterial = new Material(Resources.Load<Shader>("Shaders/StoneDust"));
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = dustMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        private void BuildBackdrop()
        {
            district = new ThemeDistrict(transform, gameplayCamera);
            cloudTexture = CreateCloudTexture(192);
            cloudSeaMaterial = new Material(Resources.Load<Shader>("Shaders/CloudSea")) { name = "Cloud sea" };
            cloudSeaMaterial.SetTexture("_MainTex", cloudTexture);
            var sea = new GameObject("Cloud sea", typeof(MeshFilter), typeof(MeshRenderer));
            sea.GetComponent<MeshFilter>().sharedMesh = ThemeWorldKit.QuadMesh;
            sea.transform.SetParent(transform, false);
            sea.transform.localPosition = new Vector3(0, -16.8f, 0);
            sea.transform.localRotation = Quaternion.Euler(90, 0, 0);
            sea.transform.localScale = new Vector3(230, 230, 1);
            cloudSea = sea.GetComponent<Renderer>();
            cloudSea.sharedMaterial = cloudSeaMaterial;
            cloudSeaMaterial.SetFloat("_Tiling", 3.4f);
            cloudSea.shadowCastingMode = ShadowCastingMode.Off;
            cloudSea.receiveShadows = false;
        }

        // Three chunky ridge builds from the shared primitive kit. They are far away
        // enough that silhouette, not detail, does the work.
        private static Mesh BuildRidgeMesh(int variant)
        {
            var parts = new List<CombineInstance>();
            void Add(Mesh mesh, Vector3 position, Vector3 scale, Quaternion rotation) =>
                parts.Add(new CombineInstance { mesh = mesh, transform = Matrix4x4.TRS(position, rotation, scale) });
            Add(ThemeWorldKit.PrismMesh, Vector3.zero, new Vector3(1.3f, 1.1f, 1.1f), Quaternion.Euler(0, variant * 31, 0));
            Add(ThemeWorldKit.ConeMesh, new Vector3(.25f, .42f, .12f), new Vector3(.55f, 1.0f, .55f), Quaternion.Euler(0, variant * 47, 7));
            Add(ThemeWorldKit.ConeMesh, new Vector3(-.35f, .30f, -.15f), new Vector3(.45f, .72f, .45f), Quaternion.Euler(0, variant * 63, -9));
            Add(ThemeWorldKit.BoxMesh, new Vector3(-.50f, -.34f, .14f), new Vector3(1.0f, .68f, .9f), Quaternion.Euler(0, variant * 23, -8));
            Add(ThemeWorldKit.BoxMesh, new Vector3(.55f, -.40f, -.20f), new Vector3(.8f, .54f, .7f), Quaternion.Euler(0, variant * 17, 11));
            var mesh = new Mesh { name = "Horizon ridge " + variant };
            mesh.CombineMeshes(parts.ToArray(), true, true);
            mesh.RecalculateBounds();
            return mesh;
        }

        // Small cypress and shrub clusters replace the old flat green cap: from the
        // gameplay camera a real garden reads as foliage, a flat slab reads as a lily pad.
        internal static Mesh CreateGardenMesh(float s, int variant)
        {
            var cone = ConeMesh(10);
            var sphere = SphereMesh(10, 5);
            var parts = new CombineInstance[7];
            for (var i = 0; i < 4; i++)
            {
                var angle = i * 1.7f + variant * .63f;
                var radius = s * (.30f + (i % 3) * .26f);
                var height = s * (.78f + (i % 2) * .40f);
                var width = s * (.19f + (i % 2) * .05f);
                parts[i] = new CombineInstance
                {
                    mesh = cone,
                    transform = Matrix4x4.TRS(
                        new Vector3(Mathf.Cos(angle) * radius, s * 1.00f, Mathf.Sin(angle) * radius),
                        Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0),
                        new Vector3(width, height, width))
                };
            }
            for (var i = 0; i < 3; i++)
            {
                var angle = i * 2.1f + variant * 1.1f;
                var radius = s * (.52f + (i % 2) * .30f);
                var size = s * (.26f + (i % 2) * .07f);
                parts[4 + i] = new CombineInstance
                {
                    mesh = sphere,
                    transform = Matrix4x4.TRS(
                        new Vector3(Mathf.Cos(angle) * radius, s * 1.00f + size * .55f, Mathf.Sin(angle) * radius),
                        Quaternion.identity, new Vector3(size, size * .78f, size))
                };
            }
            var mesh = new Mesh { name = "Island garden " + variant };
            mesh.CombineMeshes(parts, true, true);
            mesh.RecalculateBounds();
            Object.Destroy(cone);
            Object.Destroy(sphere);
            return mesh;
        }

        // Unit cypress: base at y=0, apex at y=1, radius 1.
        internal static Mesh CypressMesh() => ConeMesh(10);

        private static Mesh ConeMesh(int segments)
        {
            var vertices = new List<Vector3>(segments * 3);
            var triangles = new List<int>(segments * 3);
            for (var i = 0; i < segments; i++)
            {
                var a0 = i / (float)segments * Mathf.PI * 2f;
                var a1 = (i + 1) / (float)segments * Mathf.PI * 2f;
                var n = vertices.Count;
                vertices.Add(new Vector3(Mathf.Cos(a0), 0, Mathf.Sin(a0)));
                vertices.Add(Vector3.up);
                vertices.Add(new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1)));
                triangles.Add(n); triangles.Add(n + 1); triangles.Add(n + 2);
            }
            var mesh = new Mesh { name = "Cypress" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        // Unit low-poly sphere, radius 1, used as a clipped Mediterranean shrub.
        private static Mesh SphereMesh(int columns, int rows)
        {
            var vertices = new List<Vector3>((columns + 1) * (rows + 1));
            var triangles = new List<int>(columns * rows * 6);
            for (var r = 0; r <= rows; r++)
            {
                var phi = Mathf.PI * r / rows;
                for (var c = 0; c <= columns; c++)
                {
                    var theta = Mathf.PI * 2f * c / columns;
                    vertices.Add(new Vector3(Mathf.Sin(phi) * Mathf.Cos(theta), Mathf.Cos(phi), Mathf.Sin(phi) * Mathf.Sin(theta)));
                }
            }
            for (var r = 0; r < rows; r++)
            for (var c = 0; c < columns; c++)
            {
                var i0 = r * (columns + 1) + c;
                var i1 = i0 + 1;
                var i2 = i0 + columns + 1;
                var i3 = i2 + 1;
                triangles.Add(i0); triangles.Add(i1); triangles.Add(i2);
                triangles.Add(i1); triangles.Add(i3); triangles.Add(i2);
            }
            var mesh = new Mesh { name = "Shrub" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public void Show(bool show)
        {
            playing = show;
            if (content != null) content.gameObject.SetActive(show);
            if (district != null) district.SetVisible(show);
            if (cloudSea != null) cloudSea.enabled = show;
            if (show)
            {
                height = progress = targetProgress = 0;
                celebrate = failFlash = 0;
                if (dust != null) dust.Play();
            }
            else
            {
                if (dust != null) dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                key.intensity = 1.15f;
                key.transform.rotation = Quaternion.Euler(46, -32, 0);
            }
        }

        public void Follow(float topHeight) => height = topHeight;
        public void SetLevel(int level) => targetProgress = Mathf.Clamp(level - 1, 0, 3);

        // Purely presentational pulses driven by gameplay events.
        public void Celebrate(int combo)
        {
            celebrate = Mathf.Clamp01(.32f + combo * .075f);
            if (themes != null) failFlash = 0;
        }

        public void Fail()
        {
            celebrate = 0;
            failFlash = 1f;
        }

        private void LateUpdate()
        {
            if (!playing) return;
            var v = themes != null ? themes.Visuals : ThemeVisuals.Carrara;
            var palette = themes != null ? themes.Environment : default;
            if (themes != null)
            {
                if (districtTheme != themes.Current.id)
                {
                    districtTheme = themes.Current.id;
                    district.SetTheme(districtTheme);
                }
                materials[0].color = Color.Lerp(v.ruinColor, v.propLight, .18f);
                materials[1].color = Color.Lerp(v.ruinColor, v.propDark, .30f);
                var main = dust.main;
                main.startColor = WobblePalette.Alpha(Color.Lerp(palette.optionalParticleTint, v.accent, .25f), .15f);
            }
            var sky = RenderSettings.skybox;
            if (sky != null)
                foreach (var material in materials)
                {
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
                    material.SetFloat("_FogNear", RenderSettings.fogStartDistance);
                    material.SetFloat("_FogFar", RenderSettings.fogEndDistance);
                }
            progress = Mathf.MoveTowards(progress, targetProgress, Time.deltaTime / 2.4f);
            var stage = progress / 3;
            // World-anchored layers lag far behind tower growth instead of riding with it.
            for (var i = 0; i < monuments.Length; i++)
            {
                var m = monuments[i];
                var target = m.origin + Vector3.up * (height * (m.layer == 0 ? .16f : .055f) - stage * 5);
                m.transform.localPosition = Vector3.Lerp(m.transform.localPosition, target, 1 - Mathf.Exp(-Time.deltaTime * .7f));
                var visibility = DepthVisibility(m.renderer, 9f, 14f);
                var character = m.layer == 1 ? Mathf.Lerp(.65f, 1, stage) : i % 3 == 0 ? Mathf.Lerp(1, .35f, stage)
                    : Mathf.Lerp(.7f, 1, Mathf.Min(1, progress)) * Mathf.Lerp(1, .4f, Mathf.Clamp01((progress - 1) / 2));
                visibility *= character;
                m.visibility = visibility;
                m.properties.SetFloat("_Visibility", m.visibility);
                m.renderer.SetPropertyBlock(m.properties);
                m.renderer.enabled = m.visibility > .005f;
            }
            if (district != null)
            {
                district.ApplyVisuals(v, celebrate, failFlash);
                district.Tick(Time.deltaTime, height, stage, celebrate);
            }
            UpdateCloudSea(v, stage);
            dust.transform.position = new Vector3(0, height + 1, 0);
            var emission = dust.emission;
            emission.rateOverTime = QualitySettings.GetQualityLevel() == 0 ? .5f : Mathf.Lerp(.7f, 2.4f, stage);
            celebrate = Mathf.MoveTowards(celebrate, 0, Time.deltaTime * 1.8f);
            failFlash = Mathf.MoveTowards(failFlash, 0, Time.deltaTime * 2.4f);
            // Night destinations carry a per-theme light lift so stone stays readable.
            key.intensity = (Mathf.Lerp(1.15f, 1.22f, stage) + celebrate * .34f - failFlash * .12f) * v.keyBoost;
            key.transform.rotation = Quaternion.Euler(Mathf.Lerp(46, 51, stage), Mathf.Lerp(-32, -22, stage), 0);
        }

        private void UpdateCloudSea(ThemeVisuals v, float stage)
        {
            var accentPulse = celebrate * celebrate;
            cloudSeaMaterial.SetColor("_Color", v.cloudSeaTop);
            cloudSeaMaterial.SetColor("_DeepColor", v.cloudSeaDeep);
            cloudSeaMaterial.SetColor("_Accent", v.accent);
            cloudSeaMaterial.SetVector("_SunDir", v.sunDirection);
            cloudSeaMaterial.SetFloat("_Opacity", v.cloudSeaAlpha + accentPulse * .06f);
            cloudSeaMaterial.SetFloat("_Tiling", 3.4f);
            if (cloudSea != null)
            {
                var sea = cloudSea.transform;
                sea.localPosition = new Vector3(0, -16.8f - stage * 1.4f, 0);
            }
        }

        // Same depth contract as the monuments: nothing may enter the active travel plane.
        private float DepthVisibility(Renderer renderer, float margin, float fade)
        {
            var forward = gameplayCamera.transform.forward;
            var bounds = renderer.bounds;
            var radius = Vector3.Dot(bounds.extents, new Vector3(Mathf.Abs(forward.x), Mathf.Abs(forward.y), Mathf.Abs(forward.z)));
            var behind = Vector3.Dot(bounds.center - new Vector3(0, height, 0), forward) - radius;
            return Mathf.SmoothStep(0, 1, (behind - margin) / fade);
        }

        // Tileable value noise baked once at startup: the cloud sea costs two texture reads.
        internal static Texture2D CreateCloudTexture(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Baked cloud sea",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var u = x / (float)size * 4f;
                var v = y / (float)size * 4f;
                var n = Fbm(u, v);
                var a = Mathf.SmoothStep(.32f, .82f, n) * .92f;
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private static float Fbm(float x, float y)
        {
            var value = 0f;
            var amplitude = .55f;
            var period = 4;
            for (var octave = 0; octave < 4; octave++)
            {
                value += amplitude * PeriodicNoise(x * period, y * period, period);
                period *= 2;
                amplitude *= .5f;
            }
            return value;
        }

        private static float PeriodicNoise(float x, float y, int period)
        {
            var xi = Mathf.FloorToInt(x);
            var yi = Mathf.FloorToInt(y);
            var xf = x - xi;
            var yf = y - yi;
            xf = xf * xf * (3 - 2 * xf);
            yf = yf * yf * (3 - 2 * yf);
            var a = Lattice(xi, yi, period);
            var b = Lattice(xi + 1, yi, period);
            var c = Lattice(xi, yi + 1, period);
            var d = Lattice(xi + 1, yi + 1, period);
            return Mathf.Lerp(Mathf.Lerp(a, b, xf), Mathf.Lerp(c, d, xf), yf);
        }

        private static float Lattice(int x, int y, int period)
        {
            x = ((x % period) + period) % period;
            y = ((y % period) + period) % period;
            var n = Mathf.Sin(x * 127.1f + y * 311.7f) * 43758.5453f;
            return n - Mathf.Floor(n);
        }

        private void OnDestroy()
        {
            foreach (var material in materials) if (material != null) Destroy(material);
            Destroy(dustMaterial);
            if (district != null) district.Dispose();
            Destroy(cloudSeaMaterial);
            Destroy(cloudTexture);
            foreach (var mesh in generatedMeshes) if (mesh != null) Destroy(mesh);
        }
    }
}
