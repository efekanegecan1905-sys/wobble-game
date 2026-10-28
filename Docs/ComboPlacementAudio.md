# Combo placement audio contract

## Source of truth
Approved WAVs (unchanged, byte-identical) copied into `Assets/Resources/Audio/Placement/`:

| Level | Clip | Runtime file |
|---|---|---|
| 1 | `Wobble_Normal` | `Wobble_Normal.wav` |
| 2 | `Wobble_Combo_x2` | `Wobble_Combo_x2.wav` |
| 3 | `Wobble_Combo_x3` | `Wobble_Combo_x3.wav` |
| 4 | `Wobble_Combo_x4` | `Wobble_Combo_x4.wav` |
| 5 | `Wobble_Combo_x5` | `Wobble_Combo_x5.wav` |
| 6 | `Wobble_Combo_x6` | `Wobble_Combo_x6.wav` |
| 7 | `Wobble_Combo_x7` | `Wobble_Combo_x7.wav` |
| 8+ | `Wobble_Combo_x8` | `Wobble_Combo_x8.wav` |

`AudioManager` loads the eight clips from `Resources/Audio/Placement/` in exactly this order (`AudioManager.PlacementClipNames`), with placement volume 0.48 and fade-out 0.02 (`AudioManager.PlacementVolume`, `PlacementFadeOut`). Import settings: mono, PCM, 44.1 kHz, `normalize: 0` so the baked level/pitch progression is preserved.

## Runtime rules
- `AudioManager.Impact(perfect, combo)` plays **one** primary voice (slot 0) per successful placement.
- Level = `perfect ? clamp(combo, 2, 8) : 1`; level 9+ clamps to x8.
- `pitch` is always `1` (`AudioManager.PlacementPitch`). The WAVs already rise in pitch.
- Before scheduling, slots 0–7 are stopped, so a previous take or tail can never layer underneath.
- No marble/stone clip is referenced by the placement path. `AudioManager.Cut()` is intentionally silent.
- Falling debris keeps a secondary contact on slots 8/9 at ≤ 0.018 gain, and it is skipped entirely while a primary placement voice is playing, so it cannot mask the percussive hit.
- All placement audio routes to the existing **Gameplay** AudioMixer group (`Audio/WobbleMix`).

## Reset rules
`AudioManager.ResetPlacementCombo()` arms the next placement as `Wobble_Normal` and clears pending tails. It is called from:

- `GameManager.StartGame()` — new run,
- `GameManager.OnMissed()` — death,
- `GameManager.ContinueWithLife()` — Continue.

A non-perfect placement (combo break by a cut) resolves to combo 1 and therefore plays `Wobble_Normal` naturally.

## Verification
- Play Mode: `Assets/Tests/PlayMode/ComboAudioTests.cs`
  - Normal → x2 → x3 → x4 → x5 → x6 → x7 → x8 → x9 (clamped x8) → break → Normal,
  - after death + Continue the first hit is `Wobble_Normal`,
  - exactly one primary voice per placement, pitch 1, no marble/legacy leak.
- Edit Mode: `Assets/Editor/ComboPlacementBankTests.cs` (bank completeness/order and absence of the removed legacy bank).
