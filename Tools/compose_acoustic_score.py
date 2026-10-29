#!/usr/bin/env python3
"""Original tonal sky-island chamber score. Offline CC0 acoustic recordings only.

Menu and gameplay are separately composed performances, never pitch/time stretched.
Notation, source tuning and loop evidence accompany every master. No runtime sampler.
"""
from pathlib import Path
from functools import lru_cache
from fractions import Fraction
import argparse, hashlib, itertools, json, math, re, shutil, uuid, warnings
import numpy as np
from scipy.io import wavfile
from scipy import signal
ROOT=Path(__file__).resolve().parents[1]
SAMPLES=ROOT/'Tools/AudioSamples/VSCO2'
ASSETS=ROOT/'CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Audio/Atmosphere'
SR=44100
# name, gameplay quarter-note BPM, tonic, minor, meter, solo, accompaniment, authored motif
THEMES=[
 ('Menu_HomeAmongClouds',78,62,False,3,'piano','harp',[0,2,4, 5,4,2, 3,5,7, 6,4,1, 2,4,7, 5,3,2, 1,4,6, 4,2,0]),
 ('WindGarden_PaperWings',112,67,False,4,'flute','harp',[4,2,0, 2,5,4, 3,5,7, 6,4,1, 4,7,6, 5,3,2, 4,1,-1, 2,4,0]),
 ('RainMill_RainOnCedar',100,62,True,3,'piano','pizz',[2,0,-2, 0,2,5, 3,2,0, 1,4,6, 4,2,0, 3,5,4, 1,-1,1, 2,0,0]),
 ('CopperOrchard_GoldenAfternoon',120,60,False,4,'violin','pizz',[0,4,2, 5,4,7, 5,3,0, 1,4,6, 7,4,2, 3,5,7, 6,4,1, 2,4,0]),
 ('CloudPost_LettersOnTheBreeze',126,65,False,3,'flute','piano',[7,4,2, 4,5,7, 5,7,3, 4,6,8, 7,4,0, 3,5,2, 1,4,6, 4,2,0]),
 ('GlassLagoon_ReflectedMoon',104,69,True,4,'flute','harp',[4,7,4, 5,2,0, 3,5,7, 6,4,1, 2,4,7, 5,3,0, 1,4,6, 4,2,0]),
 ('MossArchive_StoriesInTheLeaves',102,60,True,4,'clarinet','piano',[0,-2,2, 5,4,2, 3,0,5, 4,1,-1, 2,0,4, 5,3,0, 1,4,6, 2,0,0]),
 ('SnowSpire_WarmLightInSnow',108,63,False,3,'piano','harp',[7,4,2, 5,2,4, 3,5,7, 6,4,1, 2,4,7, 5,3,7, 6,4,1, 4,2,0]),
 ('GreatBeacon_TheWayHome',116,62,False,4,'violin','harp',[0,2,4, 5,7,4, 3,5,7, 6,8,6, 7,4,2, 3,5,7, 6,4,1, 4,2,0]),
 ('StarObservatory_ConstellationLullaby',106,65,False,3,'harp','pizz',[4,7,9, 7,5,4, 5,7,3, 4,6,8, 7,4,2, 3,5,7, 6,4,1, 4,2,0]),
]
FORM=[('theme',8),('answer',8),('lift',8),('variation',8),('middle',8),('return',8),('flight',8),('home',8)]
# All three voices and melody anchors share this explicitly tonal grammar.
MAJOR=[('I',0,[0,4,7]),('vi',9,[0,4,9]),('IV',5,[0,5,9]),('V',7,[2,7,11]),
       ('I6',4,[0,4,7]),('ii',2,[2,5,9]),('V',7,[2,7,11]),('I',0,[0,4,7])]
MINOR=[('i',0,[0,3,7]),('VI',8,[0,3,8]),('iv',5,[0,5,8]),('V',7,[2,7,11]),
       ('i6',3,[0,3,7]),('iv',5,[0,5,8]),('V',7,[2,7,11]),('i',0,[0,3,7])]
MAP_BPMS=[0,76,72,82,86,74,70,78,84,72]

