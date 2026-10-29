using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Wobble.Themes;

namespace Wobble.Effects
{
    // Lightweight, pooled world feedback: stone dust, celebration sparks, a rising
    // combo aura and a single expanding shockwave ring. No lights, no post effects.
    public sealed class EffectPool
    {
        private readonly Queue<ParticleSystem> particles = new();
        private readonly Queue<ParticleSystem> sparks = new();
        private readonly MonoBehaviour host;
        private readonly Transform parent;
        private readonly Material dustMaterial;
        private readonly Material sparkMaterial;
        private readonly Texture2D glowTexture, ringTexture;

        private GameObject aura, ring;
        private Material auraMaterial, ringMaterial;
        private Color accent = new(.8f, .78f, .72f), accentSecondary = new(1f, .85f, .55f);
        private float auraTime, auraStrength, auraBase = 1.2f;
        private float ringTime, ringStrength, ringBase = .8f, ringEnd = 3.2f;
        private const float AuraDuration = 1.15f;
        private const float RingDuration = .62f;

        public EffectPool(MonoBehaviour host, Transform parent, Color accent, int count = 4)
        {
            this.host = host;
            this.parent = parent;
            this.accent = accent;
            dustMaterial = new Material(Resources.Load<Shader>("Shaders/StoneDust"));
            sparkMaterial = new Material(Resources.Load<Shader>("Shaders/Spark")) { name = "Celebration sparks" };
            glowTexture = CreateRadialTexture(64, 2.4f);
            ringTexture = CreateRingTexture(128);
            sparkMaterial.SetTexture("_MainTex", glowTexture);
            for (var i = 0; i < count; i++) particles.Enqueue(CreateDust(parent));
            for (var i = 0; i < 3; i++) sparks.Enqueue(CreateSpark(parent));
            auraMaterial = new Material(Resources.Load<Shader>("Shaders/Spark")) { name = "Combo aura" };
            auraMaterial.SetTexture("_MainTex", glowTexture);
            ringMaterial = new Material(Resources.Load<Shader>("Shaders/Spark")) { name = "Shockwave ring" };
            ringMaterial.SetTexture("_MainTex", ringTexture);
            aura = CreateQuad("Combo aura", auraMaterial, parent);
            ring = CreateQuad("Shockwave ring", ringMaterial, parent);
        }

        public void SetAccent(Color primary, Color secondary)
        {
            accent = primary;
            accentSecondary = secondary;
        }

