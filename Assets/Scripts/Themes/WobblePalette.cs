using UnityEngine;
namespace Wobble.Themes
{
    // Shared concept-art tokens. UI and world consume the same warm/cool balance.
    public static class WobblePalette
    {
        public static readonly Color Primary=Hex(0xFF966F), PrimaryPressed=Hex(0xDA7055);
        public static readonly Color Secondary=Hex(0xBCA4E8), Accent=Hex(0xF5CD83), Success=Hex(0x51CFA5);
        public static readonly Color TextPrimary=Hex(0x394650), TextSecondary=Hex(0x6D757A);
        public static readonly Color Surface=Hex(0xFFF1DE), SurfaceAlt=Hex(0xF1DDC7);
        public static readonly Color BackgroundTop=Hex(0x449DC6), BackgroundBottom=Hex(0xDCE9E6);
        public static readonly Color SkyTop=Hex(0x3196C8), SkyBottom=Hex(0xDBEAF0);
        public static readonly Color FogNear=Hex(0xE8D9CE), FogFar=Hex(0xE6ECF4);
        public static readonly Color StoneLight=Hex(0xFFF3DF), StoneMid=Hex(0xDCCBBC), StoneShadow=Hex(0xA1AAB2);
        public static readonly Color GemColor=Hex(0xA562EC), LifeColor=Hex(0xFF726E);
        public static readonly Color Warning=Hex(0xEDBA65), Danger=Hex(0xDD6264);
        public static Color Alpha(Color color,float alpha){color.a=alpha;return color;}
        private static Color Hex(uint rgb)=>new Color((rgb>>16&255)/255f,(rgb>>8&255)/255f,(rgb&255)/255f);
    }
}
