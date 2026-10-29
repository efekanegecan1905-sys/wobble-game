using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wobble.Themes
{
    // Code-native destination architecture. Every silhouette is built from a few
    // shared primitive meshes, so five worlds can look genuinely different without
    // importing models, textures or a second render pipeline.
    public static class ThemeWorldKit
    {
        public sealed class Structure
        {
            public readonly Mesh Stone;
            public readonly Mesh Accent;
            public readonly Mesh Dark;
            public Structure(Mesh stone, Mesh accent = null, Mesh dark = null)
            {
                Stone = stone; Accent = accent; Dark = dark;
            }
        }

        private sealed class PartBuilder
        {
            private readonly List<CombineInstance> parts = new();
            public void Add(Mesh mesh, Vector3 position, Vector3 scale, Quaternion rotation)
                => Add(mesh, Matrix4x4.TRS(position, rotation, scale));
            public void Add(Mesh mesh, Matrix4x4 matrix)
            {
                if (mesh == null) return;
                parts.Add(new CombineInstance { mesh = mesh, transform = matrix });
            }
            public Mesh Build(string name)
            {
                if (parts.Count == 0) return null;
                var mesh = new Mesh { name = name };
                mesh.CombineMeshes(parts.ToArray(), true, true);
                mesh.RecalculateBounds();
                return mesh;
            }
        }

        private static readonly Dictionary<string, Structure> CachedStructures = new();
        private static readonly Dictionary<string, Mesh> CachedMeshes = new();
        private static Mesh box, cylinder, cone, sphere, prism, crystal, quad;

        public static Mesh BoxMesh { get { if (box == null) box = BuildBox(); return box; } }
        public static Mesh CylinderMesh { get { if (cylinder == null) cylinder = BuildCylinder(12); return cylinder; } }
        public static Mesh ConeMesh { get { if (cone == null) cone = BuildCone(12); return cone; } }
        public static Mesh SphereMesh { get { if (sphere == null) sphere = BuildSphere(24, 12); return sphere; } }
        public static Mesh PrismMesh { get { if (prism == null) prism = BuildPrism(); return prism; } }
        public static Mesh CrystalMesh { get { if (crystal == null) crystal = BuildCrystal(); return crystal; } }
        public static Mesh QuadMesh { get { if (quad == null) quad = BuildQuad(); return quad; } }

        public static Mesh RingMesh => CachedMesh("ring", () => BuildRing(.60f, 1f, 0f, 360f, 40));
        // A hairline rim used for terrace edge lighting; the wide ring reads as a
        // glowing disc at this scale.
        public static Mesh TrimMesh => CachedMesh("ring.trim", () => BuildRing(.975f, 1f, 0f, 360f, 64));
        public static Mesh RingArcA => CachedMesh("ring.arc.a", () => BuildRing(.58f, 1f, 15f, 205f, 26));
        public static Mesh RingArcB => CachedMesh("ring.arc.b", () => BuildRing(.66f, 1f, 238f, 342f, 16));

        // --- Destination structures ------------------------------------------------

        public static Structure PalacePortico() => Cached("palace.portico", () =>
        {
            var stone = new PartBuilder(); var gold = new PartBuilder();
            stone.Add(BoxMesh, new Vector3(0, .25f, 0), new Vector3(11f, .5f, 5.6f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(0, .62f, 0), new Vector3(9.8f, .28f, 5f), Quaternion.identity);
            for (var i = 0; i < 6; i++)
            {
                var x = -4.15f + i * 1.66f;
                Column(stone, gold, new Vector3(x, .78f, 0), 5.3f, .52f);
            }
            stone.Add(BoxMesh, new Vector3(0, 6.10f, 0), new Vector3(10.8f, 1.15f, 4.5f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(0, 6.78f, 0), new Vector3(11.5f, .30f, 4.9f), Quaternion.identity);
            gold.Add(BoxMesh, new Vector3(0, 6.95f, 0), new Vector3(11.7f, .10f, 5.05f), Quaternion.identity);
            stone.Add(PrismMesh, new Vector3(0, 7.72f, 0), new Vector3(11.1f, 1.45f, 4.6f), Quaternion.identity);
            gold.Add(BoxMesh, new Vector3(0, 8.48f, 0), new Vector3(11.3f, .12f, 4.8f), Quaternion.identity);
            // A domed pavilion behind the portico gives the palace a second, taller beat.
            stone.Add(CylinderMesh, new Vector3(0, 7.55f, -2.2f), new Vector3(4.4f, 1.35f, 4.4f), Quaternion.identity);
            stone.Add(SphereMesh, new Vector3(0, 8.25f, -2.2f), new Vector3(4.7f, 3.1f, 4.7f), Quaternion.identity);
            gold.Add(CylinderMesh, new Vector3(0, 8.55f, -2.2f), new Vector3(4.8f, .12f, 4.8f), Quaternion.identity);
            gold.Add(CylinderMesh, new Vector3(0, 9.85f, -2.2f), new Vector3(.22f, .9f, .22f), Quaternion.identity);
            return new Structure(stone.Build("Palace portico"), gold.Build("Palace portico trim"));
        });

        public static Structure PalaceTower() => Cached("palace.tower", () =>
        {
            var stone = new PartBuilder(); var gold = new PartBuilder();
            stone.Add(BoxMesh, new Vector3(0, .25f, 0), new Vector3(5.4f, .5f, 5.4f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(0, 1.65f, 0), new Vector3(4.2f, 2.3f, 4.2f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(0, 3.95f, 0), new Vector3(3.6f, 2.3f, 3.6f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(0, 6.20f, 0), new Vector3(3.0f, 2.2f, 3.0f), Quaternion.identity);
            gold.Add(BoxMesh, new Vector3(0, 2.84f, 0), new Vector3(4.45f, .13f, 4.45f), Quaternion.identity);
            gold.Add(BoxMesh, new Vector3(0, 5.14f, 0), new Vector3(3.85f, .13f, 3.85f), Quaternion.identity);
            gold.Add(BoxMesh, new Vector3(0, 7.34f, 0), new Vector3(3.25f, .13f, 3.25f), Quaternion.identity);
            stone.Add(CylinderMesh, new Vector3(0, 7.95f, 0), new Vector3(2.5f, 1.2f, 2.5f), Quaternion.identity);
            stone.Add(SphereMesh, new Vector3(0, 8.8f, 0), new Vector3(2.9f, 2.6f, 2.9f), Quaternion.identity);
            gold.Add(CylinderMesh, new Vector3(0, 9.82f, 0), new Vector3(2.95f, .12f, 2.95f), Quaternion.identity);
            gold.Add(CylinderMesh, new Vector3(0, 10.55f, 0), new Vector3(.18f, 1.15f, .18f), Quaternion.identity);
            for (var i = 0; i < 4; i++)
            {
                var a = i * Mathf.PI * .5f;
                stone.Add(BoxMesh, new Vector3(Mathf.Cos(a) * 1.62f, 1.45f, Mathf.Sin(a) * 1.62f),
                    new Vector3(.42f, 2.5f, .42f), Quaternion.Euler(0, -a * Mathf.Rad2Deg, 0));
            }
            return new Structure(stone.Build("Palace tower"), gold.Build("Palace tower trim"));
        });

        public static Structure PalaceArch() => Cached("palace.arch", () =>
        {
            var stone = new PartBuilder(); var gold = new PartBuilder();
            Arch(stone, gold, 4.6f, .55f, 1.6f, 0f);
            return new Structure(stone.Build("Palace arch"), gold.Build("Palace arch trim"));
        });

        public static Structure FortressKeep() => Cached("fortress.keep", () =>
        {
            var stone = new PartBuilder(); var glow = new PartBuilder(); var dark = new PartBuilder();
            stone.Add(BoxMesh, new Vector3(0, .35f, 0), new Vector3(9f, .7f, 9f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(0, 3.1f, 0), new Vector3(6.6f, 5.2f, 6.6f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(0, 7.1f, 0), new Vector3(4.8f, 3.3f, 4.8f), Quaternion.identity);
            for (var i = 0; i < 8; i++)
            {
                var a = i * Mathf.PI * .25f;
                var x = Mathf.Cos(a) * 2.3f; var z = Mathf.Sin(a) * 2.3f;
                stone.Add(BoxMesh, new Vector3(x, 9.05f, z), new Vector3(1.1f, .9f, 1.1f),
                    Quaternion.Euler(0, -a * Mathf.Rad2Deg, 0));
            }
            for (var i = 0; i < 2; i++)
            {
                var side = i == 0 ? -1 : 1;
                stone.Add(BoxMesh, new Vector3(side * 4.2f, 3.7f, 0), new Vector3(2.2f, 7.4f, 2.2f), Quaternion.identity);
                stone.Add(ConeMesh, new Vector3(side * 4.2f, 8.9f, 0), new Vector3(2.8f, 3.1f, 2.8f), Quaternion.identity);
                for (var c = 0; c < 4; c++)
                    stone.Add(BoxMesh, new Vector3(side * 4.2f + (c - 1.5f) * .62f, 7.7f, 1.05f),
                        new Vector3(.32f, .85f, .32f), Quaternion.identity);
            }
            // A tall central basalt fang.
            stone.Add(ConeMesh, new Vector3(0, 10.65f, 0), new Vector3(2.4f, 4.4f, 2.4f), Quaternion.identity);
            dark.Add(BoxMesh, new Vector3(0, 2.20f, 3.33f), new Vector3(1.5f, 3.3f, .22f), Quaternion.identity);
            dark.Add(BoxMesh, new Vector3(0, 2.20f, -3.33f), new Vector3(1.5f, 3.3f, .22f), Quaternion.identity);
            // Molten seams on the front and sides.
            for (var i = 0; i < 5; i++)
            {
                var y = 1.2f + i * 1.15f;
                glow.Add(BoxMesh, new Vector3((i % 2 == 0 ? -1 : 1) * 1.15f, y, 3.38f),
                    new Vector3(1.5f + (i % 3) * .25f, .09f, .10f), Quaternion.Euler(0, 0, (i % 2 == 0 ? 14 : -12)));
            }
            glow.Add(BoxMesh, new Vector3(4.2f, 6.2f, 1.15f), new Vector3(.10f, 3.2f, .30f), Quaternion.Euler(0, 0, 8));
            glow.Add(BoxMesh, new Vector3(-4.2f, 3.4f, -1.15f), new Vector3(.10f, 2.2f, .26f), Quaternion.identity);
            return new Structure(stone.Build("Obsidian keep"), glow.Build("Obsidian lava"), dark.Build("Obsidian gate"));
        });

        public static Structure FortressWall() => Cached("fortress.wall", () =>
        {
            var stone = new PartBuilder(); var glow = new PartBuilder();
            stone.Add(BoxMesh, new Vector3(0, .25f, 0), new Vector3(8f, .5f, 2.6f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(0, 2.35f, 0), new Vector3(7.4f, 3.6f, 1.8f), Quaternion.identity);
            for (var i = 0; i < 6; i++)
                stone.Add(BoxMesh, new Vector3(-2.9f + i * 1.16f, 4.45f, 0), new Vector3(.72f, .7f, 1.8f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(3.55f, 3.5f, 0), new Vector3(2.2f, 6.2f, 2.2f), Quaternion.identity);
            stone.Add(ConeMesh, new Vector3(3.55f, 8.1f, 0), new Vector3(2.5f, 2.5f, 2.5f), Quaternion.identity);
            for (var i = 0; i < 4; i++)
                stone.Add(ConeMesh, new Vector3(-3.6f + i * .55f, 3.3f + (i % 2) * .55f, -1.05f),
                    new Vector3(.55f, 1.8f + (i % 2) * .7f, .55f), Quaternion.Euler(0, i * 37f, (i % 2 == 0 ? -8 : 9)));
            for (var i = 0; i < 3; i++)
                glow.Add(BoxMesh, new Vector3(-2.2f + i * 1.6f, .65f + i * .42f, 1.32f),
                    new Vector3(1.2f, .08f, .08f), Quaternion.Euler(0, 0, -9 + i * 7));
            return new Structure(stone.Build("Obsidian wall"), glow.Build("Obsidian wall lava"));
        });

        public static Structure BasaltSpire() => Cached("basalt.spire", () =>
        {
            var stone = new PartBuilder(); var glow = new PartBuilder();
            stone.Add(BoxMesh, new Vector3(0, .2f, 0), new Vector3(3.8f, .4f, 3.8f), Quaternion.identity);
            for (var i = 0; i < 3; i++)
            {
                var a = i * 2.1f;
                stone.Add(ConeMesh, new Vector3(Mathf.Cos(a) * .75f, .55f + i * .3f, Mathf.Sin(a) * .75f),
                    new Vector3(1.4f - i * .18f, 5.4f + i * 1.3f, 1.4f - i * .18f),
                    Quaternion.Euler(0, a * Mathf.Rad2Deg, (i - 1) * 7));
            }
            stone.Add(BoxMesh, new Vector3(0, 1.2f, 0), new Vector3(2.6f, 2.1f, 2.6f), Quaternion.Euler(0, 23, 0));
            glow.Add(BoxMesh, new Vector3(0, .38f, 1.45f), new Vector3(1.6f, .08f, .10f), Quaternion.identity);
            glow.Add(BoxMesh, new Vector3(.7f, 1.1f, 1.2f), new Vector3(.08f, 1.4f, .08f), Quaternion.Euler(0, 0, 11));
            return new Structure(stone.Build("Basalt spire"), glow.Build("Basalt ember"));
        });

        public static Structure Ziggurat() => Cached("ancient.ziggurat", () =>
        {
            var stone = new PartBuilder(); var gold = new PartBuilder(); var dark = new PartBuilder();
            var widths = new[] { 11f, 9.2f, 7.5f, 5.9f, 4.4f, 3.1f };
            for (var i = 0; i < widths.Length; i++)
            {
                var y = .65f + i * 1.45f;
                stone.Add(BoxMesh, new Vector3(0, y, 0), new Vector3(widths[i], 1.45f, widths[i] * .82f), Quaternion.identity);
                gold.Add(BoxMesh, new Vector3(0, y + .78f, 0), new Vector3(widths[i] + .12f, .10f, widths[i] * .82f + .12f), Quaternion.identity);
            }
            // Front stair: broad treads rising through the terraces.
            for (var i = 0; i < 7; i++)
            {
                var y = .85f + i * 1.18f;
                stone.Add(BoxMesh, new Vector3(0, y, 4.4f - i * .43f), new Vector3(2.1f, .42f, 1.15f), Quaternion.identity);
            }
            stone.Add(BoxMesh, new Vector3(0, 9.35f, 0), new Vector3(2.8f, 1.7f, 2.4f), Quaternion.identity);
            gold.Add(BoxMesh, new Vector3(0, 10.3f, 0), new Vector3(3.0f, .14f, 2.6f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(0, 10.75f, 0), new Vector3(1.5f, 1.0f, 1.4f), Quaternion.identity);
            dark.Add(BoxMesh, new Vector3(0, 9.35f, 1.25f), new Vector3(1.0f, 1.45f, .18f), Quaternion.identity);
            for (var i = 0; i < 2; i++)
            {
                var side = i == 0 ? -1 : 1;
                stone.Add(BoxMesh, new Vector3(side * 6.2f, 3.2f, 2.4f), new Vector3(.72f, 6.2f, .72f), Quaternion.identity);
                stone.Add(ConeMesh, new Vector3(side * 6.2f, 6.55f, 2.4f), new Vector3(.9f, 1.1f, .9f), Quaternion.identity);
                gold.Add(BoxMesh, new Vector3(side * 6.2f, 4.4f, 2.4f), new Vector3(.82f, .12f, .82f), Quaternion.identity);
            }
            return new Structure(stone.Build("Ancient ziggurat"), gold.Build("Ancient gold"), dark.Build("Ancient door"));
        });

        public static Structure Obelisk() => Cached("ancient.obelisk", () =>
        {
            var stone = new PartBuilder(); var gold = new PartBuilder();
            stone.Add(BoxMesh, new Vector3(0, .35f, 0), new Vector3(2.2f, .7f, 2.2f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(0, 4.2f, 0), new Vector3(1.2f, 7.2f, 1.2f), Quaternion.identity);
            stone.Add(ConeMesh, new Vector3(0, 8.15f, 0), new Vector3(1.15f, 1.3f, 1.15f), Quaternion.identity);
            gold.Add(BoxMesh, new Vector3(0, 7.72f, 0), new Vector3(1.35f, .14f, 1.35f), Quaternion.identity);
            gold.Add(BoxMesh, new Vector3(0, 3.1f, 0), new Vector3(1.32f, .10f, 1.32f), Quaternion.Euler(0, 0, 7));
            return new Structure(stone.Build("Ancient obelisk"), gold.Build("Obelisk gold"));
        });

        public static Structure BrokenColonnade() => Cached("ancient.colonnade", () =>
        {
            var stone = new PartBuilder(); var gold = new PartBuilder();
            stone.Add(BoxMesh, new Vector3(0, .22f, 0), new Vector3(7.4f, .44f, 2.8f), Quaternion.identity);
            var heights = new[] { 5.2f, 3.2f, 4.3f };
            for (var i = 0; i < 3; i++)
            {
                var x = -2.5f + i * 2.5f;
                Column(stone, gold, new Vector3(x, .55f, 0), heights[i], .46f);
                stone.Add(BoxMesh, new Vector3(x, .55f + heights[i] + .1f, 0), new Vector3(1.2f, .25f, 1.2f), Quaternion.identity);
            }
            stone.Add(BoxMesh, new Vector3(-1.2f, 6.3f, 0), new Vector3(3.8f, .5f, 2.1f), Quaternion.Euler(0, 0, -5));
            stone.Add(BoxMesh, new Vector3(1.8f, 5.2f, .1f), new Vector3(2.4f, .42f, 2.0f), Quaternion.Euler(0, 0, 8));
            return new Structure(stone.Build("Ancient colonnade"), gold.Build("Colonnade gold"));
        });

        public static Structure SandstoneArch() => Cached("ancient.arch", () =>
        {
            var stone = new PartBuilder(); var gold = new PartBuilder();
            Arch(stone, gold, 3.8f, .62f, 1.4f, 0f);
            stone.Add(BoxMesh, new Vector3(2.1f, 5.0f, 0), new Vector3(.8f, .9f, 1.4f), Quaternion.Euler(0, 0, 18));
            return new Structure(stone.Build("Sandstone arch"), gold.Build("Sandstone arch trim"));
        });

        public static Structure NeonTowerA() => Cached("neon.tower.a", () =>
        {
            var stone = new PartBuilder(); var glow = new PartBuilder(); var dark = new PartBuilder();
            stone.Add(BoxMesh, new Vector3(0, 5.2f, 0), new Vector3(2.6f, 10.4f, 2.6f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(1.8f, 3.4f, .4f), new Vector3(1.8f, 6.8f, 1.8f), Quaternion.identity);
            stone.Add(CylinderMesh, new Vector3(0, 10.8f, 0), new Vector3(.16f, 2.4f, .16f), Quaternion.identity);
            dark.Add(BoxMesh, new Vector3(0, 5.2f, 1.33f), new Vector3(2.8f, 8.8f, .10f), Quaternion.identity);
            for (var i = 0; i < 7; i++)
            {
                var y = 1.4f + i * 1.42f;
                glow.Add(BoxMesh, new Vector3(0, y, 1.38f), new Vector3(2.7f, .10f, .08f), Quaternion.identity);
                glow.Add(BoxMesh, new Vector3(0, y + .36f, 1.38f), new Vector3(1.35f, .07f, .08f), Quaternion.identity);
            }
            glow.Add(BoxMesh, new Vector3(0, 10.2f, 1.36f), new Vector3(.16f, 2.1f, .08f), Quaternion.identity);
            glow.Add(BoxMesh, new Vector3(1.8f, 5.7f, 1.34f), new Vector3(1.9f, .08f, .08f), Quaternion.identity);
            glow.Add(BoxMesh, new Vector3(1.8f, 2.9f, 1.34f), new Vector3(1.9f, .08f, .08f), Quaternion.identity);
            return new Structure(stone.Build("Neon tower A"), glow.Build("Neon tower A light"), dark.Build("Neon tower A glass"));
        });

        public static Structure NeonTowerB() => Cached("neon.tower.b", () =>
        {
            var stone = new PartBuilder(); var glow = new PartBuilder(); var dark = new PartBuilder();
            stone.Add(BoxMesh, new Vector3(0, 1.9f, 0), new Vector3(3.4f, 3.8f, 3.4f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(0, 7.0f, 0), new Vector3(2.1f, 6.4f, 2.1f), Quaternion.identity);
            stone.Add(CylinderMesh, new Vector3(0, 10.6f, 0), new Vector3(.12f, 1.6f, .12f), Quaternion.identity);
            dark.Add(BoxMesh, new Vector3(0, 7.0f, 1.08f), new Vector3(2.3f, 5.6f, .08f), Quaternion.identity);
            for (var i = 0; i < 5; i++)
            {
                var y = 4.25f + i * 1.15f;
                glow.Add(BoxMesh, new Vector3(0, y, 1.13f), new Vector3(2.2f, .08f, .07f), Quaternion.identity);
                glow.Add(BoxMesh, new Vector3(0, y, -1.13f), new Vector3(2.2f, .08f, .07f), Quaternion.identity);
            }
            glow.Add(BoxMesh, new Vector3(1.72f, 1.9f, 0), new Vector3(.08f, 3.5f, .08f), Quaternion.identity);
            glow.Add(BoxMesh, new Vector3(-1.72f, 1.9f, 0), new Vector3(.08f, 3.5f, .08f), Quaternion.identity);
            return new Structure(stone.Build("Neon tower B"), glow.Build("Neon tower B light"), dark.Build("Neon tower B glass"));
        });

        public static Structure NeonArch() => Cached("neon.arch", () =>
        {
            var stone = new PartBuilder(); var glow = new PartBuilder(); var dark = new PartBuilder();
            stone.Add(BoxMesh, new Vector3(-2.5f, 3.2f, 0), new Vector3(.8f, 6.4f, 1.2f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(2.5f, 3.2f, 0), new Vector3(.8f, 6.4f, 1.2f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(0, 6.65f, 0), new Vector3(5.8f, .75f, 1.2f), Quaternion.identity);
            dark.Add(BoxMesh, new Vector3(0, 3.05f, .64f), new Vector3(4.2f, 5.9f, .08f), Quaternion.identity);
            for (var i = 0; i < 2; i++)
            {
                var x = i == 0 ? -2.5f : 2.5f;
                glow.Add(BoxMesh, new Vector3(x, 3.2f, .68f), new Vector3(.10f, 5.8f, .06f), Quaternion.identity);
            }
            glow.Add(BoxMesh, new Vector3(0, 6.65f, .68f), new Vector3(5.6f, .10f, .06f), Quaternion.identity);
            return new Structure(stone.Build("Neon arch"), glow.Build("Neon arch light"), dark.Build("Neon arch void"));
        });

        public static Structure NeonBillboard() => Cached("neon.billboard", () =>
        {
            var stone = new PartBuilder(); var glow = new PartBuilder(); var dark = new PartBuilder();
            stone.Add(BoxMesh, new Vector3(-3.2f, 3.1f, 0), new Vector3(.5f, 6.2f, .5f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(3.2f, 3.1f, 0), new Vector3(.5f, 6.2f, .5f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(0, 6.1f, 0), new Vector3(7.2f, .55f, .55f), Quaternion.identity);
            dark.Add(BoxMesh, new Vector3(0, 3.7f, 0), new Vector3(6.2f, 4.2f, .16f), Quaternion.identity);
            for (var i = 0; i < 4; i++)
            {
                var y = 2.2f + i * 1.25f;
                glow.Add(BoxMesh, new Vector3(0, y, .12f), new Vector3(5.7f, .08f, .08f), Quaternion.identity);
            }
            glow.Add(BoxMesh, new Vector3(-3.05f, 3.7f, .12f), new Vector3(.08f, 4.0f, .08f), Quaternion.identity);
            glow.Add(BoxMesh, new Vector3(3.05f, 3.7f, .12f), new Vector3(.08f, 4.0f, .08f), Quaternion.identity);
            return new Structure(stone.Build("Neon billboard"), glow.Build("Neon billboard light"), dark.Build("Neon billboard panel"));
        });

        public static Structure SpaceMonolith() => Cached("space.monolith", () =>
        {
            var stone = new PartBuilder(); var glow = new PartBuilder(); var dark = new PartBuilder();
            stone.Add(BoxMesh, new Vector3(0, .25f, 0), new Vector3(3.2f, .5f, 3.2f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(0, 4.5f, 0), new Vector3(1.35f, 8.4f, 1.7f), Quaternion.Euler(0, 0, 2));
            stone.Add(BoxMesh, new Vector3(1.5f, 3.2f, .4f), new Vector3(.7f, 5.6f, .7f), Quaternion.Euler(0, 0, -7));
            dark.Add(BoxMesh, new Vector3(0, 4.6f, .87f), new Vector3(.55f, 6.8f, .06f), Quaternion.identity);
            glow.Add(BoxMesh, new Vector3(0, 4.6f, .92f), new Vector3(.10f, 6.9f, .04f), Quaternion.identity);
            glow.Add(BoxMesh, new Vector3(0, 8.95f, 0), new Vector3(1.0f, .08f, 1.1f), Quaternion.identity);
            stone.Add(CrystalMesh, new Vector3(-1.6f, 1.9f, .8f), new Vector3(.5f, 2.6f, .5f), Quaternion.Euler(0, 22, -18));
            glow.Add(CrystalMesh, new Vector3(-1.6f, 2.7f, .8f), new Vector3(.22f, 1.0f, .22f), Quaternion.Euler(0, 22, -18));
            return new Structure(stone.Build("Space monolith"), glow.Build("Space monolith light"), dark.Build("Space monolith void"));
        });

        public static Structure CrystalCluster() => Cached("space.crystals", () =>
        {
            var stone = new PartBuilder(); var glow = new PartBuilder();
            stone.Add(CrystalMesh, new Vector3(0, 2.1f, 0), new Vector3(1.0f, 4.2f, 1.0f), Quaternion.Euler(0, 15, 3));
            stone.Add(CrystalMesh, new Vector3(1.25f, 1.2f, .5f), new Vector3(.65f, 2.4f, .65f), Quaternion.Euler(0, -22, -17));
            stone.Add(CrystalMesh, new Vector3(-1.15f, 1.4f, -.3f), new Vector3(.55f, 2.8f, .55f), Quaternion.Euler(0, 38, 14));
            glow.Add(CrystalMesh, new Vector3(0, 4.15f, 0), new Vector3(.35f, 1.1f, .35f), Quaternion.Euler(0, 15, 3));
            glow.Add(CrystalMesh, new Vector3(1.25f, 2.35f, .5f), new Vector3(.24f, .75f, .24f), Quaternion.Euler(0, -22, -17));
            return new Structure(stone.Build("Space crystals"), glow.Build("Space crystal light"));
        });

        public static Structure Asteroid() => Cached("space.asteroid", () =>
        {
            var stone = new PartBuilder(); var glow = new PartBuilder();
            stone.Add(SphereMesh, new Vector3(0, 1.6f, 0), new Vector3(5.2f, 3.8f, 4.6f), Quaternion.Euler(0, 0, 12));
            stone.Add(SphereMesh, new Vector3(1.8f, 2.8f, .6f), new Vector3(2.4f, 2.0f, 2.1f), Quaternion.Euler(0, 0, -20));
            stone.Add(CrystalMesh, new Vector3(-1.3f, 3.0f, .4f), new Vector3(.55f, 2.2f, .55f), Quaternion.Euler(0, 28, -10));
            glow.Add(CrystalMesh, new Vector3(-1.3f, 3.6f, .4f), new Vector3(.22f, .8f, .22f), Quaternion.Euler(0, 28, -10));
            return new Structure(stone.Build("Space asteroid"), glow.Build("Space asteroid light"));
        });

        public static Structure Satellite() => Cached("space.satellite", () =>
        {
            var stone = new PartBuilder(); var glow = new PartBuilder();
            stone.Add(BoxMesh, new Vector3(0, 2.7f, 0), new Vector3(1.0f, 1.8f, 1.0f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(-2.0f, 2.7f, 0), new Vector3(2.0f, 1.4f, .18f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(2.0f, 2.7f, 0), new Vector3(2.0f, 1.4f, .18f), Quaternion.identity);
            glow.Add(BoxMesh, new Vector3(0, 3.65f, 0), new Vector3(.22f, .55f, .22f), Quaternion.identity);
            glow.Add(BoxMesh, new Vector3(-2.0f, 2.7f, .12f), new Vector3(1.7f, .10f, .06f), Quaternion.identity);
            glow.Add(BoxMesh, new Vector3(2.0f, 2.7f, .12f), new Vector3(1.7f, .10f, .06f), Quaternion.identity);
            return new Structure(stone.Build("Space satellite"), glow.Build("Space satellite light"));
        });

        public static Structure SpacePlanet() => Cached("space.planet", () =>
        {
            var stone = new PartBuilder();
            stone.Add(SphereMesh, new Vector3(0, 0, 0), new Vector3(7.2f, 7.2f, 7.2f), Quaternion.identity);
            stone.Add(SphereMesh, new Vector3(-2.4f, 1.2f, 1.1f), new Vector3(1.0f, .75f, .9f), Quaternion.identity);
            stone.Add(SphereMesh, new Vector3(1.7f, -.8f, 1.5f), new Vector3(.7f, .55f, .65f), Quaternion.identity);
            return new Structure(stone.Build("Space planet"));
        });

        // --- Hero foundation -------------------------------------------------------
        // The tower stands on a floating terrace instead of the cloud sea. Each
        // destination dresses the rim differently, which is the first thing the
        // player reads when a run starts.
        public static Structure Foundation(ThemeId id) => Cached("foundation." + id, () =>
        {
            var stone = new PartBuilder(); var accent = new PartBuilder(); var foliage = new PartBuilder();

            // Three descending terraces: a stepped sculpture from the gameplay camera,
            // not a flat plate. The tower always lands on the top surface at y = 0.
            stone.Add(CylinderMesh, new Vector3(0, -.30f, 0), new Vector3(10.4f, .60f, 10.4f), Quaternion.identity);
            stone.Add(CylinderMesh, new Vector3(0, -1.00f, 0), new Vector3(13.2f, .80f, 13.2f), Quaternion.identity);
            stone.Add(CylinderMesh, new Vector3(0, -1.95f, 0), new Vector3(15.6f, 1.10f, 15.6f), Quaternion.identity);
            // Quarried lip under the widest terrace.
            stone.Add(CylinderMesh, new Vector3(0, -2.75f, 0), new Vector3(16.2f, .50f, 16.2f), Quaternion.identity);
            // Underside: a slender inverted cone plus two broken rock masses, so the
            // pedestal reads as torn from a mountain rather than resting on a plate.
            stone.Add(ConeMesh, new Vector3(0, -5.30f, 0), new Vector3(14.4f, 4.6f, 14.4f), Quaternion.Euler(180, 0, 0));
            stone.Add(SphereMesh, new Vector3(2.2f, -4.2f, -1.6f), new Vector3(4.0f, 2.9f, 3.7f), Quaternion.identity);
            stone.Add(SphereMesh, new Vector3(-2.0f, -3.4f, 1.7f), new Vector3(3.2f, 2.2f, 2.9f), Quaternion.identity);

            // Edge lines, not discs: additive geometry only along the terrace rims.
            accent.Add(TrimMesh, new Vector3(0, .015f, 0), new Vector3(5.25f, 1f, 5.25f), Quaternion.identity);
            accent.Add(TrimMesh, new Vector3(0, -.60f, 0), new Vector3(6.65f, 1f, 6.65f), Quaternion.identity);
            accent.Add(TrimMesh, new Vector3(0, -1.40f, 0), new Vector3(7.85f, 1f, 7.85f), Quaternion.identity);

            // Manifestation helpers. Tall elements stand on the terrace surface; thin
            // accents are always slim bars, so nothing ever reads as a paper card.
            void Around(PartBuilder builder, Mesh mesh, int count, float radius, float y, Vector3 scale, float phase)
            {
                for (var i = 0; i < count; i++)
                {
                    var angle = i * Mathf.PI * 2f / count + phase;
                    builder.Add(mesh, new Vector3(Mathf.Sin(angle) * radius, y, Mathf.Cos(angle) * radius), scale,
                        Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0));
                }
            }
            void Shrubs(float radius, int count, float phase, float size)
            {
                for (var i = 0; i < count; i++)
                {
                    var angle = i * Mathf.PI * 2f / count + phase;
                    foliage.Add(SphereMesh, new Vector3(Mathf.Sin(angle) * radius, -.60f + size * .38f, Mathf.Cos(angle) * radius),
                        new Vector3(size, size * .78f, size), Quaternion.identity);
                }
            }

            switch (id)
            {
                case ThemeId.Obsidian:
                    Around(stone, ConeMesh, 5, 7.1f, .50f, new Vector3(1.5f, 3.6f, 1.5f), .2f);
                    Around(stone, ConeMesh, 5, 5.7f, .40f, new Vector3(.95f, 1.9f, .95f), .55f);
                    Around(accent, BoxMesh, 10, 7.55f, -1.10f, new Vector3(.09f, .60f, .09f), 0f);
                    break;
                case ThemeId.AncientStone:
                    Around(stone, BoxMesh, 6, 7.0f, .10f, new Vector3(1.3f, 3.0f, 1.3f), .5f);
                    Around(stone, BoxMesh, 6, 7.0f, 1.73f, new Vector3(1.8f, .26f, 1.7f), .5f);
                    Shrubs(5.6f, 6, 0f, 1.05f);
                    break;
                case ThemeId.Neon:
                    Around(stone, BoxMesh, 10, 7.1f, -.50f, new Vector3(1.0f, 1.8f, 1.0f), .4f);
                    Around(accent, BoxMesh, 10, 7.6f, -.30f, new Vector3(.10f, 2.2f, .10f), .4f);
                    Around(accent, BoxMesh, 6, 5.7f, -.60f, new Vector3(1.2f, .07f, .07f), .4f);
                    break;
                case ThemeId.Space:
                    Around(stone, CrystalMesh, 6, 7.0f, .20f, new Vector3(1.0f, 3.0f, 1.0f), .3f);
                    Around(accent, BoxMesh, 10, 7.6f, -.90f, new Vector3(.09f, .9f, .09f), 0f);
                    break;
                default:
                    // A broken parapet, not a ring of posts: stone segments with a cap
                    // read as architecture from the gameplay camera and sit below the
                    // first slab so they never compete with the tower silhouette.
                    for (var i = 0; i < 12; i++)
                    {
                        var angle = i * Mathf.PI * 2f / 12f + .26f;
                        var x = Mathf.Sin(angle) * 7.0f;
                        var z = Mathf.Cos(angle) * 7.0f;
                        var yaw = Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0);
                        stone.Add(BoxMesh, new Vector3(x, -.28f, z), new Vector3(1.6f, .78f, .34f), yaw);
                        stone.Add(BoxMesh, new Vector3(x, .16f, z), new Vector3(1.78f, .14f, .46f), yaw);
                    }
                    Shrubs(6.1f, 5, .4f, .80f);
                    break;
            }
            return new Structure(stone.Build("Destination foundation"), accent.Build("Foundation trim"), foliage.Build("Foundation garden"));
        });

        // --- Floating islands ------------------------------------------------------
        // All islands share one chiselled rock silhouette; the top dressing, glow and
        // tint carry the destination. Variants vary the surface so repeats never read
        // as the same asset stamped eight times.

        public static Structure PalaceGarden(int variant = 0) => Cached("palace.garden." + variant, () =>
        {
            var stone = new PartBuilder(); var foliage = new PartBuilder();
            Rock(stone, 1.15f, variant);
            Garden(foliage, 1.28f, variant);
            return new Structure(stone.Build("Palace garden island"), foliage.Build("Palace garden foliage"));
        });

        public static Structure SandstoneGarden(int variant = 0) => Cached("ancient.garden." + variant, () =>
        {
            var stone = new PartBuilder(); var foliage = new PartBuilder();
            Rock(stone, 1.1f, variant);
            stone.Add(BoxMesh, new Vector3(-1.6f, 1.2f, 0), new Vector3(.7f, 2.6f, .7f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(-1.6f, 2.55f, 0), new Vector3(1.0f, .22f, 1.0f), Quaternion.identity);
            Garden(foliage, 1.05f, variant + 2);
            return new Structure(stone.Build("Ancient garden island"), foliage.Build("Ancient garden foliage"));
        });

        public static Structure ObsidianIsland(int variant = 0) => Cached("obsidian.island." + variant, () =>
        {
            var stone = new PartBuilder(); var glow = new PartBuilder();
            Rock(stone, 1.1f, variant);
            stone.Add(ConeMesh, new Vector3(0, 2.2f, 0), new Vector3(1.3f, 2.5f, 1.3f), Quaternion.identity);
            glow.Add(BoxMesh, new Vector3(0, 1.05f, 1.15f), new Vector3(2.2f, .09f, .10f), Quaternion.identity);
            glow.Add(BoxMesh, new Vector3(1.0f, 1.05f, .4f), new Vector3(.10f, .09f, 1.8f), Quaternion.identity);
            glow.Add(BoxMesh, new Vector3(-.9f, .72f, -.8f), new Vector3(1.4f, .08f, .08f), Quaternion.Euler(0, 40, 0));
            return new Structure(stone.Build("Obsidian island"), glow.Build("Obsidian island ember"));
        });

        public static Structure NeonIsland(int variant = 0) => Cached("neon.island." + variant, () =>
        {
            var stone = new PartBuilder(); var glow = new PartBuilder();
            stone.Add(BoxMesh, new Vector3(0, 0, 0), new Vector3(5.8f, .7f, 5.8f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(0, -1.0f, 0), new Vector3(3.0f, 1.4f, 3.0f), Quaternion.identity);
            glow.Add(BoxMesh, new Vector3(0, .40f, 2.92f), new Vector3(5.6f, .08f, .08f), Quaternion.identity);
            glow.Add(BoxMesh, new Vector3(0, .40f, -2.92f), new Vector3(5.6f, .08f, .08f), Quaternion.identity);
            glow.Add(BoxMesh, new Vector3(2.92f, .40f, 0), new Vector3(.08f, .08f, 5.6f), Quaternion.identity);
            glow.Add(BoxMesh, new Vector3(-2.92f, .40f, 0), new Vector3(.08f, .08f, 5.6f), Quaternion.identity);
            if (variant % 2 == 1)
                glow.Add(BoxMesh, new Vector3(0, 1.9f, 0), new Vector3(.10f, 2.6f, .10f), Quaternion.identity);
            return new Structure(stone.Build("Neon island"), glow.Build("Neon island edge"));
        });

        public static Structure SpaceIsland(int variant = 0) => Cached("space.island." + variant, () =>
        {
            var stone = new PartBuilder(); var glow = new PartBuilder();
            Rock(stone, 1.0f, variant);
            stone.Add(CrystalMesh, new Vector3(0, 2.1f, 0), new Vector3(.8f, 3.1f, .8f), Quaternion.Euler(0, 24, 8));
            stone.Add(CrystalMesh, new Vector3(1.0f, 1.45f, .5f), new Vector3(.42f, 1.7f, .42f), Quaternion.Euler(0, -28, -15));
            glow.Add(CrystalMesh, new Vector3(0, 3.15f, 0), new Vector3(.28f, .9f, .28f), Quaternion.Euler(0, 24, 8));
            return new Structure(stone.Build("Space island"), glow.Build("Space island light"));
        });

        // A chiselled floating rock: domed top, tapered underside and a couple of
        // broken facets. Unit-ish size, so the district scales it per slot.
        private static void Rock(PartBuilder builder, float s, int variant)
        {
            var yaw = variant * 37f;
            builder.Add(SphereMesh, new Vector3(0, 0, 0), new Vector3(s * 1.30f, s * 1.02f, s * 1.22f), Quaternion.Euler(0, yaw, 0));
            builder.Add(ConeMesh, new Vector3(0, -s * .92f, 0), new Vector3(s * 1.02f, s * 1.55f, s * .98f), Quaternion.Euler(180, yaw, 0));
            builder.Add(BoxMesh, new Vector3(s * .26f, -s * .30f, -s * .16f), new Vector3(s * .82f, s * .50f, s * .78f),
                Quaternion.Euler(0, yaw + 21f, 8));
            builder.Add(SphereMesh, new Vector3(-s * .52f, -s * .10f, s * .30f), new Vector3(s * .52f, s * .42f, s * .48f),
                Quaternion.Euler(0, yaw - 14f, 0));
        }

        private static void Garden(PartBuilder foliage, float s, int variant)
        {
            for (var i = 0; i < 4; i++)
            {
                var angle = i * 1.7f + variant * .63f;
                var radius = s * (.32f + (i % 3) * .24f);
                var height = s * (.55f + (i % 2) * .34f);
                var width = s * (.14f + (i % 2) * .04f);
                foliage.Add(ConeMesh, new Vector3(Mathf.Cos(angle) * radius, s * .62f + height * .5f, Mathf.Sin(angle) * radius),
                    new Vector3(width, height, width), Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0));
            }
            for (var i = 0; i < 3; i++)
            {
                var angle = i * 2.1f + variant * 1.1f;
                var radius = s * (.46f + (i % 2) * .26f);
                var size = s * (.20f + (i % 2) * .06f);
                foliage.Add(SphereMesh, new Vector3(Mathf.Cos(angle) * radius, s * .62f + size * .45f, Mathf.Sin(angle) * radius),
                    new Vector3(size, size * .78f, size), Quaternion.identity);
            }
        }

        // --- Shared helpers --------------------------------------------------------

        private static void Column(PartBuilder stone, PartBuilder accent, Vector3 basePosition, float height, float width)
        {
            stone.Add(BoxMesh, basePosition + Vector3.up * .18f, new Vector3(width * 1.8f, .36f, width * 1.8f), Quaternion.identity);
            stone.Add(CylinderMesh, basePosition + Vector3.up * (height * .5f + .36f), new Vector3(width, height, width), Quaternion.identity);
            stone.Add(BoxMesh, basePosition + Vector3.up * (height + .44f), new Vector3(width * 1.8f, .20f, width * 1.8f), Quaternion.identity);
            if (accent != null)
                accent.Add(BoxMesh, basePosition + Vector3.up * (height + .58f), new Vector3(width * 1.9f, .08f, width * 1.9f), Quaternion.identity);
        }

        // A semicircular arcade: columns + radial voussoirs. The opening faces +Z.
        private static void Arch(PartBuilder stone, PartBuilder accent, float radius, float block, float depth, float springHeight)
        {
            var centre = new Vector3(0, springHeight + radius * .55f, 0);
            const int segments = 11;
            for (var i = 0; i <= segments; i++)
            {
                var angle = Mathf.PI * i / segments;
                var position = centre + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                stone.Add(BoxMesh, position, new Vector3(block * 1.65f, Mathf.PI * radius / segments * 1.05f, depth),
                    Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg - 90f));
            }
            Column(stone, accent, new Vector3(-radius, 0, 0), springHeight + radius * .55f, block);
            Column(stone, accent, new Vector3(radius, 0, 0), springHeight + radius * .55f, block);
            if (accent != null)
                accent.Add(BoxMesh, centre + new Vector3(0, radius + block * .55f, 0), new Vector3(block * 2.2f, block * .8f, depth * 1.1f), Quaternion.identity);
            stone.Add(BoxMesh, new Vector3(0, .18f, 0), new Vector3(radius * 2.6f, .36f, depth * 1.6f), Quaternion.identity);
        }

        public static Mesh CachedMesh(string key, Func<Mesh> build)
        {
            if (CachedMeshes.TryGetValue(key, out var mesh)) return mesh;
            mesh = build();
            CachedMeshes[key] = mesh;
            return mesh;
        }

        private static Structure Cached(string key, Func<Structure> build)
        {
            if (CachedStructures.TryGetValue(key, out var structure)) return structure;
            structure = build();
            CachedStructures[key] = structure;
            return structure;
        }

        private static void AddQuad(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> triangles,
            Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal)
        {
            var index = vertices.Count;
            vertices.Add(a); vertices.Add(b); vertices.Add(c); vertices.Add(d);
            normals.Add(normal); normals.Add(normal); normals.Add(normal); normals.Add(normal);
            uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(1, 0)); uvs.Add(new Vector2(1, 1)); uvs.Add(new Vector2(0, 1));
            triangles.Add(index); triangles.Add(index + 1); triangles.Add(index + 2);
            triangles.Add(index); triangles.Add(index + 2); triangles.Add(index + 3);
        }

        private static void AddTriangle(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> triangles,
            Vector3 a, Vector3 b, Vector3 c)
        {
            var normal = Vector3.Cross(b - a, c - a).normalized;
            var index = vertices.Count;
            vertices.Add(a); vertices.Add(b); vertices.Add(c);
            normals.Add(normal); normals.Add(normal); normals.Add(normal);
            uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(1, 0)); uvs.Add(new Vector2(.5f, 1));
            triangles.Add(index); triangles.Add(index + 1); triangles.Add(index + 2);
        }

        private static Mesh Finish(Mesh mesh) { mesh.RecalculateBounds(); return mesh; }

        private static Mesh BuildBox()
        {
            var vertices = new List<Vector3>(); var normals = new List<Vector3>(); var uvs = new List<Vector2>(); var triangles = new List<int>();
            AddQuad(vertices, normals, uvs, triangles, new Vector3(-.5f, -.5f, .5f), new Vector3(.5f, -.5f, .5f), new Vector3(.5f, .5f, .5f), new Vector3(-.5f, .5f, .5f), Vector3.forward);
            AddQuad(vertices, normals, uvs, triangles, new Vector3(.5f, -.5f, -.5f), new Vector3(-.5f, -.5f, -.5f), new Vector3(-.5f, .5f, -.5f), new Vector3(.5f, .5f, -.5f), Vector3.back);
            AddQuad(vertices, normals, uvs, triangles, new Vector3(-.5f, .5f, .5f), new Vector3(.5f, .5f, .5f), new Vector3(.5f, .5f, -.5f), new Vector3(-.5f, .5f, -.5f), Vector3.up);
            AddQuad(vertices, normals, uvs, triangles, new Vector3(-.5f, -.5f, -.5f), new Vector3(.5f, -.5f, -.5f), new Vector3(.5f, -.5f, .5f), new Vector3(-.5f, -.5f, .5f), Vector3.down);
            AddQuad(vertices, normals, uvs, triangles, new Vector3(.5f, -.5f, .5f), new Vector3(.5f, -.5f, -.5f), new Vector3(.5f, .5f, -.5f), new Vector3(.5f, .5f, .5f), Vector3.right);
            AddQuad(vertices, normals, uvs, triangles, new Vector3(-.5f, -.5f, -.5f), new Vector3(-.5f, -.5f, .5f), new Vector3(-.5f, .5f, .5f), new Vector3(-.5f, .5f, -.5f), Vector3.left);
            var mesh = new Mesh { name = "World box" };
            mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uvs); mesh.SetTriangles(triangles, 0);
            return Finish(mesh);
        }

        private static Mesh BuildCylinder(int segments)
        {
            var vertices = new List<Vector3>(); var normals = new List<Vector3>(); var uvs = new List<Vector2>(); var triangles = new List<int>();
            for (var i = 0; i < segments; i++)
            {
                var a0 = i / (float)segments * Mathf.PI * 2f;
                var a1 = (i + 1) / (float)segments * Mathf.PI * 2f;
                var b0 = new Vector3(Mathf.Cos(a0) * .5f, -.5f, Mathf.Sin(a0) * .5f);
                var b1 = new Vector3(Mathf.Cos(a1) * .5f, -.5f, Mathf.Sin(a1) * .5f);
                var t0 = b0 + Vector3.up; var t1 = b1 + Vector3.up;
                var index = vertices.Count;
                vertices.Add(b0); vertices.Add(t0); vertices.Add(t1); vertices.Add(b1);
                var n0 = new Vector3(Mathf.Cos(a0), 0, Mathf.Sin(a0)); var n1 = new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1));
                normals.Add(n0); normals.Add(n0); normals.Add(n1); normals.Add(n1);
                uvs.Add(new Vector2(i / (float)segments, 0)); uvs.Add(new Vector2(i / (float)segments, 1));
                uvs.Add(new Vector2((i + 1) / (float)segments, 1)); uvs.Add(new Vector2((i + 1) / (float)segments, 0));
                triangles.Add(index); triangles.Add(index + 1); triangles.Add(index + 2);
                triangles.Add(index); triangles.Add(index + 2); triangles.Add(index + 3);
            }
            var topCenter = vertices.Count; vertices.Add(new Vector3(0, .5f, 0)); normals.Add(Vector3.up); uvs.Add(new Vector2(.5f, .5f));
            for (var i = 0; i < segments; i++)
            {
                var a0 = i / (float)segments * Mathf.PI * 2f;
                var a1 = (i + 1) / (float)segments * Mathf.PI * 2f;
                var p0 = vertices.Count; vertices.Add(new Vector3(Mathf.Cos(a0) * .5f, .5f, Mathf.Sin(a0) * .5f)); normals.Add(Vector3.up); uvs.Add(new Vector2(.5f + Mathf.Cos(a0) * .5f, .5f + Mathf.Sin(a0) * .5f));
                var p1 = vertices.Count; vertices.Add(new Vector3(Mathf.Cos(a1) * .5f, .5f, Mathf.Sin(a1) * .5f)); normals.Add(Vector3.up); uvs.Add(new Vector2(.5f + Mathf.Cos(a1) * .5f, .5f + Mathf.Sin(a1) * .5f));
                triangles.Add(topCenter); triangles.Add(p1); triangles.Add(p0);
            }
            var bottomCenter = vertices.Count; vertices.Add(new Vector3(0, -.5f, 0)); normals.Add(Vector3.down); uvs.Add(new Vector2(.5f, .5f));
            for (var i = 0; i < segments; i++)
            {
                var a0 = i / (float)segments * Mathf.PI * 2f;
                var a1 = (i + 1) / (float)segments * Mathf.PI * 2f;
                var p0 = vertices.Count; vertices.Add(new Vector3(Mathf.Cos(a0) * .5f, -.5f, Mathf.Sin(a0) * .5f)); normals.Add(Vector3.down); uvs.Add(new Vector2(.5f + Mathf.Cos(a0) * .5f, .5f + Mathf.Sin(a0) * .5f));
                var p1 = vertices.Count; vertices.Add(new Vector3(Mathf.Cos(a1) * .5f, -.5f, Mathf.Sin(a1) * .5f)); normals.Add(Vector3.down); uvs.Add(new Vector2(.5f + Mathf.Cos(a1) * .5f, .5f + Mathf.Sin(a1) * .5f));
                triangles.Add(bottomCenter); triangles.Add(p0); triangles.Add(p1);
            }
            var mesh = new Mesh { name = "World cylinder" };
            mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uvs); mesh.SetTriangles(triangles, 0);
            return Finish(mesh);
        }

        private static Mesh BuildCone(int segments)
        {
            var vertices = new List<Vector3>(); var normals = new List<Vector3>(); var uvs = new List<Vector2>(); var triangles = new List<int>();
            for (var i = 0; i < segments; i++)
            {
                var a0 = i / (float)segments * Mathf.PI * 2f;
                var a1 = (i + 1) / (float)segments * Mathf.PI * 2f;
                var p0 = new Vector3(Mathf.Cos(a0) * .5f, -.5f, Mathf.Sin(a0) * .5f);
                var p1 = new Vector3(Mathf.Cos(a1) * .5f, -.5f, Mathf.Sin(a1) * .5f);
                AddTriangle(vertices, normals, uvs, triangles, p0, new Vector3(0, .5f, 0), p1);
            }
            var center = vertices.Count; vertices.Add(new Vector3(0, -.5f, 0)); normals.Add(Vector3.down); uvs.Add(new Vector2(.5f, .5f));
            for (var i = 0; i < segments; i++)
            {
                var a0 = i / (float)segments * Mathf.PI * 2f;
                var a1 = (i + 1) / (float)segments * Mathf.PI * 2f;
                var p0 = vertices.Count; vertices.Add(new Vector3(Mathf.Cos(a0) * .5f, -.5f, Mathf.Sin(a0) * .5f)); normals.Add(Vector3.down); uvs.Add(Vector2.zero);
                var p1 = vertices.Count; vertices.Add(new Vector3(Mathf.Cos(a1) * .5f, -.5f, Mathf.Sin(a1) * .5f)); normals.Add(Vector3.down); uvs.Add(Vector2.zero);
                triangles.Add(center); triangles.Add(p0); triangles.Add(p1);
            }
            var mesh = new Mesh { name = "World cone" };
            mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uvs); mesh.SetTriangles(triangles, 0);
            return Finish(mesh);
        }

        private static Mesh BuildSphere(int columns, int rows)
        {
            var vertices = new List<Vector3>((columns + 1) * (rows + 1));
            var normals = new List<Vector3>(vertices.Capacity);
            var uvs = new List<Vector2>(vertices.Capacity);
            var triangles = new List<int>(columns * rows * 6);
            for (var r = 0; r <= rows; r++)
            {
                var phi = Mathf.PI * r / rows;
                for (var c = 0; c <= columns; c++)
                {
                    var theta = Mathf.PI * 2f * c / columns;
                    var position = new Vector3(Mathf.Sin(phi) * Mathf.Cos(theta), Mathf.Cos(phi), Mathf.Sin(phi) * Mathf.Sin(theta)) * .5f;
                    vertices.Add(position); normals.Add(position.normalized); uvs.Add(new Vector2(c / (float)columns, 1f - r / (float)rows));
                }
            }
            for (var r = 0; r < rows; r++)
            for (var c = 0; c < columns; c++)
            {
                var i0 = r * (columns + 1) + c; var i1 = i0 + 1; var i2 = i0 + columns + 1; var i3 = i2 + 1;
                triangles.Add(i0); triangles.Add(i1); triangles.Add(i2);
                triangles.Add(i1); triangles.Add(i3); triangles.Add(i2);
            }
            var mesh = new Mesh { name = "World sphere" };
            mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uvs); mesh.SetTriangles(triangles, 0);
            return Finish(mesh);
        }

        private static Mesh BuildQuad()
        {
            var vertices = new List<Vector3>(); var normals = new List<Vector3>(); var uvs = new List<Vector2>(); var triangles = new List<int>();
            AddQuad(vertices, normals, uvs, triangles,
                new Vector3(-.5f, -.5f, 0), new Vector3(.5f, -.5f, 0), new Vector3(.5f, .5f, 0), new Vector3(-.5f, .5f, 0), Vector3.forward);
            var mesh = new Mesh { name = "World quad" };
            mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uvs); mesh.SetTriangles(triangles, 0);
            return Finish(mesh);
        }

        private static Mesh BuildPrism()        {
            var vertices = new List<Vector3>(); var normals = new List<Vector3>(); var uvs = new List<Vector2>(); var triangles = new List<int>();
            var a = new Vector3(-.5f, -.5f, -.5f); var b = new Vector3(.5f, -.5f, -.5f); var c = new Vector3(0, .5f, -.5f);
            var d = new Vector3(-.5f, -.5f, .5f); var e = new Vector3(.5f, -.5f, .5f); var f = new Vector3(0, .5f, .5f);
            AddTriangle(vertices, normals, uvs, triangles, a, c, b);
            AddTriangle(vertices, normals, uvs, triangles, d, e, f);
            AddQuad(vertices, normals, uvs, triangles, a, b, e, d, Vector3.down);
            AddQuad(vertices, normals, uvs, triangles, a, d, f, c, Vector3.left);
            AddQuad(vertices, normals, uvs, triangles, b, c, f, e, Vector3.right);
            var mesh = new Mesh { name = "World prism" };
            mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uvs); mesh.SetTriangles(triangles, 0);
            return Finish(mesh);
        }

        private static Mesh BuildCrystal()
        {
            var vertices = new List<Vector3>(); var normals = new List<Vector3>(); var uvs = new List<Vector2>(); var triangles = new List<int>();
            var top = new Vector3(0, .5f, 0); var bottom = new Vector3(0, -.5f, 0);
            var e0 = new Vector3(.5f, 0, 0); var e1 = new Vector3(0, 0, .5f); var e2 = new Vector3(-.5f, 0, 0); var e3 = new Vector3(0, 0, -.5f);
            AddTriangle(vertices, normals, uvs, triangles, top, e1, e0);
            AddTriangle(vertices, normals, uvs, triangles, top, e2, e1);
            AddTriangle(vertices, normals, uvs, triangles, top, e3, e2);
            AddTriangle(vertices, normals, uvs, triangles, top, e0, e3);
            AddTriangle(vertices, normals, uvs, triangles, bottom, e0, e1);
            AddTriangle(vertices, normals, uvs, triangles, bottom, e1, e2);
            AddTriangle(vertices, normals, uvs, triangles, bottom, e2, e3);
            AddTriangle(vertices, normals, uvs, triangles, bottom, e3, e0);
            var mesh = new Mesh { name = "World crystal" };
            mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uvs); mesh.SetTriangles(triangles, 0);
            return Finish(mesh);
        }

        private static Mesh BuildRing(float inner, float outer, float startDegrees, float endDegrees, int segments)
        {
            var vertices = new List<Vector3>(); var normals = new List<Vector3>(); var uvs = new List<Vector2>(); var triangles = new List<int>();
            var start = startDegrees * Mathf.Deg2Rad; var end = endDegrees * Mathf.Deg2Rad;
            for (var i = 0; i < segments; i++)
            {
                var t0 = i / (float)segments; var t1 = (i + 1) / (float)segments;
                var a0 = Mathf.Lerp(start, end, t0); var a1 = Mathf.Lerp(start, end, t1);
                var i0 = new Vector3(Mathf.Cos(a0) * inner, 0, Mathf.Sin(a0) * inner);
                var i1 = new Vector3(Mathf.Cos(a1) * inner, 0, Mathf.Sin(a1) * inner);
                var o0 = new Vector3(Mathf.Cos(a0) * outer, 0, Mathf.Sin(a0) * outer);
                var o1 = new Vector3(Mathf.Cos(a1) * outer, 0, Mathf.Sin(a1) * outer);
                AddQuad(vertices, normals, uvs, triangles, i0, i1, o1, o0, Vector3.up);
                AddQuad(vertices, normals, uvs, triangles, i0, o0, o1, i1, Vector3.down);
            }
            var mesh = new Mesh { name = "World ring" };
            mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uvs); mesh.SetTriangles(triangles, 0);
            return Finish(mesh);
        }
    }
}
