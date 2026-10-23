#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Wobble.Audio;
namespace Wobble.Editor
{
    public static class PercussivePlacementBuilder
    {
        public static void Build()
        {
            var clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/AudioAuthoring/723897.mp3");clip.LoadAudioData();
            var raw=new float[clip.samples*clip.channels];if(!clip.GetData(raw,0))throw new Exception("Cannot decode Plocks Five");
            var mono=new float[clip.samples];for(var n=0;n<mono.Length;n++)for(var c=0;c<clip.channels;c++)mono[n]+=raw[n*clip.channels+c]/clip.channels;
            const int rate=44100;const float duration=.17f;
            for(var take=0;take<5;take++)
            {
                var begin=take*mono.Length/5;var end=(take+1)*mono.Length/5;var strongest=begin;float peak=0;
                for(var n=begin;n<end;n++)if(Mathf.Abs(mono[n])>peak){peak=Mathf.Abs(mono[n]);strongest=n;}
                var onset=Math.Max(begin,strongest-(int)(clip.frequency*.025f));
                while(onset<strongest&&Mathf.Abs(mono[onset])<peak*.025f)onset++;
                onset=Math.Max(begin,onset-(int)(clip.frequency*.003f));
                var data=new float[(int)(rate*duration)];float dc=0,lp1=0,lp2=0;peak=0;
                var hp=1-Mathf.Exp(-2*Mathf.PI*85/rate);var lp=1-Mathf.Exp(-2*Mathf.PI*1450/rate);
                for(var n=0;n<data.Length;n++)
                {
                    var at=onset+n*clip.frequency/(float)rate*.88f;var k=(int)at;
                    var x=k+1<end?Mathf.Lerp(mono[k],mono[k+1],at-k):0;
                    dc+=hp*(x-dc);lp1+=lp*(x-dc-lp1);lp2+=lp*(lp1-lp2);
                    var t=n/(float)rate;data[n]=lp2*Mathf.SmoothStep(0,1,t/.0035f)*Mathf.SmoothStep(0,1,(duration-t)/.045f);
                    peak=Mathf.Max(peak,Mathf.Abs(data[n]));
                }
                if(peak<.001f)throw new Exception("Silent plock segment "+take);
                for(var n=0;n<data.Length;n++)data[n]*=.7f/peak;
                Write("Assets/Resources/Audio/PercussivePlacement_"+(take+1).ToString("00")+".wav",data,rate);
                Debug.Log($"Plock take {take+1}: onset {onset/(float)clip.frequency:F4}s, peak .70, duration .17s");
            }
            AssetDatabase.Refresh();
            for(var i=1;i<=5;i++)
            {
                var importer=(AudioImporter)AssetImporter.GetAtPath("Assets/Resources/Audio/PercussivePlacement_"+i.ToString("00")+".wav");
                var sampleSettings=importer.defaultSampleSettings;sampleSettings.loadType=AudioClipLoadType.DecompressOnLoad;
                sampleSettings.compressionFormat=AudioCompressionFormat.PCM;sampleSettings.preloadAudioData=true;
                importer.defaultSampleSettings=sampleSettings;importer.forceToMono=true;importer.SaveAndReimport();
            }
            const string path="Assets/Resources/Audio/PercussivePlacementSettings.asset";
            var settings=AssetDatabase.LoadAssetAtPath<PercussivePlacementSettings>(path);
            if(settings==null){settings=ScriptableObject.CreateInstance<PercussivePlacementSettings>();AssetDatabase.CreateAsset(settings,path);}
            for(var i=0;i<5;i++)settings.Variants[i]=Resources.Load<AudioClip>("Audio/PercussivePlacement_"+(i+1).ToString("00"));
            EditorUtility.SetDirty(settings);AssetDatabase.SaveAssets();Export(settings);
        }
        private static void Export(PercussivePlacementSettings settings)
        {
            const int rate=44100;var sequence=new[]{1,2,3,4,5,6,7,8,1,1};
            var data=new float[rate*8];var source=new float[settings.Variants[0].samples];settings.Variants[0].GetData(source,0);
            for(var i=0;i<sequence.Length;i++)
            {
                var perfect=sequence[i]>1;var pitch=settings.Pitch(perfect,sequence[i]);var gain=settings.Gain(perfect,sequence[i]);
                void Mix(float p,float g,float seconds,float fade)
                {
                    for(var n=0;n<(int)(seconds*rate);n++)
                    {
                        var at=n*p;var k=(int)at;if(k+1>=source.Length)break;
                        data[(int)(i*.75f*rate)+n]+=Mathf.Lerp(source[k],source[k+1],at-k)*g*AudioManager.EffectsHeadroom*Mathf.SmoothStep(0,1,(seconds-n/(float)rate)/fade);
                    }
                }
                Mix(pitch,gain,Mathf.Min(settings.Duration,source.Length/(float)rate/pitch),settings.FadeOut);
                Mix(1,settings.Body(perfect,sequence[i]),.12f,.045f);
            }
            Directory.CreateDirectory("Artifacts/Audio");Write("Artifacts/Audio/PercussivePlacementAudition.wav",data,rate);
        }
        private static void Write(string path,float[] data,int rate)
        {
            using var w=new BinaryWriter(File.Create(path));w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+data.Length*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
            w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(data.Length*2);
            foreach(var value in data)w.Write((short)(Mathf.Clamp(value,-1,1)*32767));
        }
        public static void BuildReview()
        {
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Wobble.unity"},locationPathName="/private/tmp/wobble-percussive-player/WOBBLE.app",target=BuildTarget.StandaloneOSX,options=BuildOptions.Development});
            if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Percussive build failed");
        }
    }
}
#endif
