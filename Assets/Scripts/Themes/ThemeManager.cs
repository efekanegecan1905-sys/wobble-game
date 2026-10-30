using UnityEngine;

namespace Wobble.Themes
{
    public sealed class ThemeManager
    {
        public WobbleTheme Current { get; private set; }
        public Material BlockMaterial { get; private set; }
        public Material TopMaterial { get; private set; }
        public EnvironmentColors Environment { get; private set; }
        // Richer, art-directed layer used by sky, props, clouds and celebration light.
        public ThemeVisuals Visuals { get; private set; }
        public Color Accent => Visuals.accent;
        public Color AccentSecondary => Visuals.accentSecondary;
        public float LevelPhase => atmospherePhase;
        private Material atmosphere;
        private EnvironmentColors fromEnvironment;
        private ThemeVisuals fromVisuals, targetVisuals;
        private Color fromTint, targetTint;
        private float transitionTime = 1f;
        private float atmosphereFrom, atmosphereTarget, atmospherePhase;
        private Color? demoTint;
        private int level = 1;
        private float comboLevel, comboPulse;
        private int comboTier, comboPulseTier;
        private readonly Wobble.Economy.MenuCatalog catalog;

        public void PreviewSurface(Color? tint)
        {
            demoTint = tint;
            fromTint = targetTint = tint ?? Current.blockTop;
            TopMaterial.color = targetTint;
            BlockMaterial.color = targetTint * .97f;
        }

        public ThemeManager()
        {
            Current = Resources.Load<WobbleTheme>("Themes/Carrara");
            catalog = Resources.Load<Wobble.Economy.MenuCatalog>("Menu/MenuCatalog");
            var shader = Resources.Load<Shader>("Shaders/Carrara");
            BlockMaterial = new Material(shader) { name = "Theme sides (Runtime)" };
            TopMaterial = new Material(shader) { name = "Theme top (Runtime)" };
            BlockMaterial.SetFloat("_Metallic", Current.metallic);
            TopMaterial.SetFloat("_Metallic", Current.metallic);
            BlockMaterial.SetFloat("_Glossiness", Current.smoothness * .92f);
            TopMaterial.SetFloat("_Glossiness", Current.smoothness);
            TopMaterial.mainTexture = BlockMaterial.mainTexture = Resources.Load<Texture2D>("Rendering/CarraraNatural");
            atmosphere = new Material(Resources.Load<Shader>("Shaders/Atmosphere"));
            RenderSettings.skybox = atmosphere;
            Visuals = fromVisuals = targetVisuals = ThemeVisuals.Get(Current.id);
            SelectTheme("Carrara", true);
        }

        public void SelectTheme(string id, bool immediate = false)
        {
            var theme = Resources.Load<WobbleTheme>("Themes/" + id);
            if (theme == null) theme = Resources.Load<WobbleTheme>("Themes/Carrara");
            Current = theme;
            // Preserve the approved menu/demo surface tones and finish. Only atmosphere is new.
            var definition = catalog == null ? null : System.Array.Find(catalog.themes, item => item.id == Current.id.ToString());
            demoTint = Current.id == ThemeId.Carrara || definition == null ? (Color?)null : definition.previewTint;
            ApplyEnvironment(level, immediate);
        }

        public void ApplyEnvironment(int level, bool immediate = false)
        {
            this.level = level;
            atmosphereFrom = atmospherePhase;
            atmosphereTarget = Mathf.Clamp01((level - 1) / 3f);
            fromEnvironment = Environment;
            fromVisuals = Visuals;
            targetVisuals = ThemeVisuals.Get(Current.id);
            fromTint = TopMaterial.color;
            targetTint = demoTint ?? Current.blockTop;
            transitionTime = immediate ? 1f : 0f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            Tick(0f);
        }

        // Presentation-only FLOW/FEVER energy. The gameplay environment colours
        // stay authoritative; only the sky accents and rim react to the combo.
        public void SetCombo(float intensity, int tier)
        {
            comboLevel = Mathf.Clamp01(intensity);
            comboTier = tier;
        }

        public void Pulse(int tier)
        {
            comboPulse = 1f;
            comboPulseTier = tier;
        }

