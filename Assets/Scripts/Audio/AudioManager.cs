using UnityEngine;
using UnityEngine.Audio;
using Wobble.Themes;

namespace Wobble.Audio
{
    // Two four-stem placement banks + two secondary voices. No unbounded PlayOneShot overlap.
    public sealed class AudioManager
    {
        public const int VariationCount = 5;
        public const int MaximumEffectVoices = 10;
        public const float EffectsHeadroom = .65f;
        private readonly AudioSource[] voices = new AudioSource[MaximumEffectVoices];
        private readonly AudioClip[] bodies = new AudioClip[VariationCount];
        private readonly ImpactEnvelope envelope;
        private readonly ImpactProfiles profiles;
        private readonly PercussivePlacementSettings placement;
        private readonly AudioClip tail, scrape;
        private readonly AudioSource musicSource;
        private readonly AudioMixerGroup gameplayGroup,uiGroup;
        private readonly System.Random random = new();
        private bool enabled = true;
        private int lastVariation = -1, bank, secondary;
        private double nextSecondary;
        public int LastVariation => lastVariation;
        public float LastPitch { get; private set; } = 1;
        public int ImpactCount { get; private set; }
        public int LastProfileIndex {get;private set;}
        public ImpactProfiles Profiles=>profiles;
        public PercussivePlacementSettings PlacementSettings=>placement;

        public AudioManager(GameObject host, WobbleTheme activeTheme,ImpactProfiles overrideProfiles=null,PercussivePlacementSettings overridePlacement=null)
        {
            profiles=overrideProfiles!=null?overrideProfiles:Resources.Load<ImpactProfiles>("Audio/CarraraImpactProfiles");
            if(profiles==null)throw new System.InvalidOperationException("Missing CarraraImpactProfiles audio asset.");
            placement=overridePlacement!=null?overridePlacement:Resources.Load<PercussivePlacementSettings>("Audio/PercussivePlacementSettings");
            if(placement==null||placement.Variants.Length<4)throw new System.InvalidOperationException("Missing percussive placement bank. Stone fallback is forbidden.");
            foreach(var clip in placement.Variants)if(clip==null)throw new System.InvalidOperationException("Missing percussive placement take. Stone fallback is forbidden.");
            envelope=host.AddComponent<ImpactEnvelope>();
            var mixer = Resources.Load<AudioMixer>("Audio/WobbleMix");
            var gameplay = Group(mixer,"Gameplay");
            gameplayGroup=gameplay;uiGroup=Group(mixer,"UI");
            var ambience = Group(mixer,"Ambience");
            for (var i=0;i<voices.Length;i++)
            {
                voices[i]=host.AddComponent<AudioSource>();
                voices[i].playOnAwake=false; voices[i].spatialBlend=0;
                voices[i].outputAudioMixerGroup=(i<8 && i%4==3)?ambience:gameplay;
                voices[i].priority=i<8?64:150;
            }
            musicSource=host.AddComponent<AudioSource>();
            musicSource.playOnAwake=false; musicSource.loop=true; musicSource.volume=.25f;
            musicSource.outputAudioMixerGroup=Group(mixer,"Music");
            for(var i=0;i<VariationCount;i++)
            {
                bodies[i]=Resources.Load<AudioClip>("Audio/MarbleImpact_"+(i+1).ToString("00"));
            }
            tail=Resources.Load<AudioClip>("Audio/MarbleRoom");
            scrape=Resources.Load<AudioClip>("Audio/MarbleScrape");
        }
        private static AudioMixerGroup Group(AudioMixer mixer,string name)
        {var matches=mixer!=null?mixer.FindMatchingGroups(name):null;return matches!=null&&matches.Length>0?matches[0]:null;}
        public void SetEnabled(bool value)
        {enabled=value;foreach(var voice in voices){voice.mute=!value;if(!value)voice.Stop();}}
        public void SetMusicEnabled(bool value)=>musicSource.mute=!value;
        public void SetMusicClip(AudioClip clip)
        {musicSource.clip=clip;if(clip!=null)musicSource.Play();else musicSource.Stop();}

