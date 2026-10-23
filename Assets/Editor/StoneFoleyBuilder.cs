#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Wobble.Editor
{
    // Offline processing of attributed recordings; no synthesized replacement transient.
    public static class StoneFoleyBuilder
    {
        [MenuItem("WOBBLE/Audio/Prepare Recorded Stone Impacts")]
        public static void Build()
        {
            for(var take=0;take<5;take++)
            {
                var id=30008+take;var sourcePath="Assets/AudioAuthoring/"+id+".mp3";
                var importer=(AudioImporter)AssetImporter.GetAtPath(sourcePath);
                var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat=AudioCompressionFormat.PCM;settings.preloadAudioData=true;
                importer.defaultSampleSettings=settings;importer.SaveAndReimport();
                var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(sourcePath);clip.LoadAudioData();
                var input=new float[clip.samples*clip.channels];if(!clip.GetData(input,0))throw new Exception("Could not decode "+id);
                var mono=new float[clip.samples];
                for(var i=0;i<mono.Length;i++)for(var c=0;c<clip.channels;c++)mono[i]+=input[i*clip.channels+c]/clip.channels;
                // Locate the main physical impact, retain a short pre-attack and remove silence.
                var strongest=0;var maximum=0f;
                for(var i=0;i<mono.Length;i++)if(Mathf.Abs(mono[i])>maximum){maximum=Mathf.Abs(mono[i]);strongest=i;}
                var start=Mathf.Max(0,strongest-(int)(clip.frequency*.045f));
                while(start<strongest&&Mathf.Abs(mono[start])<maximum*.035f)start++;
                start=Mathf.Max(0,start-(int)(clip.frequency*.004f));
                const int rate=44100;const float duration=.28f;
                var output=new float[(int)(rate*duration)];
                float slow=0,low=0,low2=0,weight=0;var peak=0f;
                // A single fixed .82 playback ratio gives mass, never a combo pitch ladder.
                var highPass=1-Mathf.Exp(-2*Mathf.PI*65/rate);
                var soften=1-Mathf.Exp(-2*Mathf.PI*1250/rate);
                var body=1-Mathf.Exp(-2*Mathf.PI*360/rate);
                for(var i=0;i<output.Length;i++)
                {
                    var at=start+i*(clip.frequency/(float)rate)*.82f;var k=(int)at;
                    var x=k+1<mono.Length?Mathf.Lerp(mono[k],mono[k+1],at-k):0;
                    slow+=highPass*(x-slow);x-=slow;
                    low+=soften*(x-low);low2+=soften*(low-low2);weight+=body*(low2-weight);
                    var t=i/(float)rate;
                    var envelope=Mathf.SmoothStep(0,1,t/.004f)*Mathf.SmoothStep(0,1,(duration-t)/.055f);
                    output[i]=(float)Math.Tanh((low2+weight*.65f)*2)*envelope;peak=Mathf.Max(peak,Mathf.Abs(output[i]));
                }
                if(peak<.005f)throw new Exception("Silent Foley "+id);
                for(var i=0;i<output.Length;i++)output[i]*=.70f/peak;
                Write("Assets/Resources/Audio/MarbleImpact_"+(take+1).ToString("00")+".wav",output,rate);
                Debug.Log("Recorded stone "+id+": source onset "+(start/(float)clip.frequency).ToString("F4")+"s, source peak "+maximum.ToString("F4")+", output peak .70, duration .28s");
            }
            AssetDatabase.Refresh();AssetDatabase.SaveAssets();
        }
        private static void Write(string path,float[] data,int rate)
        {
            using var w=new BinaryWriter(File.Create(path));
            w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+data.Length*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
            w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);
            w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(data.Length*2);
            foreach(var value in data)w.Write((short)(Mathf.Clamp(value,-1,1)*32767));
        }
        public static void ExportAudition() => ExportAuditionTo("Artifacts/Concept/ComboFoleyAudition.wav");
        public static void ExportAuditionTo(string path)
        {
            var profiles=Resources.Load<Wobble.Audio.ImpactProfiles>("Audio/CarraraImpactProfiles");
            var sequence=new[]{0,2,3,4,5,8,0,2};const int rate=44100;
            var output=new float[(int)(sequence.Length*.8f*rate)];
            for(var i=0;i<sequence.Length;i++)
            {
                var profile=profiles.Select(sequence[i]>0,sequence[i]);var start=(int)(i*.8f*rate);
                var layers=new[]{profile.baseMarble,profile.comboBody,profile.resonance,profile.ambience};
                for(var j=0;j<4;j++)
                {
                    var layer=layers[j];var clip=layer.clip!=null?layer.clip:j==0?Resources.Load<AudioClip>("Audio/MarbleImpact_01"):null;
                    if(clip==null||layer.volume<=0)continue;
                    var samples=new float[clip.samples*clip.channels];clip.GetData(samples,0);
                    var seconds=layer.duration>0?Mathf.Min(layer.duration,clip.length/layer.pitch):clip.length/layer.pitch;
                    var count=Mathf.Min((int)(seconds*rate),output.Length-start);var fade=Mathf.Min(seconds,Mathf.Max(.01f,layer.fadeOut));
                    for(var n=0;n<count;n++)
                    {
                        var at=n*clip.frequency/(float)rate*layer.pitch;var k=(int)at;if(k+1>=clip.samples)break;
                        var sample=0f;for(var c=0;c<clip.channels;c++)sample+=Mathf.Lerp(samples[k*clip.channels+c],samples[(k+1)*clip.channels+c],at-k)/clip.channels;
                        output[start+n]+=sample*layer.volume*profile.intensity*Wobble.Audio.AudioManager.EffectsHeadroom*Mathf.SmoothStep(0,1,Mathf.Clamp01((seconds-n/(float)rate)/fade));
                    }
                }
            }
            Directory.CreateDirectory(Path.GetDirectoryName(path));Write(path,output,rate);
        }
    }
}
#endif
