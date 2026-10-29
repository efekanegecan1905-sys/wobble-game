using UnityEngine;
using UnityEngine.UI;
using Wobble.Themes;

namespace Wobble.UI
{
    // A code-painted destination postcard for the world picker. Each theme gets its
    // own horizon, light and skyline, so the cards read as five different places
    // instead of five recolours of one tower study. No textures, no extra draw cost
    // beyond a handful of tinted quads per card.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ThemePoster : MaskableGraphic
    {
        public ThemeId theme = ThemeId.Carrara;
        public bool locked;

        public ThemePoster() { useLegacyMeshGeneration = false; }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var rect = rectTransform.rect;
            var v = ThemeVisuals.Get(theme);
            var top = v.skyTop; var horizon = v.skyHorizon; var low = v.skyBottom;
            var accent = v.accent;
            var accent2 = v.accentSecondary;
            var stone = v.propLight;
            var dark = v.propDark;
            var ruin = v.ruinColor;

            // Horizon placement differs per destination: a low cloud sea for the
            // palace, a high molten line for the fortress, open space for the tower.
            var horizonY = theme == ThemeId.Space ? .10f : theme == ThemeId.Obsidian ? .40f : theme == ThemeId.Neon ? .26f : .30f;

            // Sky: a handful of flat bands reads as a soft gradient at card size.
            var bands = 14;
            for (var i = 0; i < bands; i++)
            {
                var y0 = i / (float)bands; var y1 = (i + 1) / (float)bands;
                var c0 = SkyColor(y0); var c1 = SkyColor(y1);
                var yA = rect.yMin + rect.height * y0;
                var yB = rect.yMin + rect.height * y1;
                Quad(new Rect(rect.xMin, yA, rect.width, yB - yA), c0, c1);
            }

            // Celestial body and its halo.
            var cx = theme == ThemeId.Obsidian ? .82f : theme == ThemeId.Neon ? .18f : .76f;
            var cy = theme == ThemeId.Obsidian ? .80f : theme == ThemeId.Space ? .74f : .74f;
            var r = theme == ThemeId.Obsidian ? .20f : theme == ThemeId.AncientStone ? .16f : .12f;
            Disc(cx, cy, r * 2.25f, Fade(accent, .085f), 26);
            Disc(cx, cy, r * 1.45f, Fade(accent, .16f), 26);
            Disc(cx, cy, r, theme == ThemeId.Space ? stone : accent, 26);
            Disc(cx - r * .18f, cy + r * .18f, r * .62f, Fade(Color.white, .18f), 18);

            // Stars: deterministic sparkle, only where the sky is dark.
            if (v.starStrength > .3f)
            {
                var count = theme == ThemeId.Space ? 22 : 12;
                for (var i = 0; i < count; i++)
                {
                    var u = Hash(i * 7.13f + 1.7f);
                    var w = Hash(i * 3.71f + 5.1f) * .62f + .34f;
                    var size = .008f + Hash(i * 1.93f + 2.2f) * .012f;
                    var alpha = .35f + Hash(i * 5.17f + .4f) * .5f;
                    Disc(u, w, size, Fade(Color.white, alpha), 5);
                }
            }

            // Distant ridges.
            var ridgeColor = Color.Lerp(ruin, horizon, theme == ThemeId.AncientStone ? .45f : .30f);
            for (var i = 0; i < 5; i++)
            {
                var u = .05f + i * .21f;
                var height = .10f + Hash(i * 2.7f + .3f) * .10f;
                Triangle(u - .13f, horizonY, u + .13f, horizonY, u, horizonY + height, Fade(ridgeColor, .55f));
            }

            // Two floating islands give every card the same vertical world language.
            Island(.14f, .56f, .085f, Color.Lerp(stone, horizon, .35f), stone, dark);
            Island(.87f, .47f, .065f, Color.Lerp(stone, horizon, .45f), stone, dark);

            switch (theme)
            {
                case ThemeId.Obsidian:
                    Spire(.30f, horizonY, .055f, .34f, Color.Lerp(dark, ruin, .35f));
                    Spire(.66f, horizonY, .045f, .26f, Color.Lerp(dark, ruin, .5f));
                    Spire(.90f, horizonY, .035f, .20f, Color.Lerp(dark, ruin, .6f));
                    Keep(.42f, horizonY, .20f, .12f, Color.Lerp(dark, ruin, .28f), accent);
                    break;
                case ThemeId.AncientStone:
                    Ziggurat(.26f, horizonY, .26f, .11f, Color.Lerp(stone, ruin, .35f), accent);
                    Obelisk(.68f, horizonY, .030f, .26f, Color.Lerp(stone, ruin, .28f), accent);
                    Obelisk(.78f, horizonY, .024f, .19f, Color.Lerp(stone, ruin, .38f), accent);
                    break;
                case ThemeId.Neon:
                    NeonTower(.22f, horizonY, .13f, .30f, dark, accent, accent2);
                    NeonTower(.44f, horizonY, .10f, .21f, dark, accent2, accent);
                    NeonTower(.70f, horizonY, .16f, .37f, dark, accent, accent2);
                    Billboard(.86f, .52f, .22f, .11f, accent, accent2);
                    break;
                case ThemeId.Space:
                    Crystal(.28f, horizonY, .06f, .26f, Color.Lerp(stone, accent, .25f), accent2);
                    Crystal(.40f, horizonY, .04f, .17f, Color.Lerp(stone, accent, .4f), accent2);
                    Monolith(.70f, horizonY, .07f, .30f, Color.Lerp(stone, accent, .18f), accent2);
                    break;
                default:
                    Portico(.26f, horizonY, .28f, .17f, Color.Lerp(stone, horizon, .25f), accent);
                    Dome(.70f, horizonY, .22f, .16f, Color.Lerp(stone, horizon, .18f), accent);
                    break;
            }

            // Hero tower, always centred: the one element shared by every destination.
            Disc(.5f, .052f, .12f, new Color(0f, 0f, 0f, .22f), 20);
            var towerBase = .06f;
            var towerWidth = .235f;
            var blockHeight = .055f;
            var layout = new[] { 1f, .84f, .68f, .52f, .36f };
            Quad(new Rect(U(.5f - towerWidth * .54f), V(towerBase), rect.width * towerWidth * 1.08f,
                rect.height * blockHeight * layout.Length * .95f), new Color(0f, 0f, 0f, .28f), new Color(0f, 0f, 0f, .12f));
            for (var i = 0; i < layout.Length; i++)
            {
                var w = towerWidth * layout[i];
                var y = towerBase + i * blockHeight;
                var face = Color.Lerp(dark, stone, .70f);
                var light = Color.Lerp(face, Color.white, .55f);
                var shade = Color.Lerp(face, Color.black, .30f);
                Quad(new Rect(U(.5f - w * .5f), V(y), rect.width * w, rect.height * blockHeight * .84f), face, face);
                Quad(new Rect(U(.5f - w * .5f), V(y), rect.width * w * .18f, rect.height * blockHeight * .84f), shade, shade);
                Quad(new Rect(U(.5f - w * .5f), V(y + blockHeight * .84f), rect.width * w, rect.height * blockHeight * .16f), light, light);
            }
            var crown = towerBase + layout.Length * blockHeight;
            Quad(new Rect(U(.5f - towerWidth * .36f * .5f), V(crown), rect.width * towerWidth * .36f, rect.height * .009f), Fade(accent, .9f), Fade(accent, .9f));
            Quad(new Rect(U(.5f - .003f), V(crown), rect.width * .006f, rect.height * .048f), Fade(accent, .85f), Fade(accent, .35f));
            Disc(.5f, crown + .052f, .013f, Fade(accent, .95f), 8);
            // Accent motes.
            for (var i = 0; i < 6; i++)
            {
                var u = Hash(i * 9.1f + 3.4f);
                var w = .12f + Hash(i * 4.3f + 1.1f) * .40f;
                Disc(u, w, .007f, Fade(accent, .55f), 4);
            }

            if (locked) Quad(rect, new Color(0, 0, 0, .34f), new Color(0, 0, 0, .16f));

            Color SkyColor(float t)
            {
                // Bottom of the rect is the low sky, top is the zenith.
                return t < .55f
                    ? Color.Lerp(low, horizon, t / .55f)
                    : Color.Lerp(horizon, top, (t - .55f) / .45f);
            }

            float U(float u) => rect.xMin + u * rect.width;
            float V(float v) => rect.yMin + v * rect.height;

            void Quad(Rect r, Color bottom, Color topColor)
            {
                var n = vh.currentVertCount;
                vh.AddVert(new Vector3(r.xMin, r.yMin), bottom, Vector2.zero);
                vh.AddVert(new Vector3(r.xMin, r.yMax), topColor, Vector2.up);
                vh.AddVert(new Vector3(r.xMax, r.yMax), topColor, Vector2.one);
                vh.AddVert(new Vector3(r.xMax, r.yMin), bottom, Vector2.right);
                vh.AddTriangle(n, n + 1, n + 2);
                vh.AddTriangle(n, n + 2, n + 3);
            }

            void Disc(float u, float v0, float radius, Color color, int segments)
            {
                var center = new Vector3(U(u), V(v0), 0);
                var size = Mathf.Min(rect.width, rect.height) * radius;
                var n = vh.currentVertCount;
                vh.AddVert(center, color, new Vector2(.5f, .5f));
                for (var i = 0; i <= segments; i++)
                {
                    var a = i / (float)segments * Mathf.PI * 2f;
                    vh.AddVert(new Vector3(center.x + Mathf.Cos(a) * size,
                        center.y + Mathf.Sin(a) * size, 0), color,
                        new Vector2(.5f + Mathf.Cos(a) * .5f, .5f + Mathf.Sin(a) * .5f));
                }
                for (var i = 0; i < segments; i++) vh.AddTriangle(n, n + 1 + i, n + 2 + i);
            }

            void Triangle(float x0, float y0, float x1, float y1, float x2, float y2, Color color)
            {
                var n = vh.currentVertCount;
                vh.AddVert(new Vector3(U(x0), V(y0)), color, Vector2.zero);
                vh.AddVert(new Vector3(U(x1), V(y1)), color, Vector2.right);
                vh.AddVert(new Vector3(U(x2), V(y2)), color, Vector2.up);
                vh.AddTriangle(n, n + 1, n + 2);
            }

            void Island(float u, float v0, float radius, Color cap, Color capLight, Color rock)
            {
                var width = radius * 2f;
                Quad(new Rect(U(u - radius), V(v0 - radius * 1.55f), rect.width * width, rect.height * radius * .7f), rock, Fade(rock, .0f));
                Triangle(u - radius, v0, u + radius, v0, u, v0 - radius * 1.85f, rock);
                Quad(new Rect(U(u - radius * .92f), V(v0), rect.width * width * .92f, rect.height * radius * .30f), cap, capLight);
                Disc(u, v0 + radius * .26f, radius * .92f, capLight, 14);
            }

            void Spire(float u, float baseY, float width, float height, Color color)
            {
                Triangle(u - width * .5f, baseY, u + width * .5f, baseY, u + width * .10f, baseY + height, color);
                Triangle(u - width * .14f, baseY, u + width * .5f, baseY, u + width * .10f, baseY + height * .72f,
                    Color.Lerp(color, Color.black, .22f));
            }

            void Keep(float u, float baseY, float width, float height, Color color, Color glow)
            {
                Quad(new Rect(U(u - width * .5f), V(baseY), rect.width * width, rect.height * height), color,
                    Color.Lerp(color, stone, .2f));
                Quad(new Rect(U(u - width * .62f), V(baseY + height), rect.width * width * 1.24f, rect.height * .018f), glow, glow);
            }

            void Ziggurat(float u, float baseY, float width, float height, Color color, Color glow)
            {
                for (var i = 0; i < 4; i++)
                {
                    var w = width * (1f - i * .18f);
                    var y = baseY + i * height * .30f;
                    Quad(new Rect(U(u - w * .5f), V(y), rect.width * w, rect.height * height * .30f),
                        Color.Lerp(color, Color.black, .10f * i), Color.Lerp(color, Color.white, .12f + .04f * i));
                }
                Quad(new Rect(U(u - width * .10f), V(baseY + height * .30f * 4f), rect.width * width * .20f, rect.height * .02f), glow, glow);
            }

            void Obelisk(float u, float baseY, float width, float height, Color color, Color glow)
            {
                Quad(new Rect(U(u - width * .5f), V(baseY), rect.width * width, rect.height * height), color,
                    Color.Lerp(color, Color.white, .24f));
                Triangle(u - width * .5f, baseY + height, u + width * .5f, baseY + height, u, baseY + height + height * .12f,
                    Color.Lerp(color, accent, .5f));
                Quad(new Rect(U(u - width * .8f), V(baseY + height * .34f), rect.width * width * 1.6f, rect.height * .007f), glow, glow);
            }

            void NeonTower(float u, float baseY, float width, float height, Color color, Color glowA, Color glowB)
            {
                Quad(new Rect(U(u - width * .5f), V(baseY), rect.width * width, rect.height * height), color,
                    Color.Lerp(color, glowA, .18f));
                for (var i = 0; i < 4; i++)
                {
                    var y = baseY + height * (.16f + i * .21f);
                    var c = i % 2 == 0 ? glowA : glowB;
                    Quad(new Rect(U(u - width * .40f), V(y), rect.width * width * .80f, rect.height * .008f), c, c);
                }
                Quad(new Rect(U(u - width * .06f), V(baseY), rect.width * width * .12f, rect.height * height), Fade(glowB, .55f), Fade(glowB, .55f));
            }

            void Billboard(float u, float v0, float width, float height, Color glowA, Color glowB)
            {
                Quad(new Rect(U(u - width * .5f), V(v0), rect.width * width, rect.height * height), Color.Lerp(glowA, Color.black, .72f),
                    Color.Lerp(glowA, Color.white, .10f));
                Quad(new Rect(U(u - width * .46f), V(v0 + height * .12f), rect.width * width * .92f, rect.height * height * .75f), Fade(glowA, .65f), Fade(glowB, .45f));
            }

            void Crystal(float u, float baseY, float width, float height, Color color, Color glow)
            {
                Triangle(u - width * .5f, baseY, u + width * .5f, baseY, u, baseY + height, color);
                Triangle(u - width * .18f, baseY, u + width * .5f, baseY, u, baseY + height, Color.Lerp(color, Color.white, .22f));
                Quad(new Rect(U(u - width * .10f), V(baseY + height * .35f), rect.width * width * .20f, rect.height * height * .30f), Fade(glow, .45f), Fade(glow, .2f));
            }

            void Monolith(float u, float baseY, float width, float height, Color color, Color glow)
            {
                Quad(new Rect(U(u - width * .5f), V(baseY), rect.width * width, rect.height * height), color,
                    Color.Lerp(color, Color.white, .18f));
                Quad(new Rect(U(u - width * .16f), V(baseY + height * .22f), rect.width * width * .32f, rect.height * height * .56f), Fade(glow, .38f), Fade(glow, .10f));
            }

            void Portico(float u, float baseY, float width, float height, Color color, Color glow)
            {
                var top = baseY + height;
                Quad(new Rect(U(u - width * .5f), V(baseY), rect.width * width, rect.height * .022f), Color.Lerp(color, Color.black, .10f), color);
                for (var i = 0; i < 5; i++)
                {
                    var columnWidth = width * .06f;
                    var x = u - width * .42f + i * width * .21f;
                    Quad(new Rect(U(x - columnWidth * .5f), V(baseY + .022f), rect.width * columnWidth, rect.height * height * .74f), color,
                        Color.Lerp(color, Color.white, .12f));
                }
                Quad(new Rect(U(u - width * .5f), V(top - .055f), rect.width * width, rect.height * .055f), Color.Lerp(color, Color.white, .10f), color);
                Quad(new Rect(U(u - width * .5f), V(top - .062f), rect.width * width, rect.height * .008f), glow, glow);
            }

            void Dome(float u, float baseY, float width, float height, Color color, Color glow)
            {
                Quad(new Rect(U(u - width * .5f), V(baseY), rect.width * width, rect.height * height * .42f), color, Color.Lerp(color, Color.white, .10f));
                Disc(u, baseY + height * .42f, width * .30f, Color.Lerp(color, Color.white, .14f), 16);
                Quad(new Rect(U(u - width * .5f), V(baseY + height * .42f), rect.width * width, rect.height * .008f), glow, glow);
            }
        }

        private static Color Fade(Color color, float alpha) { color.a = alpha; return color; }
        private static float Hash(float n) { var v = Mathf.Sin(n * 127.1f) * 43758.5453f; return v - Mathf.Floor(v); }
    }
}