        public void Tick(float dt)
        {
            transitionTime = Mathf.Min(1f, transitionTime + dt / 2.4f);
            var t = Mathf.SmoothStep(0, 1, transitionTime);
            Environment = EnvironmentColors.Blend(fromEnvironment, Current.environment, t);
            Visuals = ThemeVisuals.Lerp(fromVisuals, targetVisuals, t);
            var p = Environment;
            var v = Visuals;
            atmospherePhase = Mathf.Lerp(atmosphereFrom, atmosphereTarget, t);
            RenderSettings.fogColor = Color.Lerp(p.fogNear, p.fogFar, atmospherePhase);
            RenderSettings.ambientSkyColor = p.ambientTint;
            RenderSettings.ambientEquatorColor = p.ambientTint * .8f;
            RenderSettings.ambientGroundColor = p.ambientTint * .45f;
            TopMaterial.color = Color.Lerp(fromTint, targetTint, t);
            BlockMaterial.color = TopMaterial.color * .97f;
            // A shared Fresnel rim keeps the dark destinations readable without extra lights.
            var rim = Color.Lerp(v.edgeColor, v.accent, .25f);
            BlockMaterial.SetColor("_RimColor", rim);
            TopMaterial.SetColor("_RimColor", rim);
            BlockMaterial.SetFloat("_RimStrength", v.edgeGlow * (.40f + comboLevel * .10f));
            TopMaterial.SetFloat("_RimStrength", v.edgeGlow * (.24f + comboLevel * .08f));
            BlockMaterial.SetFloat("_EdgeLight", .17f + comboLevel * .035f);
            TopMaterial.SetFloat("_EdgeLight", .21f + comboLevel * .045f);
            RenderSettings.fogStartDistance = Mathf.Lerp(v.fogNear + 4f, v.fogNear, atmospherePhase);
            RenderSettings.fogEndDistance = Mathf.Lerp(v.fogFar + 8f, v.fogFar, atmospherePhase);
            var breath = (.5f + .5f * Mathf.Sin(Time.time * .32f)) * Mathf.Clamp01(p.accentStrength * 1.8f + .035f);
            atmosphere.SetColor("_Top", Color.Lerp(v.skyTop, v.accent, breath * .22f));
            atmosphere.SetColor("_Horizon", Color.Lerp(v.skyHorizon, p.fogFar, atmospherePhase * .12f));
            atmosphere.SetColor("_Bottom", Color.Lerp(v.skyBottom, v.accent, breath * .35f));
            atmosphere.SetColor("_Accent", v.accent);
            atmosphere.SetFloat("_AccentStrength", Mathf.Clamp(p.accentStrength * 1.8f + .035f, .03f, .16f));
            atmosphere.SetColor("_SunColor", v.sunColor);
            atmosphere.SetVector("_SunDir", v.sunDirection);
            atmosphere.SetFloat("_SunIntensity", v.sunIntensity);
            atmosphere.SetColor("_CloudTint", v.cloudTint);
            atmosphere.SetFloat("_CloudStrength", v.cloudStrength);
            atmosphere.SetFloat("_Stars", v.starStrength);
            ApplyComboAtmosphere(v, p);
            comboPulse = Mathf.MoveTowards(comboPulse, 0f, dt / .34f);
        }

        // The five destinations answer the same combo beat with different light:
        // gold cloud lift, ember horizon, warm dust, cyan/magenta chase and star/nebula
        // shimmer. Only sky material values change here; Environment stays untouched.
        private void ApplyComboAtmosphere(ThemeVisuals v, EnvironmentColors p)
        {
            if (comboLevel <= .001f && comboPulse <= .001f) return;
            var energy = comboLevel;
            var beat = comboPulse * (comboPulseTier >= 2 ? .10f : .055f);
            var accentPower = Mathf.Clamp(p.accentStrength * 1.8f + .035f + energy * .055f + beat, .03f, .30f);
            var reaction = Current.id switch
            {
                ThemeId.Obsidian => new Color(1f, .40f, .14f),
                ThemeId.AncientStone => new Color(1f, .79f, .40f),
                ThemeId.Neon => new Color(.23f, .96f, 1f),
                ThemeId.Space => new Color(.62f, .52f, 1f),
                _ => new Color(1f, .73f, .30f)
            };
            if (Current.id == ThemeId.Neon)
            {
                var chase = .5f + .5f * Mathf.Sin(Time.time * 2.15f);
                reaction = Color.Lerp(v.accentSecondary, v.accent, chase);
            }
            atmosphere.SetColor("_Accent", Color.Lerp(v.accent, reaction, energy * .60f + beat));
            atmosphere.SetFloat("_AccentStrength", accentPower);
            atmosphere.SetColor("_Horizon", Color.Lerp(
                Color.Lerp(v.skyHorizon, p.fogFar, atmospherePhase * .12f), reaction, energy * .16f + beat * .32f));
            if (Current.id == ThemeId.Space)
                atmosphere.SetFloat("_Stars", v.starStrength + energy * .55f + beat * .8f);
            if (Current.id == ThemeId.Carrara || Current.id == ThemeId.AncientStone)
                atmosphere.SetFloat("_CloudStrength", v.cloudStrength + energy * .10f + beat * .12f);
            if (Current.id == ThemeId.Obsidian)
                atmosphere.SetFloat("_CloudStrength", v.cloudStrength + energy * .05f);
        }

        public void Dispose()
        {
            if (RenderSettings.skybox == atmosphere) RenderSettings.skybox = null;
            Object.Destroy(TopMaterial); Object.Destroy(BlockMaterial); Object.Destroy(atmosphere);
        }
    }
}
