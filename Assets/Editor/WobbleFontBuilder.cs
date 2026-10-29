#if UNITY_EDITOR
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Wobble.Editor
{
    // Rebuilds the runtime text assets from the two brand families. One static SDF
    // atlas per face keeps first-use rendering free of runtime texture generation;
    // the Turkish block and the punctuation used by the copy deck are baked in, so
    // switching language never falls through to a different typeface mid-screen.
    public static class WobbleFontBuilder
    {
        private const string DisplayPath = "Assets/Resources/Menu/WobbleDisplay.asset";
        private const string BodyPath = "Assets/Resources/Menu/WobbleBody.asset";
        private const string CoveragePath = "Assets/Resources/Menu/WobbleLocalizedSans.asset";

        // Latin, Turkish, the currency and punctuation the decks actually use, plus a
        // small reserve so a future copy tweak cannot ship a blank cell.
        private const string Reserve =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" +
            "ÇĞİÖŞÜçğıöşüÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÑÒÓÔÕÖØÙÚÛÜÝßàáâãäåæçèéêëìíîïñòóôõöøùúûüýÿ" +
            " !\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~" +
            "·×÷–—‘’“”…‹›•€₺°±©®™";

        public static void Build()
        {
            var charset = Charset();
            var body = Create("Assets/Fonts/Inter-SemiBold.ttf", "Wobble Body", 88, 9, charset, true);
            Save(body, BodyPath);
            var display = Create("Assets/Fonts/Nunito-Black.ttf", "Wobble Display", 96, 10, charset, true);
            Save(display, DisplayPath);
            var coverage = Create("Assets/TextMesh Pro/Fonts/LiberationSans.ttf", "Wobble Localized Fallback", 64, 6, charset, false);
            Save(coverage, CoveragePath);

            var coverageAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(CoveragePath);
            var bodyAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BodyPath);
            var displayAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DisplayPath);
            displayAsset.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset> { bodyAsset, coverageAsset };
            bodyAsset.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset> { coverageAsset };
            coverageAsset.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
            EditorUtility.SetDirty(displayAsset);
            EditorUtility.SetDirty(bodyAsset);
            EditorUtility.SetDirty(coverageAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("WOBBLE fonts rebuilt: display=" + displayAsset.atlasTextures.Length + " atlas, body=" +
                      bodyAsset.atlasTextures.Length + " atlas, coverage=" + coverageAsset.atlasTextures.Length + " atlas");
        }

        private static TMP_FontAsset Create(string sourcePath, string assetName, int sampling, int padding, string charset, bool strict)
        {
            var source = AssetDatabase.LoadAssetAtPath<Font>(sourcePath);
            if (source == null) throw new System.Exception("Font source missing: " + sourcePath);
            var font = TMP_FontAsset.CreateFontAsset(source, sampling, padding, GlyphRenderMode.SDFAA,
                2048, 2048, AtlasPopulationMode.Dynamic, true);
            font.name = assetName;
            if (strict && !font.TryAddCharacters(charset, out var missing) && !string.IsNullOrEmpty(missing))
                throw new System.Exception(assetName + " is missing glyphs: " + missing);
            if (!strict) font.TryAddCharacters(charset, out _);
            font.atlasPopulationMode = AtlasPopulationMode.Static;
            return font;
        }

        private static void Save(TMP_FontAsset font, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (existing != null) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(font, path);
            AssetDatabase.AddObjectToAsset(font.material, font);
            foreach (var atlas in font.atlasTextures) AssetDatabase.AddObjectToAsset(atlas, font);
            EditorUtility.SetDirty(font);
        }

        private static string Charset()
        {
            var deck = string.Empty;
            foreach (var file in new[] { "Assets/LocalizationAuthoring/Wobble.tsv" })
                if (File.Exists(file)) deck += File.ReadAllText(file);
            var chars = (deck + Reserve).Where(c => !char.IsControl(c) && c != '\uFEFF').Distinct().ToArray();
            return new string(chars);
        }
    }
}
#endif
