# Extended cinematic startup

The direct cinematic-intro request is implemented here. The accompanying attachment describes a separate audio-only x8 placement redesign; that redesign is not applied by this change. Existing placement audio, combo profiles, gameplay, economy, menu layout and Home art direction remain intact.

Total startup: **5.8 seconds**, unscaled time.

- 0–0.6: dark-to-palette fade; the existing sky and garden appear.
- 0.6–1.8: six marble letters gather from alternating heights and depths; eight small mesh fragments move through foreground/background layers; a small dust burst begins.
- 1.8–2.9: letters settle sequentially, with one fixed-pitch quiet percussive accent and two dust particles per letter.
- 3–4: gentle camera arc, warmer letter shading, controlled moving highlight, background monuments reveal.
- 4–4.8: readable assembled logo, a single signature hit at 4.2 and a 12-particle dust burst.
- 4.65–5.3: Home sculpture emerges. From 4.8 the logo moves upward and toward camera, out of the frame, instead of cutting to Home.
- 5–5.8: existing Home UI fades/slides in and becomes interactive only when startup completes.

No additional lights, post-process passes, textures, colliders or runtime-created audio clips. Eight shared-mesh fragment renderers are active only with the intro logo; dust is capped at 32 particles. Existing analytic sky/fog remains; no real volumetrics or extra depth-of-field pass is introduced. Normal Home camera behavior resumes smoothly at the endpoint.

Intro audio uses the downloaded percussive plock, six quiet accents and one stronger plock with a low stone support hit. Fixed pitch 1.0; existing two menu voices and audio-enabled setting are respected. Placement sound is unchanged.

Every fresh application startup runs the intro. Retry and gameplay→Home do not. Existing development option `--wobble-skip-intro` and `UIManager.SkipIntro()` end it immediately, clear dust and prevent deferred intro audio. No first-launch save flag was added.

Review build entry: `Wobble.Editor.ConceptPresentationBuilder.BuildCinematicReview`; output `/private/tmp/wobble-cinematic-player/WOBBLE.app`.

Validation, 12 October 2026: **24/24 Play Mode tests passed**. Startup remained within 5.5–6.5 seconds, six assembly cues and one signature cue were observed, skip prevented delayed cues, and retry/Home return did not replay startup. Six timed intro frames and three portrait Home frames were captured under `Artifacts/Intro/Previews`; test results are `Artifacts/Intro/PlayMode.xml`. The 1.4, 4.5 and 5.2 second frames were visually inspected for assembly, readable hold and world/UI transition. These checks do not establish sustained mobile performance or subjective final-art quality.
