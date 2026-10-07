"""Finish original scores and generated foley without changing their source artwork.

Music release tails wrap into the beginning of the next phrase. Foley starts promptly;
wind joins through a crossfaded overlap rather than periodic silence. No audio bank is copied.
"""
from pathlib import Path
import argparse, json, subprocess, urllib.request, wave
import numpy as np

root = Path(__file__).resolve().parents[1]
folder = root / 'CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Audio/Atmosphere'
bpm = dict(Menu_HomeAmongClouds=84, Menu_HomeAmongCloudsBalanced=84, WindGarden_PaperWings=92, RainMill_RainOnCedar=76,
           CopperOrchard_GoldenAfternoon=104, CloudPost_LettersOnTheBreeze=96,
           GlassLagoon_ReflectedMoon=80, MossArchive_StoriesInTheLeaves=72,
           SnowSpire_WarmLightInSnow=78, GreatBeacon_TheWayHome=100)

def read(path):
    with wave.open(str(path), 'rb') as file:
        assert file.getsampwidth() == 2
        return np.frombuffer(file.readframes(file.getnframes()), '<i2').reshape(-1, file.getnchannels()).astype(float) / 32768, file.getframerate()

def write(path, data, rate):
    with wave.open(str(path), 'wb') as file:
        file.setnchannels(data.shape[1]); file.setsampwidth(2); file.setframerate(rate)
        file.writeframes((np.clip(data, -1, 1) * 32767).astype('<i2').tobytes())

def gain(data, rms, ceiling):
    data = data - np.mean(data, axis=0)
    return data * min(rms / max(1e-7, np.sqrt(np.mean(data ** 2))), ceiling / max(1e-7, np.max(np.abs(data))))

def loop(data, rate):
    n = int(rate * .35)
    result = data[n:].copy()
    t = np.linspace(0, 1, n)[:, None]
    weight = .5 - .5 * np.cos(t * np.pi)
    result[-n:] = result[-n:] * (1 - weight) + data[:n] * weight
    return result

parser = argparse.ArgumentParser()
parser.add_argument('--scores', action='store_true')
parser.add_argument('--music-only', action='store_true')
parser.add_argument('--recordings', type=Path)
parser.add_argument('--folder', type=Path, default=folder)
parser.add_argument('--only-score')
parser.add_argument('--report', type=Path)
args = parser.parse_args()
folder = args.folder
if args.scores or args.music_only:
    for path in folder.glob('*.wav'):
        if args.only_score and path.stem != args.only_score:
            continue
        if args.music_only and path.stem not in bpm:
            continue
        if path.stem not in bpm and path.stem not in {'Repair_Chimes', 'Defuse_Release', 'Bridge_Unfold', 'Success_Home', 'Unlock_Turn'}:
            continue
        data, rate = read(path)
        if path.stem in bpm:
            length = int(32 * 6 * 30 / bpm[path.stem] * rate)
            body, tail = data[:length].copy(), data[length:]
            body[:len(tail)] += tail
            data = gain(body, .12, .72)
        else:
            activity = np.flatnonzero(np.max(np.abs(data), axis=1) > .001)
            data = data[:min(len(data), activity[-1] + int(rate * .12))]
            data = gain(data, .08, .40)
            data[-int(rate * .06):] *= np.linspace(1, 0, int(rate * .06))[:, None]
        write(path, data, rate)
if args.recordings:
    for name, url in json.loads(args.recordings.read_text()).items():
        mp3 = Path('/private/tmp') / (name + '.mp3')
        converted = Path('/private/tmp') / (name + '.wav')
        with urllib.request.urlopen(url) as response, mp3.open('wb') as output:
            output.write(response.read())
        subprocess.run(['/usr/bin/afconvert', '-f', 'WAVE', '-d', 'LEI16@44100', str(mp3), str(converted)], check=True)
        data, rate = read(converted)
        if name == 'LivingWind':
            data = gain(loop(data, rate), .16, .65)
        else:
            power = np.max(np.abs(data), axis=1)
            activity = np.flatnonzero(power > max(.001, power.max() * .008))
            data = data[max(0, activity[0] - int(rate * .004)):min(len(data), activity[-1] + int(rate * .08))]
            data = gain(data, .09, .45)
            fade = min(int(rate * .025), len(data) // 4)
            data[-fade:] *= np.linspace(1, 0, fade)[:, None]
            data[:min(90, len(data))] *= np.linspace(0, 1, min(90, len(data)))[:, None]
        write(folder / (name + '.wav'), data, rate)
        mp3.unlink(); converted.unlink()
report = {}
for path in sorted(folder.glob('*.wav')):
    if args.only_score and path.stem != args.only_score:
        continue
    data, rate = read(path)
    report[path.stem] = dict(seconds=round(len(data) / rate, 3), channels=data.shape[1], peak=round(float(abs(data).max()), 5),
        rms=round(float(np.sqrt(np.mean(data ** 2))), 5), seam=round(float(abs(data[0] - data[-1]).max()), 5))
dest = args.report or root / 'Reports/Atmosphere/audio-metrics.json'
dest.parent.mkdir(parents=True, exist_ok=True)
dest.write_text(json.dumps(report, indent=2) + '\n')
print(f'Finished {len(report)} audio assets; clipping count: {sum(v["peak"] >= 1 for v in report.values())}')
