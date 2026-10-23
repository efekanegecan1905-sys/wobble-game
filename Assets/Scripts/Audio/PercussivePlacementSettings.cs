using UnityEngine;
namespace Wobble.Audio
{
    [CreateAssetMenu(menuName="WOBBLE/Audio/Percussive Placement")]
    public sealed class PercussivePlacementSettings : ScriptableObject
    {
        public AudioClip[] Variants=new AudioClip[5];
        [Range(.8f,1.2f)] public float BasePitch=1;
        [Range(0,.15f)] public float ComboPitchStep=.07f;
        [Range(1,1.5f)] public float MaxComboPitch=1.49f;
        [Range(2,12)] public int ComboPitchClampLevel=8;
        [Range(0,1)] public float Volume=.48f;
        [Range(0,.01f)] public float RandomPitchRange=.005f;
        [Range(0,.02f)] public float RandomVolumeRange=.005f;
        [Range(0,.03f)] public float PerfectVolumeStep=.005f;
        [Range(1,1.03f)] public float MaxVolumeMultiplier=1.02f;
        [Range(.05f,.3f)] public float Duration=.17f;
        [Range(.01f,.08f)] public float FadeOut=.035f;
        [Tooltip("Quiet low-pitch copy of the same percussive take, never stone Foley.")]
        [Range(0,.05f)] public float ResonanceAmount=.008f;
        [Range(0,.08f)] public float HighComboBodyCompensation=.025f;
        public bool LogRuntimePitch;
        public int Index(bool perfect,int combo)=>perfect?Mathf.Clamp(combo,2,ComboPitchClampLevel)-1:0;
        public float Pitch(bool perfect,int combo)=>Mathf.Min(MaxComboPitch,BasePitch+Index(perfect,combo)*ComboPitchStep);
        public float Gain(bool perfect,int combo)=>Volume*Mathf.Min(MaxVolumeMultiplier,1+Mathf.Max(0,Index(perfect,combo)-1)*PerfectVolumeStep);
        public float Body(bool perfect,int combo)=>!perfect||combo<4?0:ResonanceAmount+HighComboBodyCompensation*Mathf.InverseLerp(4,8,combo);
    }
}
