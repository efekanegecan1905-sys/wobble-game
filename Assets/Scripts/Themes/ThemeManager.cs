using UnityEngine;

namespace Wobble.Themes
{
    public sealed class ThemeManager
    {
        public WobbleTheme Current { get; private set; }
        public Material BlockMaterial { get; private set; }
        public Material TopMaterial { get; private set; }
        public EnvironmentColors Environment { get; private set; }
        private Material atmosphere;
        private EnvironmentColors fromEnvironment;
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
            var p = Environment;
            atmospherePhase = Mathf.Lerp(atmosphereFrom, atmosphereTarget, t);
            RenderSettings.fogColor = Color.Lerp(p.fogNear, p.fogFar, atmospherePhase);
            RenderSettings.ambientSkyColor = p.ambientTint;
            RenderSettings.ambientEquatorColor = p.ambientTint * .8f;
            RenderSettings.ambientGroundColor = p.ambientTint * .45f;
            TopMaterial.color = Color.Lerp(fromTint, targetTint, t);
            BlockMaterial.color = TopMaterial.color * .97f;
            RenderSettings.fogStartDistance = Mathf.Lerp(48, 42, atmospherePhase);
            RenderSettings.fogEndDistance = Mathf.Lerp(110, 95, atmospherePhase);
            var breath = (.5f + .5f * Mathf.Sin(Time.time * .32f)) * p.accentStrength;
            atmosphere.SetColor("_Top", Color.Lerp(p.skyTop, p.accentTint, breath * .35f));
            atmosphere.SetColor("_Horizon", Color.Lerp(p.skyBottom, p.fogFar, atmospherePhase * .16f));
            atmosphere.SetColor("_Bottom", Color.Lerp(p.fogNear, p.accentTint, breath));
            atmosphere.SetColor("_Accent", p.accentTint);
            atmosphere.SetFloat("_AccentStrength", p.accentStrength);
        }

        public void Dispose()
        {
            if (RenderSettings.skybox == atmosphere) RenderSettings.skybox = null;
            Object.Destroy(TopMaterial); Object.Destroy(BlockMaterial); Object.Destroy(atmosphere);
        }
    }
}
