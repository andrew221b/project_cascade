"""Run unchanged pure NUnit methods; no Unity API or gameplay substitute."""
from pathlib import Path
import subprocess,tempfile,shutil,json,re
ROOT=Path(__file__).resolve().parents[2]; HERE=Path(__file__).resolve().parent
UNITY=Path('/Applications/Unity/Hub/Editor/6000.3.8f1/Unity.app/Contents/Resources/Scripting')
with tempfile.TemporaryDirectory(prefix='cascade-pure-suite-') as directory:
 work=Path(directory);lib=UNITY/'MonoBleedingEdge/lib/mono/4.5';cache=ROOT/'CascadeUnity/Library/PackageCache'
 libraries=[next(cache.glob(pattern)) for pattern in ('com.unity.nuget.newtonsoft-json@*/Runtime/Newtonsoft.Json.dll','com.unity.ext.nunit@*/net40/unity-custom/nunit.framework.dll')]
 refs=[lib/name for name in ('mscorlib.dll','System.dll','System.Core.dll','Facades/netstandard.dll')]+libraries
 sources=[p for name in ('Domain','Solver','Application','Content','Persistence') for p in sorted((ROOT/'CascadeUnity/Assets/_Cascade'/name).glob('*.cs'))]
 tests=ROOT/'CascadeUnity/Assets/_Cascade/Tests/EditMode';excluded=[]
 for p in tests.glob('*.cs'):
  if p.name=='ReferenceFixtures.cs':
   s=p.read_text().replace('using UnityEngine;','').replace('Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName',json.dumps(str(ROOT)));(work/p.name).write_text(s);sources.append(work/p.name)
  elif 'UnityEngine' in p.read_text() or 'UnityEditor' in p.read_text() or 'Cascade.Presentation' in p.read_text():excluded.append(p.name)
  else:sources.append(p)
 # Shared content helper only, using the production importer. Unity-dependent ContentTests are excluded.
 (work/'ContentHelper.cs').write_text('using System.IO;using Cascade.Content;namespace Cascade.Tests{internal static class ContentTests{public static ContentCatalog LoadCatalog(){string R(string n)=>File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",n));return ContentImporter.Import(R("levels.json"),R("localized_levels.json"),R("words_en.json"),R("words_ru.json"),R("words_uk.json"));}}}')
 sources.extend([work/'ContentHelper.cs',HERE/'pure_runner.cs'])
 subprocess.run([str(UNITY/'NetCoreRuntime/dotnet'),str(UNITY/'DotNetSdkRoslyn/csc.dll'),'-nologo','-nostdlib','-langversion:9',f'-out:{work/"Suite.exe"}']+[f'-r:{p}' for p in refs]+[str(p) for p in sources],check=True)
 for p in libraries:shutil.copy2(p,work/p.name)
 (HERE/'pure-suite-excluded.json').write_text(json.dumps(excluded,indent=2))
 mono=str(UNITY/'MonoBleedingEdge/bin/mono'); exe=str(work/'Suite.exe')
 names=subprocess.check_output([mono,exe,str(HERE),'--list'],text=True).splitlines();complete=[]
 for name in names:
  output=HERE/'pure-fixtures'/name;output.mkdir(parents=True,exist_ok=True)
  with (output/'output.txt').open('w') as log:
   try:r=subprocess.run([mono,exe,str(output),name],stdout=log,stderr=subprocess.STDOUT,timeout=90);status='finished';code=r.returncode
   except subprocess.TimeoutExpired:status='timeout';code=None
  complete.append({'fixture':name,'status':status,'exit':code});print(name,status,code,flush=True)
  (HERE/'pure-fixture-index.json').write_text(json.dumps(complete,indent=2))
 results=[];passed=failed=skipped=0
 for x in complete:
  path=HERE/'pure-fixtures'/x['fixture']/'pure-suite.json'
  if path.exists():results+=json.loads(path.read_text())
 for x in results:
  passed+=x['status']=='Passed';failed+=x['status']=='Failed';skipped+=x['status']=='Skipped'
 (HERE/'pure-suite.json').write_text(json.dumps(results,indent=2));(HERE/'pure-suite-summary.json').write_text(json.dumps({'pass':passed,'fail':failed,'skip':skipped,'timeouts':[x['fixture'] for x in complete if x['status']=='timeout'],'fixtures':len(complete),'method':'Actual pure production C# and NUnit methods via isolated fixture processes,90-second fixture watchdog; native Unity tests excluded.'},indent=2))
