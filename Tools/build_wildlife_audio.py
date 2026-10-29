"""Prepare the licensed wildlife phrases from a local source cache; never downloads or changes other audio."""
import argparse
import hashlib
import json
import math
from pathlib import Path
import wave

import numpy as np
from scipy.signal import butter, resample_poly, sosfiltfilt

RECIPES = [
    ('SongbirdA', 'BirdSource.wav', .63, 1.34, 650, 11000,
     'syncopika', 'https://opengameart.org/content/bird-chirping-sounds'),
    ('SongbirdB', 'BirdSource.wav', 1.89, 2.64, 650, 11000,
     'syncopika', 'https://opengameart.org/content/bird-chirping-sounds'),
    ('Seabird', 'GullDecoded.wav', .03, 2.58, 350, 11000,
     'nigelcoop', 'https://freesound.org/people/nigelcoop/sounds/73497/'),
    ('Owl', 'OwlDecoded.wav', 8.54, 10.08, 700, 10500,
     'raoul_slayer', 'https://freesound.org/people/raoul_slayer/sounds/101344/'),
    ('Bumblebee', 'BeeDecoded.wav', .10, 1.45, 90, 4200,
     'Department64', 'https://freesound.org/people/Department64/sounds/103047/'),
    ('SongbirdContactA', 'BirdSource.wav', .66, 1.04, 1100, 11000,
     'syncopika', 'https://opengameart.org/content/bird-chirping-sounds'),
    ('SongbirdContactB', 'BirdSource.wav', 2.00, 2.43, 1100, 11000,
     'syncopika', 'https://opengameart.org/content/bird-chirping-sounds'),
    ('SongbirdRobinA', 'RobinDecoded.wav', 14.9, 17.15, 1600, 10500,
     'BeeProductive', 'https://freesound.org/people/BeeProductive/sounds/372219/'),
    ('SongbirdRobinB', 'RobinDecoded.wav', 21.5, 23.9, 1600, 10500,
     'BeeProductive', 'https://freesound.org/people/BeeProductive/sounds/372219/'),
    ('SongbirdWrenA', 'WrenDecoded.wav', 3.2, 5.9, 1800, 10500,
     'hoshisato', 'https://freesound.org/people/hoshisato/sounds/853671/'),
    ('SongbirdWrenB', 'WrenDecoded.wav', 20.8, 23.6, 1800, 10500,
     'hoshisato', 'https://freesound.org/people/hoshisato/sounds/853671/'),
    ('SongbirdRobinC', 'RobinDecoded.wav', 30.0, 32.5, 1600, 10500,
     'BeeProductive', 'https://freesound.org/people/BeeProductive/sounds/372219/'),
    ('SongbirdRobinD', 'RobinDecoded.wav', 34.0, 36.5, 1600, 10500,
     'BeeProductive', 'https://freesound.org/people/BeeProductive/sounds/372219/'),
    ('SongbirdWrenC', 'WrenDecoded.wav', 30.0, 32.5, 1800, 10500,
     'hoshisato', 'https://freesound.org/people/hoshisato/sounds/853671/'),
    ('SongbirdWrenD', 'WrenDecoded.wav', 34.0, 36.5, 1800, 10500,
     'hoshisato', 'https://freesound.org/people/hoshisato/sounds/853671/'),
    ('SeabirdContact', 'GullDecoded.wav', .9, 1.67, 600, 11000,
     'nigelcoop', 'https://freesound.org/people/nigelcoop/sounds/73497/'),
    ('OwlAnswer', 'OwlDecoded.wav', 12.8, 14.35, 700, 10500,
     'raoul_slayer', 'https://freesound.org/people/raoul_slayer/sounds/101344/'),
    ('CricketA', 'CricketDecoded.wav', 8.65, 9.7, 2700, 10500,
     'kmckinney7', 'https://freesound.org/people/kmckinney7/sounds/353073/'),
    ('CricketB', 'CricketDecoded.wav', 17.9, 19.15, 2700, 10500,
     'kmckinney7', 'https://freesound.org/people/kmckinney7/sounds/353073/'),
    ('CricketC', 'CricketDecoded.wav', 38.9, 40.6, 2700, 10500,
     'kmckinney7', 'https://freesound.org/people/kmckinney7/sounds/353073/'),

]


def build(cache, output, report):
    output.mkdir(parents=True, exist_ok=True)
    results = []
    for name, filename, start, end, highpass, lowpass, author, url in RECIPES:
        source = cache / filename
        with wave.open(str(source)) as wav:
            assert wav.getsampwidth() == 2
            rate = wav.getframerate()
            samples = np.frombuffer(wav.readframes(wav.getnframes()), '<i2')
            mono = samples.reshape(-1, wav.getnchannels()).mean(axis=1) / 32768
        x = mono[round(start * rate):round(end * rate)]
        x = sosfiltfilt(butter(3, [highpass, lowpass], btype='bandpass', fs=rate, output='sos'), x)
        divisor = math.gcd(rate, 32000)
        x = resample_poly(x, 32000 // divisor, rate // divisor)
        if name == 'Bumblebee':
            # Crossfade the recording across the loop seam, preserving its natural wing sound.
            count = 3200
            blend = np.linspace(0, 1, count)
            seam = x[-count:] * (1-blend) + x[:count] * blend
            x = np.concatenate((x[count:-count], seam))
        else:
            count = min(640, len(x)//8)
            x[:count] *= np.linspace(0, 1, count)
            x[-count:] *= np.linspace(1, 0, count)
        x *= (.55 if name == 'Bumblebee' else .65) / max(abs(x))
        pcm = np.round(x * 32767).astype('<i2')
        target = output / (name + '.wav')
        with wave.open(str(target), 'wb') as wav:
            wav.setparams((1, 2, 32000, 0, 'NONE', 'not compressed'))
            wav.writeframes(pcm.tobytes())
        results.append(dict(name=name, source=filename, source_sha256=hashlib.sha256(source.read_bytes()).hexdigest(),
                            author=author, url=url, license='CC0-1.0', source_slice_seconds=[start, end],
                            processing=f'mono; bandpass {highpass}–{lowpass} Hz; resample 32000 Hz; fades or loop crossfade; peak normalization',
                            seconds=len(x)/32000, peak=float(max(abs(x))), rms=float(np.sqrt(np.mean(x*x))),
                            bytes=target.stat().st_size, sha256=hashlib.sha256(target.read_bytes()).hexdigest()))
    report.parent.mkdir(parents=True, exist_ok=True)
    report.write_text(json.dumps(results, ensure_ascii=False, indent=2)+'\n')
    print(json.dumps({'clips': len(results), 'bytes': sum(r['bytes'] for r in results),
                      'seconds': sum(r['seconds'] for r in results)}, indent=2))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('cache', type=Path)
    parser.add_argument('output', type=Path)
    parser.add_argument('report', type=Path)
    args = parser.parse_args()
    build(args.cache, args.output, args.report)
