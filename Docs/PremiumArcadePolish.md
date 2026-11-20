# Premium arcade presentation

Continues `a15d21c` on `feature/arcade-modes`. The approved `ArcadeModeUI.cs` and `ArcadePreview.cs` are unchanged. Bundle ID remains `com.wobble.game`.

## Typography and captures

Compared Nunito/Inter, Fredoka/Rubik, Rubik/Rubik and Bricolage/Rubik in the actual composited Unity Game View (`Artifacts/PremiumArcade/Font-*.png`). Selected Bricolage Grotesque 650 for celebrations and Rubik 600 for gameplay HUD: expressive titles with stronger numeric hierarchy and consistent Turkish letters. The inspected Fredoka source lacks Ğ, İ, Ş, ğ and ş; it is retained only as an Editor comparison, not a shipping fallback. Home and mode-selection fonts are unchanged.

Official sources: [Fredoka](https://github.com/google/fonts/tree/main/ofl/fredoka), [Rubik](https://github.com/google/fonts/tree/main/ofl/rubik), [Bricolage Grotesque](https://github.com/google/fonts/tree/main/ofl/bricolagegrotesque). OFL licenses accompany the fonts. Static instances were generated from these variable fonts with fontTools. Two static 1024² SDF atlases contain the gameplay character set including Turkish letters; no runtime atlas growth or fallback is needed for new gameplay labels. `GameplayFontBuilder.Build()` recreates missing assets.

## Gameplay presentation

- Only gameplay BlockViews opt into stronger top/side contrast and active-slab edge emphasis; preview shader defaults remain zero. No dimensions, cutting equations or colliders changed.
- Camera follow smoothing changes from 160 to 130 ms; the existing corridor-fit constraints remain. Zero-delta-time frames skip SmoothDamp to avoid invalid camera coordinates during pause/capture. The balance needle has the same guard.
- Explicitly rebind the TMP atlas material when changing gameplay fonts; the original Classic HUD had outlined material instances that otherwise retained the old atlas. Tests now check atlas/material agreement. Rush and Balance helper copy has a cream backing for dark themes.
- Rush cues at 15, 10, 5, 4, 3, 2, 1 and 0 seconds use existing percussion, subtle camera impulses, readable nonblocking text and staged coral accents. Timed Trials share expiry presentation. Cue animations use unscaled time but never pause the run clock.
- The timer accumulates in double precision and clamps sub-10-microsecond float-step residue at expiry. Expiry is resolved before that frame's input. Late placement callbacks cannot score; results are guarded by game state.

## Genuine records and replay

`RunRecord` snapshots the prior persisted record at run start. Strict improvement is required; equal/lower values do not claim a record. Classic, Rush and Balance retain their separate existing score records. Trials compare earned stars for the selected level; no new time/score persistence was invented.

`RunCelebration` owns presentation only. Classic keeps its existing result/economy/continue flow with a gold record title, prior/best comparison and shared sparks. Rush counts up its final score. Balance centers its result gauge and visual tower spring. Trials reveal stars 170 ms apart and show +5 gems only for the existing first-clear reward. Replay and next-level buttons work while animations run. Near-record results use encouraging text.

Effects allocate once at setup: 18 reusable UI sparks, reusable cue and result panels. No added per-placement Instantiate/Destroy, packages, post-processing passes or audio assets. Static atlases add approximately 2 MiB of raw alpha texture data before platform overhead. Existing rendering, pooled world effects and audio voices are reused. Physical iPhone GPU/frame-time profiling was not performed for this pass.

## Validation

New EditMode checks cover strict records, per-level stars, timer accumulation, late scoring and countdown deduplication. New PlayMode checks cover final input boundaries, duplicate results/rewards, persisted stars, fast retry, all five themes, both locales, missing glyphs, overflow and portrait layouts. Captures are real Game View renders, not mockups. Full suite results and capture inventory are stored alongside representative screenshots in `Artifacts/PremiumArcade`.

Validation on Unity 6000.0.81f1: 38/38 EditMode passed. The full 44-case PlayMode run passed 41, skipped one existing demo-only test, and caught two atlas regressions. After the final cached-material repair, all five presentation tests passed (including both failures); combined coverage is 43 passing PlayMode cases and one skip. The full run and final focused rerun are retained separately rather than presenting a merged result as one test execution. Inspected the corrected Classic glyphs, dark-theme helper contrast and time-up/result separation in fresh captures. There are 102 PNG captures with dimensions and SHA-256 hashes in `capture-inventory.json`.

The font comparison, Classic gameplay/record, Rush stages and results, Balance danger/recovery/record, and Trials objectives/improved stars/first-clear/expiry are in that directory. Rush is captured at 1080×1920, 1170×2532 and 1080×2400 in TR/EN; the existing integration suite also covers all mode HUDs at those sizes.

Official installed Unity guidance used: `using-unity`, `ui-ugui`, `optimize-text-mesh-pro`, `unity-cli` and its build/test/verification workflow. Reviewed [LitMotion](https://github.com/annulusgames/LitMotion), [UIEffect](https://github.com/mob-sakai/UIEffect), [UI particles](https://github.com/mob-sakai/ParticleEffectForUGUI) and [Cinemachine](https://github.com/Unity-Technologies/com.unity.cinemachine); existing WOBBLE infrastructure sufficed, so none was imported.
