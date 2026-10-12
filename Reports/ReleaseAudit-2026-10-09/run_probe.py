"""Independent audit probes; compile unchanged production pure C# sources."""
from pathlib import Path
import subprocess, tempfile, shutil, sys
ROOT=Path(__file__).resolve().parents[2]
HERE=Path(__file__).resolve().parent
UNITY=Path('/Applications/Unity/Hub/Editor/6000.3.8f1/Unity.app/Contents/Resources/Scripting')
with tempfile.TemporaryDirectory(prefix='cascade-release-audit-') as directory:
    work=Path(directory); lib=UNITY/'MonoBleedingEdge/lib/mono/4.5'
    json=next((ROOT/'CascadeUnity/Library/PackageCache').glob('com.unity.nuget.newtonsoft-json@*/Runtime/Newtonsoft.Json.dll'))
    refs=[lib/name for name in ('mscorlib.dll','System.dll','System.Core.dll','Facades/netstandard.dll')]+[json]
    sources=[p for name in ('Domain','Solver','Application','Content','Persistence') for p in sorted((ROOT/'CascadeUnity/Assets/_Cascade'/name).glob('*.cs'))]
    subprocess.run([str(UNITY/'NetCoreRuntime/dotnet'),str(UNITY/'DotNetSdkRoslyn/csc.dll'),'-nologo','-nostdlib','-langversion:9',f'-out:{work/"Probe.exe"}']+[f'-r:{p}' for p in refs]+[str(p) for p in sources]+[str(HERE/'probe.cs')],check=True)
    shutil.copy2(json,work/'Newtonsoft.Json.dll')
    subprocess.run([str(UNITY/'MonoBleedingEdge/bin/mono'),str(work/'Probe.exe'),str(ROOT),str(HERE)]+sys.argv[1:],check=True)
