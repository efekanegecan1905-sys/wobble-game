using UnityEngine;

namespace Wobble.Themes
{
    // Art direction for each destination. Runtime-only: no assets, no gameplay.
    // The theme's EnvironmentColors remain the authoritative gameplay palette; this
    // recipe only enriches sky, atmosphere, props and celebratory light.
    public struct ThemeVisuals
    {
        public Color skyTop, skyHorizon, skyBottom;
        public Color sunColor, cloudTint;
        public Vector3 sunDirection;
        public float sunIntensity, cloudStrength, starStrength;

        public Color accent, accentSecondary;
        public Color propLight, propDark, foliage, flower, portal;
        // Base tone for midground/distant ruins; chosen to read against that theme's sky.
        public Color ruinColor;
        public Color cloudSeaTop, cloudSeaDeep;
        public Color edgeColor;
        // Rim strength and key-light lift: night destinations need extra light to stay readable.
        public float edgeGlow, keyBoost;
        // Alpha of the low cloud sea; thinner for space so the nebula sky shows through.
        public float cloudSeaAlpha;
        public float fogNear, fogFar;

        public static ThemeVisuals Lerp(ThemeVisuals a, ThemeVisuals b, float t) => new()
        {
            skyTop = Color.Lerp(a.skyTop, b.skyTop, t),
            skyHorizon = Color.Lerp(a.skyHorizon, b.skyHorizon, t),
            skyBottom = Color.Lerp(a.skyBottom, b.skyBottom, t),
            sunColor = Color.Lerp(a.sunColor, b.sunColor, t),
            cloudTint = Color.Lerp(a.cloudTint, b.cloudTint, t),
            sunDirection = Vector3.Slerp(a.sunDirection, b.sunDirection, t),
            sunIntensity = Mathf.Lerp(a.sunIntensity, b.sunIntensity, t),
            cloudStrength = Mathf.Lerp(a.cloudStrength, b.cloudStrength, t),
            starStrength = Mathf.Lerp(a.starStrength, b.starStrength, t),
            accent = Color.Lerp(a.accent, b.accent, t),
            accentSecondary = Color.Lerp(a.accentSecondary, b.accentSecondary, t),
            propLight = Color.Lerp(a.propLight, b.propLight, t),
            propDark = Color.Lerp(a.propDark, b.propDark, t),
            foliage = Color.Lerp(a.foliage, b.foliage, t),
            flower = Color.Lerp(a.flower, b.flower, t),
            portal = Color.Lerp(a.portal, b.portal, t),
            ruinColor = Color.Lerp(a.ruinColor, b.ruinColor, t),
            cloudSeaTop = Color.Lerp(a.cloudSeaTop, b.cloudSeaTop, t),
            cloudSeaDeep = Color.Lerp(a.cloudSeaDeep, b.cloudSeaDeep, t),
            edgeColor = Color.Lerp(a.edgeColor, b.edgeColor, t),
            edgeGlow = Mathf.Lerp(a.edgeGlow, b.edgeGlow, t),
            keyBoost = Mathf.Lerp(a.keyBoost, b.keyBoost, t),
            cloudSeaAlpha = Mathf.Lerp(a.cloudSeaAlpha, b.cloudSeaAlpha, t),
            fogNear = Mathf.Lerp(a.fogNear, b.fogNear, t),
            fogFar = Mathf.Lerp(a.fogFar, b.fogFar, t)
        };

        public static ThemeVisuals Get(ThemeId id)
        {
            switch (id)
            {
                case ThemeId.Obsidian: return Obsidian;
                case ThemeId.AncientStone: return Ancient;
                case ThemeId.Neon: return Neon;
                case ThemeId.Space: return Space;
                default: return Carrara;
            }
        }

        private static Color H(uint rgb) => new((rgb >> 16 & 255) / 255f, (rgb >> 8 & 255) / 255f, (rgb & 255) / 255f);

        // Bright Carrara Palace: saturated cerulean sky, warm sun, white marble ruins.
        // The gameplay camera looks down, so skyBottom/horizon carry the visible colour.
        public static readonly ThemeVisuals Carrara = new()
        {
            skyTop = H(0x0F6FC8), skyHorizon = H(0x9FD4F4), skyBottom = H(0x53ACE6),
            sunColor = H(0xFFEFC0), sunDirection = new Vector3(-0.55f, 0.58f, 0.42f).normalized,
            sunIntensity = 1.18f, cloudTint = H(0xFFFDF6), cloudStrength = .58f, starStrength = 0f,
            accent = H(0xFFAE3D), accentSecondary = H(0xFF6F61),
            propLight = H(0xF7EDDD), propDark = H(0xCBB89F), foliage = H(0x3F8A55), flower = H(0xFF7E6B),
            portal = H(0xF4DCBB), ruinColor = H(0x8A98A4), cloudSeaTop = H(0xFDFEFF), cloudSeaDeep = H(0x7FAAD4),
            edgeColor = H(0xFFD98A), edgeGlow = 0.10f, keyBoost = 1f, cloudSeaAlpha = .74f,
            fogNear = 46f, fogFar = 118f
        };

