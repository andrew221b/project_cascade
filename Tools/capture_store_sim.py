#!/usr/bin/env python3
"""Capture actual iOS Simulator pixels, never Editor or synthetic gameplay."""
import argparse, hashlib, json, pathlib, subprocess, time
from datetime import datetime, timezone

def run(*args):
    return subprocess.check_output(['xcrun','simctl',*args],text=True).strip()

p=argparse.ArgumentParser();p.add_argument('--device',required=True);p.add_argument('--folder',default='iPhone')
args=p.parse_args();root=pathlib.Path(__file__).resolve().parents[1]
devices=json.loads(run('list','devices','-j'))['devices']
device=next(d for group in devices.values() for d in group if d['udid']==args.device)
assert device['name'].startswith('CASCADE Store Capture'), 'Dedicated capture simulator required.'
container=pathlib.Path(run('get_app_container',args.device,'com.vickand.cascade.capture','data'))/'Documents'
dest=root/'Reports/StoreAssets-Oct09/Raw'/args.folder;dest.mkdir(parents=True,exist_ok=True)
entries=[]
for command in ['day','night','advanced','words','bag','map']:
    (container/'store-capture-command.txt').write_text(command+'\n')
    status=container/'store-capture-status.txt';deadline=time.monotonic()+60
    while time.monotonic()<deadline:
        text=status.read_text() if status.exists() else ''
        if text.startswith(command+' ready;'):break
        if text.startswith('error:'):raise RuntimeError(text)
        time.sleep(.25)
    else:raise TimeoutError('Native screen not ready: '+command)
    output=dest/(command+'.png')
    run('io',args.device,'screenshot','--type=png','--mask=ignored',str(output))
    entries.append({'file':output.name,'device':device['name'],'deviceId':args.device,
        'capture':'xcrun simctl io screenshot --mask=ignored (unmasked native framebuffer)','nativeStatus':text,'capturedUtc':datetime.now(timezone.utc).isoformat(),
        'sha256':hashlib.sha256(output.read_bytes()).hexdigest()})
(dest/'sources.json').write_text(json.dumps(entries,indent=2)+'\n')
print('Captured',len(entries),'native screens on',device['name'])
