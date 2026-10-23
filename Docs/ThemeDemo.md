# Theme unlock demo

Launch the development player with `--wobble-theme-demo`. Release players ignore this switch. The latest presentation is retained; only demo access, theme tint selection and an unlock animation are added.

- Wallet displays DEMO; gems are unlimited without modifying the stored balance.
- Themes start with Carrara owned. Click each other card to unlock it at its displayed catalog price without spending gems.
- A 650 ms gold pulse and scale animation runs before ownership and selection update. Repeat clicks are ignored during the animation.
- Selected tint applies to the shared stone materials in Home and gameplay, and survives level environment transitions. Carrara restores its original tint.
- Existing theme previews are tint studies of the same Carrara texture, not separate authored material or environment packs.
- Save writes use an in-memory store for the entire demo session. Existing preferences and scores are initially read, but demo changes never write PlayerPrefs. Unlocks reset on relaunch.
- Normal launches retain the existing locked/coming-soon catalog. No production purchases or monetization were added.

Build with `Wobble.Editor.ConceptPresentationBuilder.BuildThemeDemo`; output `/private/tmp/wobble-theme-demo/WOBBLE.app`.

Validation: 23/23 Play Mode tests passed on 12 October 2026, including all four unlock animations, repeat-click gating, tint persistence across level changes, unlimited spending and unchanged PlayerPrefs. Evidence: `Artifacts/Concept/ThemeDemo-PlayMode.xml`.
