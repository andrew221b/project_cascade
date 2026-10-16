#!/usr/bin/env python3
"""Validate shipped acoustic masters, harmonic notation and references; not a listening test."""
from pathlib import Path
import argparse,hashlib,itertools,json,re,wave
import numpy as np
from compose_acoustic_score import ROOT,ASSETS,THEMES,FORM,compose
parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output',type=Path,default=ROOT/'Reports/AudioWorldsOct15/score-validation.json')
args=parser.parse_args();report=[];intros=[]
for i,theme in enumerate(THEMES):
 name=theme[0];path=ASSETS/(('Menu_HomeAmongCloudsBalanced' if i==0 else name)+'.wav');score=compose(theme,i)
 with wave.open(str(path)) as w:
  assert (w.getframerate(),w.getnchannels(),w.getsampwidth())==(44100,2,2)
  rate=w.getframerate();data=np.frombuffer(w.readframes(w.getnframes()),'<i2').reshape(-1,2).astype(float)/32768
 assert abs(len(data)/rate-score['seconds'])<1/rate
 assert len(data)/rate>=178 and np.max(np.abs(data))<.98
 assert np.max(np.abs(data[0]-data[-1]))<=1/32768
 assert {n['instrument'] for n in score['notes']}=={'piano','harp','cello'}
 assert score['bars']==64 and [s['name'] for s in score['sections']]==[s[0] for s in FORM]
 meta=path.with_suffix('.wav.meta').read_text();assert 'loadType: 2' in meta and 'loadInBackground: 1' in meta and 'preloadAudioData: 0' in meta
 melodies=[];levels={};cadences=[];beat=60/score['bpm']
 for section in score['sections']:
  end=section['start']+section['bars']*4*beat;excerpt=data[round(section['start']*rate):round(end*rate)]
  levels[section['name']]=round(20*np.log10(np.sqrt(np.mean(excerpt**2))),2)
  if section['bars']==8:
   melodies.append(tuple(n['pitch'] for n in score['notes'] if n['role']=='melody' and section['start']-.02<=n['time']<end-.02))
   first=round(section['start']/beat/4)
   if section['name']!='bridge':
    assert score['harmony'][first+3]['function']=='V'
    assert score['harmony'][first+7]['function'] in ('I','i')
    cadence=[n for n in score['notes'] if n['role']=='melody' and (first+7)*4*beat-.02<=n['time']<end-.02]
    assert cadence[-1]['pitch']%12==theme[2]%12
    cadences.append(section['name'])
 assert len(set(melodies))>=5, (name,'Refrains must develop')
 for chord in score['harmony']:
  assert len(chord['voices'])==3 and max(chord['voices'])-min(chord['voices'])<=12
  assert all(n%12 in chord['pitch_classes'] for n in chord['voices'])
  if chord['minor'] and chord['function'] in ('V','V7'):assert (chord['key']+11)%12 in chord['pitch_classes']
 for note in score['notes']:
  if note['role']!='melody':continue
  bar=min(63,int((note['time']+.015)/(beat*4)))
  assert note['pitch']%12 in score['harmony'][bar]['pitch_classes'],(name,bar,note)
 # Every world speaks in the opening bars, rather than waiting through identical introductions.
 assert any(n['role']=='melody' and n['time']<1 for n in score['notes'])
 intros.append(data[:rate*4:50,0]);sha=hashlib.sha256(path.read_bytes()).hexdigest()
 report.append(dict(name=name,asset=path.name,seconds=round(len(data)/rate,2),sha256=sha,sections_dbfs=levels,
                    developed_periods=len(set(melodies)),tonic_resolutions=cadences,peak=float(abs(data).max()),seam=float(abs(data[0]-data[-1]).max())))
assert len({r['sha256'] for r in report})==10
assert not (ASSETS/'Menu_HomeAmongClouds.wav').exists(), 'Do not recreate the retired duplicate'
correlations=[float(abs(np.corrcoef(a,b)[0,1])) for a,b in itertools.combinations(intros,2)]
assert max(correlations)<.92
asset=(ASSETS.parent.parent/'ToyAudio.asset').read_text();block=asset[asset.index('  music:'):asset.index('  naturalWind:')]
guids=re.findall(r'guid: ([a-f0-9]+)',block)
active={re.search('guid: ([a-f0-9]+)',p.read_text())[1]:p for p in ASSETS.glob('*.wav.meta')}
assert len(guids)==11 and len(set(guids))==10 and all(g in active for g in guids)
assert guids[0]==guids[1]
samples=ROOT/'Tools/AudioSamples/VSCO2';sources=json.loads((samples/'sources.json').read_text())
for source in sources:
 p=samples/source['file'];assert p.read_bytes()[:4]==b'RIFF'
 if 'sha256' in source:assert hashlib.sha256(p.read_bytes()).hexdigest()==source['sha256']
assert 'CC0 1.0 Universal' in (samples/'LICENSE').read_text()
args.output.parent.mkdir(parents=True,exist_ok=True)
args.output.write_text(json.dumps(dict(tracks=report,checks='PASS',maximum_intro_correlation=max(correlations),
 native_playback='Reported separately by native PlayMode tests',artistic_judgement='Requires human listening'),indent=2)+'\n')
print('PASS: ten distinct 64-bar acoustic masters; 11 valid references; PCM, streaming, headroom/seams; developed periods, dominant/tonic cadences, harmonic-minor leading tones and source integrity.')