        private ParticleSystem CreateDust(Transform parent)
        {
            var go = new GameObject("Pooled stone dust");
            go.transform.SetParent(parent);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false; main.duration = .25f; main.startLifetime = new ParticleSystem.MinMaxCurve(.18f, .45f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(.05f, .16f); main.startSize = new ParticleSystem.MinMaxCurve(.014f, .035f);
            main.gravityModifier = .06f;
            main.startColor = accent;
            main.maxParticles = 32; main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission; emission.rateOverTime = 0;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(2f, .02f, 2f);
            var fade = ps.colorOverLifetime; fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) });
            fade.color = gradient;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = dustMaterial;
            go.SetActive(false);
            return ps;
        }

        private ParticleSystem CreateSpark(Transform parent)
        {
            var go = new GameObject("Pooled celebration spark");
            go.transform.SetParent(parent);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false; main.duration = .12f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(.28f, .72f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(.012f, .034f);
            main.gravityModifier = .32f;
            main.startColor = Color.white;
            main.maxParticles = QualitySettings.GetQualityLevel() == 0 ? 28 : 64;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission; emission.rateOverTime = 0;
            var shape = ps.shape; shape.enabled = false;
            var fade = ps.colorOverLifetime; fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, .7f), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(.9f, .55f), new GradientAlphaKey(0, 1) });
            fade.color = gradient;
            var limit = ps.limitVelocityOverLifetime;
            limit.enabled = false;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = sparkMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            go.SetActive(false);
            return ps;
        }

        private GameObject CreateQuad(string name, Material material, Transform parent)
        {
            // Shared kit mesh: CreatePrimitive would ask for a MeshCollider that
            // IL2CPP strips, and every quad only needs a filter and renderer.
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.GetComponent<MeshFilter>().sharedMesh = ThemeWorldKit.QuadMesh;
            go.transform.SetParent(parent, false);
            go.transform.localRotation = Quaternion.Euler(90, 0, 0);
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            go.SetActive(false);
            return go;
        }

        public void Play(Vector3 position, Vector3 size, bool perfect = true, Vector3? cutEdge = null, float cutFraction = 0)
        {
            var ps = particles.Count > 0 ? particles.Dequeue() : null;
            if (ps == null) return;
            ps.transform.position = position - Vector3.up * (size.y * .5f - .015f);
            var shape = ps.shape;
            shape.scale = new Vector3(size.x * .9f, .02f, size.z * .9f);
            ps.gameObject.SetActive(true);
            ps.Play(true);
            var count = perfect ? 12 : 6 + Mathf.RoundToInt(cutFraction * 12);
            if (QualitySettings.GetQualityLevel() == 0) count = Mathf.Max(3, count / 2);
            for (var i = 0; i < count; i++)
            {
                var side = i % 4; var along = (i / 4) / (float)Mathf.Max(1, Mathf.CeilToInt(count / 4f) - 1) * 2 - 1;
                var offset = side < 2 ? new Vector3((side == 0 ? -1 : 1) * size.x * .5f, 0, along * size.z * .5f)
                    : new Vector3(along * size.x * .5f, 0, (side == 2 ? -1 : 1) * size.z * .5f);
                if (!perfect && cutEdge.HasValue)
                {
                    var edge = cutEdge.Value - position;
                    offset = Mathf.Abs(edge.x) > Mathf.Abs(edge.z)
                        ? new Vector3(Mathf.Sign(edge.x) * size.x * .5f, 0, Mathf.Lerp(-size.z * .5f, size.z * .5f, i / (float)Mathf.Max(1, count - 1)))
                        : new Vector3(Mathf.Lerp(-size.x * .5f, size.x * .5f, i / (float)Mathf.Max(1, count - 1)), 0, Mathf.Sign(edge.z) * size.z * .5f);
                }
                ps.Emit(new ParticleSystem.EmitParams { position = ps.transform.position + offset, velocity = offset.normalized * .09f + Vector3.up * .07f }, 1);
            }
            host.StartCoroutine(Return(ps));
        }

        // Perfect placement and combo feedback. Called after the placement event.
        public void Celebrate(bool perfect, Vector3 position, Vector3 size, int combo)
        {
            var top = position + Vector3.up * (size.y * .5f + .015f);
            // x5 and x8 are the escalation beats: more sparks, a wider shockwave and a
            // brighter aura, without adding a second effect system or a second draw call
            // for the common case.
            var tier = combo >= 8 ? 2 : combo >= 5 ? 1 : 0;
            var high = combo >= 4;
            var primary = high ? accentSecondary : accent;
            var secondary = high ? accent : accentSecondary;
            SpawnSparks(top, size, (perfect ? 12 + Mathf.Min(18, combo * 2) : 6) + tier * 8,
                perfect ? primary : secondary, perfect ? 1f + tier * .18f : .55f);
            if (!perfect) return;
            EmitRing(top, .62f, 2.6f + Mathf.Min(1.4f, combo * .12f) + tier * .85f, 1.15f + tier * .22f, primary);
            ShowAura(top, size, combo, secondary);
            if (high) SpawnSparks(top, size, 8 + tier * 7, secondary, 1.1f + tier * .12f);
            if (tier >= 2) SpawnSparks(top, size, 10, Color.Lerp(primary, Color.white, .45f), 1.35f);
        }

        public void Fail(Vector3 position, Vector3 size)
        {
            var top = position + Vector3.up * (size.y * .5f + .02f);
            SpawnSparks(top, size, QualitySettings.GetQualityLevel() == 0 ? 16 : 30, accent, 1.25f);
            SpawnSparks(top, size, 16, new Color(1f, .62f, .28f), 1f);
            EmitRing(position + Vector3.up * .03f, 1.1f, 6.8f, 1.5f, accentSecondary);
            if (aura != null) aura.SetActive(false);
        }

        private void SpawnSparks(Vector3 position, Vector3 size, int count, Color color, float strength)
        {
            var ps = sparks.Count > 0 ? sparks.Dequeue() : null;
            if (ps == null) return;
            if (QualitySettings.GetQualityLevel() == 0) count = Mathf.Max(6, count / 2);
            ps.transform.position = position;
            ps.gameObject.SetActive(true);
            ps.Play(true);
            var radius = Mathf.Max(size.x, size.z) * .42f;
            for (var i = 0; i < count; i++)
            {
                var angle = i * Mathf.PI * 2f / count + Random.Range(-.35f, .35f);
                var outward = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                var start = position + outward * radius * Random.Range(.25f, 1f);
                var velocity = outward * Random.Range(.35f, 1.35f) * strength + Vector3.up * Random.Range(.75f, 2.15f) * strength;
                ps.Emit(new ParticleSystem.EmitParams
                {
                    position = start,
                    velocity = velocity,
                    startSize = Random.Range(.012f, .038f) * (.75f + strength * .25f),
                    startLifetime = Random.Range(.28f, .68f),
                    startColor = color
                }, 1);
            }
            host.StartCoroutine(ReturnSparks(ps));
        }

        private void EmitRing(Vector3 position, float start, float end, float strength, Color color)
        {
            if (ring == null) return;
            ring.transform.position = position;
            ring.transform.localScale = Vector3.one * start;
            ringMaterial.SetColor("_Color", color);
            ringMaterial.SetFloat("_Intensity", strength);
            ringBase = start; ringEnd = end; ringStrength = strength; ringTime = 0f;
            ring.SetActive(true);
        }

        private void ShowAura(Vector3 position, Vector3 size, int combo, Color color)
        {
            if (aura == null) return;
            var tier = combo >= 8 ? 2 : combo >= 5 ? 1 : 0;
            aura.transform.position = position;
            auraBase = Mathf.Max(1.05f, Mathf.Max(size.x, size.z) * (.90f + Mathf.Min(.35f, combo * .035f) + tier * .14f));
            aura.transform.localScale = Vector3.one * auraBase;
            auraMaterial.SetColor("_Color", color);
            auraMaterial.SetFloat("_Intensity", .55f);
            auraStrength = .55f + Mathf.Min(.55f, combo * .055f) + tier * .16f;
            auraTime = 0;
            aura.SetActive(true);
        }

        public void Tick(float dt)
        {
            if (aura != null && aura.activeSelf)
            {
                auraTime += dt;
                var t = auraTime / AuraDuration;
                if (t >= 1f) aura.SetActive(false);
                else
                {
                    aura.transform.localScale = Vector3.one * Mathf.Lerp(auraBase, auraBase * 1.42f, t);
                    auraMaterial.SetFloat("_Intensity", auraStrength * (1f - t) * (1f - t) * 1.6f);
                }
            }
            if (ring != null && ring.activeSelf)
            {
                ringTime += dt;
                var t = ringTime / RingDuration;
                if (t >= 1f) ring.SetActive(false);
                else
                {
                    ring.transform.localScale = Vector3.one * Mathf.Lerp(ringBase, ringEnd, Mathf.Sqrt(t));
                    ringMaterial.SetFloat("_Intensity", ringStrength * (1f - t) * (1f - t) * 1.8f);
                }
            }
        }

        private IEnumerator Return(ParticleSystem ps)
        {
            yield return new WaitForSeconds(1f);
            ps.gameObject.SetActive(false);
            particles.Enqueue(ps);
        }

        private IEnumerator ReturnSparks(ParticleSystem ps)
        {
            yield return new WaitForSeconds(.85f);
            ps.gameObject.SetActive(false);
            sparks.Enqueue(ps);
        }

        private static Texture2D CreateRadialTexture(int size, float power)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Soft radial glow", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear
            };
            var pixels = new Color32[size * size];
            var centre = (size - 1) * .5f;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var d = Mathf.Clamp01(1f - new Vector2(x - centre, y - centre).magnitude / centre);
                var a = Mathf.Pow(d, power);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private static Texture2D CreateRingTexture(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Shockwave ring", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear
            };
            var pixels = new Color32[size * size];
            var centre = (size - 1) * .5f;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var d = new Vector2(x - centre, y - centre).magnitude / centre;
                var a = Mathf.SmoothStep(.52f, .74f, d) * (1f - Mathf.SmoothStep(.86f, 1f, d));
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        public void Dispose()
        {
            Object.Destroy(dustMaterial);
            Object.Destroy(sparkMaterial);
            Object.Destroy(auraMaterial);
            Object.Destroy(ringMaterial);
            Object.Destroy(glowTexture);
            Object.Destroy(ringTexture);
        }
    }
}
