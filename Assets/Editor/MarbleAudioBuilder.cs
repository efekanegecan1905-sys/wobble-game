#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace Wobble.Editor
{
    public static class MarbleAudioBuilder
    {
        public const int Rate=44100;
        [MenuItem("WOBBLE/Feel/Build Marble Audio")]
        public static void Build()
        {
            if(File.Exists("Assets/Resources/Audio/RecordedStoneCredits.txt"))
                throw new InvalidOperationException("Recorded stone impacts are installed. Use StoneFoleyBuilder to prepare them; do not overwrite them with synthesis.");
            Directory.CreateDirectory("Assets/Resources/Audio");
            for(var i=0;i<5;i++)
            {
                Write("MarbleImpact_"+(i+1).ToString("00"),Synthesize(i,0,.24f,.70f));
                Write("MarbleContact_"+(i+1).ToString("00"),Synthesize(i,1,.09f,.18f));
            }
            Write("MarbleRoom",Synthesize(41,2,.32f,.13f));
            Write("MarbleScrape",Synthesize(23,3,.16f,.25f));
            for(var i=0;i<4;i++)Write("PerfectTone_"+(i+2),Synthesize(i,4,.27f,.40f));
            AssetDatabase.Refresh();
            foreach(var path in Directory.GetFiles("Assets/Resources/Audio","*.wav"))
            {
                var importer=(AudioImporter)AssetImporter.GetAtPath(path);
                var settings=importer.defaultSampleSettings;
                settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;
                settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;
                settings.preloadAudioData=true;
                importer.defaultSampleSettings=settings;importer.forceToMono=true;importer.SaveAndReimport();
            }
            BuildMixer();AssetDatabase.SaveAssets();
        }
        // Designed stone-like impact candidate, not recorded Foley.
        // Broadband stochastic excitation through broad low-mid bands: no pitched oscillators.
        private sealed class Band
        {
            private readonly double b0,b2,a1,a2;private double x1,x2,y1,y2;
            public Band(double frequency,double q)
            {
                var w=2*Math.PI*frequency/Rate;var alpha=Math.Sin(w)/(2*q);var a0=1+alpha;
                b0=alpha/a0;b2=-alpha/a0;a1=-2*Math.Cos(w)/a0;a2=(1-alpha)/a0;
            }
            public float Tick(float x)
            {
                var y=b0*x+b2*x2-a1*y1-a2*y2;x2=x1;x1=x;y2=y1;y1=y;return (float)y;
            }
        }
        public static float[] Synthesize(int variant,int layer,float duration,float peak)
        {
            var data=new float[(int)(Rate*duration)];var rng=new System.Random(4109+variant*131+layer*71);
            var drift=1+(variant-2)*.006;
            var body=new Band((layer==4?170:210)*drift,.72);
            var weight=new Band(350*drift,.85);var grain=new Band(660*drift,.70);
            float max=0,low=0,low2=0,dc=0;
            for(var i=0;i<data.Length;i++)
            {
                var t=i/(float)Rate;var noise=(float)rng.NextDouble()*2-1;
                var a=body.Tick(noise);var b=weight.Tick(noise);var c=grain.Tick(noise);
                var attack=Mathf.SmoothStep(0,1,t/(layer==1?.006f:.009f));
                var end=Mathf.SmoothStep(0,1,(duration-t)/.035f);
                var decay=layer==0?25f:layer==1?55f:layer==2?16f:layer==3?22f:23f;
                var envelope=Mathf.Exp(-Mathf.Max(0,t-.012f)*decay);
                var material=layer==0?1.05f*a+.72f*b+.13f*c
                    :layer==1?.35f*b+.15f*c:layer==2?a*.7f
                    :layer==3?.25f*b+.45f*c:1.1f*a+.35f*b;
                // Saturation adds density before a two-pole softening filter; no hard digital clip.
                var dense=(float)Math.Tanh(material*3.2f);
                low+=.13f*(dense-low);low2+=.13f*(low-low2);
                dc+=.006f*(low2-dc);
                data[i]=(low2-dc)*attack*end*envelope;max=Mathf.Max(max,Mathf.Abs(data[i]));
            }
            for(var i=0;i<data.Length;i++)data[i]*=peak/Mathf.Max(.001f,max);
            return data;
        }
        private static void Write(string name,float[] data)
        {
            using var writer=new BinaryWriter(File.Create("Assets/Resources/Audio/"+name+".wav"));
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+data.Length*2);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);
            writer.Write(Rate);writer.Write(Rate*2);writer.Write((short)2);writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(data.Length*2);
            foreach(var sample in data)writer.Write((short)(Mathf.Clamp(sample,-1,1)*32767));
        }
        private static void BuildMixer()
        {
            const string path="Assets/Resources/Audio/WobbleMix.mixer";
            if(AssetDatabase.LoadAssetAtPath<AudioMixer>(path)!=null)return;
            // Editor-only authoring API; no reflection or editor dependency in the player.
            var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.Audio.AudioMixerController")).First(t=>t!=null);
            var mixer=(UnityEngine.Object)type.GetMethod("CreateMixerControllerAtPath",BindingFlags.Public|BindingFlags.Static).Invoke(null,new object[]{path});
            var master=type.GetProperty("masterGroup").GetValue(mixer);
            foreach(var name in new[]{"Gameplay","UI","Music","Ambience"})
            {
                var group=type.GetMethod("CreateNewGroup").Invoke(mixer,new object[]{name,false});
                type.GetMethod("AddChildToParent").Invoke(mixer,new[]{group,master});
                EditorUtility.SetDirty((UnityEngine.Object)group);
            }
            EditorUtility.SetDirty((UnityEngine.Object)master);EditorUtility.SetDirty(mixer);
        }
    }
}
#endif
