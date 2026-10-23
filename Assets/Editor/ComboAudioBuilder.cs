#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Wobble.Audio;
namespace Wobble.Editor
{
    public static class ComboAudioBuilder
    {
        public static void BuildReview()
        {
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Wobble.unity"},locationPathName="/private/tmp/wobble-combo-player/WOBBLE.app",target=BuildTarget.StandaloneOSX,options=BuildOptions.Development});
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Combo audio review build failed: "+report.summary.result);
        }
        [MenuItem("WOBBLE/Audio/Create Combo Layers and Profiles")]
        public static void Build()
        {
            Write("ComboBody",MarbleAudioBuilder.Synthesize(7,4,.30f,.5f));
            var resonance=MarbleAudioBuilder.Synthesize(11,4,.44f,.4f);var peak=0f;
            for(var i=0;i<resonance.Length;i++){resonance[i]*=Mathf.Exp(i/(float)MarbleAudioBuilder.Rate*10);peak=Mathf.Max(peak,Mathf.Abs(resonance[i]));}
            for(var i=0;i<resonance.Length;i++)resonance[i]*=.4f/peak;
            Write("ComboResonance",resonance);AssetDatabase.Refresh();
            foreach(var name in new[]{"ComboBody","ComboResonance"})
            {
                var importer=(AudioImporter)AssetImporter.GetAtPath("Assets/Resources/Audio/"+name+".wav");
                var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;settings.preloadAudioData=true;
                importer.defaultSampleSettings=settings;importer.forceToMono=true;importer.SaveAndReimport();
            }
            const string path="Assets/Resources/Audio/CarraraImpactProfiles.asset";
            // Never reset designer tuning when regenerating clips.
            if(AssetDatabase.LoadAssetAtPath<ImpactProfiles>(path)==null)
            {
                var asset=ScriptableObject.CreateInstance<ImpactProfiles>();
                foreach(var profile in new[]{asset.Combo2,asset.Combo3,asset.Combo4,asset.Combo5Plus})
                {
                    profile.comboBody.clip=Resources.Load<AudioClip>("Audio/ComboBody");
                    profile.resonance.clip=Resources.Load<AudioClip>("Audio/ComboResonance");
                    profile.ambience.clip=Resources.Load<AudioClip>("Audio/MarbleRoom");
                }
                asset.Combo4.comboBody.pan=-.06f;asset.Combo4.resonance.pan=.06f;
                asset.Combo5Plus.comboBody.pan=-.09f;asset.Combo5Plus.resonance.pan=.09f;
                AssetDatabase.CreateAsset(asset,path);
            }
            AssetDatabase.SaveAssets();
        }
        private static void Write(string name,float[] data)
        {
            using var w=new BinaryWriter(File.Create("Assets/Resources/Audio/"+name+".wav"));
            w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+data.Length*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);
            w.Write(MarbleAudioBuilder.Rate);w.Write(MarbleAudioBuilder.Rate*2);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(data.Length*2);
            foreach(var sample in data)w.Write((short)(Mathf.Clamp(sample,-1,1)*32767));
        }
    }
}
#endif
