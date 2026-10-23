# Combo-reactive Carrara impact

Tune `Assets/Resources/Audio/CarraraImpactProfiles.asset` in the Unity Inspector. Gameplay, scoring, timing and incoming directions are unchanged. The existing caller supplies the registered Perfect multiplier; the first Perfect is x2.

| Profile | Base | Body | Resonance | Ambience |
| --- | ---: | ---: | ---: | ---: |
| Normal | .42 | 0 | 0 | 0 |
| Combo2 | .43 | .06 | 0 | 0 |
| Combo3 | .44 | .10 | .04 | 0 |
| Combo4 | .45 | .14 | .09 | .015 |
| Combo5Plus | .46 | .18 | .14 | .03 |

Each profile exposes intensity, random pitch/volume variation, and each layer's clip, volume, pitch, duration, fade-out and stereo pan. Duration 0 means the complete clip. Empty base clips use the existing five-take marble bank, with immediate repeats prevented. Normal always plays only the base. Combo layer clips are assigned in the asset; muted layers consume no playback voice.

All default pitches are 1; the base retains only ±2% random variation, independent of combo. Intensity is .9 and gain variation ±3%. Weight increases through separate low-mid noise-body and resonance textures, longer controlled tails and a tiny room layer, not musical notes or a rising pitch ladder. Small opposing pans at x4/x5 add width without changing material. The base remains dominant. x5 and every higher combo select the exact same profile.

A non-Perfect placement immediately selects Normal and stops prior combo support tails. A subsequent first Perfect selects Combo2. There is no persistent audio combo counter to reset.

Two reused four-layer banks and two secondary voices retain the existing 10-effect-voice budget. A single DSP-clock envelope runner fades configured tails. Effects headroom is .65; default peak-budget tests include two overlapping placements, secondary impacts and music. Retest headroom after increasing Inspector gains or replacing clips.

`WOBBLE/Audio/Create Combo Layers and Profiles` regenerates the two synthetic supporting WAVs, but preserves an existing profile asset's designer tuning. Do not regenerate after substituting approved recordings. These are original noise-based sound designs, not recorded Carrara Foley. Phone-speaker listening and user approval remain subjective release checks.

Tests cover actual placements through Normal, x2, x3, x4, x5, higher combos, break and restart; immediate tail cancellation; mute; bounded voices; Inspector pitch/gain/tail overrides; default base dominance and headroom.

Validation: Unity 6000.0.81f1 isolated-project run passed all 22 Play Mode tests and 15 Edit Mode tests. XML reports are in `Artifacts/ComboAudio`. A standalone macOS development build succeeded at `/private/tmp/wobble-combo-player/WOBBLE.app`. Gameplay scripts and GameManager were compared with the previous presentation baseline and are unchanged.
