#!/usr/bin/env python3
"""Validate upload dimensions/color types and native-source integrity without modifying images."""
import hashlib,json,pathlib,struct
root=pathlib.Path(__file__).resolve().parents[1]/'Reports/StoreAssets-Oct09'
checks=[]
for folder,w,h in [('Apple/iPhone-Large',1320,2868),('Apple/iPhone-Medium',1206,2622),('Apple/iPad-13',2064,2752),('Google/Screenshots',1080,1920)]:
    files=sorted((root/folder).glob('*.png'));assert len(files)==5,(folder,len(files))
    for p in files:
        b=p.read_bytes();assert struct.unpack('>IIBB',b[16:26])==(w,h,8,2),p
        checks.append({'file':str(p.relative_to(root)),'width':w,'height':h,'opaque':True,'sha256':hashlib.sha256(b).hexdigest()})
for file,w,h,kind in [('Apple/icon-1024.png',1024,1024,2),('Google/icon-512.png',512,512,6),('Google/feature-1024x500.png',1024,500,2),('Apple/header-3840x1646.png',3840,1646,2),('Apple/search-1920x1280.png',1920,1280,2)]:
    b=(root/file).read_bytes();assert struct.unpack('>IIBB',b[16:26])==(w,h,8,kind),file
    if file=='Google/icon-512.png':assert len(b)<=1024*1024
    checks.append({'file':file,'width':w,'height':h,'pngColorType':kind,'bytes':len(b)})
for folder in ['Raw/iPhone','Raw/iPad']:
    sources=json.load(open(root/folder/'sources.json'));assert len(sources)==6
    for s in sources:assert hashlib.sha256((root/folder/s['file']).read_bytes()).hexdigest()==s['sha256']
(root/'asset-validation.json').write_text(json.dumps({'passed':True,'checks':checks,'nativeSourceImages':12},indent=2)+'\n')
print('PASS: 20 store screenshots, 5 fixed-size assets, 12 native source hashes.')
