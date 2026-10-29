#!/usr/bin/env python3
"""Check actual shipped PCM, score harmony/tuning and routing assets; not a listening test."""
from pathlib import Path
import argparse,hashlib,itertools,json,re,wave
import numpy as np
from compose_acoustic_score import ROOT,ASSETS,THEMES,FORM,SR,compose,Sampler,fundamental
p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output',type=Path,default=ROOT/'Reports/MusicOct17/score-validation.json');a=p.parse_args()
records=[];intros=[];sampler=Sampler();tuned=[]
for inst,bank in sampler.banks.items():
 pitches=[60,64,69,72] if inst!='cello' else [38,43,48]
 for pitch in pitches:
  data=sampler.note(inst,pitch);measured=fundamental(data,SR,pitch);cents=100*(measured-pitch)
  assert abs(cents)<8,(inst,pitch,cents)
  tuned.append(dict(instrument=inst,pitch=pitch,residual_cents=round(cents,3)))
for index,t in enumerate(THEMES):
 for menu in ([False] if index==0 else [False,True]):
  score=compose(t,index,menu);path=ASSETS/(score['name']+'.wav')
  with wave.open(str(path)) as w:
   assert (w.getframerate(),w.getnchannels(),w.getsampwidth())==(SR,2,2)
   data=np.frombuffer(w.readframes(w.getnframes()),'<i2').reshape(-1,2).astype(np.float32)/32768
  assert abs(len(data)/SR-score['seconds'])<1/SR
  assert score['bars']==64 and score['seconds']>90
  assert np.max(abs(data))<.95 and np.max(abs(data[0]-data[-1]))<=1/32768
  assert np.sqrt(np.mean(data*data))>.075
  assert np.max(abs(data.mean(0)))<1e-5
  meta=path.with_suffix('.wav.meta').read_text();assert 'loadType: 2' in meta and 'loadInBackground: 1' in meta and 'preloadAudioData: 0' in meta
  assert [s['name'] for s in score['sections']]==[s[0] for s in FORM]
  melody=sorted((n for n in score['notes'] if n['role']=='melody'),key=lambda n:n['time'])
  assert melody[0]['time']<.01
  for note in score['notes']:
   chord=score['harmony'][note['bar']]
   assert note['pitch']%12 in chord['pitch_classes'],(score['name'],note)
   assert 36<=note['pitch']<=84
  for first,next_note in zip(melody,melody[1:]):
   assert first['time']+first['duration']<=next_note['time']+.008,(score['name'],'Overlapping melody',first,next_note)
  for chord in score['harmony']:
   assert len(chord['voices'])==3 and max(chord['voices'])-min(chord['voices'])<=12
   if score['minor'] and chord['function']=='V':assert (score['root']+11)%12 in chord['pitch_classes']
  section_levels=[]
  for s in score['sections']:
   x=data[round(s['start']*SR):round((s['start']+s['bars']*score['meter']*60/score['bpm'])*SR)]
   section_levels.append(float(20*np.log10(np.sqrt(np.mean(x*x)))))
  assert max(section_levels)-min(section_levels)>1,'An audible dynamic arc is required'
  # Short-time energy shows articulated movement, rather than an unvarying bed.
  size=round(.03*SR);mono=data.mean(1);frames=mono[:len(mono)//size*size].reshape(-1,size)
  env=np.sqrt(np.mean(frames*frames,axis=1));motion=float(np.std(env)/np.mean(env));assert motion>.12
  intros.append(data[:SR*4:50,0]);records.append(dict(name=score['name'],context=score['context'],world=score['world'],bpm=score['bpm'],meter=score['meter'],seconds=score['seconds'],peak=float(np.max(abs(data))),rms=float(np.sqrt(np.mean(data*data))),seam=float(np.max(abs(data[0]-data[-1]))),motion=motion,section_levels_dbfs=section_levels,sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
assert len(records)==19 and len({r['sha256'] for r in records})==19
for index in range(1,10):
 game=next(r for r in records if r['world']==index-1 and r['context']=='gameplay');menu=next(r for r in records if r['world']==index-1 and r['context']=='menu')
 assert game['bpm']-menu['bpm']>=25
 assert game['bpm']-records[0]['bpm']>=22
correlations=[float(abs(np.corrcoef(x,y)[0,1])) for x,y in itertools.combinations(intros,2)];assert max(correlations)<.90
asset=(ASSETS.parent.parent/'ToyAudio.asset').read_text();block=asset[asset.index('  music:'):asset.index('  naturalWind:')]
guids=re.findall(r'guid: ([a-f0-9]+)',block);active={re.search('guid: ([a-f0-9]+)',p.read_text())[1]:p for p in ASSETS.glob('*.wav.meta')}
assert len(guids)==20 and len(set(guids))==19 and all(g in active for g in guids) and guids[0]==guids[1]
original=json.loads((ROOT/'Reports/MusicOct17/existing-meta-hashes.json').read_text())
for name,digest in original.items():assert hashlib.sha256((ASSETS/(name+'.wav.meta')).read_bytes()).hexdigest()==digest
samples=ROOT/'Tools/AudioSamples/VSCO2';sources=json.loads((samples/'sources.json').read_text())
for source in sources:
 file=samples/source['file'];assert file.read_bytes()[:4]==b'RIFF';assert hashlib.sha256(file.read_bytes()).hexdigest()==source['sha256']
assert 'CC0 1.0 Universal' in (samples/'LICENSE').read_text()
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(dict(checks='PASS',tracks=records,source_count=len(sources),tuning=tuned,maximum_intro_correlation=max(correlations),native_playback='Reported separately',listening='Objective checks do not establish artistic quality; human listening required.'),indent=2)+'\n')
print('PASS: 19 distinct tonal acoustic loops, 9 contrasting menu/game pairs, calibrated 7-instrument bank, non-overlapping melody, bar length/seams/headroom/streaming, retained GUIDs and CC0 source integrity.')
