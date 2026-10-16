#!/usr/bin/env python3
"""Conservative, explicit art retirement list; never infer deletion from a grep miss."""
from pathlib import Path
import argparse, hashlib, json, re, zipfile

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / 'CascadeUnity/Assets'
ART = ASSETS / '_Cascade/Content/Resources/CascadeArt'
REPORT = ROOT / 'Reports/AssetCleanupOct15'
PLAN = REPORT / 'retired-assets.json'

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def candidates():
    entries = []
    def add(folder, names, reason, replacement=None, extension='.png'):
        for name in names:
            entries.append((ART / folder / (name + extension), reason, replacement))
    add('HomeOct13', ['IslandSmall','IslandLeftLow','IslandRightHigh','IslandLeftHigh','IslandRightLow',
        'LeafLeft','LeafRight','LeafMidLeft','LeafMidRight'],
        'No loader or serialized reference remains; home uses WorldIslandArt and MenuLeafDrift.')
    add('HomeOct13', ['BagButton','ShopButton','SettingsButton'],
        'HomeReferenceButton constructs NavButtonFrame plus the existing BagIcon/ShopIcon/SettingsIcon; baked button variants are never loaded.')
    add('WorldMenusOct14', ['ShopFrameMask','HomeFrameMask','PauseFrameMask'],
        'WorldFrameArt.MenuFrame loads HomeOct13/FrameForeground for these screens; only MapFrameMask is used here.')
    add('WorldFramesOct14', ['Frame'+str(i) for i in range(9)],
        'WorldFrameArt.Texture uses the shared GameplayFrame; menu uses the first-world frame and FrameSurface material reskins.')
    add('MapReferenceOct14', ['Chapel','Cottage','Garden','Portal','Tower','LogoEn','LogoRu','LogoUk','Plank','Ground'],
        'Replaced map uses WorldIslandArt, CleanMenuArt.WorldTitle and WorldFramesOct14/ContinuousGround; no direct or dynamic caller loads these old variants.')
    for folder, names in [('Journey',['WindGardenMap','RainMillMap','CopperOrchardMap']),
        ('Woodland',['CloudPostMap','GlassLagoonMap','MossArchiveMap','SnowSpireMap','GreatBeaconMap','StarObservatoryMap','ObservatoryIslands'])]:
        add(folder,names,'Only the removed private DrawLegacyJourney loaded the old flattened maps. Gameplay and current menus use independent Base skies and 54 WorldIslandArt sprites.')
    add('GameplayReferenceOct14',['FailureMika'],
        'FailureReferenceArt uses FailureReferenceAtlas/FailureAtlas. No loader uses the earlier separate Mika draft.')
    add('Workshop',['VictoryMikaLanternOct11','VictoryMikaLanternPortraitOct11','VictoryMikaLanternPortraitOct12','VictoryMikaLanternPortraitOct13'],
        'Completion uses VictoryMikaLanternCutoutOct12. These earlier character drafts have no runtime or rebuild-tool loader.')
    add('ShopReferenceOct13',['PreviewPaper'],
        'No loader remains; current previews reuse DescriptionPaper and PreviewPlatform.')
    for folder in ['ShopReferenceOct13','ParcelReferenceOct13']:
        for path in sorted((ART/folder).glob('*.png')):
            replacement=ART/'CleanMenuOct14'/path.name
            if replacement.exists():
                add(folder,[path.stem],
                    'ReferenceShopArt/ParcelArt prefer the existing CleanMenuArt sprite with this name. All current consumers already share the replacement.',
                    str(replacement.relative_to(ROOT)))
    add('Audio/Atmosphere',['Menu_HomeAmongClouds'],
        'Exact byte duplicate of the active Balanced score. ToyAudio.asset references only Balanced; the importer name comparison does not load this file.',
        str((ART/'Audio/Atmosphere/Menu_HomeAmongCloudsBalanced.wav').relative_to(ROOT)),'.wav')
    return entries

