# Hybrid premium arcade + stone impact

Audio-only continuation. Visuals, UI, environment, theme demo and gameplay were not modified. Existing two four-layer banks, voice budget, random non-repeating stone variations, mixer routing and combo profile selection remain.

## Sources and processing

- Stone body: existing five recorded stone-on-stone impacts by thanvannispen, CC BY 4.0. See `RecordedStoneAudio.md`. Retained, not regenerated or relabeled CC0. Carrara material is unverified.
- Accent: [Plocks Five by Jerimee](https://freesound.org/people/Jerimee/sounds/723897/), CC0. Source evidence saved in `Artifacts/AudioSources/723897.html`; public HQ MP3 in `Assets/AudioAuthoring/723897.mp3`. This is not a lossless master. The first of five examples was extracted, slowed to .90, high-passed at 100 Hz and twice low-passed at 1700 Hz; 4 ms attack, 35 ms end fade, 95 ms total, peak .70. No new synthetic primary impact.
- Optional resonance: existing synthesized `ComboResonance.wav`, only .008 / .015 gain at x4 / x5. Ambience disabled.
- OpenGameArt Impact and Stone Door references were inspected; Stone Door is a door effect, not selected for the short placement. No unverified license or original download was assumed.

## Inspector tuning

Open `Assets/Resources/Audio/CarraraImpactProfiles.asset`. Each profile exposes intensity, base stone volume/pitch/tail, **Arcade Plock** volume/pitch/tail, resonance and ambience. The plock field retains its serialized `comboBody` name to preserve the existing architecture.

| Profile | Stone gain | Accent gain share | Pitch | Resonance gain |
|---|---:|---:|---:|---:|
| Normal | .46 | 8% | 1.000 | 0 |
| Combo2 | .46 | 12% | 1.025 | 0 |
| Combo3 | .46 | 16% | 1.050 | 0 |
| Combo4 | .48 | 20% | 1.075 | .008 |
| Combo5Plus | .50 | 22% | 1.100 | .015 |

Shares are relative linear fader gains, not measured perceived loudness. Both main stems peak at .70; stone lasts longer. Pitch jitter is zero so x5 never randomly exceeds 1.10. Existing small shared volume variation is .015. Combo selection clamps at x5. A break clears old secondary tails and plays the Normal stone + quiet plock, not a silent accent slot. A new Perfect starts Combo2.

## Exact replacement slots

- `Assets/Resources/Audio/ArcadePlock.wav`: **real downloaded CC0 source**, not a generated placeholder; optional lossless replacement, mono 44.1 kHz PCM, about 60–100 ms, peak at or below .70.
- `Assets/Resources/Audio/MarbleImpact_01.wav` through `_05.wav`: existing recorded stone, not synthetic placeholders; optional verified slab/CC0 replacements should preserve filenames and metadata.
- `Assets/Resources/Audio/ComboResonance.wav`: **synthesized supporting placeholder**; replace with a subtle short stone tail if desired, or set its profile volume to zero.
- `ComboBody.wav` and `MarbleRoom.wav` remain for compatibility, but are not active placement layers in the new profiles.

`HybridImpactBuilder.Build` is an explicit authoring action that retunes the profile asset; it never overwrites an existing ArcadePlock WAV. Normal game startup does not generate or retune audio. Audition: `Artifacts/Audio/HybridComboAudition.wav`, Normal → x2 → x3 → x4 → x5 → x8 → Normal → x2. Listening on physical phone speakers is still required; automated signal tests cannot certify subjective satisfaction.

## Validation

12 October 2026: **17/17 Edit Mode** and **23/23 Play Mode** tests passed. Tests verify five gain shares and pitches, normal accent after combo break, x5 clamp, Inspector layer tuning/tail duration, conservative mix headroom, unchanged gameplay and existing theme-demo behavior. Evidence: `Artifacts/Audio/Hybrid-EditMode.xml` and `Hybrid-PlayMode.xml`. Review build target: `/private/tmp/wobble-hybrid-player/WOBBLE.app` (no theme-demo flag).
