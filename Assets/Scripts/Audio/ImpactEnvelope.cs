using UnityEngine;
namespace Wobble.Audio
{
    // One bounded envelope runner for the eight reused placement voices.
    public sealed class ImpactEnvelope : MonoBehaviour
    {
        private readonly AudioSource[] sources=new AudioSource[8];
        private readonly double[] ends=new double[8];
        private readonly float[] gains=new float[8],fades=new float[8];
        public void Clear(int slot){sources[slot]=null;}
        public void Track(int slot,AudioSource source,double start,float seconds,float fade)
        {sources[slot]=source;ends[slot]=start+seconds;gains[slot]=source.volume;fades[slot]=Mathf.Min(seconds,Mathf.Max(.01f,fade));}
        private void Update()
        {
            var now=AudioSettings.dspTime;
            for(var i=0;i<8;i++)
            {
                var source=sources[i];if(source==null)continue;
                var remaining=ends[i]-now;
                if(remaining<=0){source.Stop();sources[i]=null;continue;}
                source.volume=gains[i]*Mathf.SmoothStep(0,1,Mathf.Clamp01((float)remaining/fades[i]));
            }
        }
    }
}
