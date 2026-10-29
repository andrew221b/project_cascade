# Local ACE-Step installation and first music preview — 17 October 2026
Current status: completed 19-track world score (24.38 minutes), prepared as loops and installed at existing Unity GUIDs. Native 8/8 tests pass; no iPhone installation or artistic listening approval claimed. See Reports/WorldScoreOct17/README.md. Earlier sections below record the preceding first-preview/design stages.

The previous 19-track score was generated offline by `Tools/compose_acoustic_score.py`: predefined motifs, chord/scale rules, sampled CC0 VSCO2 instruments, note scheduling and WAV rendering in Python. Unity only played the rendered clips. ACE-Step was not used. The user rejected its audible musical result. Prior tuning/routing/PCM tests are technical evidence, not proof of tasteful phrasing, melody or arrangement; those tracks must not be treated as artistically accepted.

## Installed locally

Official repository: https://github.com/ace-step/ACE-Step-1.5 . Pinned commit `ca1e85fe9430179831e6bc6be790c332190a3866`. Separate Python 3.12.15 environment, frozen dependencies; all 125 installed packages pass compatibility checking. Apple M4 Pro / 24 GiB, MPS and native MLX available. No Unity package or system Python modification.

- Application: `/Volumes/MY DISK/AI_Tools/ACE-Step-1.5`
- Environment/caches/launcher configuration/logs: `/Volumes/MY DISK/AI_Tools/ACE-Step-Support`
- Music masters and request: `/Volumes/MY DISK/FlyingLadders/Audio/ACE-Step`
- Double-click `/Volumes/MY DISK/AI_Tools/Start ACE-Step Studio.command`; local UI `http://127.0.0.1:7865`.
- Codex/API launcher: `/Volumes/MY DISK/AI_Tools/Start ACE-Step API.command`; local API `http://127.0.0.1:8015`.

Complete 9.4 GiB checkpoint snapshot: turbo DiT, VAE, Qwen embedding and 1.7B LM. Installation and support total approximately 13 GB (decimal), entirely on the external disk. Launchers bind loopback only, disable sharing/analytics, and prevent a second Studio/API instance when either configured port is occupied. Offline model loading enabled after complete downloads; MLX VAE decode chunks 512 and UI batch size 1. Control+C stops the active launcher. Paths need updating if this installation is moved.

## Shape Mania pack inspection

Source project: `/Volumes/MY DISK/unity_projects/shapemania/unity_projects/shape_sorting-final`. Found 249 audio assets: five finished music WAVs (Menu, Journey I/II/III, Duel) and 244 short sounds. No individual melodic note/instrument bank or MIDI was found in the inspected project.

The Unity Asset Store download cache contains Midnight Mode Vol.1 (20 WAVs), Midnight Mode Vol.2 (22 WAVs) and Free 8 Synthwave Tracks (17 WAVs). These are finished electronic full/loop mixes, not isolated instrument stems or multisamples. Five candidates were safely extracted for local review; see `pack-review-inventory.json` for source/package paths and file hashes, and `package-inventory.json` for the complete audio-path listing in those three packages. The package names are inventory facts, not a purchase/license verification or a claim that the tracks were auditioned.

No Shape Mania source or cached package was changed. No third-party finished track was mashed together, sent to an external generator or used for model training. A new coherent arrangement was generated instead.

## First actual generation

Title: «Огонёк за облаками» / Hopeful Memory. A requested original acoustic chamber waltz: bittersweet nostalgia and melancholy resolving toward hope; piano melody with clarinet answers, harp, strings and gentle pizzicato. Requested 90 BPM / 3/4 / D major shaded by B minor, clear phrases, contrasting middle and returning theme. These describe generation intent, not a verified score transcription or an artistic endorsement.

Real offline API generation completed successfully using native MLX DiT/VAE and the 1.7B LM. Seed `17102631`; turbo 8 steps; one 96-second instrumental. See `composition-request.json` and `generation-result.json`. Raw WAV stereo 48 kHz PCM16 retained externally as `HopefulMemory-Take1.wav`. This is one complete generated performance, not a procedural per-note reconstruction or a splice of separate melodies.

Preview: `HopefulMemory-Preview.m4a`, 256 kbps AAC. Only a 40 ms onset and 1-second tail fade were applied; no time stretch, instrument overlay or rearrangement. WAV signal check: 96 seconds, finite stereo samples, peak approximately -1 dBFS, no full-scale samples, no interior 250 ms windows below -65 dBFS. These checks cannot establish harmony, taste, phrasing or emotional effect. No automated claim of having listened to the track.

