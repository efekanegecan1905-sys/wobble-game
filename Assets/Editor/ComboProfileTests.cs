#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;
using Wobble.Audio;
namespace Wobble.Editor.Tests
{
    public sealed class ComboProfileTests
    {
        [Test] public void HybridProfilesKeepStoneDominantWithSubtlePitchAndClamp()
        {
            var asset=Resources.Load<ImpactProfiles>("Audio/CarraraImpactProfiles");Assert.That(asset,Is.Not.Null);
            var profiles=new[]{asset.Normal,asset.Combo2,asset.Combo3,asset.Combo4,asset.Combo5Plus};
            for(var i=0;i<5;i++)
            {
                var p=profiles[i];Assert.That(p.baseMarble.pitch,Is.EqualTo(1+i*.025f).Within(.0001));Assert.That(p.baseMarble.clip,Is.Null,"All defaults must share the variation bank.");
                Assert.That(p.pitchVariation,Is.Zero);
                Assert.That(p.comboBody.clip,Is.Not.Null);if(p.resonance.volume>0)Assert.That(p.resonance.clip,Is.Not.Null);
                Assert.That(p.comboBody.pitch,Is.EqualTo(p.baseMarble.pitch));
                var shares=new[]{.08f,.12f,.16f,.20f,.22f};
                Assert.That(p.comboBody.volume/(p.comboBody.volume+p.baseMarble.volume),Is.EqualTo(shares[i]).Within(.001));
                Assert.That(p.comboBody.clip.length,Is.LessThan(.1f));
                if(i>0)Assert.That(p.comboBody.volume,Is.GreaterThan(profiles[i-1].comboBody.volume));
                Assert.That(p.baseMarble.volume*.7f,Is.GreaterThan(p.comboBody.volume*.5f+p.resonance.volume*.4f+p.ambience.volume*.13f));
            }
            Assert.That(asset.Select(false,99),Is.SameAs(asset.Normal));Assert.That(asset.Select(true,1),Is.SameAs(asset.Combo2));
            for(var i=5;i<100;i++)Assert.That(asset.Select(true,i),Is.SameAs(asset.Combo5Plus));
            Assert.That(asset.Combo5Plus.resonance.volume,Is.LessThan(.02f));
        }
    }
}
#endif
