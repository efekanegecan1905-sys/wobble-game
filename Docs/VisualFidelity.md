# WOBBLE — Carrara visual fidelity pass

## Scope and audit

Rendering-only migration. The saved Wobble scene was not rebuilt. The initial audit found Built-in rendering, gamma color space, no render asset/post stack, 2× MSAA in the selected quality tier, runtime shadow overrides (Medium / 65 units), normalized UVs on a runtime-generated 512² marble, and legacy UI text.

Movement, bounce, input, cut calculations, scoring, combos and level progression remain unchanged. StackManager/FallingPiecePool only gained stone-coordinate transfer between the two cut surfaces. Collider height and stack spacing remain 0.40 units; the mesh is visually 2% thinner with a tiny chamfer. Audio, haptics and save logic are unchanged.

## Rendering specification

1. **Pipeline:** URP 17.0.4, Forward rendering, Linear color space, HDR camera/intermediate buffers.
2. **Render scale:** 1.0 on every tier; camera dynamic resolution disabled; no low-resolution upscaling.
3. **MSAA:** High/Medium 4×; Low 2×. Camera MSAA enabled; no FXAA/TAA. A no-pass renderer probe observes URP's actual target descriptor in tests and development-player review.
4. **Camera:** 40° FOV; smooth existing follow/orbit timing. A rotation-invariant travel-corridor bound brings the camera closer without cropping full-width incoming slabs or changing direction rules.
5. **Shadows:** High 2048, Medium 1024, Low 512 directional atlas; two cascades, soft filtering. Only slabs within 3.6 vertical units of the active top cast. Distant scenery never casts.
6. **Shadow distance:** 48 world units; bias 0.35, normal bias 0.30. Runtime shadow-quality overrides removed.
7. **Texture:** 1254×1254 native authored marble, imported without resampling; 2048 import cap. This exceeds the requested 1024 minimum but is not a native 2048 master. No false resolution gain by upscaling. Android/iOS ASTC 6×6, approximately 0.89 MiB including mipmaps; desktop compressed texture approximately 1–2 MiB.
8. **Filtering:** Trilinear, mipmaps, anisotropy 8; Mirror addressing.
9. **Mapping:** Object-attached, metric triplanar coordinates at 0.18 tiles/world unit. Size updates crop the same pattern. Both cut halves inherit source coordinates; rigidbody rotation carries the texture with the stone. Two shared materials and property blocks, no per-piece texture copies.
10. **Post:** ACES, exposure +0.15, contrast +3, saturation −4; restrained HDR bloom (threshold 1.15, intensity 0.12) catches the short PERFECT sweep. Half-resolution subtle SSAO on High/Medium, disabled on Low. No motion blur, grain, chromatic aberration, DOF or vignette.
11. **UI:** TextMeshPro SDF (2048² static font atlas), Scale With Screen Size, 1080×1920 reference, match 0.5; safe area retained. Feedback follows the tower and clears the incoming corridor.
12. **Tiers:** High: 4×/2048 shadows/AO 0.38/full particles. Medium: 4×/1024/AO 0.24 with fewer samples. Low: 2×/512/no AO/half dust count. All retain full render scale and the same marble.
13. **Performance:** 60 FPS target, not a mobile-device measurement. Final Apple M4 standalone development-player review averaged 1.56–1.88 ms frame delta for the static 12-slab scene; see the table below. This is neither GPU timing nor a before/after cost measurement. Costs include three marble samples per fragment, 4× MSAA bandwidth, HDR/post, and half-resolution AO. Verify thermals and GPU timing on target phones before selecting a lower tier. 8× MSAA deliberately not enabled without a measured device budget.
14. **Changed files:** See inventory below.
15. **Limitations:** Native phone GPU, thermal, safe-area and driver validation still required. Generated master is 1254², not 2048². Mirror addressing avoids hard seams but can reveal symmetry on unusually large surfaces. Desktop development-player timings cannot establish midrange-mobile performance.

## Verification and use

