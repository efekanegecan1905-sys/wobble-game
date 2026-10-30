#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NUnit.Framework;
using Wobble.Gameplay;

namespace Wobble.Tests
{
    // Presentation contract only: tiers follow x5/x8, escalation is reported once,
    // attack is fast and the release is a visible relaxation. No scoring lives here.
    public sealed class ComboEnergyTests
    {
        [Test]
        public void TiersFollowPerfectMilestones()
        {
            var energy = new ComboEnergy();
            Assert.That(energy.Push(true, 4), Is.Zero);
            Assert.That(energy.TargetTier, Is.Zero);
            Assert.That(energy.Push(true, 5), Is.EqualTo(1));
            Assert.That(energy.Push(true, 7), Is.Zero, "Staying inside FLOW must not re-trigger the beat.");
            Assert.That(energy.Push(true, 8), Is.EqualTo(2));
            Assert.That(energy.Push(true, 12), Is.Zero, "FEVER is held, not re-entered.");
        }

        [Test]
        public void LevelRampsInFastAndReleasesSoftly()
        {
            var energy = new ComboEnergy();
            energy.Push(true, 8);
            for (var i = 0; i < 12; i++) energy.Tick(1f / 60f);   // .20 s
            Assert.That(energy.Level, Is.GreaterThan(.95f));
            Assert.That(energy.Intensity, Is.GreaterThan(.45f));

            energy.Push(false, 0);
            energy.Tick(1f / 60f);
            Assert.That(energy.Level, Is.GreaterThan(.8f), "Break must not snap to zero in one frame.");
            for (var i = 0; i < 26; i++) energy.Tick(1f / 60f);   // ~.43 s
            Assert.That(energy.Level, Is.LessThan(.01f));
        }

        [Test]
        public void ResetClearsEverything()
        {
            var energy = new ComboEnergy();
            energy.Push(true, 8);
            for (var i = 0; i < 30; i++) energy.Tick(1f / 60f);
            energy.Reset();
            Assert.That(energy.Tier, Is.Zero);
            Assert.That(energy.Level, Is.Zero);
            Assert.That(energy.Intensity, Is.Zero);
        }
    }
}
#endif
