#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Audio;
using Wobble.Gameplay;
using Wobble.Audio;

namespace Wobble.Editor.Tests
{
    public sealed class FeelTests
    {
        [Test] public void PacingRisesOnlyAtLevelBoundariesAndClamps()
        {
            var levels=new LevelManager();Assert.That(levels.Speed,Is.EqualTo(4));
            var reachedCap=false;
            for(var i=1;i<=500;i++)
            {
                var previous=levels.Speed;levels.RegisterPlacement();
                var expected=Mathf.Min(LevelManager.MaximumSpeed,LevelManager.InitialSpeed+(i/10)*LevelManager.LevelSpeedGain);
                Assert.That(levels.Speed,Is.EqualTo(expected).Within(.0001));
                Assert.That(levels.Level,Is.EqualTo(1+i/10));
                Assert.That(levels.Speed,Is.GreaterThanOrEqualTo(previous));
                Assert.That(levels.Speed,Is.InRange(4,6.6f));reachedCap|=levels.Speed>=6.599f;
            }
            Assert.That(reachedCap,Is.True);levels.ResetRun();Assert.That(levels.Speed,Is.EqualTo(4));
        }
        [Test] public void ImpactAssetsAreShortBoundedDistinctAndMobileMidrangeWeighted()
        {
            float[] previous=null;
            for(var i=1;i<=5;i++)
            {
                var clip=Resources.Load<AudioClip>("Audio/MarbleImpact_"+i.ToString("00"));
                Assert.That(clip,Is.Not.Null);Assert.That(clip.length,Is.InRange(.20f,.30f));
                var samples=new float[clip.samples];clip.GetData(samples,0);
                Assert.That(samples.Max(x=>Mathf.Abs(x)),Is.InRange(.69f,.71f));
                Assert.That(samples[0],Is.Zero);Assert.That(Mathf.Abs(samples[^1]),Is.LessThan(.001f));
                if(previous!=null)Assert.That(samples.SequenceEqual(previous),Is.False);
                previous=samples;
                // DFT energy at 25 Hz intervals; confirms useful body above sub-bass.
                double mids=0,highs=0;
                for(var hz=25;hz<=6000;hz+=25)
                {
                    double real=0,imag=0;
                    for(var n=0;n<samples.Length;n++) {var angle=2*Math.PI*hz*n/clip.frequency;real+=samples[n]*Math.Cos(angle);imag+=samples[n]*Math.Sin(angle);}
                    var power=real*real+imag*imag;if(hz>=100&&hz<=800)mids+=power;if(hz>=2500)highs+=power;
                }
                Assert.That(mids,Is.GreaterThan(highs*30));
            }
            // Conservative worst-case sum, both placement banks plus secondary voices.
            var profiles=Resources.Load<ImpactProfiles>("Audio/CarraraImpactProfiles");
            Assert.That(profiles,Is.Not.Null);
            var p=profiles.Combo5Plus;
            var peakBound=2*(.7f*p.baseMarble.volume+.7f*p.comboBody.volume+.4f*p.resonance.volume+.13f*p.ambience.volume)*p.intensity*(1+p.volumeVariation)+2*.7f*.19f;
            Assert.That(peakBound*AudioManager.EffectsHeadroom+.25f,Is.LessThan(1f));
        }
        [Test] public void MixerHasAllRequestedGroups()
        {
            var mixer=Resources.Load<AudioMixer>("Audio/WobbleMix");Assert.That(mixer,Is.Not.Null);
            foreach(var group in new[]{"Master","Gameplay","UI","Music","Ambience"})Assert.That(mixer.FindMatchingGroups(group),Is.Not.Empty);
        }
        [Test] public void ComboCannotAlterPhysicalSpeed()
        {
            var combo=new ComboManager();var level=new LevelManager();
            for(var i=0;i<100;i++)combo.Register(true);
            Assert.That(level.Speed,Is.EqualTo(4));
        }
    }
}
#endif
