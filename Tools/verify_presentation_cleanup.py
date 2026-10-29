#!/usr/bin/env python3
"""Verify the explicit October 16 retirement manifest, never infer asset deletion."""
from pathlib import Path
import json
import re

ROOT = Path(__file__).resolve().parents[1]
PLAN = ROOT / 'Docs/PRESENTATION_CLEANUP_OCT16.json'
TEXT_SUFFIXES = {'.meta', '.asset', '.mat', '.prefab', '.unity', '.cs', '.json', '.shader', '.anim', '.controller', '.spriteatlas', '.uxml', '.uss'}

def verify():
    plan = json.loads(PLAN.read_text())
    assert plan['applied']
    retired = {row['guid']: row['path'] for row in plan['assets']}
    for row in plan['assets']:
        for rel in [row['path'], row['path'] + '.meta']:
            assert not (ROOT / rel).exists(), 'Retired file returned: ' + rel
    for path in (ROOT / 'CascadeUnity/Assets').rglob('*'):
        if not path.is_file() or path.suffix not in TEXT_SUFFIXES:
            continue
        hits = set(re.findall(r'\b[a-f0-9]{32}\b', path.read_text(errors='ignore'))) & retired.keys()
        assert not hits, (str(path), [retired[h] for h in hits])
    art = ROOT / 'CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt'
    # Guard the directly affected shared replacements; dynamic world/costume/rig loads
    # are additionally exercised in native PlayMode tests, not inferred from GUIDs.
    replacements = ['Reactions/ToolBodiesClean.png', 'Reactions/ToolMotifsClean.png',
        'Atmosphere/BirdRigOct08.png', 'Atmosphere/InsectRigOct08.png',
        'Navigation/WorldComic0.png', 'Navigation/WorldComic1.png', 'Navigation/WorldComic2.png',
        'Navigation/WorldComic0Identity.png', 'Navigation/WorldComicDepartureIdentity.png',
        'Craft/WorkshopRoomQuiet.png', 'Craft/WorkshopPortraitQuiet.png',
        'CleanMenuOct14/SquareButton.png', 'GameplayReferenceOct14/ReferenceDaySky.png',
        'ParcelReferenceOct13/ClosedParcel.png', 'ParcelReferenceOct13/OpenedParcel.png']
    roots = [art, ROOT / 'CascadeUnity/Assets/_Cascade/Resources/CascadeArt']
    for rel in replacements:
        assert sum((root / rel).is_file() for root in roots) == 1, 'Missing or ambiguous shared replacement: ' + rel
    print(f"PASS: {plan['png_count']} retired PNGs and {plan['code_files']} obsolete source files absent, no surviving retired GUID reference; shared replacements present.")

if __name__ == '__main__':
    verify()
