#!/usr/bin/env python3
"""Compile the unchanged pure C# game sources and replay every campaign triad."""
from pathlib import Path
import subprocess
import tempfile
import shutil

ROOT = Path(__file__).resolve().parents[1]
UNITY = Path('/Applications/Unity/Hub/Editor/6000.3.8f1/Unity.app/Contents/Resources/Scripting')
with tempfile.TemporaryDirectory(prefix='cascade-triad-check-') as directory:
    work = Path(directory)
    libraries = UNITY / 'MonoBleedingEdge/lib/mono/4.5'
    json_library = next((ROOT / 'CascadeUnity/Library/PackageCache').glob('com.unity.nuget.newtonsoft-json@*/Runtime/Newtonsoft.Json.dll'))
    references = [libraries / name for name in ('mscorlib.dll', 'System.dll', 'System.Core.dll', 'Facades/netstandard.dll')] + [json_library]
    sources = [file for folder in ('Domain', 'Solver', 'Application', 'Content') for file in sorted((ROOT / 'CascadeUnity/Assets/_Cascade' / folder).glob('*.cs'))]
    command = [str(UNITY / 'NetCoreRuntime/dotnet'), str(UNITY / 'DotNetSdkRoslyn/csc.dll'), '-nologo', '-langversion:9', '-nostdlib', f'-out:{work / "Audit.exe"}']
    command += [f'-r:{file}' for file in references] + [str(file) for file in sources] + [str(ROOT / 'Tools/check_triad_campaign.cs')]
    subprocess.run(command, check=True)
    shutil.copy2(json_library, work / 'Newtonsoft.Json.dll')
    subprocess.run([str(UNITY / 'MonoBleedingEdge/bin/mono'), str(work / 'Audit.exe'), str(ROOT)], check=True)
