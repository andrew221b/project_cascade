#!/usr/bin/env python3
"""Compile actual engine sources; certify alternatives or audit personalised stages."""
from pathlib import Path
import os,subprocess,tempfile,shutil,sys
ROOT=Path(__file__).resolve().parents[1]
EDITOR=Path(os.environ.get('CASCADE_UNITY_EDITOR','/Applications/Unity/Hub/Editor/6000.3.8f1/Unity.app/Contents'))
RUNTIME=EDITOR/'Resources/Scripting'
with tempfile.TemporaryDirectory(prefix='cascade-stage-variety-') as directory:
 work=Path(directory);lib=RUNTIME/'MonoBleedingEdge/lib/mono/4.5'
 json_library=next((ROOT/'CascadeUnity/Library/PackageCache').glob('com.unity.nuget.newtonsoft-json@*/Runtime/Newtonsoft.Json.dll'))
 refs=[lib/name for name in ('mscorlib.dll','System.dll','System.Core.dll','Facades/netstandard.dll')]+[json_library]
 sources=[p for folder in ('Domain','Solver','Application','Content','Persistence') for p in sorted((ROOT/'CascadeUnity/Assets/_Cascade'/folder).glob('*.cs'))]
 cmd=[str(RUNTIME/'NetCoreRuntime/dotnet'),str(RUNTIME/'DotNetSdkRoslyn/csc.dll'),'-nologo','-langversion:9','-nostdlib',f'-out:{work / "Audit.exe"}']
 subprocess.run(cmd+[f'-r:{p}' for p in refs]+list(map(str,sources))+[str(ROOT/'Tools/check_stage_variety.cs')],check=True)
 shutil.copy2(json_library,work/'Newtonsoft.Json.dll')
 subprocess.run([str(RUNTIME/'MonoBleedingEdge/bin/mono'),str(work/'Audit.exe'),str(ROOT)]+sys.argv[1:],check=True,env=dict(os.environ,DYLD_LIBRARY_PATH=str(EDITOR/'Frameworks')))
