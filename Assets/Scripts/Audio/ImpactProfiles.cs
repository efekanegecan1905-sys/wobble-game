using System;
using UnityEngine;
namespace Wobble.Audio
{
    [Serializable] public sealed class ImpactLayer
    {
        public AudioClip clip;
        [Range(0,1)] public float volume;
        [Range(.5f,1.5f)] public float pitch=1;
        [Tooltip("Maximum duration in seconds; 0 plays the whole clip.")]
        [Range(0,1)] public float duration;
        [Range(.01f,.15f)] public float fadeOut=.04f;
        [Range(-1,1)] public float pan;
        public ImpactLayer(float gain,float seconds=0){volume=gain;duration=seconds;}
    }
    [Serializable] public sealed class ImpactProfile
    {
        [Range(0,1)] public float intensity=.9f;
        [Range(0,.04f)] public float pitchVariation=0;
        [Range(0,.05f)] public float volumeVariation=.03f;
        public ImpactLayer baseMarble=new(.42f);
        [InspectorName("Arcade Plock")]
        [Tooltip("Short secondary interaction accent; serialized name retained for existing profile compatibility.")]
        public ImpactLayer comboBody=new(0);
        public ImpactLayer resonance=new(0);
        public ImpactLayer ambience=new(0);
        public ImpactProfile(float baseGain,float body,float resonanceGain,float bodyDuration,float resonanceDuration,float room)
        {
            baseMarble.volume=baseGain;comboBody=new(body,bodyDuration);resonance=new(resonanceGain,resonanceDuration);ambience=new(room);
        }
    }
    [CreateAssetMenu(menuName="WOBBLE/Audio/Impact Profiles")]
    public sealed class ImpactProfiles : ScriptableObject
    {
        [Tooltip("Stone body stays dominant. Arcade Plock is a separate short accent, including on Normal.")]
        public ImpactProfile Normal=Hybrid(.46f,.08f,1,0);
        public ImpactProfile Combo2=Hybrid(.46f,.12f,1.025f,0);
        public ImpactProfile Combo3=Hybrid(.46f,.16f,1.05f,0);
        public ImpactProfile Combo4=Hybrid(.48f,.20f,1.075f,.008f);
        public ImpactProfile Combo5Plus=Hybrid(.50f,.22f,1.10f,.015f);
        private static ImpactProfile Hybrid(float body,float share,float pitch,float tail)
        {
            var p=new ImpactProfile(body,body*share/(1-share),tail,.095f,.13f,0);
            p.volumeVariation=.015f;p.baseMarble.pitch=p.comboBody.pitch=p.resonance.pitch=pitch;
            p.baseMarble.duration=.28f;p.baseMarble.fadeOut=.035f;p.comboBody.fadeOut=.025f;p.resonance.fadeOut=.06f;
            return p;
        }
        public ImpactProfile Select(bool perfect,int combo)=>!perfect?Normal:combo<=2?Combo2:combo==3?Combo3:combo==4?Combo4:Combo5Plus;
        public int Index(bool perfect,int combo)=>!perfect?0:Mathf.Clamp(combo,2,5)-1;
    }
}
