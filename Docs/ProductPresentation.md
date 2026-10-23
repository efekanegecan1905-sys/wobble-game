# WOBBLE — product presentation pass

11 October 2026 · Existing Unity 6 project, no gameplay rebuild.

## 1. Home reworked

Recomposed the previous Home: spaced WOBBLE wordmark, compact currency, Carrara subtitle, central sculpture, score above a wider stone-action button and restrained bottom navigation. Added grain, a polished edge, a lower bevel and an event-driven press spring to the primary menu controls. No rounded dashboard cards.

## 2. Startup architecture

The existing `HomePresentation` owns a single lightweight gallery. Six editor-baked extruded stone letters assemble on that stage, then give way to the Home sculpture. `UIManager` coordinates the black curtain, assembly, highlight sweep, twelve short dust particles, one signature hit and Home fade-in. No video, scene load, second menu manager or runtime mesh baking. Unity's default splash is disabled so it does not precede the custom identity sequence.

## 3. Startup duration

Custom animation: **2.7 seconds**; first 0.35 seconds nearly black, assembly approximately 0.4–1.5 seconds, signature at 1.9 seconds, Home reveal from 2.15 seconds. OS/process/asset cold-start time is additional. Development builds support `--wobble-skip-intro`; tests can call `SkipIntro()`. Retry never starts the intro.

## 4. Home 3D presentation

Eight overlapping Carrara slabs, existing bevel/material treatment, seven shared-mesh distant chamfered monoliths, slow rotation and a tiny camera drift. One separate camera at an isolated stage position; no extra gameplay scenes. Submenus reuse this stage.

## 5. Menu transitions

Existing `MenuNavigation` now cross-fades outgoing and incoming panels with an 18-unit vertical settle over **0.24 seconds**. Input is gated during entry. Home→Play clears the menu and pushes the gallery camera in over **0.58 seconds** before invoking the existing gameplay event. Rapid repeated Play taps cannot launch multiple transitions. Shop/Themes/Daily shift the gallery framing slightly; Settings reduces its rotation rate. Controls animate only during interaction, not with one idle Update per button.

## 6. Shop

Starter Pack is a featured area with its existing reward contents and price. Gems precede Lives and the remaining special product. Prices, purchase placeholders and the unavailable purchase button remain unchanged. No checkout or external purchase integration added.

## 7. Themes

Five textured isometric three-slab material studies replace the plain color squares. These are lightweight code-native 2.5D UI previews, **not five implemented 3D environments**. Carrara remains selected. Locked studies show their existing gem requirement and localized coming-soon status; unlocking logic is unchanged.

## 8. Settings / Daily

Compact settings rows retain their existing save, quality and language callbacks. Daily preserves all seven rewards and the local-time claiming rule; a narrow marker identifies the current day and Day 7 has a larger, subtly tinted area. No reward amounts changed.

## 9. Game Over

The existing slab fall and 0.6-second wait are preserved; no time-scale change. Result UI fades in over 0.28 seconds. A large localized score, best, perfect count and maximum combo replace the previous equal-weight summary. Retry transitions in **0.32 seconds**, directly to the same StartGame path. Home and the disabled future Continue hook remain.

## 10. Marble audio changes

Existing `AudioManager`, ten-voice effect budget and mixer reused. Physical body remains dominant; contact gain reduced from 0.095 to 0.025, support gain from 0.055–0.079 to 0.018–0.030. Variation narrowed to pitch **0.98–1.02** and volume **0.97–1.03**.

After the user explicitly requested “create that sound yourself,” the old modal/sine-wave clips were replaced with a new stochastic impact design: broad 210/350/660 Hz body bands, soft saturation, two-pole high-frequency softening, DC removal, a 9 ms soft onset and a 240 ms duration. The support is broad low-mid weight rather than a musical oscillator. Five seeds create distinct but closely related impacts. This follows the user's later instruction, which superseded the original prohibition on another synthesized candidate.

## 11. Foley status — unresolved release-quality requirement

