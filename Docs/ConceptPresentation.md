# WOBBLE — concept-driven presentation

12 October 2026. Existing Unity 6000.0.81f1 project; no gameplay rebuild.

## 1–3. Reference inspection and adoption

Inspected the five requested repositories' published structure and documentation before implementation:

| Reference | Approach used in WOBBLE | Intentionally not imported |
| --- | --- | --- |
| [Unity UI Toolkit Design System](https://github.com/sinanata/unity-ui-toolkit-design-system) | Central semantic colors, shared surface treatment and typography roles | Dark theme, UI Toolkit migration, component package |
| [Unity Mobile Gamebase](https://github.com/Laputa-Unity/unity-mobile-gamebase) | Preserve separate presentation, catalog, save and reward responsibilities | Singleton architecture, gameplay, data format, SDKs |
| [PurrUI](https://github.com/PurrNet/PurrUI) | Keep view entry/exit and input gating within the existing navigation owner | ViewStack package or a second menu manager |
| [Tab-based menu](https://github.com/enessayaci/unity-ui-toolkit-tab-based-menu-system) | Shared header/content offsets, consistent navigation controls | UXML/USS implementation or replacement screen system |
| [UnityGameTemplate](https://github.com/Team-on/UnityGameTemplate) | Check screen/settings organization and multi-resolution review approach | Entire template, ProjectSettings, localization and runtime managers |

These were design references, not source-code transplants. Existing Canvas/TMP/Unity Localization remains the implementation.

## 4. Concept fidelity

The supplied eight-screen image was the primary reference for sky/cream/coral/lavender colors, Home hierarchy, central marble stack, round plinth, quarry arch, wallet chips, bottom navigation, three-column cards and a translucent results plaque. Screens share the same world and tokens.

This is a real-time, lightweight interpretation, **not a pixel match**. The concept's sculpted ruins, dense foliage, cinematic cloud detail, ornate gem/chest illustrations and polished stone-letter logo are not reproduced at that fidelity. Geometry and reward icons remain deliberately simpler. The result still needs art-direction approval; this report does not certify App Store-ready art.

## 5. Startup

Custom intro **2.7 → 4.0 seconds**, excluding process/asset startup. Navy-grey fades into the bright world; existing six 3D Carrara letters assemble, sweep and trigger one signature impact at 2.7 seconds. Home fades in during 3.2–4.0 seconds. Retry does not replay startup.

## 6. Home

Eight alternating wide/narrow, slightly offset marble slabs replace the uniform broad stack. Shared marble terrace geometry, a curved voussoir arch, two circular plinth tiers, restrained cypress/coral garden forms and slow oscillating camera/sculpture motion provide a real 3D centerpiece. Home displays large WOBBLE/CARRARA at top-left, wallets at top-right, coral OYNA, best-score plaque and four cream navigation buttons. Background geometry has no colliders or shadow casting.

## 7. Palette

`WobblePalette.cs` centralizes Primary, PrimaryPressed, Secondary, Accent, Success, TextPrimary, TextSecondary, Surface, SurfaceAlt, BackgroundTop/Bottom, SkyTop/Bottom, FogNear/Far, StoneLight/Mid/Shadow, GemColor, LifeColor, Warning and Danger. ThemeManager, world, menus and results consume these tokens. Post saturation changed from -4 to +8, exposure .15 to .25. No ProjectSettings replacement.

## 8. Typography

Two active families: **Lilita One** for display/headings/CTA/numerics and **Noto Sans** for body/support. Lilita One downloaded from the official [Google Fonts source](https://github.com/google/fonts/tree/main/ofl/lilitaone), with its SIL OFL included in Assets/Fonts. Existing licensed Noto Sans reused. Static TMP atlases include the localization corpus; Noto provides display fallback. Older unused font assets remain for compatibility.

## 9. Icons

Existing code-native icon component extended: filled faceted purple gems and coral hearts; bold consistent navigation strokes. No emoji or raster icon pack. Shared nine-sliced, gently shaded rounded surfaces, light rims and small shadows give cards/buttons consistent depth.

## 10–13. Subscreens

- Shop: cream featured Starter Pack, large reward icon, three-column product cards, coral price labels; existing products, placeholder prices and unavailable checkout unchanged.
- Themes: three-column marble material studies with name, selected/locked status and existing gem requirement. The five existing themes remain; no invented unlocks or substitution of the concept's example catalog.
- Daily: 3 + 3 + wide Day 7 rhythm, colored reward icons, warm gold seventh tile and coral claim action. All seven rewards, claim eligibility and save behavior unchanged.
- Settings: cream controls with readable label/value hierarchy and coral state emphasis. Existing sound/music/haptic toggles, graphics selector and language popup remain functional. No fake sliders or unsupported legal links were added.

## 14. Transitions

Existing menu cross-fade/18-unit settle **.24 s** and Home→Play **.58 s** retained. Retry **.32 s** retained. Results→Home now fades over **.24 s** with repeat-input gating. Section camera framing remains subtle.

## 15. Gameplay world

Brighter blue-to-pastel sky, cheap analytic cloud banks, warm fog, rebalanced ambient light, cream plinth and taller quarry silhouettes. Existing 24-instance architecture, behind-the-active-tower visibility guard, parallax and continuous ten-block level moods remain. No scene reload or camera-control redesign.

## 16–17. Audio

The already completed five-profile combo system is preserved, not rebuilt: Normal base only; x2 body; x3 body/resonance; x4 fuller resonance; x5+ strongest body/resonance with tiny ambience. x5 clamps, a break cancels enrichment, and every layer's gain/pitch/tail remains Inspector-configurable.

On continuation, the five synthetic main impacts were replaced with **real recorded stone-on-stone Foley** from thanvannispen's loud1–5 series, licensed CC BY 4.0. Public HQ MP3 renditions were processed offline: onset trim, mono, fixed .82 playback ratio, low-mid weight, softened attack, controlled fade, 280 ms duration and .70 peak. Attribution is available in Settings → Audio Credits and bundled with the project. See [RecordedStoneAudio.md](RecordedStoneAudio.md) for the five exact sources, license, processing and source-quality limitations.

These are **not verified Carrara marble recordings or lossless studio masters**. Combo support remains synthesized noise texture; no combo profile was rebuilt. Phone-speaker listening and subjective approval are still required. Existing placement punch, dust, contact feedback and haptics remain synchronized through their existing placement event.

## 18. Game Over

Background remains visible under a light veil. Cream display heading, rounded translucent dark result plaque, coral Retry and cream Home use the same design language. Score, best, perfect count and maximum combo still use the localized summary. The inactive future Continue control is hidden; no continue mechanic was added. Existing fall/.6-second wait and .28-second result entry preserved.

## 19. Changed files

- Scripts/UI: HomePresentation, MenuScreens, MenuIcon, UIManager, ProductPlayerReview.
- Scripts/Themes: ThemeManager, CarraraWorld.
- Scripts/Core/GameManager: **only the plinth material color expression**; no simulation changes.
- Resources/Shaders: Atmosphere, ArchitecturalStone.
- Editor/MarbleAudioBuilder: guard against accidentally overwriting installed recorded bodies.
- Five Resources/Audio/MarbleImpact WAVs; AudioManager provenance comment only.
- Localization authoring and generated TR/EN/shared tables: Audio Credits label.
- Resources/Rendering/CarraraPost.asset.
- Tests/PlayMode: ProductPresentationTests (4-second intro timing), MenuPlayModeTests (.24-second Home transition).

## 20. Added files

- Scripts/Themes/WobblePalette.cs; Scripts/UI/CasualSurface.cs.
- Editor/ConceptPresentationBuilder.cs; Editor/ConceptPresentationTests.cs.
- Fonts/LilitaOne-Regular.ttf and LilitaOne-OFL.txt.
- Resources/Menu/WobbleDisplay.asset and WobbleBody.asset (with embedded TMP atlas/material assets).
- This report and Artifacts/Concept validation outputs; associated Unity metadata.
- Editor/StoneFoleyBuilder; AudioAuthoring/30008–30012 MP3s; Resources/Audio/RecordedStoneCredits.txt.
- Presentation/QuarryGarden, ColumnGarden and ArchGarden baked shared meshes.
- Resources/Shaders/ConceptSky.hlsl: identical sky/cloud calculation for skybox and distant-geometry blending.
- Docs/RecordedStoneAudio.md and Artifacts/AudioSources source/license evidence.

## 21. Performance scope

No new scene, UI framework, package, volumetric pass or real-time light. One cached 64×64 rounded sprite, shared stone materials, existing two placement banks, bounded VFX and 24 gameplay background instances. Baked terraces, three-column groups and arches replace single-pier silhouettes without adding renderers; maximum 352 triangles per instance. A single marble-grain sample adds background surface detail. Home adds static simple geometry; background shadows/colliders are disabled. Screenshots and short desktop frame samples are not mid-range iOS/Android 60 FPS certification. On-device sustained profiling remains required.

## 22. Remaining placeholders

Recorded stone bodies still need subjective marble-character approval and ideally lossless polished-slab masters; supporting audio remains synthetic. Other placeholders: simple code-authored ruins, foliage and reward symbols; locked theme studies rather than full worlds; existing IAP/ads/future hooks. No payment, analytics or policy integration was added. Presentation fidelity and physical-phone listening still require approval.

## 23. Validation

Final test/player results are recorded below after the last build. Review covers TR/EN and 1080×1920, 1170×2532, 1080×2400. Automated images cover splash, Home, all submenus, gameplay and results; player flow exercises Play and fast Retry. Audio output is a signal test, not perceptual approval.

Final validation, 12 October 2026:

- Development macOS build succeeded with the final shared sky shader and recorded Foley.
- Edit Mode: **17/17 passed** after the recorded-Foley/environment changes.
- Final Play Mode: **22/22 passed**, completed at 12:14:02 UTC, after the last sky-shader change.
- Standalone review: **60 captures** across the three portrait resolutions, including TR/EN Audio Credits and all four gameplay environment moods. No reported overflow failures; fast retry without the intro passed.
- Apple M4 desktop Home mean frame samples: **1.81 / 2.37 / 2.08 ms**; gameplay: **1.46 / 2.00 / 1.75 ms**, in the resolution order above. These short development samples do not establish mobile performance.
- Observed audio listener peak **0.2392**, without a reported signal failure. This is not a listening-quality judgment.
- Evidence: `Artifacts/Concept/EditMode.xml`, `PlayMode.xml`, `Player/verification.txt`, `Player.log`, and the `Player` screenshots.
- Listening sample: `Artifacts/Concept/ComboFoleyAudition.wav` — Normal → x2 → x3 → x4 → x5 → x8 → Normal → x2. The x8 step demonstrates clamping; the following Normal demonstrates a combo break.

Final image inspection confirms that the earlier vertical cloud banding is removed and the shared cream/coral/blue presentation is consistent. **This is not a pixel-matched or final-art recreation of the concept.** The simple foliage, architectural detail, lighting depth and largely open gameplay sky remain visibly below the supplied concept's art fidelity. No claim of App Store readiness, verified Carrara Foley, or physical-device audio approval is made.
