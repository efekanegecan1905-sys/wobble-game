#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Wobble.Audio;
namespace Wobble.Editor.Tests
{
    public sealed class ComboPlacementBankTests
    {
        private static readonly string[] Approved =
            { "Wobble_Normal", "Wobble_Combo_x2", "Wobble_Combo_x3", "Wobble_Combo_x4",
              "Wobble_Combo_x5", "Wobble_Combo_x6", "Wobble_Combo_x7", "Wobble_Combo_x8" };

        [Test] public void ApprovedBankIsCompleteInExactOrder()
        {
            Assert.That(AudioManager.PlacementClipNames.Length, Is.EqualTo(8));
            for (var i = 0; i < AudioManager.PlacementClipNames.Length; i++)
            {
                var path = "Audio/Placement/" + AudioManager.PlacementClipNames[i];
                var clip = Resources.Load<AudioClip>(path);
                Assert.That(clip, Is.Not.Null, $"Missing {path}");
                Assert.That(clip.name, Is.EqualTo(Approved[i]), $"Wrong take at level {i + 1}");
                Assert.That(clip.channels, Is.EqualTo(1));
                Assert.That(clip.frequency, Is.EqualTo(44100));
                Assert.That(clip.length, Is.InRange(.10f, .32f));
            }
            // Rising pitch progression is baked: Normal/x2..x4 are .30s, x5..x8 shorten as pitch rises.
            Assert.That(Resources.Load<AudioClip>("Audio/Placement/Wobble_Normal").length, Is.InRange(.29f, .31f));
            Assert.That(Resources.Load<AudioClip>("Audio/Placement/Wobble_Combo_x8").length, Is.InRange(.11f, .13f));
            Assert.That(AudioManager.PlacementPitch, Is.EqualTo(1f), "Runtime must not pitch-shift the bank");
        }

        [Test] public void RemovedStoneAndLegacyPercussiveBanksAreGone()
        {
            var legacy = Enumerable.Range(1, 5).Select(i => Resources.Load<AudioClip>("Audio/PercussivePlacement_" + i.ToString("00")));
            Assert.That(legacy.All(clip => clip == null), "Legacy percussive placement bank is still loaded by Resources");
            Assert.That(Resources.Load<AudioClip>("Audio/PercussivePlacementSettings"), Is.Null);
        }
    }
}
#endif
