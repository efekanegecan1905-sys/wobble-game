#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Wobble.Editor
{
    // Imports the authored master at native resolution; no runtime texture generation.
    public static class MarbleTextureBaker
    {
        public static void Bake()
        {
            const string path = "Assets/Resources/Rendering/CarraraNatural.png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.mipmapEnabled = true;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 8;
            importer.wrapMode = TextureWrapMode.Mirror;
            importer.maxTextureSize = 2048;
            importer.isReadable = false;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            foreach (var platform in new[] { "Android", "iPhone" })
                importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = platform, overridden = true, maxTextureSize = 2048, format = TextureImporterFormat.ASTC_6x6, compressionQuality = 100 });
            importer.SaveAndReimport();
        }
    }
}
#endif
