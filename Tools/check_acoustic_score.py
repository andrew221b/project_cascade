#!/usr/bin/env python3
"""Validate shipped audio, notation and references; not a listening/Editor test."""
from pathlib import Path
import hashlib,json,re,wave
import numpy as np
from compose_acoustic_score import ROOT,ASSETS,THEMES,FORM,compose
report=[]
for i,theme in enumerate(THEMES):
 name=theme[0]; path=ASSETS/(name+'.wav'); score=compose(theme,i)
 with wave.open(str(path)) as w:
  assert (w.getframerate(),w.getnchannels(),w.getsampwidth())==(44100,2,2)
  rate=w.getframerate(); data=np.frombuffer(w.readframes(w.getnframes()),'<i2').reshape(-1,2).astype(float)/32768
 assert abs(len(data)/rate-score['seconds'])<1/rate
 assert len(data)/rate>=178 and np.max(np.abs(data))<.98
 assert np.max(np.abs(data[0]-data[-1]))<=1/32768
 assert {n['instrument'] for n in score['notes']}=={'piano','harp','cello'}
 assert score['bars']==64 and [s['name'] for s in score['sections']]==[s[0] for s in FORM]
 meta=path.with_suffix('.wav.meta').read_text();assert 'loadType: 2' in meta and 'loadInBackground: 1' in meta and 'preloadAudioData: 0' in meta
 melodies=[]; levels={}
 for s in score['sections']:
  end=s['start']+s['bars']*4*60/score['bpm']; excerpt=data[round(s['start']*rate):round(end*rate)]
  levels[s['name']]=round(20*np.log10(np.sqrt(np.mean(excerpt**2))),2)
  if s['bars']==8: melodies.append(tuple(n['pitch'] for n in score['notes'] if n['role']=='melody' and s['start']-.02<=n['time']<end-.02))
 assert len(set(melodies))>=5, 'Refrains must develop instead of being duplicated audio'
 report.append(dict(name=name,seconds=round(len(data)/rate,2),sha256=hashlib.sha256(path.read_bytes()).hexdigest(),sections_dbfs=levels))
assert len({r['sha256'] for r in report})==10
assert (ASSETS/'Menu_HomeAmongCloudsBalanced.wav').read_bytes()==(ASSETS/'Menu_HomeAmongClouds.wav').read_bytes()
asset=(ASSETS.parent.parent/'ToyAudio.asset').read_text()
block=asset[asset.index('  music:'):asset.index('  naturalWind:')]
guids=re.findall(r'guid: ([a-f0-9]+)',block)
active={re.search('guid: ([a-f0-9]+)',p.read_text())[1]:p for p in ASSETS.glob('*.wav.meta')}
assert len(guids)==11 and len(set(guids))==10 and all(g in active for g in guids)
assert guids[0]==guids[1], 'Legacy fallback must not reintroduce synthetic loop'
# Source sample integrity/license are recorded to make offline reproduction auditable.
samples=ROOT/'Tools/AudioSamples/VSCO2';sources=json.loads((samples/'sources.json').read_text())
for s in sources:
 p=samples/s['file'];assert p.read_bytes()[:4]==b'RIFF';s['sha256']=hashlib.sha256(p.read_bytes()).hexdigest()
(samples/'sources.json').write_text(json.dumps(sources,indent=2)+'\n')
assert 'CC0 1.0 Universal' in (samples/'LICENSE').read_text()
out=ROOT/'Reports/AcousticScore-Oct09/validation.json';out.write_text(json.dumps(dict(tracks=report,checks='PASS',native_playback='NOT RUN',listening='NOT VERIFIED'),indent=2))
print('PASS: 10 unique 64-bar acoustic scores, 11 active/fallback references, PCM/seams/headroom, streaming imports, refrain variation, sample/license provenance.')
print('Durations: %.1f–%.1f seconds. Native playback/listening NOT VERIFIED.'%(min(x['seconds'] for x in report),max(x['seconds'] for x in report)))
