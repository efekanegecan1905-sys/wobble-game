#if UNITY_EDITOR
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
namespace Wobble.Editor
{
    public static class GameplayFontBuilder
    {
        public const string Characters="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789ÇĞİÖŞÜçğıöşü !?.,:;+-/()%×·–—'\"";
        public static void Build()
        {
            System.IO.Directory.CreateDirectory("Assets/Resources/Gameplay");
            Create("Assets/Fonts/Gameplay/Rubik-SemiBold.ttf","Assets/Resources/Gameplay/HudFont.asset",true);
            Create("Assets/Fonts/Gameplay/Bricolage-SemiBold.ttf","Assets/Resources/Gameplay/CelebrationFont.asset",true);
            Create("Assets/Editor/FontComparisons/Fredoka-SemiBold.ttf","Assets/Editor/FontComparisons/Fredoka.asset",false);
            AssetDatabase.SaveAssets();
        }
        private static void Create(string source,string destination,bool strict)
        {
            if(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(destination)!=null)return;
            var font=TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>(source),88,9,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,false);
            font.name=System.IO.Path.GetFileNameWithoutExtension(source);
            var all=new string(Characters.Distinct().ToArray());
            font.TryAddCharacters(all,out var missing);
            if(strict&&!string.IsNullOrEmpty(missing))throw new System.Exception(source+" missing: "+missing);
            font.atlasPopulationMode=AtlasPopulationMode.Static;
            font.fallbackFontAssetTable=new System.Collections.Generic.List<TMP_FontAsset>();
            AssetDatabase.CreateAsset(font,destination);AssetDatabase.AddObjectToAsset(font.material,font);
            foreach(var atlas in font.atlasTextures)AssetDatabase.AddObjectToAsset(atlas,font);
            EditorUtility.SetDirty(font);
        }
    }
}
#endif
