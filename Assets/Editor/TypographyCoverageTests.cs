#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Wobble.Tests
{
    // The Turkish and English decks must share one type identity. Every glyph a
    // player can meet is baked into the shipped atlases; no screen may silently
    // fall through to a system face.
    public sealed class TypographyCoverageTests
    {
        // Uppercase, lowercase, digits, the Turkish block and the punctuation the
        // copy deck actually prints.
        private const string Required =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" +
            "ÇĞİÖŞÜçğıöşü" +
            " .,:;!?%+-–—'’\"“”()[]/&×·‹›";

        [TestCase("Assets/Resources/Menu/WobbleDisplay.asset")]
        [TestCase("Assets/Resources/Menu/WobbleBody.asset")]
        [TestCase("Assets/Resources/Menu/WobbleLocalizedSans.asset")]
        public void FontsCoverTheWholeDeck(string path)
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            Assert.That(font, Is.Not.Null, "Missing font asset: " + path);
            var missing = string.Empty;
            foreach (var c in Required) if (!font.HasCharacter(c)) missing += c;
            Assert.That(missing, Is.Empty, path + " is missing glyphs: " + missing);
        }

        [Test]
        public void DisplayAndBodyUseDistinctFamilies()
        {
            var display = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/Menu/WobbleDisplay.asset");
            var body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/Menu/WobbleBody.asset");
            Assert.That(display, Is.Not.Null);
            Assert.That(body, Is.Not.Null);
            Assert.That(display.faceInfo.familyName, Is.Not.EqualTo(body.faceInfo.familyName));
        }

        [Test]
        public void AuthoringDeckHasValidRowsAndKnownKeys()
        {
            var lines = System.IO.File.ReadAllLines("Assets/LocalizationAuthoring/Wobble.tsv");
            Assert.That(lines[0], Is.EqualTo("key\ttr\ten"));
            var seen = new System.Collections.Generic.HashSet<string>();
            for (var i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                var cells = lines[i].Split('\t');
                Assert.That(cells.Length, Is.EqualTo(3), "Row " + (i + 1) + " must have key + tr + en.");
                Assert.That(seen.Add(cells[0]), Is.True, "Duplicate key: " + cells[0]);
                Assert.That(cells[1], Is.Not.Empty, "Empty Turkish value: " + cells[0]);
                Assert.That(cells[2], Is.Not.Empty, "Empty English value: " + cells[0]);
            }
            foreach (var key in new[] { "hud.flow", "hud.fever", "hud.wobble", "hud.stabilized", "result.newbest" })
                Assert.That(seen.Contains(key), Is.True, "Missing feel-pass key: " + key);
        }
    }
}
#endif
