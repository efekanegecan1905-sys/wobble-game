#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Wobble.Editor
{
    // Focused replacement for MenuAssetBuilder.Configure when only the copy deck
    // changed. Configure also rewrites the TMP atlases; this tool leaves the font
    // assets, catalog and addressable groups untouched.
    public static class LocalizationSync
    {
        private const string TablePath = "Assets/LocalizationAuthoring/Wobble.tsv";

        [MenuItem("WOBBLE/Menu/Sync Localization Strings")]
        public static void SyncStrings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>("Assets/Localization/WobbleLocalization.asset");
            if (settings != null) LocalizationEditorSettings.ActiveLocalizationSettings = settings;
            var collection = LocalizationEditorSettings.GetStringTableCollection("Wobble");
            if (collection == null) throw new Exception("Wobble string table collection missing. Run Configure Menu Assets once.");

            var codes = new[] { "tr", "en" };
            var lines = File.ReadAllLines(TablePath);
            var keys = 0;
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cells = line.Split('\t');
                if (cells.Length != 3) throw new Exception("Invalid localization row: " + line);
                for (var i = 0; i < 2; i++)
                {
                    var table = (StringTable)collection.GetTable(new LocaleIdentifier(codes[i]));
                    if (table == null) throw new Exception("Missing " + codes[i] + " string table.");
                    var value = cells[i + 1].Replace("\\n", "\n");
                    var entry = table.AddEntry(cells[0], value);
                    entry.IsSmart = value.Contains("{");
                    EditorUtility.SetDirty(table);
                }
                keys++;
            }
            collection.RefreshAddressables();
            collection.SetPreloadTableFlag(true);
            EditorUtility.SetDirty(collection.SharedData);
            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssets();
            Debug.Log($"WOBBLE localization synced: {keys} keys x 2 locales. Fonts untouched.");
        }

        // Reports any character the copy deck needs but a shipped atlas cannot draw.
        // Used by the edit-mode typography test and by hand before a build.
        [MenuItem("WOBBLE/Menu/Validate Glyph Coverage")]
        public static void ValidateGlyphCoverage()
        {
            var deck = File.ReadAllText(TablePath) + "0123456789×+·‹›";
            var required = new string(deck.Where(c => !char.IsControl(c) && c != '\uFEFF' && c != '\t' && c != '\n').Distinct().ToArray());
            foreach (var path in new[]
                     {
                         "Assets/Resources/Menu/WobbleDisplay.asset",
                         "Assets/Resources/Menu/WobbleBody.asset",
                         "Assets/Resources/Menu/WobbleLocalizedSans.asset"
                     })
            {
                var font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(path);
                if (font == null) throw new Exception("Missing font asset: " + path);
                var missing = new string(required.Where(c => !font.HasCharacter(c)).ToArray());
                if (missing.Length > 0) Debug.LogError(path + " missing glyphs: " + missing);
                else Debug.Log(path + " covers the whole deck.");
            }
        }
    }
}
#endif
