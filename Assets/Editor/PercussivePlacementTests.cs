#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Wobble.Audio;
namespace Wobble.Editor.Tests
{
    public sealed class PercussivePlacementTests
    {
        [Test] public void PercussiveBankAndCurveAreBounded()
        {
            var p=Resources.Load<PercussivePlacementSettings>("Audio/PercussivePlacementSettings");Assert.That(p,Is.Not.Null);
            Assert.That(p.Variants.Length,Is.EqualTo(5));float[] previous=null;
            foreach(var clip in p.Variants)
            {
                Assert.That(clip.name,Does.StartWith("PercussivePlacement_"));Assert.That(clip.length,Is.InRange(.16f,.18f));
                var samples=new float[clip.samples];Assert.That(clip.GetData(samples,0),Is.True);
                Assert.That(samples.Max(x=>Mathf.Abs(x)),Is.InRange(.69f,.71f));
                Assert.That(samples[0],Is.Zero);Assert.That(Mathf.Abs(samples[^1]),Is.LessThan(.001));
                if(previous!=null)Assert.That(samples.SequenceEqual(previous),Is.False);previous=samples;
            }
            for(var combo=2;combo<=8;combo++)Assert.That(p.Pitch(true,combo),Is.EqualTo(1+(combo-1)*.07f).Within(.0001));
            Assert.That(p.Pitch(true,999),Is.EqualTo(1.49f).Within(.0001));Assert.That(p.Pitch(false,999),Is.EqualTo(1));
            Assert.That(p.RandomPitchRange,Is.LessThanOrEqualTo(.01f));
            var worst=2*.7f*(p.Gain(true,99)*(1+p.RandomVolumeRange)+p.Body(true,99))*AudioManager.EffectsHeadroom;
            worst+=2*.7f*.035f*AudioManager.EffectsHeadroom+.25f;
            Assert.That(worst,Is.LessThan(1),"Two placement banks + two debris voices + reserved music headroom");
            Debug.Log("Percussive conservative peak including .25 music reserve: "+worst.ToString("F4"));
        }
    }
}
#endif
