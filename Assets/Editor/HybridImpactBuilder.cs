#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Wobble.Audio;
namespace Wobble.Editor
{
    public static class HybridImpactBuilder
    {
        public static void BuildReview()
        {
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Wobble.unity"},locationPathName="/private/tmp/wobble-hybrid-player/WOBBLE.app",target=BuildTarget.StandaloneOSX,options=BuildOptions.Development});
            if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Hybrid audio review build failed");
        }
        [MenuItem("WOBBLE/Audio/Prepare Hybrid Stone Plock")]
        public static void Build()
        {
            const string source="Assets/AudioAuthoring/723897.mp3";
            var importer=(AudioImporter)AssetImporter.GetAtPath(source);
            var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat=AudioCompressionFormat.PCM;settings.preloadAudioData=true;
            importer.defaultSampleSettings=settings;importer.SaveAndReimport();
            var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(source);clip.LoadAudioData();
            var input=new float[clip.samples*clip.channels];if(!clip.GetData(input,0))throw new Exception("Cannot decode CC0 plock source");
            var mono=new float[clip.samples];
            for(var i=0;i<mono.Length;i++)for(var c=0;c<clip.channels;c++)mono[i]+=input[i*clip.channels+c]/clip.channels;
            // The source contains five separated examples. Use only the first.
            var end=Math.Min(mono.Length,(int)(clip.frequency*.45f));var peak=0f;var strongest=0;
            for(var i=0;i<end;i++)if(Mathf.Abs(mono[i])>peak){peak=Mathf.Abs(mono[i]);strongest=i;}
            var start=Math.Max(0,strongest-(int)(clip.frequency*.02f));
            while(start<strongest&&Mathf.Abs(mono[start])<peak*.02f)start++;
            start=Math.Max(0,start-(int)(clip.frequency*.002f));
            const int rate=44100;const float duration=.095f;
            var data=new float[(int)(rate*duration)];float dc=0,low=0,low2=0;peak=0;
            var hp=1-Mathf.Exp(-2*Mathf.PI*100/rate);var lp=1-Mathf.Exp(-2*Mathf.PI*1700/rate);
            for(var i=0;i<data.Length;i++)
            {
                var at=start+i*clip.frequency/(float)rate*.9f;var k=(int)at;
                var x=k+1<mono.Length?Mathf.Lerp(mono[k],mono[k+1],at-k):0;
                dc+=hp*(x-dc);low+=lp*(x-dc-low);low2+=lp*(low-low2);
                var t=i/(float)rate;
                data[i]=low2*Mathf.SmoothStep(0,1,t/.004f)*Mathf.SmoothStep(0,1,(duration-t)/.035f);
                peak=Mathf.Max(peak,Mathf.Abs(data[i]));
            }
            if(peak<.001f)throw new Exception("Empty plock extraction");
            for(var i=0;i<data.Length;i++)data[i]*=.7f/peak;
            const string output="Assets/Resources/Audio/ArcadePlock.wav";
            // Existing designer replacement files are never overwritten automatically.
            if(!File.Exists(output))Write(output,data,rate);
            AssetDatabase.Refresh();
            var accent=AssetDatabase.LoadAssetAtPath<AudioClip>(output);
            var profiles=Resources.Load<ImpactProfiles>("Audio/CarraraImpactProfiles");
            var all=new[]{profiles.Normal,profiles.Combo2,profiles.Combo3,profiles.Combo4,profiles.Combo5Plus};
            var shares=new[]{.08f,.12f,.16f,.20f,.22f};
            for(var i=0;i<all.Length;i++)
            {
                var p=all[i];var pitch=1+i*.025f;var body=i<3?.46f:i==3?.48f:.50f;
                p.intensity=.9f;p.pitchVariation=0;p.volumeVariation=.015f;
                p.baseMarble.volume=body;p.baseMarble.pitch=pitch;p.baseMarble.duration=.28f;p.baseMarble.fadeOut=.035f;
                p.comboBody.clip=accent;p.comboBody.volume=body*shares[i]/(1-shares[i]);
                p.comboBody.pitch=pitch;p.comboBody.duration=.095f;p.comboBody.fadeOut=.025f;p.comboBody.pan=0;
                p.resonance.volume=i==4?.015f:i==3?.008f:0;p.resonance.pitch=pitch;p.resonance.duration=.13f;p.resonance.fadeOut=.06f;p.resonance.pan=0;
                p.ambience.volume=0;
            }
            EditorUtility.SetDirty(profiles);AssetDatabase.SaveAssets();
            StoneFoleyBuilder.ExportAuditionTo("Artifacts/Audio/HybridComboAudition.wav");
            Debug.Log("Hybrid profiles: stone + CC0 Plocks Five excerpt, 8/12/16/20/22% gain shares, pitch 1.00–1.10.");
        }
        private static void Write(string path,float[] data,int rate)
        {
            using var w=new BinaryWriter(File.Create(path));
            w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+data.Length*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
            w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);
            w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(data.Length*2);
            foreach(var value in data)w.Write((short)(Mathf.Clamp(value,-1,1)*32767));
        }
    }
}
#endif