**The bundled audio is newly designed synthetic stone-like audio, not real marble Foley.** It awaits perceptual/user approval. No downloaded recording is included. A [stone-slab search candidate](https://freesound.org/people/newlocknew/sounds/592581/) has a noncommercial license and a crunch character, so it was not imported. [BigSoundBank's recorded stone falls](https://bigsoundbank.com/chute-de-pierre-s1022.html) are described by their author as CC0, but depict 1–3 kg stones falling onto a stone pile, not a 10–20 kg polished marble slab. The attempted download returned an HTML download page rather than a WAV; no recording from it was installed. We do not claim its sound quality or licensing for any bundled file.

The project is ready for verified licensed stone-on-stone takes. The resource-name/metadata replacement contract is documented in `Assets/Resources/Audio/PLACEHOLDERS.md`. Phone-speaker listening and the user's auditory approval remain necessary.

## 12. Placement variations

Five newly designed body clips (**not five Foley takes**) and five contact layers; immediate body repetition is prevented. Room, scrape and four support clips were also regenerated with the new non-oscillator texture; all sixteen remain replaceable by the same resource names.

## 13. Combo audio

Same physical body for normal/perfect; perfect has a small body gain increase. Support grows subtly from x2 to x5, then clamps. No combo-linked movement speed or pitch increase. Existing physical dust, haptics and camera impulse remain; no new gameplay Flow mode.

## 14. Menu / splash audio

Selection uses a quiet designed body; panel entry uses the muted scrape; Play uses a stronger body; splash triggers one signature body hit. Routed to the existing **UI** mixer group, using the two bounded secondary voices. Sound mute is respected. These identity cues share the new sound family and await user approval.

## 15. Gameplay environment

Replaced the three runtime combined-cube shapes with shared editor-baked chamfered, tapered, fractured-crown meshes. Twenty-four existing instances, placement and visibility safety checks remain. No background colliders or shadow casters. Whole renderer bounds stay behind the active tower plane, preserving block visibility.

## 16. Level environment progression

Existing ten-placement level events, speed tuning, sky/fog/light progression and parallax are unchanged: Quarry → vertical Monument forms → Heights → distant Above. New silhouettes inherit the same continuous transitions; no scene loads, scoring hooks or additional difficulty changes.

## 17. Modified files

- `Assets/Scripts/UI/HomePresentation.cs`
- `Assets/Scripts/UI/UIManager.cs`
- `Assets/Scripts/UI/MenuScreens.cs`
- `Assets/Scripts/UI/MenuNavigation.cs`
- `Assets/Scripts/UI/MenuPlayerReview.cs` — delayed navigation expectations
- `Assets/Scripts/Audio/AudioManager.cs`
- `Assets/Editor/MarbleAudioBuilder.cs`, `Assets/Editor/FeelTests.cs` and the sixteen existing WAV assets
- `Assets/Scripts/Themes/CarraraWorld.cs`
- `Assets/LocalizationAuthoring/Wobble.tsv` and generated TR/EN String Tables/shared data
- `Assets/Tests/PlayMode/MenuPlayModeTests.cs`
- `Assets/Tests/PlayMode/WobblePlayModeTests.cs` — scope the existing placement test to its own StackManager/top instead of unordered global BlockView discovery; score assertions unchanged
- `ProjectSettings/ProjectSettings.asset` — default Unity splash flags only

All gameplay directory files, `GameManager`, gameplay camera, save/economy logic and MenuCatalog were compared with the task-start snapshot and preserved.

## 18. Added files

- `Assets/Scripts/UI/StoneButtonFeedback.cs`
- `Assets/Scripts/UI/StonePreview.cs`
- `Assets/Scripts/UI/ProductPlayerReview.cs` — explicit development-only verification
- `Assets/Editor/ProductPresentationBuilder.cs`
- `Assets/Tests/PlayMode/ProductPresentationTests.cs`
- `Assets/Resources/Presentation/` — six logo meshes and three architectural meshes
- `Assets/Resources/Audio/PLACEHOLDERS.md`
- This report and `Artifacts/Product/` verification outputs

## 19. Dependencies

No new packages, SDKs or external runtime assets. Existing URP, TMP, Unity Localization, marble texture, shaders, audio mixer and particle/audio pools reused. The isolated verification project is `/private/tmp/wobble-fidelity.quohnF`; the actual user project is not replaced.

## 20. Portrait validation

Actual development player ran at all three portrait render targets: **1080×1920, 1170×2532 and 1080×2400**. Forty-two captured images cover splash, Home/four submenus in TR/EN, gameplay and TR/EN results. No missing-translation, overflowing-text or shader-error-magenta failure was reported. Fast retry returned to play without replaying intro. These are desktop render targets, not physical phone/notch validation. Player artifacts are in `Artifacts/Product/Player/`.

## 21. Test results

Edit Mode: **14/14 passed** (13 WOBBLE tests plus the existing Addressables stub), including new-bank spectrum, peak, duration and variation checks. Final Play Mode: **20/20 passed**, zero failures/skips, 95.07 seconds, completed 11 October 2026 at 18:27 UTC. The result is recorded in `Artifacts/Product/PlayMode.xml`. On continuation, the workspace's scripts, tests and WAV files were verified against the tested isolated copy; no production changes followed the passing run. Existing gameplay, bounce, pace, menu, save and locale assertions are retained; startup bounds, no duplicate Play transition, portrait splash/results and fast retry were added.

One older test used unordered global BlockView discovery and intermittently selected a presentation/pool object rather than the active run's tower. It now uses that run's `StackManager.Top` and `ActiveBlock`; gameplay code was not changed to make it pass.

## 22. Performance / audio output

Apple M4 / Mac16,12 development player, 40 warm-up + 120 unthrottled frame samples per measured target:

| Target | Home mean / p95 | Gameplay mean / p95 |
|---|---:|---:|
| 1080×1920 | 1.41 / 2.15 ms | 1.56 / 2.92 ms |
| 1170×2532 | 2.37 / 3.69 ms | 2.08 / 3.07 ms |
| 1080×2400 | 1.64 / 2.17 ms | 1.67 / 2.21 ms |

The prior pass recorded gameplay means of 1.40 / 1.88 / 1.67 ms. This is **not a controlled before/after benchmark**: capture timing, framing and background load differ. No mobile 60 FPS guarantee follows from these short desktop samples. Geometry uses shared baked meshes; gameplay background is 24 × 32 = 768 triangles, no background shadows/colliders, no new reflection/volumetric system. Startup has six letter renderers and at most twelve dust particles; button springs have no idle Update.

The actual player's audio listener observed peak **0.1565** during twelve placements, below clipping. Playback and signal presence were exercised, but this is **not perceptual listening approval** or proof of phone-speaker character. The custom sound should be auditioned by the user; its source and synthesis are disclosed above.

## 23. Remaining placeholders

The newly designed sound family needs user listening approval; it is synthetic sound design, not Foley. Four locked themes are material studies, not complete themed worlds. Architecture/logo are original lightweight procedural-authored meshes, not hand-sculpted final art. IAP, ads and future Continue remain the already existing placeholders. No claim that this pass alone makes the product release-ready.

## 24. Recommended release polish

Audition and approve the five newly designed impacts and signature/UI family on actual phone speakers. Licensed Foley remains an optional drop-in alternative if the designed sound is not approved. Profile sustained play on a mid-range iPhone and Android device, including thermal load and safe areas/notches. Obtain visual art-direction approval of the gallery and logo, test touch accessibility on-device, and separately complete the existing commerce/ads/privacy/store-release work. None of those integrations were silently added here.

## Handoff

Playable development build: `/private/tmp/wobble-product-player/WOBBLE.app`. Review images, XML reports and measured audio/performance output: `Artifacts/Product/`. Handoff verified 12 October 2026; no gameplay refactor or economy change.
