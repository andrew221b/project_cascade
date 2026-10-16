#!/usr/bin/env python3
"""Original long-form CASCADE score, rendered from credited CC0 acoustic recordings.
No synthesizer, GM bank, runtime sampler or new Unity dependency. Offline only.
Requires numpy/scipy. Run --output <folder>; --install explicitly replaces existing clips.
"""
from pathlib import Path
from functools import lru_cache
from fractions import Fraction
import argparse, hashlib, json, math, re, shutil, warnings
import numpy as np
from scipy.io import wavfile
from scipy import signal
ROOT=Path(__file__).resolve().parents[1]
SAMPLES=ROOT/'Tools/AudioSamples/VSCO2'
ASSETS=ROOT/'CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Audio/Atmosphere'
SR=44100
# Each theme has its own eight-bar question/answer melody, harmony and tempo.
THEMES=[
 ('Menu_HomeAmongClouds',76,62,False,[0,2,4, 5,4,2, 1,2,0, -1,0,2, 4,5,4, 2,1,0, -2,0,1, 2,1,0]),
 ('WindGarden_PaperWings',84,67,False,[0,1,4, 2,4,5, 4,2,1, 2,0,-1, 0,2,5, 4,5,2, 1,2,0, -1,1,0]),
 ('RainMill_RainOnCedar',70,57,True,[2,1,0, -2,-1,1, 2,4,2, 1,0,-1, 0,2,4, 3,2,0, -1,1,2, 1,-1,0]),
 ('CopperOrchard_GoldenAfternoon',86,60,False,[2,4,2, 0,1,2, 4,5,4, 2,1,-1, 0,4,5, 4,2,4, 2,1,0, -2,-1,0]),
 ('CloudPost_LettersOnTheBreeze',80,65,False,[0,2,1, 4,2,0, 1,4,5, 4,2,1, 2,5,4, 2,0,1, -1,0,2, 1,-1,0]),
 ('GlassLagoon_ReflectedMoon',72,62,True,[4,2,1, 0,-1,0, 2,1,4, 2,0,-2, -1,1,2, 4,3,2, 0,1,-1, 1,2,0]),
 ('MossArchive_StoriesInTheLeaves',68,59,True,[-2,0,1, 2,0,-1, -2,-1,2, 1,0,-2, 0,1,4, 2,1,0, -1,0,2, 1,-1,0]),
 ('SnowSpire_WarmLightInSnow',72,64,False,[2,1,0, 4,2,1, 0,-1,1, 2,4,2, 5,4,2, 1,2,0, -1,1,2, 0,1,0]),
 ('GreatBeacon_TheWayHome',82,58,False,[0,2,4, 2,4,5, 4,1,2, 0,-1,1, 2,4,5, 4,5,4, 2,1,-1, 0,1,0]),
 ('StarObservatory_ConstellationLullaby',66,62,False,[4,2,0, 1,-1,0, 2,4,1, 0,-2,-1, 0,2,4, 2,1,0, -2,-1,1, 2,1,0]),
]
FORM=[('intro',4),('verse',8),('chorus',8),('verse_varied',8),('chorus_answer',8),('bridge',8),('chorus_return',8),('chorus_coda',8),('turnaround',4)]
# Eight-bar periods: a dominant question, then a tonic answer. Inversions keep
# the inner voices close; minor dominants use the raised seventh, not modal V.
PROGRESSIONS={
 False:[('I',0,[0,4,7]),('vi',9,[0,4,9]),('ii6',5,[2,5,9]),('V',7,[2,7,11]),
        ('I6',4,[0,4,7]),('IV',5,[0,5,9]),('V7',7,[2,5,7,11]),('I',0,[0,4,7])],
 True:[('i',0,[0,3,7]),('VI',8,[0,3,8]),('iiø6',5,[2,5,8]),('V',7,[2,7,11]),
       ('i6',3,[0,3,7]),('iv',5,[0,5,8]),('V7',7,[2,5,7,11]),('i',0,[0,3,7])]
}
RHYTHMS=[[(0,1.25),(1.5,.5),(2.25,1.3)],[(0,.7),(1,.7),(2,1.5)],
         [(.25,1.5),(2,.5),(3,.7)],[(0,.5),(.75,.5),(2,1.5)],
         [(0,1.7),(2.5,.45),(3.25,.5)],[(.5,1),(2,.6),(3, .7)],
         [(0,1.25),(1.75,.75),(3,.65)],[(0,1.75),(2.5,1.1)],
         [(0,.85),(1.25,.5),(2.5,1.2)],[(.25,1.25),(2,.75),(3,.65)]]

