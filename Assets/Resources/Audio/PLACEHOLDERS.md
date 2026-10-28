# Audio provenance and replacement contract

## Current placement identity — approved WOBBLE combo pack
Every successful placement plays exactly one clip from the approved pack in `Assets/Resources/Audio/Placement/`: `Wobble_Normal.wav` and `Wobble_Combo_x2.wav` … `Wobble_Combo_x8.wav`. The rising pitch progression is baked into the files and is never shifted at runtime. No stone, marble, ArcadePlock, ComboBody or ComboResonance layer is scheduled by the placement event; the legacy PercussivePlacement_01–05 bank has been removed. See Docs/ComboPlacementAudio.md for routing and reset rules.

## Current main impact bank
The five MarbleImpact_01–05 WAVs now derive from real stone-on-stone recordings by thanvannispen, licensed CC BY 4.0. They replace the earlier synthetic bodies. RecordedStoneCredits.txt contains attribution, source links, license and modification notice; the game exposes it under Settings → Audio Credits.

Sources are public HQ MP3 renditions of stone_on_stone_impact_loud1–5.aif, not the original lossless AIFFs. Material is recorded stone, not verified Carrara marble. Processing is deterministic offline trim, mono, fixed .82 playback ratio, high-pass, two-pole high-frequency softening, low-mid weight, saturation and fades. Runtime files are 280 ms PCM16 mono 44.1 kHz, peak .70. See Docs/RecordedStoneAudio.md for details and limitations.

## Supporting assets
ArcadePlock.wav derives from Jerimee's CC0 Plocks Five public HQ MP3, shortened and filtered to 95 ms. It is a downloaded source, not a synthetic placeholder. Normal now plays stone + 8% plock gain share, increasing to 22% at x5, with pitch 1.00–1.10. ComboResonance remains a synthesized supporting placeholder used very quietly at x4/x5. ComboBody, MarbleRoom, contact/scrape and legacy PerfectTone remain for compatibility; ComboBody, MarbleRoom and PerfectTone are not active placement layers. See Docs/HybridImpactAudio.md for exact drop-in WAV slots.

## Designer/release contract
Preserve WAV .meta GUIDs when replacing takes. Use closely related short stone-on-stone recordings with controlled peaks and retain any required attribution. Re-run spectrum/headroom and Play Mode tests after replacement. StoneFoleyBuilder reproduces the new main bank; MarbleAudioBuilder refuses to overwrite an installed recorded bank. ComboAudioBuilder preserves existing profile tuning.

Phone-speaker listening and user approval remain release gates. Quantitative spectrum/headroom tests do not establish premium sound or prove a marble material. Do not claim this hybrid as verified Carrara Foley.
