# Theme environments and mobile framing — 23 October 2026

The existing five `WobbleTheme` ScriptableObjects now each contain an Inspector-editable
`environment` profile. Fields: skyTop, skyBottom, fogNear, fogFar, ambientTint,
keyLightTint, distantStructureTint, accentTint, optionalParticleTint, accentStrength.
The theme assets remain the single source of truth; there is no second theme manager.

`ThemeManager.SelectTheme` loads the selected theme into shared runtime materials and
blends its environment over 2.4 seconds. GameManager reads the saved theme when entering
a run; menu demo selection also uses this same path. Existing theme unlock rules remain.
Approved block tints still come from the existing menu catalog, with the existing marble
texture and finish preserved; the new profile supplies the matching environment colors.
`CarraraWorld` consumes the blended profile for the existing 24 architectural elements
and existing dust system. Key lighting uses the profile rather than a Carrara-only override.

| Theme | Environment | Restrained accent |
| --- | --- | --- |
| Carrara | Pastel blue, cream, warm daylight | Champagne dust and 0.025 atmosphere strength |
| Obsidian | Navy with lighter slate mist separating black slabs | Cool silver-blue dust, 0.035 strength |
| Ancient Stone | Sandstone, beige fog, warm sunlight | Amber dust, 0.04 strength |
| Neon | Blue-violet, cool lit stone, cyan dust | Soft magenta atmosphere veil, 0.09 strength |
| Space | Indigo, lavender mist, pale cool sunlight | Violet veil, pale cosmic dust, 0.065 strength |

The sky and architectural fog share the same lightweight shader function, including a
slow, low-opacity accent veil. No additional geometry, particle systems, bloom passes,
lights, scene loads, or textures were added. Existing dust limits remain 12/36 particles.
HUD foreground colors adapt to bright/dark profiles, with contrasting Perfect feedback.

## Camera

FOV stays 40 degrees, elevation 27 degrees, vertical follow damping 0.16 seconds and
placement headroom 0.35 world units. The movement boundary is unchanged at 3.8 units.
The previous rotation-invariant sphere fit was replaced by projected corridor corner
fitting. The horizontal usable fraction changes from 0.91 to 0.95 (approximately 2.5%
edge clearance); vertical fitting retains comfortable HUD clearance. Full-size slab
extents are used even after cuts, so shrinking blocks do not cause zoom oscillation.
Inward distance changes use 0.35-second damping, with a visibility floor during rotation.
Height does not add camera retreat; distance remains bounded by the fixed corridor and aspect.

At 9:19.5, level-2 angle, measured distance is 36.06 instead of 37.98 world units,
giving approximately 5.3% greater apparent size. The gain varies with orbit angle;
diagonal views benefit more. Keeping the entire movement corridor visible limits zoom.

## Validation and remaining tuning

Play Mode covers all five themes, saved selection, interpolated transition colors,
unchanged world geometry, closer framing, scoring and bounce regression checks.
Portrait visibility checks cover 9:16, 9:19.5 and 9:21 throughout level camera rotations.
Five rendered gameplay previews include Perfect feedback and are in
`Artifacts/ThemeEnvironment`. Test reports are saved alongside them.

Validation results: 18/18 Edit Mode, 26/26 full Play Mode regression. After the final
HUD and approved-surface-color adjustment, 10 menu/gameplay checks passed; the remaining
profile test initially hit floating-point equality noise and passed on rerun using a
0.0001 color tolerance (`FinalProfiles.xml`). That test covers all five profiles and
produced the final screenshots. No gameplay assertion was removed.

The previews were visually checked for silhouettes, slab edge separation and HUD text.
These are Unity-rendered mobile-aspect previews, not a physical iPhone performance test.
Final device tuning should check Obsidian/Neon side-face detail at low screen brightness,
and whether the conservative camera zoom feels sufficiently close during play.
