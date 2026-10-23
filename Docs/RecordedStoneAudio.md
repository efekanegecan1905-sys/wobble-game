# Recorded stone impact replacement

The five runtime MarbleImpact WAVs now derive from actual stone-on-stone recordings, replacing the previous synthesized bodies. This is **recorded stone Foley**, not verified Carrara-marble Foley. The original creator labels the material as stone/concrete; no marble identity or recording equipment is inferred.

## Source and attribution

Creator: **thanvannispen (Than van Nispen tot Pannerden)**.
Series: stone_on_stone_impact_loud1.aif through loud5.aif, posted 27 January 2007.

- [Take 1 / 30008](https://freesound.org/people/thanvannispen/sounds/30008/)
- [Take 2 / 30009](https://freesound.org/people/thanvannispen/sounds/30009/)
- [Take 3 / 30010](https://freesound.org/people/thanvannispen/sounds/30010/)
- [Take 4 / 30011](https://freesound.org/people/thanvannispen/sounds/30011/)
- [Take 5 / 30012](https://freesound.org/people/thanvannispen/sounds/30012/)

All five source pages identified **[Creative Commons Attribution 4.0](https://creativecommons.org/licenses/by/4.0/)** at retrieval on 12 October 2026. Attribution, source URLs, license and modification notice are included in Resources/Audio/RecordedStoneCredits.txt and the in-game Settings → Audio Credits panel. The original creator does not endorse WOBBLE.

Public HQ MP3 links explicitly supplied by the source pages were used. These are lossy web renditions, **not the original lossless AIFF masters**. No login/download gate was bypassed. Source MP3s are retained under Assets/AudioAuthoring for reproducibility; retrieved source/license HTML is under Artifacts/AudioSources. They are not loaded by gameplay.

## Processing

StoneFoleyBuilder performs an offline mono fold-down, main-impact onset trim, fixed .82 playback ratio, 65 Hz high-pass/DC control, two-pole 1250 Hz high-frequency softening, broad 360 Hz weight reinforcement, soft saturation, 4 ms entry and 55 ms end fade. Each output is 280 ms, mono 44.1 kHz PCM16, peak .70. The body contains no generated tone or synthetic transient. Existing GUIDs/resource names are preserved.

All five takes use the same processing; their differences come from separate recordings. Runtime pitch variation is still only ±2%, not combo-linked. Normal remains base-only; the existing x2–x5+ support textures and Inspector settings are unchanged. Supporting body/resonance/room assets are still synthetic noise-based sound design, so the complete combo mix is a hybrid.

The original preview transients have very high source peaks (some reach 1.0). Processing bounds output and reduces harsh high-frequency content but cannot recover information lost to source clipping or lossy encoding. Do not describe these as pristine studio masters.

## Status

This replaces the synthetic main-body placeholder with a reproducible real-recording candidate. Automated spectrum, boundary and signal checks are not proof of premium subjective quality. User audition and phone-speaker verification remain necessary. If that character is not approved, obtain licensed polished-slab lossless recordings; the five resource slots accept direct replacements. The old synthesized bank is recoverable from the task-start snapshot, and the synthesis menu now refuses to overwrite installed recorded bodies accidentally.
