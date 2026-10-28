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
            BlockMaterial.SetColor("_RimColor", Color.Lerp(v.edgeColor, v.accent, .25f));
            TopMaterial.SetColor("_RimColor", Color.Lerp(v.edgeColor, v.accent, .25f));
            BlockMaterial.SetFloat("_RimStrength", v.edgeGlow * .40f);
            TopMaterial.SetFloat("_RimStrength", v.edgeGlow * .24f);
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
        }

        public void Dispose()
        {
            if (RenderSettings.skybox == atmosphere) RenderSettings.skybox = null;
            Object.Destroy(TopMaterial); Object.Destroy(BlockMaterial); Object.Destroy(atmosphere);
        }
    }
}
