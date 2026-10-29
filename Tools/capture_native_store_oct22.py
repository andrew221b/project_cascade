#!/usr/bin/env python3
"""Native framebuffer screenshots. No layout rendering, resizing or artwork is performed here."""
import argparse,hashlib,json,pathlib,subprocess,time,uuid
from datetime import datetime,timezone
from PIL import Image
p=argparse.ArgumentParser();p.add_argument('--platform',choices=['ios','android'],required=True);p.add_argument('--device',required=True);p.add_argument('--output',required=True);p.add_argument('--scenes',default='shapes-wind,words-rain,mix-lagoon,shapes-snow,trophies,map');p.add_argument('--locales',default='en,ru,uk');a=p.parse_args()
bundle='com.vickand.flyingladders.capture';dest=pathlib.Path(a.output);dest.mkdir(parents=True,exist_ok=True)
def sim(*args):return subprocess.check_output(['xcrun','simctl',*args],text=True,stderr=subprocess.PIPE).strip()
adb='/Users/andrew/Library/Android/sdk/platform-tools/adb'
def android(*args,binary=False):return subprocess.check_output([adb,'-s',a.device,*args],text=not binary,stderr=subprocess.PIPE)
if a.platform=='ios':
 devices=json.loads(sim('list','devices','-j'))['devices'];device=next(d for group in devices.values() for d in group if d['udid']==a.device)
 assert device['name'].startswith('FlyingLaddersStore'), 'Dedicated project capture simulator required.'
 data=pathlib.Path(sim('get_app_container',a.device,bundle,'data'))/'Documents'
 def request(value):
  tmp=data/'store-capture-command.tmp';tmp.write_text(json.dumps(value));tmp.replace(data/'store-capture-command.txt')
 def status():
  p=data/'store-capture-status.txt';return p.read_text() if p.exists() else ''
 def shot(path):
  tmp=pathlib.Path('/tmp/flying-frame-'+a.device+'.png');tmp.unlink(missing_ok=True);sim('io',a.device,'screenshot','--type=png','--mask=ignored',str(tmp));path.write_bytes(tmp.read_bytes());tmp.unlink()
 device_name=device['name'];method='xcrun simctl io screenshot; native unmasked framebuffer'
else:
 device_name=android('emu','avd','name').splitlines()[0].strip();assert device_name.startswith('FlyingLaddersStore'),'Dedicated project AVD required'
 data='/sdcard/Android/data/'+bundle+'/files'
 def request(value):
  local=pathlib.Path('/tmp/flying-capture-request-'+a.device+'.json');local.write_text(json.dumps(value));android('push',str(local),data+'/store-capture-command.tmp');android('shell','mv',data+'/store-capture-command.tmp',data+'/store-capture-command.txt')
 def status():
  try:return android('shell','cat',data+'/store-capture-status.txt').strip()
  except subprocess.CalledProcessError:return ''
 def shot(path):path.write_bytes(android('exec-out','screencap','-p',binary=True))
 method='adb exec-out screencap -p; native Android framebuffer'
entries=[]
for locale in a.locales.split(','):
 folder=dest/locale.upper();folder.mkdir(exist_ok=True)
 proof=dest.parent.parent/'Provenance'/dest.name/locale.upper();proof.mkdir(parents=True,exist_ok=True)
 for index,scene in enumerate(a.scenes.split(','),1):
  old_status=status();ident=uuid.uuid4().hex;request({'id':ident,'locale':locale,'scene':scene});deadline=time.monotonic()+240
  while time.monotonic()<deadline:
   state=status()
   if state.startswith(ident+' ready;'):break
   if state.startswith('error:') and state!=old_status:raise RuntimeError(state)
   time.sleep(.5)
  else:raise TimeoutError(scene+' not ready: '+state)
  display_scene='shapes-beacon' if scene=='shapes-snow' else scene;name=f'{index:02d}-{display_scene}.png';original=proof/name;shot(original)
  with Image.open(original) as im:
   im.load()
   if 'A' in im.getbands():assert im.getchannel('A').getextrema()==(255,255),'Native framebuffer unexpectedly transparent'
   rgb=im.convert('RGB');final=folder/name;rgb.save(final,icc_profile=im.info.get('icc_profile'))
   with Image.open(final) as check:assert check.mode=='RGB' and check.tobytes()==rgb.tobytes()
   size=list(rgb.size)
  entries.append({'file':str(final.relative_to(dest)),'device':device_name,'deviceId':a.device,'size':size,'method':method,'nativeStatus':state,'capturedUtc':datetime.now(timezone.utc).isoformat(),'sha256':hashlib.sha256(final.read_bytes()).hexdigest(),'formatOnly':'Opaque RGB PNG; native RGB pixels unchanged, alpha channel removed; no crop/resize/overlay'})
  print(f'{device_name} {locale} {scene} {size}',flush=True)
(dest/'sources.json').write_text(json.dumps(entries,ensure_ascii=False,indent=2)+'\n')
print('Captured',len(entries),'native screens.',flush=True)