        public void Impact(bool perfect,int combo=1)
        {
            if(!enabled)return;
            var count=placement.Variants.Length;
            var index=lastVariation<0?random.Next(count):random.Next(count-1);
            if(lastVariation>=0 && index>=lastVariation)index++;
            lastVariation=index;
            LastProfileIndex=placement.Index(perfect,combo);
            var pitchJitter=perfect?((float)random.NextDouble()*2-1)*placement.RandomPitchRange:0;
            LastPitch=Mathf.Clamp(placement.Pitch(perfect,combo)+pitchJitter,.5f,placement.MaxComboPitch);
            var gain=placement.Gain(perfect,combo)*(1+((float)random.NextDouble()*2-1)*placement.RandomVolumeRange);
            var slot=(bank++%2)*4;
            var when=AudioSettings.dspTime+.002;
            // Remove preceding combo tails on a break; do not carry enrichment into Normal.
            if(!perfect)for(var i=0;i<8;i++){voices[i].Stop();envelope.Clear(i);}
            for(var i=0;i<4;i++){voices[slot+i].Stop();envelope.Clear(slot+i);}
            var clip=placement.Variants[index];
            Schedule(slot,clip,gain,LastPitch,when);voices[slot].panStereo=0;
            envelope.Track(slot,voices[slot],when,Mathf.Min(placement.Duration,clip.length/LastPitch),placement.FadeOut);
            var body=placement.Body(perfect,combo);
            if(body>0)
            {
                Schedule(slot+1,clip,body,1,when);voices[slot+1].panStereo=0;
                envelope.Track(slot+1,voices[slot+1],when,Mathf.Min(.12f,clip.length),.045f);
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(placement.LogRuntimePitch)Debug.Log($"WOBBLE placement perfect={perfect} combo={combo} pitch={LastPitch:F4} take={index+1}");
#endif
            ImpactCount++;
        }
        private void Layer(int slot,ImpactLayer layer,AudioClip fallback,float gain,float jitter,double when)
        {
            var clip=layer.clip!=null?layer.clip:fallback;if(clip==null||layer.volume<=0)return;
            var pitch=Mathf.Clamp(layer.pitch*jitter,.5f,1.5f);
            Schedule(slot,clip,layer.volume*gain,pitch,when);
            voices[slot].panStereo=Mathf.Clamp(layer.pan,-1,1);
            var length=clip.length/pitch;var duration=layer.duration>0?Mathf.Min(layer.duration,length):length;
            envelope.Track(slot,voices[slot],when,duration,layer.fadeOut);
        }
        private void Schedule(int slot,AudioClip clip,float gain,float pitch,double when)
        {
            if(clip==null)return;
            var voice=voices[slot];voice.clip=clip;voice.volume=gain*EffectsHeadroom;voice.pitch=pitch;voice.PlayScheduled(when);
        }
        public void Cut(float fraction)
        { } // Cutting coincides with placement: no old scrape/stone layer may leak here.
        public void FallingImpact(float speed)
        {if(speed>=.65f)Secondary(bodies[random.Next(VariationCount)],Mathf.Lerp(.015f,.035f,Mathf.Clamp01(speed/8)),.16);}
        private void Secondary(AudioClip clip,float gain,double cooldown)
        {
            var now=AudioSettings.dspTime;
            if(!enabled || now<nextSecondary)return;
            nextSecondary=now+cooldown;
            var slot=8+(secondary++%2);voices[slot].Stop();
            voices[slot].outputAudioMixerGroup=gameplayGroup;
            Schedule(slot,clip,gain,.98f+(float)random.NextDouble()*.04f,now+.006);
        }
        public void Placement()=>Impact(false);
        public void Perfect()=>Impact(true,2);
        public void Falling()=>FallingImpact(3);
        public void LevelUp() { } // No achievement beep over the physical placement.
        public void GameOver()=>Secondary(bodies[0],.19f,.2);
        // Attributed recorded stone bodies; supporting layers remain designed noise textures.
        public void MenuTap()=>MenuSound(bodies[1],.055f);
        public void MenuSlide()=>MenuSound(scrape,.045f);
        public void PlayTransition()=>MenuSound(bodies[2],.22f);
        public int IntroPlacementCount {get;private set;}
        public int SignatureCount {get;private set;}
        public void IntroPlacement(){IntroPlacementCount++;MenuSound(profiles.Normal.comboBody.clip,.065f);}
        public void Signature()
        {
            SignatureCount++;
            MenuSound(profiles.Normal.comboBody.clip,.28f);
            MenuSound(bodies[0],.12f);
        }
        private void MenuSound(AudioClip clip,float volume)
        {
            if(!enabled)return;
            var slot=8+(secondary++%2);voices[slot].Stop();voices[slot].outputAudioMixerGroup=uiGroup;
            Schedule(slot,clip,volume,1,AudioSettings.dspTime+.006);
        }
        public void Dispose(){foreach(var voice in voices)if(voice!=null)voice.Stop();}
    }
}
