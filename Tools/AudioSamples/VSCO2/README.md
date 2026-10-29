# VSCO 2 CE — selected acoustic recordings

CC0 1.0 Universal. See LICENSE and sources.json for exact URLs and SHA-256 hashes. Recorded by Sam Gossner / Versilian Studios and Simon Dalzell / Ivy Audio; sample cutting by Elan Hickler / Soundemote.

Official source: https://versilian-studios.com/vsco-community/
Repository: https://github.com/sgossner/VSCO-2-CE

48 source recordings: upright piano, harp, cello, flute, clarinet, solo violin and pizzicato viola. Used offline by Tools/compose_acoustic_score.py; source recordings are outside Assets and no runtime sampler is shipped. Arrangements are original CASCADE compositions.

Filename octave conventions differ: harp/solo violin use scientific octaves; flute/clarinet/cello/viola use the source library labels (C3=MIDI 60 for flute). Upright roots are MIDI 21 + 2 × filename index. The composer measures the stable fundamental of every sample, applies a concert-pitch correction during resampling, and retains the natural expression/vibrato. Exact correction values are in Reports/MusicOct17/tuning.json. Added source samples remain unmodified, with original hashes.
