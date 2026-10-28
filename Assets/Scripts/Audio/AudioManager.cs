using UnityEngine;
using UnityEngine.Audio;
using Wobble.Themes;

namespace Wobble.Audio
{
    // One approved percussive placement bank (Normal + combo x2..x8) + two quiet secondary voices.
    // Exactly one primary placement voice is scheduled per successful placement: no marble/stone
    // layer is added underneath and the baked combo pitch is never shifted again at runtime.
    public sealed class AudioManager
    {
        public const int MaximumEffectVoices = 10;
        public const float EffectsHeadroom = .65f;
        public const float PlacementPitch = 1f;
        public const int PlacementLevelCount = 8;
        public const float PlacementVolume = .48f;
        public const float PlacementFadeOut = .02f;
        // Level 1 = Normal, 2..8 = x2..x8. Order is the approved mapping and must not change.
        public static readonly string[] PlacementClipNames =
        {
            "Wobble_Normal", "Wobble_Combo_x2", "Wobble_Combo_x3", "Wobble_Combo_x4",
            "Wobble_Combo_x5", "Wobble_Combo_x6", "Wobble_Combo_x7", "Wobble_Combo_x8"
        };

        private readonly AudioSource[] voices = new AudioSource[MaximumEffectVoices];
        private readonly AudioClip[] bodies = new AudioClip[5];
        private readonly AudioClip[] placementClips = new AudioClip[PlacementLevelCount];
        private readonly ImpactEnvelope envelope;
        private readonly ImpactProfiles profiles;
        private readonly AudioClip scrape;
        private readonly AudioSource musicSource;
        private readonly AudioMixerGroup gameplayGroup, uiGroup;
        private readonly System.Random random = new();
        private bool enabled = true;
        private int secondary;
        private double nextSecondary;
        private bool placementResetPending = true;
        private int placementLevelShift = 1;

        public bool LogRuntimePlacement;
        public float LastPitch => PlacementPitch;
        public int ImpactCount { get; private set; }
        public int LastComboLevel { get; private set; } = 1;
        public string LastPlacementClip { get; private set; }
        public ImpactProfiles Profiles => profiles;
        public AudioClip PlacementClip(int level) => placementClips[Mathf.Clamp(level, 1, PlacementLevelCount) - 1];

        public AudioManager(GameObject host, WobbleTheme activeTheme, ImpactProfiles overrideProfiles = null, AudioClip[] overridePlacement = null)
        {
            profiles = overrideProfiles != null ? overrideProfiles : Resources.Load<ImpactProfiles>("Audio/CarraraImpactProfiles");
            if (profiles == null) throw new System.InvalidOperationException("Missing CarraraImpactProfiles audio asset.");
            for (var i = 0; i < placementClips.Length; i++)
            {
                var path = "Audio/Placement/" + PlacementClipNames[i];
                placementClips[i] = overridePlacement != null && overridePlacement.Length > i && overridePlacement[i] != null
                    ? overridePlacement[i]
                    : Resources.Load<AudioClip>(path);
                if (placementClips[i] == null) throw new System.InvalidOperationException("Missing combo placement take: " + path + ". Stone fallback is forbidden.");
            }
            envelope = host.AddComponent<ImpactEnvelope>();
            var mixer = Resources.Load<AudioMixer>("Audio/WobbleMix");
            var gameplay = Group(mixer, "Gameplay");
            gameplayGroup = gameplay; uiGroup = Group(mixer, "UI");
            var ambience = Group(mixer, "Ambience");
            for (var i = 0; i < voices.Length; i++)
            {
                voices[i] = host.AddComponent<AudioSource>();
                voices[i].playOnAwake = false; voices[i].spatialBlend = 0;
                voices[i].outputAudioMixerGroup = (i < 8 && i % 4 == 3) ? ambience : gameplay;
                voices[i].priority = i < 8 ? 64 : 150;
            }
            musicSource = host.AddComponent<AudioSource>();
            musicSource.playOnAwake = false; musicSource.loop = true; musicSource.volume = .25f;
            musicSource.outputAudioMixerGroup = Group(mixer, "Music");
            for (var i = 0; i < bodies.Length; i++)
            {
                bodies[i] = Resources.Load<AudioClip>("Audio/MarbleImpact_" + (i + 1).ToString("00"));
            }
            scrape = Resources.Load<AudioClip>("Audio/MarbleScrape");
        }

        private static AudioMixerGroup Group(AudioMixer mixer, string name)
        { var matches = mixer != null ? mixer.FindMatchingGroups(name) : null; return matches != null && matches.Length > 0 ? matches[0] : null; }

