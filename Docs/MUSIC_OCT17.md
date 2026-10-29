# Music revision — 17 October 2026

Design note: add nine optional world-menu AudioClip references to the existing ToyAudioLibrary. Existing menu/gameplay GUIDs are retained; new menu clips receive deterministic new GUIDs. No save data, rules, prices, IDs or packages change. Missing menu clips safely fall back to the home theme. The asset builder maintains the new array.

The home theme is a lyrical 78 BPM triple-time theme. Nine world maps/comics/conversations use individually arranged 70–86 BPM themes; levels use 100–126 BPM performances with different rhythmic gestures and instrumentation. Pause/results/checkpoints continue the level performance.

The two existing sources hand off through quiet over 0.85 seconds. The incoming performance starts at the midpoint, preserving the first note. Different keys and meters never compete at audible gain. Returning during the handoff mirrors the envelope and preserves playback position. Mute, focus and output-reset recovery remain owned by the same pool.

Original offline compositions use the existing CC0 VSCO2 acoustic recordings plus flute, clarinet, violin and pizzicato recordings from the same official library. Exact URLs/hashes are in Tools/AudioSamples/VSCO2/sources.json. Samples stay outside Assets. The seven banks are calibrated to concert pitch; every melodic anchor and accompaniment tone belongs to its sounding chord. Minor dominants include the raised leading note. Ornaments no longer overlap held melody notes. The short, quiet chamber reverb preserves attacks. Stereo loops retain exact bar length and wrap their release tails.

Reproduce with Tools/compose_acoustic_score.py --install using numpy/scipy; check with Tools/check_acoustic_score.py and Tools/check_score_routing.py. All music remains background-loaded streaming Vorbis; no runtime sampler or new dependency is shipped.

Validation and listening limitations are recorded in Reports/MusicOct17/README.md. Supersedes the music/routing parts of ACOUSTIC_SCORE_OCT09.md and AUDIO_WORLDS_POLISH_OCT15_RU.md.

## Subsequent listening review and local generator

The user rejected the audible quality of this procedural sample-based score. Technical harmony/routing checks above do not establish musical quality or acceptance. A separate local ACE-Step installation and first model-generated preview are documented in ACESTEP_OCT17.md; it has not replaced these Unity clips.

Current shipped project audio now uses the selected ACE-Step score documented in ACESTEP_OCT17.md / Reports/WorldScoreOct17. The procedural notation checks and --install instructions above describe the rejected historical score and must not be used to overwrite the new masters. Runtime routing and GUIDs remain the same.
