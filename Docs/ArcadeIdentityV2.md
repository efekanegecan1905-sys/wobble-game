# WOBBLE — Bright Architectural Arcade V2

Unity 6000.0.81f1 · `feature/arcade-modes` · base `f408acd`.

## Presentation

The four-card grid is replaced by a full-screen, single-stage showcase. Horizontal swipes, arrow buttons and direct page dots select a mode. The layout presents one animated marble tower, a large title, a short description, a separate record/progress line, and a coral action button. Content fits the screen safe area; trial selection, results and gameplay use persistent UI objects.

Palette: Sky Mist `#E7F5F7`, Cloud Cream `#FFF7E8`, Deep Ink `#152B3F`, Play Coral `#FF765C`, Reward Gold `#F7C96D`, Soft Aqua `#48B4CE`, Balance Violet `#7B70D6`. Nunito Black remains the display face and Inter SemiBold the body face. Primary font atlases are checked for every visible character, including Turkish glyphs; no new font/package is introduced.

Motion uses the existing unscaled coroutine approach and `StoneButtonFeedback`: 120 ms button feedback, 340 ms directional/parallax mode switch, 220 ms title reveal, 200 ms outgoing play/retry/next transition, five-second preview cycles, staggered 350 ms result stars and a delayed reward reveal. Rapid navigation cancels and resets in-flight motion. Launch buttons reject duplicate submissions while transitioning.

## Mode identity

- **Classic:** rising marble preview and aqua accent. Existing gameplay, scoring, combo, HUD, cutting, audio and economy remain the reference behavior.
- **Rush:** fast travelling preview block and animated countdown; dedicated timer-first HUD, remaining miss marks, time bar, TURBO state and final-ten-second beat. Coral light/plinth reactions complement existing short countdown camera impulses. Existing speeds are exposed as tuning values (5.5 / 7.5).
- **Balance:** leaning/recovering preview with a vector stability dial. Gameplay uses placement-driven balance with a damped visual spring (70 stiffness / 13 damping), coherent height-dependent bend, and a responsive danger/recovery instrument. Only render children move: slab collider roots and the cutting footprint stay unchanged. No independent slab rigidbodies or random collapse were added.
- **Trials:** finish gate, rising tower, three illuminating preview stars and the existing quiet menu percussion on the victory beat. Separate objective sheet shows block count, Perfect chain, remaining time and minimum width, with objective milestones. Trial index chooses the initial environmental stage. Completion reveals saved stars and a first-clear reward message; retry/next buttons fade into the next run. Existing ten levels, star calculation, progression and exactly-once five-gem reward remain intact.

`ArcadeStyle`, `ArcadeHUD`, `ArcadeGauge`, `ArcadePreview` and `ArcadeModeUI` own presentation. `ArcadeTuning` exposes mode speed/spring parameters. Classic never enters the Balance spring branch or mode-specific environmental accent branches.

## Captures and validation

`Artifacts/ArcadeV2/` contains real Unity Game View captures, including overlay UI after world post-processing. The helper handles Metal's texture orientation, selects exact pixel resolutions, avoids stealing window focus, and restores the previous Game View size after each test. Earlier camera-space captures were replaced because they incorrectly tone-mapped overlay colors.

- `Showcase-{0..3}-{tr|en}-{1920|2532|2400}.png`: all four selections at 1080×1920, 1170×2532 and 1080×2400.
- `HUD-{1..3}-{tr|en}-{1920|2532|2400}.png`: dedicated gameplay HUD localization/overflow matrix.
- `Classic.png`, `Rush.png`, `Balance.png`, `Trials-objectives.png`: gameplay.
- `Rush-final10.png`, `Balance-danger.png`, `Balance-recovery.png`, `Trials-completion.png`: key moments.
- `TrialLevels-{tr|en}.png`, `Selection-tablet-{tr|en}.png`: level navigation and additional tablet layout.
- `Validation.json`: final test results and capture inventory.

Tests cover shared Classic behavior, Rush miss/life/record isolation, actual placement balance, unchanged collider bounds, first-clear reward idempotency, 20 theme/mode combinations, rapid navigation reusing the same preview, launch while time scale is zero, counterbalance with alternating movement axes, text overflow and primary-font glyph coverage.

## Performance and scope

One selected preview camera and one 768×768 render texture replace four preview instances. The stage reuses shared meshes and three materials. Hidden stages disable their camera; switching modes allocates no new stage/camera/texture. Persistent UI pages and vector gauge geometry avoid per-navigation object creation. Preview shadows, HDR, MSAA, post-processing, opaque and depth copies are disabled. Existing pooled gameplay effects are reused; no new heavy effect or third-party package is installed.

The application retains its 60 FPS target. This is an Editor validation pass, **not** a measured iPhone GPU/frame-pacing result. Physical-device FPS, thermal behavior, touch safe areas and subjective motion/audio review remain recommended. No device install is claimed.

Permanent bundle ID remains `com.wobble.game`. Five themes, saves, purchase/economy behavior, approved percussive sound assets and main branch are preserved.

## Guidance used

Installed official Unity plugin 0.1.10-beta: `ui-ugui`, `optimize-text-mesh-pro`, `urp-postprocessing`, `unity-cli`, and compatible collision-isolation guidance from `fix-3d-physics-collisions` (the installed replacement for `physics-3d-collision`). No new skills installation or repeated purchase/audio audit was needed.

Motion references reviewed: [PrimeTween](https://github.com/KyryloKuzyk/PrimeTween) and [Unity UI Extensions](https://github.com/Unity-UI-Extensions/com.unity.uiextensions). Existing coroutine/tween facilities already support the needed cancellation and timing, so neither library was imported.

## Test outcome

- EditMode: **31 passed / 31**.
- Full PlayMode: **38 passed, 1 expected demo-only skip / 39**; zero failures.
- The full rerun passed the two earlier failing legacy checks after Game View capture isolation was fixed. No legacy gameplay or demo-unlock code was changed to suppress those failures.
- The final Trials HUD copy avoids a `1 / 0` chain target when no chain is required, and displays current width alongside the minimum. A subsequent arcade integration run validates this final presentation and regenerates its captures (see `Validation.json`).

Final arcade regression after the last HUD correction: **9 / 9 passed**, zero failures. All 56 captures were regenerated and inventoried with SHA-256 hashes.
