from pathlib import Path
from collections import Counter,defaultdict
import json,re,wave,hashlib
from PIL import Image
ROOT=Path(__file__).resolve().parents[2]; OUT=Path(__file__).resolve().parent; ASSETS=ROOT/'CascadeUnity/Assets'
def dump(name,value):(OUT/name).write_text(json.dumps(value,ensure_ascii=False,indent=2))
guids=defaultdict(list);missing=[]
for p in ASSETS.rglob('*.meta'):
 match=re.search(r'^guid: (\w+)',p.read_text(errors='replace'),re.M)
 if match:guids[match[1]].append(str(p.relative_to(ASSETS)))
for p in ASSETS.rglob('*'):
 if p.is_file() and p.suffix!='.meta' and not p.name.startswith('.') and not Path(str(p)+'.meta').exists():missing.append(str(p.relative_to(ASSETS)))
dump('asset-integrity.json',{'uniqueGuids':len(guids),'duplicateGuids':{k:v for k,v in guids.items() if len(v)>1},'filesWithoutMeta':missing})
resources=[p for p in ASSETS.rglob('*') if p.is_file() and p.suffix!='.meta' and 'Resources' in p.parts]
textures=[];audio=[]
for p in resources:
 meta=Path(str(p)+'.meta').read_text(errors='replace') if Path(str(p)+'.meta').exists() else ''
 if p.suffix.lower() in ('.png','.jpg','.jpeg'):
  with Image.open(p) as im:w,h=im.size
  textures.append({'file':str(p.relative_to(ASSETS)),'sourceBytes':p.stat().st_size,'width':w,'height':h,'rgbaUncompressedBytes':w*h*4,'maxTextureSize':re.findall(r'maxTextureSize: (\d+)',meta),'readable':re.findall(r'isReadable: (\d+)',meta),'compression':re.findall(r'textureCompression: (\d+)',meta)})
 if p.suffix.lower()=='.wav':
  with wave.open(str(p)) as f:duration=f.getnframes()/f.getframerate();pcm=f.getnframes()*f.getsampwidth()*f.getnchannels()
  audio.append({'file':str(p.relative_to(ASSETS)),'durationSeconds':duration,'sourceBytes':p.stat().st_size,'pcmBytes':pcm,'loadType':re.findall(r'loadType: (\d+)',meta),'preload':re.findall(r'preloadAudioData: (\d+)',meta),'background':re.findall(r'loadInBackground: (\d+)',meta),'compression':re.findall(r'compressionFormat: (\d+)',meta)})
dump('resources.json',{'files':len(resources),'sourceBytes':sum(p.stat().st_size for p in resources),'textures':textures,'audio':audio,'note':'Source file size / uncompressed theoretical texture bounds, not actual player memory or build size.'})
manifest=json.loads((ROOT/'CascadeUnity/Packages/manifest.json').read_text());lock=json.loads((ROOT/'CascadeUnity/Packages/packages-lock.json').read_text());dump('packages.json',{'requested':manifest['dependencies'],'locked':lock['dependencies']})
keys=defaultdict(list)
for p in resources:
 i=p.parts.index('Resources');key='/'.join(p.parts[i+1:]);key=str(Path(key).with_suffix(''));keys[key].append(str(p.relative_to(ASSETS)))
dump('resource-key-collisions.json',{k:v for k,v in keys.items() if len(v)>1})
dump('first-party-sizes.json',{name:{'files':len(list((ASSETS/'_Cascade'/name).rglob('*.cs'))),'lines':sum(len(p.read_text().splitlines()) for p in (ASSETS/'_Cascade'/name).rglob('*.cs'))} for name in ('Domain','Solver','Application','Content','Persistence','Presentation','Editor','Tests')})
print('Resources:',len(resources),'source MiB',round(sum(p.stat().st_size for p in resources)/1024**2,1),'textures',len(textures),'audio',len(audio),'duplicate GUIDs',sum(len(v)>1 for v in guids.values()),'missing metas',len(missing))