        public void SetEnabled(bool value)
        { enabled = value; foreach (var voice in voices) { voice.mute = !value; if (!value) voice.Stop(); } }
        public void SetMusicEnabled(bool value) => musicSource.mute = !value;
        public void SetMusicClip(AudioClip clip)
        { musicSource.clip = clip; if (clip != null) musicSource.Play(); else musicSource.Stop(); }

        // One successful placement: exactly one primary clip from the approved bank.
        // level 1 = Wobble_Normal, level 2..8 = Wobble_Combo_x2..x8, 9+ clamps to x8.
        // A run/reset starts at Normal and then walks x2..x8 even though the live combo starts at x2.
        public void Impact(bool perfect, int combo)
        {
            if (!enabled) return;
            int level;
            if (placementResetPending) { level = 1; placementLevelShift = 1; }
            else level = perfect ? Mathf.Clamp(combo - placementLevelShift, 1, PlacementLevelCount) : 1;
            placementResetPending = false;
            if (!perfect) placementLevelShift = 0;
            var clip = PlacementClip(level);
            if (clip == null) return;
            var when = AudioSettings.dspTime + .002;
            // Exactly one primary voice: every placement slot is stopped before the new hit.
            for (var i = 0; i < 8; i++) { voices[i].Stop(); envelope.Clear(i); }
            Schedule(0, clip, PlacementVolume, PlacementPitch, when);
            voices[0].panStereo = 0;
            envelope.Track(0, voices[0], when, clip.length, PlacementFadeOut);
            LastComboLevel = level;
            LastPlacementClip = clip.name;
            ImpactCount++;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (LogRuntimePlacement) Debug.Log($"WOBBLE placement combo={combo} level={level} clip={clip.name} pitch={PlacementPitch:F2}");
#endif
        }

        // Combo break, death and Continue all arm the next placement as Wobble_Normal.
        public void ResetPlacementCombo()
        {
            placementResetPending = true;
            placementLevelShift = 1;
            for (var i = 0; i < 8; i++) { voices[i].Stop(); envelope.Clear(i); }
        }

        private void Secondary(AudioClip clip, float gain, double cooldown)
        {
            var now = AudioSettings.dspTime;
            if (!enabled || clip == null || now < nextSecondary) return;
            nextSecondary = now + cooldown;
            var slot = 8 + (secondary++ % 2); voices[slot].Stop();
            voices[slot].outputAudioMixerGroup = gameplayGroup;
            Schedule(slot, clip, gain, .98f + (float)random.NextDouble() * .04f, now + .006);
        }

        public void Cut(float fraction)
        { } // Cutting coincides with placement: no old scrape/stone layer may leak here.
        private bool PrimaryPlaying()
        {
            for (var i = 0; i < 8; i++) if (voices[i].isPlaying) return true;
            return false;
        }
        // Falling debris keeps a very quiet secondary contact only; it never plays under a placement.
        public void FallingImpact(float speed)
        {
            if (speed < .65f || PrimaryPlaying()) return;
            Secondary(bodies[random.Next(bodies.Length)], Mathf.Lerp(.008f, .018f, Mathf.Clamp01(speed / 8f)), .16);
        }
        public void Placement() => Impact(false, 1);
        public void Perfect() => Impact(true, 2);
        public void Falling() => FallingImpact(3);
        public void LevelUp() { } // No achievement beep over the physical placement.
        public void GameOver() => Secondary(bodies[0], .12f, .2);
        // Attributed recorded stone bodies; supporting layers remain designed noise textures.
        public void MenuTap() => MenuSound(bodies[1], .055f);
        public void MenuSlide() => MenuSound(scrape, .045f);
        public void PlayTransition() => MenuSound(bodies[2], .22f);
        public int IntroPlacementCount { get; private set; }
        public int SignatureCount { get; private set; }
        public void IntroPlacement() { IntroPlacementCount++; MenuSound(profiles.Normal.comboBody.clip, .065f); }
        public void Signature()
        {
            SignatureCount++;
            MenuSound(profiles.Normal.comboBody.clip, .28f);
            MenuSound(bodies[0], .12f);
        }
        private void Schedule(int slot, AudioClip clip, float gain, float pitch, double when)
        {
            if (clip == null) return;
            var voice = voices[slot]; voice.clip = clip; voice.volume = gain * EffectsHeadroom; voice.pitch = pitch; voice.PlayScheduled(when);
        }
        private void MenuSound(AudioClip clip, float volume)
        {
            if (!enabled) return;
            var slot = 8 + (secondary++ % 2); voices[slot].Stop(); voices[slot].outputAudioMixerGroup = uiGroup;
            Schedule(slot, clip, volume, 1, AudioSettings.dspTime + .006);
        }
        public void Dispose() { foreach (var voice in voices) if (voice != null) voice.Stop(); }
    }
}