        // Obsidian Fortress: ember horizon, volcanic rock, molten seams.
        public static readonly ThemeVisuals Obsidian = new()
        {
            skyTop = H(0x050307), skyHorizon = H(0x76301A), skyBottom = H(0x070406),
            sunColor = H(0xFF7A3C), sunDirection = new Vector3(-0.62f, 0.16f, 0.55f).normalized,
            sunIntensity = 1.35f, cloudTint = H(0x4A1826), cloudStrength = .40f, starStrength = 0.22f,
            accent = H(0xFF6B2C), accentSecondary = H(0xFFC15E),
            propLight = H(0x4A2B37), propDark = H(0x22121D), foliage = H(0x5A2433), flower = H(0xFF5A2A),
            portal = H(0x8A3A34), ruinColor = H(0x5C4A52), cloudSeaTop = H(0x5A6678), cloudSeaDeep = H(0x232B3A),
            edgeColor = H(0xFF6A22), edgeGlow = 2.0f, keyBoost = 2.2f, cloudSeaAlpha = .92f,
            fogNear = 36f, fogFar = 100f
        };

        // Ancient Monument: sun-baked sandstone, gold dust, timeless ruins.
        public static readonly ThemeVisuals Ancient = new()
        {
            skyTop = H(0xB9672E), skyHorizon = H(0xF7D8A2), skyBottom = H(0xE0AE66),
            sunColor = H(0xFFE8B0), sunDirection = new Vector3(-0.50f, 0.48f, 0.44f).normalized,
            sunIntensity = 1.15f, cloudTint = H(0xF7E4C0), cloudStrength = .46f, starStrength = 0f,
            accent = H(0xFFC35C), accentSecondary = H(0xE07A4A),
            propLight = H(0xE8C793), propDark = H(0xB07F45), foliage = H(0x6E8440), flower = H(0xE08A4D),
            portal = H(0xD9A869), ruinColor = H(0xB6925F), cloudSeaTop = H(0xFBE7C2), cloudSeaDeep = H(0xC79A5E),
            edgeColor = H(0xFFD37A), edgeGlow = 0.12f, keyBoost = 1f, cloudSeaAlpha = .74f,
            fogNear = 44f, fogFar = 116f
        };

        // Neon City: near-black plum night, magenta neon, cyan secondary light.
        // Deliberately high value separation so the theme never collapses into one purple wash.
        public static readonly ThemeVisuals Neon = new()
        {
            skyTop = H(0x05030F), skyHorizon = H(0xA326E0), skyBottom = H(0x1A0A33),
            sunColor = H(0xFF57C8), sunDirection = new Vector3(-0.34f, 0.34f, 0.58f).normalized,
            sunIntensity = 1.55f, cloudTint = H(0x7A2078), cloudStrength = .26f, starStrength = 0.55f,
            accent = H(0xFF3FCB), accentSecondary = H(0x3BF5FF),
            propLight = H(0x4A2380), propDark = H(0x0E0722), foliage = H(0x5A1E9C), flower = H(0xFF3FCB),
            portal = H(0x9B34FF), ruinColor = H(0x4E2C86), cloudSeaTop = H(0x2C0F55), cloudSeaDeep = H(0x08020F),
            edgeColor = H(0x3BF5FF), edgeGlow = 1.45f, keyBoost = 1.70f, cloudSeaAlpha = .62f,
            fogNear = 34f, fogFar = 98f
        };

        // Space Tower: cosmic indigo, violet nebulae, starlight.
        public static readonly ThemeVisuals Space = new()
        {
            skyTop = H(0x05030F), skyHorizon = H(0x4B2FC0), skyBottom = H(0x0E0826),
            sunColor = H(0xD8CDFF), sunDirection = new Vector3(-0.45f, 0.40f, 0.52f).normalized,
            sunIntensity = 1.20f, cloudTint = H(0x3A2478), cloudStrength = .22f, starStrength = 2.40f,
            accent = H(0xB58CFF), accentSecondary = H(0x6FE6FF),
            propLight = H(0x3B2A70), propDark = H(0x100A24), foliage = H(0x3E2C7C), flower = H(0x9A7BFF),
            portal = H(0x7A5AF0), ruinColor = H(0x53419E), cloudSeaTop = H(0x1C1250), cloudSeaDeep = H(0x05030E),
            edgeColor = H(0x6FE6FF), edgeGlow = 1.50f, keyBoost = 1.75f, cloudSeaAlpha = .55f,
            fogNear = 38f, fogFar = 108f
        };
    }
}
