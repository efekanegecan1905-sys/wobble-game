#if UNITY_EDITOR
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
namespace Wobble.Editor
{
    public static class ConceptPresentationBuilder
    {
        public static void Prepare()
        {
            PrepareArchitecture();
            var strings=UnityEditor.Localization.LocalizationEditorSettings.GetStringTableCollection("Wobble");
            foreach(var code in new[]{"tr","en"})
            {
                var table=(UnityEngine.Localization.Tables.StringTable)strings.GetTable(new UnityEngine.Localization.LocaleIdentifier(code));
                table.AddEntry("settings.credits",code=="tr"?"Ses Kaynakları":"Audio Credits");EditorUtility.SetDirty(table);
            }
            EditorUtility.SetDirty(strings.SharedData);
            const string bodyPath="Assets/Resources/Menu/WobbleBody.asset";
            if(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(bodyPath)==null)
            {
                var source=AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/NotoSans.ttf");
                var body=TMP_FontAsset.CreateFontAsset(source,96,12,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,2048,2048);
                body.name="Wobble Body";
                body.TryAddCharacters(new string((File.ReadAllText("Assets/LocalizationAuthoring/Wobble.tsv")+"₺0123456789× ·‹+").Where(c=>!char.IsControl(c)).Distinct().ToArray()),out var missing);
                body.atlasPopulationMode=AtlasPopulationMode.Static;
                AssetDatabase.CreateAsset(body,bodyPath);AssetDatabase.AddObjectToAsset(body.material,body);
                foreach(var atlas in body.atlasTextures)AssetDatabase.AddObjectToAsset(atlas,body);
                EditorUtility.SetDirty(body);
            }
            const string path="Assets/Resources/Menu/WobbleDisplay.asset";
            if(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path)==null)
            {
                var source=AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/LilitaOne-Regular.ttf");
                var font=TMP_FontAsset.CreateFontAsset(source,96,12,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,2048,2048);
                font.name="Wobble Display";
                var chars=File.ReadAllText("Assets/LocalizationAuthoring/Wobble.tsv")+"0123456789×+";
                font.TryAddCharacters(new string(chars.Where(c=>!char.IsControl(c)).Distinct().ToArray()),out var missing);
                font.fallbackFontAssetTable=new System.Collections.Generic.List<TMP_FontAsset>{Resources.Load<TMP_FontAsset>("Menu/WobbleLocalizedSans")};
                font.atlasPopulationMode=AtlasPopulationMode.Static;
                AssetDatabase.CreateAsset(font,path);AssetDatabase.AddObjectToAsset(font.material,font);
                foreach(var atlas in font.atlasTextures)AssetDatabase.AddObjectToAsset(atlas,font);
                EditorUtility.SetDirty(font);
            }
            var display=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            display.fallbackFontAssetTable=new System.Collections.Generic.List<TMP_FontAsset>{AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(bodyPath)};
            EditorUtility.SetDirty(display);AssetDatabase.SaveAssets();
        }
        public static void BuildReview()
        {
            BuildPlayer("/private/tmp/wobble-concept-player/WOBBLE.app");
        }
        public static void BuildThemeDemo()
        {
            BuildPlayer("/private/tmp/wobble-theme-demo/WOBBLE.app");
        }
        public static void BuildCinematicReview()
        {
            BuildPlayer("/private/tmp/wobble-cinematic-player/WOBBLE.app");
        }
        private static void BuildPlayer(string path)
        {
            Prepare();
            StoneFoleyBuilder.ExportAudition();
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Wobble.unity"},locationPathName=path,target=BuildTarget.StandaloneOSX,options=BuildOptions.Development});
            if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Concept build failed.");
        }
        private static void PrepareArchitecture()
        {
            var pier=Resources.Load<Mesh>("Presentation/Monolith");
            foreach(var name in new[]{"QuarryGarden","ColumnGarden","ArchGarden"})
            {
                var path="Assets/Resources/Presentation/"+name+".asset";
                if(AssetDatabase.LoadAssetAtPath<Mesh>(path)!=null)continue;
                var parts=new System.Collections.Generic.List<CombineInstance>();
                void Part(Vector3 p,Vector3 scale,float angle=0)=>parts.Add(new CombineInstance{mesh=pier,transform=Matrix4x4.TRS(p,Quaternion.Euler(0,0,angle),scale)});
                if(name=="QuarryGarden")
                {
                    Part(new Vector3(0,-.37f,0),new Vector3(1.3f,.27f,1.1f));
                    Part(new Vector3(-.15f,-.13f,-.06f),new Vector3(.9f,.3f,.86f));
                    Part(new Vector3(.10f,.14f,.02f),new Vector3(.65f,.27f,.64f));
                    Part(new Vector3(.10f,.40f,.02f),new Vector3(.44f,.26f,.43f));
                }
                else if(name=="ColumnGarden")
                {
                    Part(new Vector3(0,-.45f,0),new Vector3(1,.1f,.75f));
                    for(var i=0;i<3;i++){Part(new Vector3((i-1)*.34f,0,0),new Vector3(.23f,.85f,.35f));Part(new Vector3((i-1)*.34f,.45f,0),new Vector3(.29f,.1f,.40f));}
                }
                else
                {
                    Part(new Vector3(-.42f,-.16f,0),new Vector3(.19f,.7f,.35f));
                    Part(new Vector3(.42f,-.16f,0),new Vector3(.19f,.7f,.35f));
                    for(var i=0;i<9;i++){var a=i/8f*Mathf.PI;Part(new Vector3(Mathf.Cos(a)*.42f,.16f+Mathf.Sin(a)*.30f,0),new Vector3(.18f,.18f,.35f),a*Mathf.Rad2Deg-90);}
                }
                var mesh=new Mesh{name=name};mesh.CombineMeshes(parts.ToArray());mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);
            }
        }
    }
}
#endif
