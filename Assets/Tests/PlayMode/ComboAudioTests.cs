using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Wobble.Audio;
using Wobble.Core;
using Wobble.Gameplay;
using Wobble.UI;
namespace Wobble.Tests
{
    // Contract: exactly one approved Wobble_* clip per placement, baked pitch (never shifted),
    // Normal → x2 → ... → x8 → x8, and break/death/Continue re-arm Wobble_Normal.
    public sealed class ComboAudioTests
    {
        private static readonly string[] Approved =
            { "Wobble_Normal", "Wobble_Combo_x2", "Wobble_Combo_x3", "Wobble_Combo_x4",
              "Wobble_Combo_x5", "Wobble_Combo_x6", "Wobble_Combo_x7", "Wobble_Combo_x8" };

        private GameManager game; private AudioManager audio; private StackManager stack;
        private string saved; private bool hadSave;

        private void Boot()
        {
            typeof(GameManager).GetMethod("StartGame", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(game, null);
            audio = (AudioManager)typeof(GameManager).GetField("audio", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(game);
            audio.SetEnabled(true);
            stack = (StackManager)typeof(GameManager).GetField("stack", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(game);
        }

        private bool Place(bool perfect)
        {
            var offset = perfect ? Vector3.zero : stack.ActiveBlock.Axis * .2f;
            stack.ActiveBlock.transform.position = stack.Top.transform.position + Vector3.up * StackManager.BlockHeight + offset;
            var before = audio.ImpactCount;
            stack.PlaceCurrent();
            Assert.That(audio.ImpactCount, Is.EqualTo(before + 1), "One primary placement impact per placement");
            return perfect;
        }

        [UnitySetUp] public IEnumerator Setup()
        {
            hadSave = PlayerPrefs.HasKey("wobble.save.v1"); saved = PlayerPrefs.GetString("wobble.save.v1");
            var old = Object.FindFirstObjectByType<GameManager>(); if (old != null) Object.Destroy(old.gameObject); yield return null;
            game = new GameObject("Combo audio contract").AddComponent<GameManager>();
            yield return new WaitUntil(() => game.GetComponent<UIManager>().Localization.Ready);
            Boot();
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            Object.Destroy(game.gameObject); yield return null;
            if (hadSave) PlayerPrefs.SetString("wobble.save.v1", saved); else PlayerPrefs.DeleteKey("wobble.save.v1"); PlayerPrefs.Save();
        }

        [UnityTest] public IEnumerator NormalThenX2ToX8ThenBreakRearmsNormal()
        {
            var pattern = new[] { false, true, true, true, true, true, true, true, true, false, true };
            var expected = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 8, 1, 2 };
            var log = new System.Collections.Generic.List<string>();
            for (var i = 0; i < pattern.Length; i++)
            {
                Place(pattern[i]);
                Assert.That(audio.LastComboLevel, Is.EqualTo(expected[i]), $"step {i} level");
                Assert.That(audio.LastPlacementClip, Is.EqualTo(Approved[expected[i] - 1]), $"step {i} clip");
                Assert.That(audio.LastPitch, Is.EqualTo(1f), "Baked WAV pitch must never be shifted again");

                var voices = game.GetComponents<AudioSource>();
                var primary = voices.Take(8).Where(v => v.isPlaying).ToArray();
                Assert.That(primary.Length, Is.EqualTo(1), $"step {i}: exactly one primary placement voice");
                Assert.That(primary[0].clip.name, Is.EqualTo(Approved[expected[i] - 1]));
                Assert.That(primary[0].pitch, Is.EqualTo(1f).Within(.0001f));
                Assert.That(primary[0].clip.name, Does.StartWith("Wobble_"), "Only approved Wobble takes may be primary");

                // No marble/stone or legacy percussive placement may leak from a primary voice.
                foreach (var voice in voices.Take(8).Where(v => v.isPlaying))
                    Assert.That(voice.clip.name, Does.Not.Contain("MarbleImpact"), "Old stone placement leak");
                foreach (var voice in voices.Take(8).Where(v => v.isPlaying))
                    Assert.That(voice.clip.name, Does.Not.StartWith("PercussivePlacement_"), "Removed bank leaked");
                // Falling debris may keep only a very quiet secondary contact.
                foreach (var voice in voices.Skip(8).Where(v => v.isPlaying))
                    Assert.That(voice.volume, Is.LessThanOrEqualTo(.018f * AudioManager.EffectsHeadroom + .001f), "Debris must not mask placement");

                log.Add($"step={i} input={(pattern[i] ? "perfect" : "cut")} level={audio.LastComboLevel} clip={audio.LastPlacementClip} pitch={audio.LastPitch:F2}");
                yield return new WaitForSeconds(.09f);
            }
            Assert.That(audio.PlacementClip(9), Is.SameAs(audio.PlacementClip(8)));
            var folder = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Artifacts/Audio"));
            System.IO.Directory.CreateDirectory(folder);
            System.IO.File.WriteAllLines(System.IO.Path.Combine(folder, "ComboPlacementRuntime.txt"), log);

            audio.SetEnabled(false);
            var count = audio.ImpactCount; audio.Impact(true, 5);
            Assert.That(audio.ImpactCount, Is.EqualTo(count), "Muted audio schedules nothing");
        }

        [UnityTest] public IEnumerator DeathAndContinueRearmNextPlacementToNormal()
        {
            Place(true);                                   // Normal (run start arms Normal)
            Place(true);                                   // x2
            Assert.That(audio.LastComboLevel, Is.EqualTo(2));

            // Miss the tower: the block never overlaps the top face.
            stack.ActiveBlock.transform.position = stack.Top.transform.position + Vector3.right * 12f;
            stack.PlaceCurrent();
            Assert.That(game.State, Is.EqualTo(GameState.GameOver), "Miss must end the run");

            yield return new WaitForSeconds(.75f);         // show-results routine releases the guard
            game.ContinueWithLife();
            Assert.That(game.State, Is.EqualTo(GameState.Playing), "Continue must resume play");

            Place(true);
            Assert.That(audio.LastPlacementClip, Is.EqualTo("Wobble_Normal"), "First hit after Continue is Normal");
            Assert.That(audio.LastPitch, Is.EqualTo(1f));
            Place(true);
            Assert.That(audio.LastComboLevel, Is.EqualTo(2), "Progression resumes at x2 after the reset hit");
        }

        [UnityTest] public IEnumerator ExplicitResetForcesSingleNormalTake()
        {
            audio.Impact(false, 1);                        // consume the run-start reset
            Assert.That(audio.LastPlacementClip, Is.EqualTo("Wobble_Normal"));
            audio.Impact(true, 8);
            Assert.That(audio.LastPlacementClip, Is.EqualTo("Wobble_Combo_x8"));
            audio.ResetPlacementCombo();
            audio.Impact(true, 8);
            Assert.That(audio.LastPlacementClip, Is.EqualTo("Wobble_Normal"));
            audio.Impact(true, 9);
            Assert.That(audio.LastPlacementClip, Is.EqualTo("Wobble_Combo_x8"));
            yield return null;
        }
    }
}