## Validation and boundaries

- Dependencies/backend imports passed; main model download exited successfully.
- Offline local API health/model checks and real audio generation passed.
- The shipped Studio launcher loaded models offline and served a valid Gradio configuration; see `studio-check.json`. Both validation server processes were stopped afterward to release memory. Studio UI generation itself was not exercised; the same model pipeline was exercised through the API.
- AAC export completed successfully. Metadata, hashes and evidence retained.
- No Unity runtime/audio assets replaced, no iPhone build installed, no commercial API/account used. No Unity tests rerun for this isolated offline-tool change. Human listening is still required before making this the game's approved music direction.

## Follow-up form review and world direction

The same 96-second first take was reviewed by signal analysis, not falsely described as a headphone audition. Onset autocorrelation gives approximately 88–90 BPM (coarse-frame estimate). Interior one-second RMS p10/p90 are -21.36/-16.42 dBFS: 4.94 dB of measured variation. Pitch statistics strongly favour a tonal centre (A major profile correlation 0.938; requested D major is not asserted as the actual transcription). Sixteen-second blocks show changed pitch/timbre distributions in the middle and return to a similar distribution at 64 seconds. This supports a tentative A/development/return reading, not proof of exact verse/refrain boundaries or the quality of the melody. See musical-analysis.json and analysis.png.

A portable full-length MP3 of the same take was encoded and decoded successfully: 96 seconds, stereo 48 kHz, finite samples and no full-scale peaks. A chat delivery copy is in the visualization workspace; report and external-output copies retained. No new song, Unity replacement or iPhone build is implied.

world-music-direction.json records nine distinct future world briefs, moderate menu/gameplay tempo ranges, memorable separate melodies and clear theme/refrain contrast. Snow uses sparse brittle celesta/bells around a warmer string centre; rain uses descending piano/cello lines that resolve toward a brighter refrain. These are concrete generation directions, not a claim that all nine world tracks are already finished or artistically approved.

## Cyclic master follow-up

The first 96-second preview had a fade and was not yet a seamless loop. An 80-second cyclic master is now prepared offline from musically matching interior boundaries, retaining the middle and return. No intro/outro fade, silence or added wrap sample discontinuity; PCM24 decode and phase/onset/pitch-context checks pass. Actual wrap is at 12 seconds in HopefulMemory-Loop-Seam.mp3. See Reports/ACEstepOct17/Loop/README.md and loop-check.json for exact method, metrics and limitations. Future world briefs now require cyclic form. Unity assets/imports and the installed iPhone build are unchanged.

## Independent nine-world composition-design review

Only one generic ACE-Step composition has been generated; the 80-second loop is an edit of that take. None of the nine world-specific compositions is rendered. All nine briefs were independently reviewed against the actual world order and story in JourneyStory.cs / WorldPrologues.cs, with canonical Russian names from ui_localization.tsv. This is a composition-design review, not an audition of nine tracks that do not yet exist.

The review found overreliance on instrumentation and a generic bright contrasting middle. Updated every brief with metre/pulse, a specific melodic question/answer, phrase development, story connection, pitfalls and different menu/gameplay treatment. Rain, lagoon and snow no longer depend on the same piano/bell imagery. Great Beacon now expresses Ada/Rin continuity and unresolved resolve, not a premature finale. Star Observatory reflects the actual theft revelation, responsibility and shared repair, rather than a tranquil epilogue. The acoustic palette stays related while the melodies, rhythmic gestures and emotional direction remain distinct. Optional small connecting gestures are future composition directions, not claims of a leitmotif already present in the preview.

See composition-design-review.json for findings, honest status and first-take evidence limits. World-music-direction.json is the revised generation-facing source. No audio, Unity runtime, project package or iPhone build was changed during this review. Musical taste, actual nine-track cohesion and repeated-cycle listening are not certified by numerical tests.


Deployment follow-up — 18 October 2026: all 19 prepared loops were included in iPhone build 202610181, installed on the paired physical iPhone 17 at the user's request. Current profile and backup verified byte-identical after updating. Launch and process liveness after four seconds confirmed by CoreDevice. See Reports/iPhoneInstall-202610181/README.md for evidence and listening/performance limitations.
