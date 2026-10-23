# WOBBLE

A production-oriented Unity 6 baseline for a fast, one-tap, four-direction 3D stacking game.

## Open and play

1. Open this folder in Unity Hub with Unity **6000.0.81f1** (or a compatible Unity 6 release).
2. Open `Assets/Scenes/Wobble.unity`.
3. Press Play. Click/tap anywhere or press Space to place the moving block.

The scene and rendering assets are already configured. Do not run the older **Configure Project** command on this existing project: it recreates the scene and baseline themes.

## Visual fidelity

URP/Linear, native render scale, High 4× MSAA, cut-stable marble and TextMeshPro are configured. Install the three portrait review presets with **WOBBLE → Rendering → Install Portrait Game View Presets**. Rendering-only asset setup is available separately under **Configure Fidelity Assets** and does not rebuild the scene.

See [the rendering report](Docs/VisualFidelity.md) for settings, verification, quality tiers and device-testing limitations.

## Mobile

The project is portrait-first and configured for iOS 13+ and Android API 26+. Switch platform in Build Profiles, add `Assets/Scenes/Wobble.unity`, and build normally.

## Controls

- Touch/click/Space: place block
- UI: Play, Retry, Home, and haptics toggle

All scene content is bootstrapped at runtime, so the scene remains deliberately minimal and portable.
