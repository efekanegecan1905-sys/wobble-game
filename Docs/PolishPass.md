# WOBBLE — Carrara polish pass

This pass preserves the existing manager structure, overlap/cutting rules, score formula, four incoming directions and ten-block level cadence. No economy features or game modes were added.

## Movement

`MovingBlock` now reflects motion at both boundaries. Each frame's remaining travel is consumed after a reversal, retaining constant speed without a pause or wrap. The same GameObject, renderer, height and non-moving coordinate survive indefinitely. `Direction` continues to describe the incoming direction; `TravelSign` describes current motion along that axis. Placement freezes the current position before cutting.

The spawn sequence is still **+X, -Z, -X, +Z**. The travel corridor is now ±4.2 units from the current tower top, giving a quicker first approach and fitting portrait screens.

## Camera and presentation

- 27° camera elevation, 44° vertical field of view.
- Tracks the actual top in X/Y/Z, including the lateral drift from imperfect cuts.
- Targets 0.65 units above the top, anticipating the next slab.
- SmoothDamp follow: 0.16 seconds.
- Aspect-aware distance fits the complete slab at both ends of either axis.
- 24° orbit per level with a 0.95-second smooth transition around the active top.
- 0.40-unit slabs with small beveled upper/lower edges, preserving exact cutting extents and static colliders.
- Warm off-white Carrara, metallic 0, smoothness 0.27 (roughness approximately 0.73), irregular noise-warped veins, per-slab pattern offsets and restrained roughness variation.
- Neutral fill, softened directional shadows, gradient atmosphere and low-contrast distant stone silhouettes.
- Safe-area-aware portrait HUD. Perfect/combo feedback follows the top, rises slightly and fades; level text has its own animation so it cannot be overwritten by a perfect.

## Pace and feedback

- Initial speed: **4.5 units/second**.
- Gain: **+0.14** per successful placement.
- Cap: **8.0**.
- Level relief: **−0.65**, never below 4.5; first level transition produces 5.25.
- Every ten placements, orbit, lighting, fog and material tint transition together for **0.95 seconds**, without pausing the active block.
- Perfect: 0.22-second render-only slab punch, tiny camera impulse, warm flash/top sweep, pooled dust, cached procedural stone snap and light native mobile haptic.
- Falling offcuts retain marble, use side-dependent angular velocity and a small outward separation, then return to the pool after leaving view (five-second safety limit).
- Game-over results appear after 0.6 seconds so the missed slab has time to fall.

## Files changed or added

| Area | Files |
| --- | --- |
| Movement, slab and physics | `Assets/Scripts/Gameplay/MovingBlock.cs`, `StackManager.cs`, `BlockView.cs`, `FallingPiecePool.cs`, `LevelManager.cs` |
| Camera and orchestration | `Assets/Scripts/Camera/CameraController.cs`, `Assets/Scripts/Core/GameManager.cs`, `AssemblyInfo.cs` |
| Materials and environment | `Assets/Scripts/Themes/ThemeManager.cs`, `WobbleTheme.cs`, `Assets/Resources/Themes/Carrara.asset` |
| Shaders | `Assets/Resources/Shaders/Carrara.shader`, `Atmosphere.shader`, `StoneDust.shader` |
| Feedback and UI | `Assets/Scripts/Effects/EffectPool.cs`, `Assets/Scripts/UI/UIManager.cs`, `Assets/Scripts/Audio/AudioManager.cs`, `HapticManager.cs` |
| Native haptics | `Assets/Plugins/iOS/WobbleHaptics.mm`, `Assets/Plugins/Android/WobbleHaptics.androidlib/AndroidManifest.xml`, `project.properties` |
| Editor defaults | `Assets/Editor/ProjectBuilder.cs` (Carrara default colors only; configuration was not rerun) |
| Verification | `Assets/Tests/PlayMode/MovingBlockBounceTests.cs`, `WobblePlayModeTests.cs` |
| Review | `Artifacts/Previews/*.png`, this document |

Unity generates corresponding `.meta` files for new assets.

## Verification

Unity 6000.0.81f1: **12/12 Play Mode tests and 3/3 cutter Edit Mode tests passed** on this pass. Generated portrait previews are in `Artifacts/Previews/`.

The Play Mode suite covers exact boundary hits and reversals for all four incoming directions; overshoot without travel loss; large time steps; visibility/activation and object identity; six live bounces per direction; placement after bouncing; thirty placements through levels; top-five and movement-corridor framing at 9:16, 9:19.5 and 9:21 during multiple orbits; and Home → game → results with portrait renders. Tests restore the player's saved data after automated runs.

Open `Assets/Scenes/Wobble.unity`, enter Play Mode, and click PLAY. Let a slab travel to both edges before placing it. Click inside the game or press Space to place.

## Remaining limitations

The previews are Unity renders, not on-device captures. iOS/Android performance, safe-area cutouts and haptic strength still need physical-device verification. Native haptic code has not been compiled with mobile SDKs in this pass. Audio remains procedural rather than recorded marble Foley. Shadows provide contact depth; no screen-space ambient occlusion or post-processing package was added. Background detail is deliberately sparse. The marble is procedural, so very close-up views will not match scanned stone.
