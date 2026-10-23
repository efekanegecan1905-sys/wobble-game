# Placement audio replacement — stone removed

The approved 5.8-second intro (including its audio), UI, Home, palette, gameplay, score, movement, cutting mechanics and environment are unchanged. Secondary visual polish is deferred until the new sound receives listening approval.

## 1. Sources

`Assets/AudioAuthoring/723897.mp3`: Jerimee's **Plocks Five**, CC0, https://freesound.org/people/Jerimee/sounds/723897/ . Public HQ MP3 rendition, not lossless masters. License evidence remains `Artifacts/AudioSources/723897.html`.

All five source examples are isolated into `Assets/Resources/Audio/PercussivePlacement_01.wav` through `_05.wav`. Their source onsets are 0.0668, 0.5752, 1.0256, 1.5372 and 2.0260 seconds. Each output is 170 ms, mono PCM16 44.1 kHz, peak .70. These are the sole primary placement bank. **No MarbleImpact, ArcadePlock hybrid layer, ComboBody, ComboResonance or MarbleRoom stem is used by normal/Perfect placement.**

## 2–3. Pitch and clamp

Normal 1.00; first Perfect/x2 1.07; x3 1.14; x4 1.21; x5 1.28; x6 1.35; x7 1.42; x8+ 1.49. The combo is read from the existing actual display multiplier. Tiny additive ±.005 variation is applied only after calculation, and clamped so it never exceeds 1.49. Normal has no pitch jitter and resets exactly to BasePitch.

## 4. Variation

Five related percussive examples; immediate repeat is excluded. No stone fallback is permitted: a missing bank fails explicitly. High-combo reinforcement uses a quiet unpitched copy of the same selected percussive sample, not another material or a musical tone.

## 5. Reset and event audit

`GameManager.OnPlaced → AudioManager.Impact` remains the only primary event. `EffectPool` and UI feedback do not play another impact. `StackManager`'s existing Cut callback now produces no audio, removing the simultaneous scrape leak without changing cutting. A non-perfect placement clears both previous placement banks and schedules only the Normal percussive hit. Debris retains stone only through the physics contact callback, reduced to .015–.035 gain versus .48 primary gain, with existing cooldown and two-voice cap. Intro/menu/game-over sounds remain separate and unchanged.

## 6. High-combo treatment

Source processing: fixed .88 rate, 85 Hz high-pass, two-pole 1450 Hz low-pass, 3.5 ms softened attack and 45 ms tail fade. At maximum runtime pitch the softened upper band remains around 2.2 kHz. x4+ quiet same-take low-mid reinforcement increases gently to x8. This is not a phone-speaker listening certification.

## 7. Headroom and rapid placement

Existing bounded ten effects voices are retained; only two voices per placement bank are used. Placement tails are at most 170 ms and shorten with pitch, low support at most 120 ms. Volume changes remain small (maximum 1.02 base multiplier plus ±.005 variation). The Edit Mode test checks a conservative sum of two simultaneous placement banks, two debris voices and .25 reserved music headroom below full scale. This is a calculated bound, not a claim of subjective listening approval.

## 8. Inspector controls

`Assets/Resources/Audio/PercussivePlacementSettings.asset`: Variants, BasePitch, ComboPitchStep, MaxComboPitch, ComboPitchClampLevel, Volume, RandomPitchRange, RandomVolumeRange, PerfectVolumeStep, MaxVolumeMultiplier, Duration, FadeOut, ResonanceAmount, HighComboBodyCompensation, LogRuntimePitch.

The old `CarraraImpactProfiles.asset` is retained for approved intro compatibility; it no longer controls placement. No normal startup regenerates or retunes clips.

Audition: `Artifacts/Audio/PercussivePlacementAudition.wav`, Normal → x2 → x3 → x4 → x5 → x6 → x7 → x8 → combo break → Normal. The audition fixes the sample and disables jitter to isolate the pitch curve. Actual runtime pitches are separately recorded by Play Mode using real placements and current jitter settings in `Artifacts/Audio/PercussiveRuntimePitch.txt`.

## Validation — 12 October 2026

18/18 Edit Mode and 24/24 Play Mode tests passed. The first import test caught a small compression-induced nonzero first sample; PCM import fixed it without relaxing the test. The conservative mix bound is **0.7596**, including .25 reserved music headroom. Actual source pitches: 1.0000, 1.0668, 1.1394, 1.2083, 1.2762, 1.3494, 1.4196, 1.4855, 1.0000, 1.0000; the additional first-Perfect restart is 1.0701. Tests assert one primary event per placement, only percussive clips in playing placement voices, bounded rapid overlap, no Cut sound, x8 clamp and exact reset. Intro duration/skip/retry tests still pass.

UIManager, HomePresentation, GameManager, all Gameplay scripts, MenuScreens and all Theme scripts retain their exact pre-task SHA-1 values. No secondary visual changes were made. Sound preference and phone-speaker translation still need the user's listening check; automated tests establish routing and signal behavior, not subjective satisfaction.

Review player: `/private/tmp/wobble-percussive-player/WOBBLE.app`. Test evidence: `Artifacts/Audio/Percussive-EditMode.xml`, `Percussive-PlayMode.xml`, `PercussiveRuntimePitch.txt`.