- Open the existing `Assets/Scenes/Wobble.unity` scene; don't use the old scene-rebuilding Configure Project command.
- Rendering assets are committed project inputs; regeneration is optional through **WOBBLE → Rendering → Configure Fidelity Assets**.
- **WOBBLE → Rendering → Install Portrait Game View Presets** installs 1080×1920, 1170×2532 and 1080×2400 for Standalone/iOS/Android.
- PlayMode coverage includes all four repeated bounce directions, placement/scoring smoke, 30 placements and five camera rotations across portrait ratios, actual URP target size/MSAA, TMP/scaler settings, texture filtering and cut-coordinate continuity.
- Native-size review images and NUnit results are stored under `Artifacts/Fidelity`.
- Final validation: **13/13 PlayMode and 3/3 EditMode tests passed**. The standalone build completed successfully and its final runtime log contained no shader errors or exceptions. A shader-stripping issue found only in the first standalone run was corrected by making the plinth's shader a persistent Resources reference.
- `Wobble.Editor.FidelityPlayerBuilder.BuildReview` builds a separate macOS development player in `/private/tmp/wobble-fidelity-player`. Launch with `--wobble-visual-check` for offscreen native-resolution captures and 120 warmed-up frame samples per resolution. The opt-in harness is absent from release builds and restores the saved score after the run.

Final standalone observations (Apple M4, Mac16,12; uncapped offscreen camera, static 12-slab scene, 40 warmup + 120 sampled frames per size):

| Requested / actual target | Actual MSAA | Mean frame delta | p95 frame delta |
| --- | --- | --- | --- |
| 1080×1920 / 1080×1920 | 4× | 1.56 ms | 2.44 ms |
| 1170×2532 / 1170×2532 | 4× | 1.88 ms | 2.77 ms |
| 1080×2400 / 1080×2400 | 4× | 1.69 ms | 2.39 ms |

These short desktop samples do not establish sustained mobile FPS or the cost of active physics/effects. Native mobile hardware remains the release gate. Source evidence: `Artifacts/Fidelity/Standalone-render-verification.txt`, `PlayMode.xml`, `EditMode.xml`, and `Previews/player-*.png`.

## File inventory

- `Packages/manifest.json`, `Packages/packages-lock.json`
- `ProjectSettings/GraphicsSettings.asset`, `QualitySettings.asset`, `ProjectSettings.asset`
- `Assets/UniversalRenderPipelineGlobalSettings.asset`
- `Assets/Resources/Rendering/*`: pipeline assets, renderers, AO, post profile, marble, SDF font
- `Assets/TextMesh Pro/*`: official essential resources and font license
- `Assets/Resources/Shaders/Carrara.shader`, `Silhouette.shader`
- `Assets/Resources/Themes/Carrara.asset`
- `Assets/Scripts/Camera/CameraController.cs`, `FidelityController.cs`, `FidelityRenderProbe.cs`, `FidelityPlayerReview.cs`
- `Assets/Scripts/Core/GameManager.cs` (camera/lighting setup only)
- `Assets/Scripts/Themes/ThemeManager.cs`
- `Assets/Scripts/Gameplay/BlockView.cs`, `StackManager.cs`, `FallingPiecePool.cs` (rendered mesh / material coordinates only)
- `Assets/Scripts/UI/UIManager.cs`, `Assets/Scripts/Effects/EffectPool.cs`
- `Assets/Scripts/Wobble.Runtime.asmdef`
- `Assets/Editor/FidelityBuilder.cs`, `FidelityPlayerBuilder.cs`, `PortraitGameViewPresets.cs`, `MarbleTextureBaker.cs`
- `Assets/Tests/PlayMode/WobblePlayModeTests.cs`, `Wobble.PlayModeTests.asmdef`
- Associated new Unity `.meta` files; README, this report and verification artifacts

## Marble asset provenance

The imagegen skill informed replacement of an overly regular procedural draft with a natural mineral pattern. Built-in image generation was used, not a CLI/API-key workflow. Final workspace asset: `Assets/Resources/Rendering/CarraraNatural.png`.

Final prompt:

> Use case: photorealistic-natural. Asset type: PBR albedo texture for a premium mobile game, not a scene or mockup. Generate a square 2048 x 2048 pixel flat Carrara marble master texture filling every pixel edge to edge. Orthographic straight-on texture scan, completely uniform diffuse light, no lighting gradients, reflections, bevels or shadows. Warm almost-white ivory calcite base, sparse irregular soft light grey mineral veins with branching feathery fractured edges and tiny natural substructure, subtle cloudy micro variation. Authentic honed Italian Carrara stone, quiet and architectural. Vein coverage under 12 percent; most of the texture is beautiful clean ivory white stone. Natural non-repeating multi-directional veins, no regular stripes or sine waves, no strong black veins, no blue, no glossy ice or ceramic, no noise or grain effect. Ideally tileable with no obvious seams, no frame, no text, no logo, no watermark.

The tool returned 1254×1254; that actual size is preserved and reported above.