def serialized_users(guids, excluded):
    users={guid:[] for guid in guids}
    for path in ASSETS.rglob('*'):
        if not path.is_file() or path in excluded or path.suffix not in {'.meta','.asset','.prefab','.unity','.mat','.shader','.cs','.json','.uxml','.uss','.asmdef','.controller','.anim','.overrideController'}:
            continue
        data=path.read_text(errors='ignore')
        for guid in set(re.findall(r'\b[a-f0-9]{32}\b',data)) & guids:
            users[guid].append(str(path.relative_to(ROOT)))
    return users

def make_plan():
    entries=candidates();excluded={p for p,_,_ in entries}|{Path(str(p)+'.meta') for p,_,_ in entries}
    rows=[]
    for path,reason,replacement in entries:
        assert path.exists(),path
        meta=Path(str(path)+'.meta');assert meta.exists(),meta
        guid=re.search(r'^guid: (\w+)',meta.read_text(),re.M)[1]
        rows.append(dict(path=str(path.relative_to(ROOT)),bytes=path.stat().st_size,sha256=digest(path),
            meta_sha256=digest(meta),guid=guid,reason=reason,replacement=replacement))
    users=serialized_users({r['guid'] for r in rows},excluded)
    for row in rows:
        row['serialized_users']=users[row['guid']]
        assert not row['serialized_users'],(row['path'],row['serialized_users'])
        if row['replacement']:assert (ROOT/row['replacement']).exists()
    active=ART/'Audio/Atmosphere/Menu_HomeAmongCloudsBalanced.wav'
    assert digest(active)==digest(ART/'Audio/Atmosphere/Menu_HomeAmongClouds.wav')
    REPORT.mkdir(parents=True,exist_ok=True)
    PLAN.write_text(json.dumps(dict(files=len(rows),bytes=sum(r['bytes'] for r in rows),assets=rows),indent=2)+'\n')
    print(json.dumps(dict(files=len(rows),MiB=round(sum(r['bytes'] for r in rows)/1048576,2),plan=str(PLAN))))

def apply_plan():
    plan=json.loads(PLAN.read_text());rows=plan['assets']
    excluded={ROOT/r['path'] for r in rows}|{Path(str(ROOT/r['path'])+'.meta') for r in rows}
    assert not any(serialized_users({r['guid'] for r in rows},excluded).values())
    for row in rows:
        path=ROOT/row['path'];assert digest(path)==row['sha256'];assert digest(Path(str(path)+'.meta'))==row['meta_sha256']
        if row['replacement']:assert (ROOT/row['replacement']).exists()
    backup=Path('/private/tmp/cascade-asset-cleanup-oct15-backup.zip')
    assert not backup.exists(),'Do not overwrite the recovery archive'
    with zipfile.ZipFile(backup,'x',zipfile.ZIP_DEFLATED) as archive:
        for path in sorted(excluded):archive.write(path,str(path.relative_to(ROOT)))
    for path in sorted(excluded):path.unlink()
    plan.update(applied=True,recovery_archive=str(backup))
    PLAN.write_text(json.dumps(plan,indent=2)+'\n')
    print(f'Removed {len(rows)} retired assets and their matching meta files; active references preserved.')

def verify():
    plan=json.loads(PLAN.read_text());assert plan.get('applied')
    for row in plan['assets']:
        assert not (ROOT/row['path']).exists()
        assert not Path(str(ROOT/row['path'])+'.meta').exists()
        if row['replacement']:assert (ROOT/row['replacement']).exists()
    assert not any(serialized_users({r['guid'] for r in plan['assets']},set()).values())
    print('PASS: no remaining serialized reference targets a retired GUID; all shared replacements exist.')

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--apply',action='store_true');parser.add_argument('--verify',action='store_true');args=parser.parse_args()
    if args.apply:apply_plan()
    elif args.verify:verify()
    else:make_plan()
