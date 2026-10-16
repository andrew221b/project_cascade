#!/usr/bin/env python3
"""Offline art QA: compare solid island silhouettes at their actual map positions.

Uses the local QA Python environment (Pillow, NumPy, SciPy), never player code.
Does not modify source artwork or Unity metadata.
"""
from pathlib import Path
import argparse
import hashlib
import json
import math
import re

import numpy as np
from PIL import Image
from scipy.ndimage import distance_transform_edt

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output', type=Path, default=ROOT / 'Reports/MapPolishOct15/silhouette-clearance.json')
options = parser.parse_args()
presentation = ROOT / 'CascadeUnity/Assets/_Cascade/Presentation'
source = (presentation / 'CascadeGame.MapReference.cs').read_text()
source = source.split('var places=new[]{', 1)[1].split('};', 1)[0]
rectangles = [tuple(map(int, values)) for values in re.findall(r'new Vector4\((-?\d+),(-?\d+),(\d+),(\d+)\)', source)]
source = (presentation / 'GameplayReferenceArt.cs').read_text().split('static readonly Vector2[][] offsets=', 1)[1].split('};', 1)[0]
offsets = [[tuple(map(int, values)) for values in re.findall(r'new Vector2\((-?\d+),(-?\d+)\)', line)]
           for line in source.splitlines() if 'new Vector2' in line]
themes = ['WindGarden', 'RainMill', 'CopperOrchard', 'CloudPost', 'GlassLagoon',
          'MossArchive', 'SnowSpire', 'GreatBeacon', 'StarObservatory']
assert len(rectangles) == 5 and len(offsets) == len(themes)
assert [(r[2], r[3]) for r in rectangles] == [(302, 260), (264, 271), (234, 220), (216, 270), (220, 211)]
worlds, pins = [], {}
for world, theme in enumerate(themes):
    masks = []
    for island, (x, y, width, height) in enumerate(rectangles):
        dx, dy = offsets[world][island]
        x, y = x + dx, y + dy
        path = ROOT / 'CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/WorldIslandsOct14' / f'{theme}{island}.png'
        pins[str(path.relative_to(ROOT))] = hashlib.sha256(path.read_bytes()).hexdigest()
        image = Image.open(path).convert('RGBA')
        image.thumbnail((width, height), Image.Resampling.LANCZOS)
        mask = np.zeros((1280, 720), dtype=bool)
        px, py = round(x + (width-image.width)/2), round(y + (height-image.height)/2)
        mask[py:py+image.height, px:px+image.width] = np.asarray(image.getchannel('A')) >= 150
        masks.append(mask)
    pairs = []
    for first in range(4):
        distances = distance_transform_edt(~masks[first])
        for second in range(first+1, 5):
            distance = float(distances[masks[second]].min())
            pairs.append({'first': first, 'second': second, 'solid_gap_design_pixels': round(distance, 3)})
    minimum = min(pair['solid_gap_design_pixels'] for pair in pairs)
    worlds.append({'world': world, 'theme': theme, 'minimum_solid_gap_design_pixels': minimum, 'pairs': pairs})
    print(f'{theme}: minimum solid gap {minimum:.1f} design pixels')

# Conservative upper bound for the relative translation and rotation in MapReferenceAmbiance.
radius = max(math.hypot(r[2], r[3])/2 for r in rectangles)
motion_bound = 2*(math.hypot(1.5, 3.3) + radius*math.radians(.55))
passed = all(world['minimum_solid_gap_design_pixels'] >= 24 for world in worlds)
report = {'passed': passed, 'island_sizes_unchanged': True, 'worlds': worlds,
          'relative_motion_displacement_upper_bound_design_pixels': round(motion_bound, 3),
          'source_png_sha256': pins,
          'scope': 'All 90 solid-silhouette pairs at 720x1280 reference coordinates, alpha >=150. Soft shadows/glow are not solid rock; subpixel placement is rounded for raster measurement. No asset edited.'}
options.output.parent.mkdir(parents=True, exist_ok=True)
options.output.write_text(json.dumps(report, indent=2)+'\n')
if not passed:
    raise SystemExit('FAIL: a pair of painted islands still has less than 24 design pixels of clearance')
print('PASS: all 9 worlds retain the five original island sizes and clear solid silhouettes')
