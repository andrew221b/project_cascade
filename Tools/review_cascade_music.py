#!/usr/bin/env python3
"""Inspect every revised WAV/score, then optionally install at the existing GUID paths.

Signal and notation checks are not a headphone listening test.
"""
import argparse
import hashlib
import json
import re
import shutil
import wave
from pathlib import Path

import numpy as np

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / 'CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Audio/Atmosphere'
REPORT = ROOT / 'Reports/Audio-Oct09'
BACKUP = Path('/private/tmp/cascade-oct09-audio-before')
parser = argparse.ArgumentParser()
parser.add_argument('--render', type=Path, required=True)
parser.add_argument('--baseline-score', type=Path, required=True)
parser.add_argument('--install', action='store_true')
args = parser.parse_args()
REPORT.mkdir(parents=True, exist_ok=True)

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def read(path):
    with wave.open(str(path), 'rb') as source:
        assert source.getsampwidth() == 2 and source.getnchannels() == 2
        return np.frombuffer(source.readframes(source.getnframes()), '<i2').reshape(-1, 2).astype(float) / 32768, source.getframerate()

def signal(data, rate, seconds):
    mono = data.mean(axis=1)
    window = np.hanning(4096)
    blocks = np.array([mono[i:i+4096] * window for i in range(0, len(mono)-4096, rate)])
    power = abs(np.fft.rfft(blocks, axis=1)) ** 2
    frequencies = np.fft.rfftfreq(4096, 1/rate)
    centre = float((power * frequencies).sum() / power.sum())
    split = int(seconds / 2 * rate)
    def rms(section):
        return float(np.sqrt(np.mean(section ** 2)))
    return dict(seconds=len(data)/rate, peak=float(abs(data).max()), rms=rms(data),
                clipped_samples=int((abs(data) >= 1).sum()), seam=float(abs(data[0]-data[-1]).max()),
                spectral_centroid_hz=centre,
                b_over_a_db=float(20*np.log10(rms(data[split:])/rms(data[:split]))),
                phrase_boundary_db=float(20*np.log10(rms(data[split:split+rate*2])/rms(data[split-rate*2:split]))))

report = {}
scores = sorted(args.render.glob('*.score.json'))
assert len(scores) == 10, 'Must review the menu and all nine worlds'
for path in scores:
    score = json.loads(path.read_text())
    name = score['name']; original = json.loads((args.baseline_score/path.name).read_text())
    target = ASSETS/(name+'.wav')
    before_path = BACKUP/target.name if digest(target) == digest(args.render/target.name) and (BACKUP/target.name).exists() else target
    revised, rate = read(args.render/(name+'.wav')); before, oldrate = read(before_path)
    assert rate == oldrate == 44100
    assert abs(len(revised)/rate-score['seconds']) < 1/rate
    assert score['seconds'] == original['seconds'] and score['bpm'] == original['bpm']
    assert all(33 <= n['pitch'] <= 84 and 0 < n['velocity'] < 90 and n['end'] > n['time'] for n in score['notes'])
    # Main solo on the first large beat of every bar: accompaniment is never
    # conflated with the requested solo transposition.
    pitches = []
    roots = dict(Menu=0,WindGarden=3,RainMill=0,CopperOrchard=3,CloudPost=7,GlassLagoon=3,MossArchive=3,SnowSpire=0,GreatBeacon=3,StarObservatory=3)
    lead = roots[name.split('_')[0]]
    bars = 16 if name.startswith('StarObservatory') else 32
    for bar in range(bars):
        at = bar * 6 * 30 / score['bpm']
        instrument = 3 if name.startswith('Menu_') and bar >= 16 else lead
        def note(notes):
            return next(n['pitch'] for n in notes if n['instrument'] == instrument and abs(n['time']-at) < .04)
        pitches.append(note(score['notes'])-note(original['notes']))
    assert np.median(pitches) == -12 and all(-13 <= delta <= -12 for delta in pitches), name+' solo must be one octave lower'
    measured = signal(revised, rate, score['seconds'])
    assert measured['clipped_samples'] == 0 and measured['peak'] <= .721
    assert measured['seam'] <= 1/32768 and measured['rms'] > .06
    assert abs(measured['phrase_boundary_db']) < 6, name+' has an abrupt phrase level change'
    report[name] = dict(before=signal(before, oldrate, score['seconds']), after=measured,
                        solo_semitones_min=min(pitches), solo_semitones_max=max(pitches),
                        before_sha256=digest(before_path), after_sha256=digest(args.render/(name+'.wav')),
                        meta_sha256=digest(ASSETS/(name+'.wav.meta')))
    assert report[name]['before_sha256'] != report[name]['after_sha256']
assert len({entry['after_sha256'] for entry in report.values()}) == 10
# Confirm the serialized menu/world references still resolve to these ten clips.
library = (ASSETS.parent.parent/'ToyAudio.asset').read_text()
references = re.search(r'  menuMusic:.*\n  worldMusic:\n(.*?)  naturalWind:', library, re.S).group()
guids = re.findall(r'guid: ([0-9a-f]{32})', references)
expected = {re.search(r'guid: ([0-9a-f]{32})', (ASSETS/(name+'.wav.meta')).read_text()).group(1) for name in report}
assert len(guids) == 10 and set(guids) == expected
(REPORT/'score-review.json').write_text(json.dumps(report, indent=2)+'\n')
if args.install:
    BACKUP.mkdir(parents=True, exist_ok=True)
    for name, entry in report.items():
        target = ASSETS/(name+'.wav')
        if not (BACKUP/target.name).exists():
            shutil.copy2(target, BACKUP/target.name)
        if digest(target) != entry['after_sha256']:
            shutil.copyfile(args.render/target.name, target)
        assert digest(target) == entry['after_sha256'] and digest(ASSETS/(name+'.wav.meta')) == entry['meta_sha256']
    (REPORT/'installation.txt').write_text('Installed ten revised scores at the existing asset paths. All ten .meta hashes unchanged.\n')
print('PASS: ten distinct stereo loops; tempo/duration retained; every solo lowered an octave; no clipping, no seam step. '+('Installed.' if args.install else 'Review only.'))