def compose(theme,index):
 name,bpm,root,minor,motif=theme; beat=60/bpm; rng=np.random.default_rng(151015+index)
 scale=[0,2,3,5,7,8,10] if minor else [0,2,4,5,7,9,11]
 def degree(d,key=root): return key+12*(d//7)+scale[d%7]
 events=[]; sections=[]; harmony=[]; bar=0; previous=[root-12,root-8,root-5]; last_lead=root
 def add(inst,pitch,start,duration,velocity,role):
  events.append(dict(instrument=inst,pitch=int(pitch),time=max(0,start*beat+float(rng.uniform(-.009,.009))),duration=duration*beat,gain=velocity*float(rng.uniform(.95,1.035)),role=role))
 def nearest(desired,pcs,previous_pitch=None):
  candidates=[p for p in range(max(52,root-5),min(82,root+17)) if p%12 in pcs]
  return min(candidates,key=lambda p:abs(p-desired)+(max(0,abs(p-previous_pitch)-5)*.8 if previous_pitch is not None else 0))
 for section,count in FORM:
  sections.append(dict(name=section,start=bar*4*beat,bars=count))
  chorus=section.startswith('chorus'); bridge=section=='bridge'; sparse=section in ('intro','turnaround')
  for j in range(count):
   b=bar+j; start=b*4; h=j%8; key=root; local_minor=minor
   # The middle eight visits the relative major/dominant, then returns via V7.
   if bridge and j<6: key=root+(3 if minor else 7);local_minor=False
   progression=PROGRESSIONS[local_minor]
   if section=='intro': h=[0,5,2,6][j]
   if section=='turnaround': h=[5,2,6,6][j]
   if bridge and j>=6: h=6
   label,bass,intervals=progression[h]
   if section=='chorus_answer' and j==6: label,bass,intervals=('cadential64',7,[0,3 if minor else 4,7])
   pcs={(key+n)%12 for n in intervals}
   # Three middle-register voices choose the nearest legal inversion independently,
   # preserving their order and leaving the melody and cello their own registers.
   import itertools
   candidates=[p for p in range(root-19,root+1) if p%12 in pcs]
   triples=[v for v in itertools.combinations(candidates,3) if len({p%12 for p in v})==3 and v[-1]-v[0]<=12]
   voices=min(triples,key=lambda v:sum(abs(a-c) for a,c in zip(previous,v))+abs(sum(v)/3-(root-9))*.35)
   previous=voices
   harmony.append(dict(bar=b,key=key,minor=local_minor,function=label,pitch_classes=sorted(pcs),voices=list(voices),bass_class=(key+bass)%12))
   volume=.9 if chorus else .64 if bridge else .63
   if sparse: volume=.48
   if index==9: volume*=.8
   # Each world keeps its own accompaniment gesture instead of ten identical intros.
   pattern=[(0,0),(1,2),(2,1),(3,2)] if index in (1,3,7,8) else [(0,0),(1.5,1),(2.5,2)]
   if bridge: pattern=[(0,0),(.06,1),(2.5,2)]
   if sparse: pattern=[(0,0),(2,2)]
   accompaniment='piano' if index in (2,6) else 'harp'
   for t,v in pattern: add(accompaniment,voices[v],start+t,1.25,.20*volume,'accompaniment')
   bp=key-24+bass
   while bp>49: bp-=12
   while bp<36: bp+=12
   add('cello',bp,start+.035,3.45,.095*volume,'bass')
   if chorus and j%2==0:
    for v in (0,1): add('piano',voices[v],start+.04+v*.025,2.7,.055,'harmony')
   phrase=motif[(j%8)*3:(j%8)*3+3]
   if chorus: phrase=[phrase[0]+2,phrase[1]+1,phrase[2]]
   if section=='verse_varied': phrase=[phrase[0],phrase[1]+1,phrase[2]]
   if bridge: phrase=[motif[(21-j*3)%24]-1,motif[(22-j*3)%24],motif[(23-j*3)%24]-1]
   if section=='chorus_coda' and j>=4: phrase=[p-1 for p in phrase]
   timings=RHYTHMS[index]
   if sparse: timings=[(.25,1.3),(2,1.35)]
   if j in (3,7) and not sparse: timings=[(0,1.4),(2,1.65)]
   for n,(offset,duration) in enumerate(timings):
    desired=degree(phrase[n],key)
    # Strong beats belong to the chord; a weak neighbour resolves immediately.
    pitch=nearest(desired,pcs,last_lead)
    if j==7 and n==len(timings)-1 and not bridge: pitch=nearest(root,{root%12})
    if j==3 and n==len(timings)-1 and not sparse: pitch=nearest(key+7,{(key+7)%12})
    lead='harp' if index in (3,4,7,9) and (sparse or section in ('verse','bridge')) else 'piano'
    add(lead,pitch,start+offset,duration,.42 if chorus else .32 if sparse else .37,'melody')
    if section in ('verse_varied','chorus_return') and n==0 and duration>1:
     # A diatonic upper neighbour on an unaccented eighth returns to its anchor.
     neighbour=min((degree(d,key) for d in range(-7,15) if degree(d,key)>pitch),default=pitch+2)
     add(lead,neighbour,start+offset+.65,.24,.20,'passing');add(lead,pitch,start+offset+.95,.3,.23,'resolution')
    last_lead=pitch
   # A short answer in the lower register at the end of each two-bar thought.
   if chorus and j in (1,5):
    response=nearest(root+phrase[-1],pcs)-12
    add('harp' if lead=='piano' else 'piano',response,start+3.55,.38,.14,'response')
  bar+=count
 return dict(name=name,bpm=bpm,seconds=bar*4*beat,bars=bar,sections=sections,harmony=harmony,notes=events)

class Sampler:
 def __init__(self):
  self.banks={k:[] for k in ('piano','harp','cello')}
  names={'C':0,'D':2,'E':4,'F':5,'G':7,'A':9,'B':11}
  for p in sorted(SAMPLES.glob('*.wav')):
   if p.name.startswith('Player'):
    inst='piano'; pitch=21+2*int(p.stem.split('_')[-1])
   else:
    inst='harp' if p.name.startswith('KSHarp') else 'cello'
    m=re.search(r'_([A-G])(\d)_',p.name)
    pitch=12*(int(m[2])+(1 if inst=='harp' else 2))+names[m[1]]
   with warnings.catch_warnings():
    warnings.simplefilter('ignore'); sr,x=wavfile.read(p)
   x=x.astype(np.float32)/float(2**(8*x.dtype.itemsize-1))
   if x.ndim==1:x=np.stack([x,x],axis=1)
   if sr!=SR:x=signal.resample_poly(x,SR, sr,axis=0)
   # Remove recorder DC, preserve natural stereo/attack and relative decay.
   x=signal.sosfilt(signal.butter(2,35,fs=SR,btype='highpass',output='sos'),x,axis=0).astype(np.float32)
   x*=.65/max(.001,float(np.max(np.abs(x))))
   self.banks[inst].append((pitch,x))
 @lru_cache(maxsize=256)
 def note(self,inst,pitch):
  source,x=min(self.banks[inst],key=lambda z:abs(z[0]-pitch))
  ratio=Fraction(2**((source-pitch)/12)).limit_denominator(600)
  y=signal.resample_poly(x,ratio.numerator,ratio.denominator,axis=0).astype(np.float32)
  cutoff=4800 if inst=='piano' else 4200 if inst=='harp' else 3000
  return signal.sosfilt(signal.butter(2,cutoff,fs=SR,output='sos'),y,axis=0).astype(np.float32)
 def render(self,score):
  frames=round(score['seconds']*SR); tail=SR*10; mix=np.zeros((frames+tail,2),np.float32)
  for e in score['notes']:
   inst=e['instrument']; src=self.note(inst,e['pitch']); hold=e['duration']
   release=.85 if inst=='cello' else 1.4
   size=min(len(src),round((hold+release)*SR)); x=src[:size].copy()
   attack=min(size,round(SR*(.16 if inst=='cello' else .007)))
   x[:attack]*=np.linspace(0,1,attack,dtype=np.float32)[:,None]
   end=min(size,round(hold*SR)); n=size-end
   if n:x[end:]*=np.linspace(1,0,n,dtype=np.float32)[:,None]**1.5
   x[-min(512,size):]*=np.linspace(1,0,min(512,size),dtype=np.float32)[:,None]
   pan={'piano':-.08,'harp':.25,'cello':-.22}[inst]
   x*=np.array([math.sqrt(1-pan),math.sqrt(1+pan)],np.float32)*e['gain']
   at=round(e['time']*SR); length=min(size,len(mix)-at); mix[at:at+length]+=x[:length]
  # Diffuse stereo room: low-passed, non-rhythmic reflection cloud, no bright echoes.
  irlen=round(2.7*SR); rng=np.random.default_rng(491); ir=rng.normal(0,1,(irlen,2)).astype(np.float32)
  ir*=np.exp(-np.arange(irlen)[:,None]/(SR*.47)); ir[:round(.025*SR)]=0
  ir=signal.sosfilt(signal.butter(2,2400,fs=SR,output='sos'),ir,axis=0)
  ir/=np.sqrt((ir*ir).sum(axis=0)); wet=np.zeros_like(mix)
  for ch in range(2):wet[:,ch]=signal.fftconvolve(mix[:,ch],ir[:,ch])[:len(mix)]
  mix+=wet*.15
  # Carry ALL release/reverb across the exact musical boundary; no silent gap/fade reset.
  mix[:tail]+=mix[frames:frames+tail]; mix=mix[:frames]
  mix-=mix.mean(axis=0)
  rms=float(np.sqrt(np.mean(mix.astype(np.float64)**2))); peak=float(np.max(np.abs(mix)))
  gain=min(.105/rms,.73/peak); mix*=gain
  # Correct only the very short sample-boundary discontinuity; sustain is retained.
  edge=128; delta=mix[-1]-mix[0]; mix[:edge]+=delta[None,:]*np.linspace(1,0,edge)[:,None]
  return np.clip(mix,-.98,.98)

def main():
 parser=argparse.ArgumentParser();parser.add_argument('--output',type=Path,default=ROOT/'Reports/AcousticScore-Oct09');parser.add_argument('--theme');parser.add_argument('--install',action='store_true');args=parser.parse_args()
 args.output.mkdir(parents=True,exist_ok=True); sampler=Sampler(); report=[]
 for i,t in enumerate(THEMES):
  if args.theme and args.theme!=t[0]:continue
  score=compose(t,i);x=sampler.render(score);path=args.output/(t[0]+'.wav')
  wavfile.write(path,SR,(x*32767).astype(np.int16));(args.output/(t[0]+'.score.json')).write_text(json.dumps(score,indent=2))
  record=dict(name=t[0],seconds=len(x)/SR,notes=len(score['notes']),peak=float(np.max(np.abs(x))),rms=float(np.sqrt(np.mean(x*x))),seam=float(np.max(np.abs(x[0]-x[-1]))),sha256=hashlib.sha256(path.read_bytes()).hexdigest())
  assert record['peak']<.98 and record['seam']<1e-5
  if args.install:
   # One-time backup; never replace user GUID/import settings.
   backup=args.output/'previous';backup.mkdir(exist_ok=True)
   targets=['Menu_HomeAmongCloudsBalanced'] if i==0 else [t[0]]
   for name in targets:
    dest=ASSETS/(name+'.wav');assert dest.exists() and dest.with_suffix('.wav.meta').exists()
    if not (backup/dest.name).exists():shutil.copy2(dest,backup/dest.name)
    shutil.copyfile(path,dest)
  report.append(record);print(f"{t[0]}: {record['seconds']:.1f}s, {record['notes']} notes, peak {record['peak']:.3f}",flush=True)
 (args.output/'render-report.json').write_text(json.dumps(report,indent=2))
if __name__=='__main__':main()
