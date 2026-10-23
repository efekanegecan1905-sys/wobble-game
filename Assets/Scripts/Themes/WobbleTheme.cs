using UnityEngine;

namespace Wobble.Themes
{
    [System.Serializable]
    public struct EnvironmentColors
    {
        public Color skyTop, skyBottom, fogNear, fogFar;
        public Color ambientTint, keyLightTint, distantStructureTint, accentTint, optionalParticleTint;
        [Range(0, .15f)] public float accentStrength;
        public static EnvironmentColors Blend(EnvironmentColors a, EnvironmentColors b, float t) => new()
        {
            skyTop=Color.Lerp(a.skyTop,b.skyTop,t), skyBottom=Color.Lerp(a.skyBottom,b.skyBottom,t),
            fogNear=Color.Lerp(a.fogNear,b.fogNear,t), fogFar=Color.Lerp(a.fogFar,b.fogFar,t),
            ambientTint=Color.Lerp(a.ambientTint,b.ambientTint,t), keyLightTint=Color.Lerp(a.keyLightTint,b.keyLightTint,t),
            distantStructureTint=Color.Lerp(a.distantStructureTint,b.distantStructureTint,t),
            accentTint=Color.Lerp(a.accentTint,b.accentTint,t), optionalParticleTint=Color.Lerp(a.optionalParticleTint,b.optionalParticleTint,t),
            accentStrength=Mathf.Lerp(a.accentStrength,b.accentStrength,t)
        };
    }
    public enum ThemeId { Carrara, Obsidian, AncientStone, Neon, Space }

    [CreateAssetMenu(menuName = "WOBBLE/Theme", fileName = "WobbleTheme")]
    public class WobbleTheme : ScriptableObject
    {
        public ThemeId id = ThemeId.Carrara;
        public string displayName = "Carrara";
        public Color blockTop = new(.92f, .90f, .85f);
        public Color blockSide = new(.89f, .87f, .825f);
        public Color veinColor = new(.51f, .52f, .51f);
        public Color background = new(0.025f, 0.03f, 0.045f);
        public Color fog = new(0.045f, 0.05f, 0.07f);
        public Color accent = new(.94f, .88f, .75f);
        [Header("Gameplay environment — shared world, theme-specific colors")]
        public EnvironmentColors environment;
        [Range(0f, 1f)] public float metallic = 0f;
        [Range(0f, 1f)] public float smoothness = .27f;
        public AudioClip placementSound;
        public AudioClip perfectSound;
        public AudioClip fallingSound;
        public AudioClip levelUpSound;
        public AudioClip gameOverSound;
    }
}
