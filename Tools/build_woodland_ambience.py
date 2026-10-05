"""Original deterministic quiet wind/rain beds. No sourced recording or model audio."""
from pathlib import Path
import math, random, struct, wave

root=Path(__file__).resolve().parents[1]/'CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Woodland'
rate=24000; duration=8; count=rate*duration
for name,seed in [('WindBed',240510),('RainBed',240511)]:
    rng=random.Random(seed);low=slow=0.;samples=[]
    for i in range(count):
        t=i/rate;noise=rng.uniform(-1,1);low+=(noise-low)*.045;slow+=(low-slow)*.006
        envelope=min(1,t/.25,(duration-t)/.25)
        gust=.7+.3*math.sin(2*math.pi*t/duration)
        value=(slow*1.8*gust if name=='WindBed' else (low*.28+noise*.025))*envelope
        samples.append(struct.pack('<h',round(max(-1,min(1,value))*32767)))
    with wave.open(str(root/(name+'.wav')),'wb') as out:
        out.setnchannels(1);out.setsampwidth(2);out.setframerate(rate);out.writeframes(b''.join(samples))
