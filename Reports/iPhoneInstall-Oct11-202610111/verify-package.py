from pathlib import Path
import hashlib,json,plistlib,subprocess,sys
r=Path(__file__).resolve().parent
assert (r/'xcode-exit.txt').read_text().strip()=='0','Xcode must succeed first'
export=Path((r/'export-path.txt').read_text().strip())
app=r/'DerivedData/Build/Products/Release-iphoneos/CASCADE.app'
assert app.is_dir(),'Missing final signed app'
info=plistlib.loads((app/'Info.plist').read_bytes())
assert info['CFBundleIdentifier']=='com.vickand.cascade','Unexpected app identity'
assert str(info['CFBundleVersion'])=='202610111','Unexpected build number'
source=export/'Data';target=app/'Data'
files=[p for p in source.rglob('*') if p.is_file() and p.name!='.DS_Store']
assert files,'Missing exported Data'
missing=[];different=[]
for p in files:
 q=target/p.relative_to(source)
 if not q.is_file(): missing.append(str(p.relative_to(source)))
 elif hashlib.sha256(p.read_bytes()).digest()!=hashlib.sha256(q.read_bytes()).digest(): different.append(str(p.relative_to(source)))
assert not missing and not different, f'Incomplete game Data: missing={missing}, different={different}'
for relative in ['boot.config','Managed/Metadata/global-metadata.dat']:
 assert (target/relative).is_file(),f'Missing critical resource {relative}'
with (r/'signature-verification.txt').open('w') as out:
 signature=subprocess.run(['codesign','--verify','--deep','--strict',str(app)],stdout=out,stderr=subprocess.STDOUT)
assert signature.returncode==0,'Code signature verification failed'
settings=Path('/Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2/CascadeUnity/ProjectSettings/ProjectSettings.asset')
assert settings.read_bytes()==(r/'ProjectSettings.before.asset.txt').read_bytes(),'Project settings changed'
hashes=json.loads((r/'source-hashes.json').read_text())
root=Path('/Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2')
changed=[p for p,h in hashes.items() if hashlib.sha256((root/p).read_bytes()).hexdigest()!=h]
assert not changed,'Source changed during build: '+str(changed)
metadata={'app':str(app),'bundleIdentifier':info['CFBundleIdentifier'],'version':info['CFBundleShortVersionString'],'buildNumber':info['CFBundleVersion'],'exportDataFilesVerified':len(files),'signatureVerified':True,'projectSettingsPreserved':True,'sourceUnchanged':True}
(r/'build-metadata.json').write_text(json.dumps(metadata,indent=2))
print('Verified build',info['CFBundleVersion'],'with',len(files),'game Data files and a valid signature.')