def compose(theme,index,menu=False):
 name,bpm,root,minor,meter,solo,accomp,motif=theme
 if index==0: name='Menu_HomeAmongCloudsBalanced'
 elif menu: name+='_Menu';bpm=MAP_BPMS[index];meter=3;solo='piano' if index in (1,4,7) else 'harp' if index in (3,5,9) else 'clarinet'
 beat=60/bpm; rng=np.random.default_rng(171017+index*37+int(menu));events=[];harmony=[];sections=[]
 scale=[0,2,3,5,7,8,10] if minor else [0,2,4,5,7,9,11]
 def degree(d):return root+12*(d//7)+scale[d%7]
 def nearest(desired,pcs,low=55,high=84):
  return min((p for p in range(low,high+1) if p%12 in pcs),key=lambda p:abs(p-desired))
 def add(inst,pitch,start,duration,gain,role,bar,chord):
  # Events do not carry a foreign harmony across the bar. Natural release is short;
  # unlike the old score, ornaments replace a note instead of sounding on top of it.
  if duration<=0:return
  events.append(dict(instrument=inst,pitch=int(pitch),time=max(0,start*beat+float(rng.uniform(-.004,.004))),
    duration=duration*beat,gain=gain*float(rng.uniform(.96,1.035)),role=role,bar=bar,chord=sorted(chord)))
 bar=0;previous=(root-12,root-8,root-5)
 for section,count in FORM:
  sections.append(dict(name=section,start=bar*meter*beat,bars=count));bright=section in ('lift','return','flight');quiet=section in ('middle','home')
  for j in range(count):
   b=bar+j;start=b*meter; h=j
   if section=='middle':h=[5,1,2,3,5,2,6,7][j]
   label,bass,intervals=(MINOR if minor else MAJOR)[h];pcs={(root+n)%12 for n in intervals}
   candidates=[p for p in range(root-21,root) if p%12 in pcs]
   triples=[v for v in itertools.combinations(candidates,3) if len({p%12 for p in v})==3 and v[-1]-v[0]<=12]
   voices=min(triples,key=lambda v:sum(abs(a-c) for a,c in zip(previous,v))+abs(sum(v)/3-(root-10))*.5);previous=voices
   harmony.append(dict(bar=b,function=label,pitch_classes=sorted(pcs),voices=list(voices),bass_class=(root+bass)%12))
   energy=.82 if bright else .57 if quiet else .7
   if menu or index==0:energy*=.82
   # The recognizable beat comes from articulated acoustic attacks, not a pad.
   if menu or index==0:
    pulse=[(0,0),(.98,2),(1.98,1)]
    if index in (2,5,9):pulse=[(0,0),(.5,2),(1.5,1),(2.5,2)]
   elif meter==3:
    # Compound 6/8 for rain/cloud/snow, triple quarter feel for the observatory.
    pulse=[(0,0),(.5,1),(1,2),(1.5,0),(2,2),(2.5,1)] if index!=9 else [(0,0),(1,2),(1.5,1),(2,2),(2.5,1)]
   else:
    patterns={1:[0,.5,1,2,2.5,3,3.5],3:[0,1,1.5,2,3,3.5],5:[0,.75,1.5,2,2.75,3.5],6:[0,.5,1.5,2,2.5,3.5],8:[0,.5,1,1.5,2,2.5,3,3.5]}
    pulse=[(t,[0,2,1,2][n%4]) for n,t in enumerate(patterns[index])]
   for n,(offset,v) in enumerate(pulse):
    inst='harp' if menu or index==0 else accomp
    g=(.21 if n==0 else .15)*energy
    add(inst,voices[v],start+offset,min(.43 if inst=='pizz' else .65,meter-offset-.04),g,'pulse',b,pcs)
   bp=root-24+bass
   while bp>48:bp-=12
   while bp<36:bp+=12
   # Cello bows breathe at the half bar; plucked root establishes the downbeat.
   add('cello',bp,start+.025,meter*.78,.095*energy,'bass',b,pcs)
   if not menu and index!=0:
    add('pizz',bp+12,start,.42,.20*energy,'bass_pulse',b,pcs)
    add('pizz',nearest(bp+19,pcs,40,64),start+meter/2,.38,.12*energy,'bass_pulse',b,pcs)
   if bright and not menu:
    for v in voices[1:]:add('violin',v+12,start+.055,meter-.28,.045,'warmth',b,pcs)
   phrase=motif[j*3:j*3+3]
   if section in ('lift','flight'):phrase=[phrase[0]+2,phrase[1]+1,phrase[2]+2]
   if section=='middle':phrase=[motif[(21-j*3)%24]-2,motif[(22-j*3)%24]-1,motif[(23-j*3)%24]-2]
   if section=='answer':phrase=[phrase[2],phrase[0],phrase[1]]
   if section=='return':phrase=[phrase[0],phrase[1]+2,phrase[2]]
   if section=='home':phrase=[phrase[0]-1,phrase[1],phrase[2]-2]
   # Authored rhythmic identities. These are sequential notes, with audible rests.
   if menu or index==0:rhythm=[(0,.86),(1,.64),(1.85,.96)]
   elif index==1:rhythm=[(0,.7),(.75,.35),(1.25,.65),(2,.7),(3,.78)]
   elif index==2:rhythm=[(0,.43),(.5,.42),(1,.40),(1.5,.9),(2.5,.36)]
   elif index==3:rhythm=[(0,.65),(.75,.37),(1.5,.38),(2,.8),(3,.7)]
   elif index==4:rhythm=[(0,.88),(1,.35),(1.5,.45),(2,.82)]
   elif index==5:rhythm=[(0,1.12),(1.25,.55),(2,.78),(3,.75)]
   elif index==6:rhythm=[(0,.86),(1,.38),(1.5,.84),(2.5,.36),(3,.78)]
   elif index==7:rhythm=[(0,.86),(1,.38),(1.5,.85),(2.5,.38)]
   elif index==8:rhythm=[(0,.66),(.75,.38),(1.5,.38),(2,1.12),(3.25,.55)]
   else:rhythm=[(0,.86),(1,.38),(1.5,.38),(2,.76)]
   if j in (3,7):rhythm=[(0,meter*.46),(meter*.55,meter*.39)]
   if quiet:rhythm=[(0,meter*.42),(meter*.57,meter*.35)]
   lead=solo
   if section in ('answer','middle') and not menu:lead='clarinet' if solo in ('flute','piano') else 'piano'
   leadroot=root-(12 if lead=='clarinet' and root>65 else 0)
   pitches=[nearest(degree(d)+(leadroot-root),pcs,53 if lead=='clarinet' else 60,81) for d in phrase]
   # No anchor is chosen outside the sounding chord, including the minor V.
   for n,(offset,dur) in enumerate(rhythm):
    pitch=pitches[n%3]
    if j==7 and n==len(rhythm)-1:pitch=nearest(root+12,{root%12},60,81)
    if n==len(rhythm)-1 and j==3:pitch=nearest(root+7,{(root+7)%12},60,81)
    strength=(.40 if bright else .31 if quiet else .36)*(1 if not menu else .92)
    if n%3==1:strength*=.88
    add(lead,pitch,start+offset,dur,strength,'melody',b,pcs)
   # A short contrasting reply occurs only in the lead's rests: no rival little
   # melody is superimposed on a held note, and no semitone rub is synthesized.
   if bright and j in (1,5) and rhythm[-1][0]+rhythm[-1][1]<meter-.15:
    at=rhythm[-1][0]+rhythm[-1][1]+.06
    add('harp' if lead!='harp' else 'piano',nearest(root+7,pcs,60,79),start+at,meter-at-.04,.18,'reply',b,pcs)
  bar+=count
 return dict(name=name,bpm=bpm,meter=meter,compound=index in (2,4,7) and not menu,root=root,minor=minor,
   context='menu' if menu or index==0 else 'gameplay',world=index-1,bars=bar,seconds=bar*meter*beat,sections=sections,harmony=harmony,notes=events)

def fundamental(x,sr,expected):
 """Concert-pitch calibration, median of stable windows, not filename guessing."""
 mono=x.mean(axis=1);hz=440*2**((expected-69)/12);values=[]
 for at in (.18,.32,.5,.7):
  y=mono[round(at*sr):round((at+.18)*sr)].astype(np.float64)
  if len(y)<128 or np.max(abs(y))<1e-5:continue
  y-=y.mean();a=signal.correlate(y,y,mode='full',method='fft')[len(y)-1:]
  lo=max(2,math.floor(sr/(hz*1.045)));hi=min(len(a)-2,math.ceil(sr/(hz*.955)))
  lag=lo+int(np.argmax(a[lo:hi+1]));den=2*(a[lag-1]-2*a[lag]+a[lag+1])
  frac=(a[lag-1]-a[lag+1])/den if abs(den)>1e-14 else 0
  values.append(69+12*math.log2(sr/(lag+frac)/440))
 return float(np.median(values)) if values else float(expected)

class Sampler:
 def __init__(self):
  self.banks={k:[] for k in ('piano','harp','cello','flute','clarinet','violin','pizz')};self.tuning=[]
  semis={'C':0,'D':2,'E':4,'F':5,'G':7,'A':9,'B':11}
  for p in sorted(SAMPLES.glob('*.wav')):
   name=p.name
   if name.startswith('Player'):inst='piano';pitch=21+2*int(p.stem.split('_')[-1])
   else:
    inst=next((v for k,v in [('KSHarp','harp'),('susvib','cello'),('LDFlute','flute'),('DCClar','clarinet'),('LLVln','violin'),('ViolaEns','pizz')] if name.startswith(k)),None)
    if inst is None:continue
    m=re.search(r'_([A-G])(#?)(\d)_',name);octave=int(m[3])+(1 if inst in ('harp','violin') else 2)
    pitch=12*octave+semis[m[1]]+bool(m[2])
   with warnings.catch_warnings():warnings.simplefilter('ignore');sr,x=wavfile.read(p)
   divisor=float(2**(8*x.dtype.itemsize-1));x=x.astype(np.float32)/divisor
   if x.ndim==1:x=np.stack([x,x],axis=1)
   measured=fundamental(x,sr,pitch)
   self.tuning.append(dict(file=name,nominal=pitch,measured=measured,correction_cents=round((pitch-measured)*100,3)))
   if sr!=SR:x=signal.resample_poly(x,SR,sr,axis=0).astype(np.float32)
   power=np.max(abs(x),axis=1);active=np.flatnonzero(power>max(.0002,float(power.max())*.015))
   if len(active):x=x[max(0,int(active[0])-132):]
   x=signal.sosfilt(signal.butter(2,38,fs=SR,btype='highpass',output='sos'),x,axis=0).astype(np.float32)
   # Normalise representative body, retaining a bounded natural attack.
   body=x[:min(len(x),round(SR*.65))];rms=float(np.sqrt(np.mean(body*body)));peak=float(np.max(abs(x)))
   x*=min(.18/max(.001,rms),.82/max(.001,peak))
   self.banks[inst].append((measured,x))
  assert all(self.banks.values()),'Missing an acoustic bank'
 @lru_cache(maxsize=180)
 def note(self,inst,pitch):
  source,x=min(self.banks[inst],key=lambda z:abs(z[0]-pitch))
  ratio=Fraction(2**((source-pitch)/12)).limit_denominator(2500)
  y=signal.resample_poly(x,ratio.numerator,ratio.denominator,axis=0).astype(np.float32)
  cutoff=7000 if inst in ('flute','piano','violin') else 5500
  return signal.sosfilt(signal.butter(2,cutoff,fs=SR,output='sos'),y,axis=0).astype(np.float32)
 def render(self,score):
  frames=round(score['seconds']*SR);tail=SR*3;mix=np.zeros((frames+tail,2),np.float32)
  for e in score['notes']:
   inst=e['instrument'];src=self.note(inst,e['pitch']);hold=e['duration']
   release=.085 if inst in ('flute','clarinet','violin','cello') else .18
   size=min(len(src),round((hold+release)*SR));x=src[:size].copy()
   attack=min(size,round(SR*(.06 if inst in ('violin','cello') else .018 if inst in ('flute','clarinet') else .003)))
   x[:attack]*=np.sin(np.linspace(0,math.pi/2,attack,dtype=np.float32))[:,None]**2
   # Bound release at the next harmonic bar; compatible notes may ring naturally.
   barend=round((e['bar']+1)*score['meter']*60/score['bpm']*SR)-round(e['time']*SR)
   size=min(size,max(attack,barend+round(.025*SR)));x=x[:size]
   fade=min(round(release*SR),size//2);x[-fade:]*=np.cos(np.linspace(0,math.pi/2,fade,dtype=np.float32))[:,None]**2
   # Gentle phrase shape in bowed/wind recordings, rather than organ-flat sustain.
   if inst in ('flute','clarinet','violin','cello'):
    u=np.linspace(0,1,size,dtype=np.float32);x*=(.83+.17*np.sin(math.pi*u))[:,None]
   pan={'piano':-.1,'harp':.27,'cello':-.2,'flute':-.06,'clarinet':.08,'violin':.18,'pizz':-.27}[inst]
   x*=np.array([math.sqrt(1-pan),math.sqrt(1+pan)],np.float32)*e['gain']
   at=round(e['time']*SR);length=min(size,len(mix)-at);mix[at:at+length]+=x[:length]
  # Short quiet chamber, preserving the pulse and tonal clarity. No long wash.
  rng=np.random.default_rng(111);n=round(.95*SR);ir=rng.normal(0,1,(n,2)).astype(np.float32)
  ir*=np.exp(-np.arange(n)[:,None]/(SR*.18));ir[:round(.018*SR)]=0
  ir=signal.sosfilt(signal.butter(2,4200,fs=SR,output='sos'),ir,axis=0).astype(np.float32)
  ir/=np.sqrt((ir*ir).sum(axis=0));wet=np.zeros_like(mix)
  for ch in range(2):wet[:,ch]=signal.fftconvolve(mix[:,ch],ir[:,ch])[:len(mix)]
  mix+=wet*.065;mix[:tail]+=mix[frames:frames+tail];mix=mix[:frames];mix-=mix.mean(axis=0)
  rms=float(np.sqrt(np.mean(mix.astype(np.float64)**2)));peak=float(np.max(abs(mix)));mix*=min(.12/rms,.82/peak)
  edge=128;delta=mix[-1]-mix[0];mix[:edge]+=delta[None,:]*np.linspace(1,0,edge)[:,None]
  return np.clip(mix,-.95,.95)

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output',type=Path,default=ROOT/'Reports/MusicOct17');p.add_argument('--theme');p.add_argument('--context',choices=['all','menu','gameplay'],default='all');p.add_argument('--install',action='store_true');a=p.parse_args()
 a.output.mkdir(parents=True,exist_ok=True);sampler=Sampler();report=[]
 for i,t in enumerate(THEMES):
  for menu in ([False] if i==0 else [False,True]):
   score=compose(t,i,menu)
   if a.theme and a.theme!=t[0]:continue
   if a.context!='all' and score['context']!=a.context:continue
   x=sampler.render(score);path=a.output/(score['name']+'.wav');wavfile.write(path,SR,(x*32767).astype(np.int16))
   (a.output/(score['name']+'.score.json')).write_text(json.dumps(score,indent=2)+'\n')
   record=dict(name=score['name'],seconds=len(x)/SR,bpm=score['bpm'],meter=score['meter'],context=score['context'],notes=len(score['notes']),peak=float(np.max(abs(x))),rms=float(np.sqrt(np.mean(x*x))),seam=float(np.max(abs(x[0]-x[-1]))),sha256=hashlib.sha256(path.read_bytes()).hexdigest())
   assert record['peak']<.95 and record['seam']<1e-5
   if a.install:
    dest=ASSETS/path.name;meta=dest.with_suffix('.wav.meta')
    if not meta.exists():
     assert menu,'Preserve existing gameplay/music GUIDs'
     template=(ASSETS/'Menu_HomeAmongCloudsBalanced.wav.meta').read_text();template=re.sub(r'guid: [a-f0-9]+','guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'cascade:score:'+score['name']).hex,template);meta.write_text(template)
    shutil.copyfile(path,dest)
   report.append(record);print(f"{score['name']}: {score['bpm']} BPM, {record['seconds']:.1f}s, peak {record['peak']:.3f}",flush=True)
 (a.output/'tuning.json').write_text(json.dumps(sampler.tuning,indent=2)+'\n');(a.output/'render-report.json').write_text(json.dumps(report,indent=2)+'\n')
if __name__=='__main__':main()
