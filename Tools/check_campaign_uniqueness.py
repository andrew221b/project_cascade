#!/usr/bin/env python3
"""Check every playable category using bound production C# levels, without editing content."""
from pathlib import Path
import argparse
import os
import shutil
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output', type=Path, default=ROOT / 'Reports/CampaignUniquenessOct15')
parser.add_argument('--previous', type=Path, help='Directory containing the previous production_triad_recipes.json for save compatibility checks')
options = parser.parse_args()
editor = Path(os.environ.get('CASCADE_UNITY_EDITOR', '/Applications/Unity/Hub/Editor/6000.3.8f1/Unity.app/Contents'))
runtime = editor / 'Resources/Scripting'

with tempfile.TemporaryDirectory(prefix='cascade-uniqueness-') as directory:
    work = Path(directory)
    libraries = runtime / 'MonoBleedingEdge/lib/mono/4.5'
    json_library = next((ROOT / 'CascadeUnity/Library/PackageCache').glob('com.unity.nuget.newtonsoft-json@*/Runtime/Newtonsoft.Json.dll'))
    references = [libraries / name for name in ('mscorlib.dll', 'System.dll', 'System.Core.dll', 'Facades/netstandard.dll')] + [json_library]
    sources = [file for folder in ('Domain', 'Solver', 'Application', 'Content', 'Persistence')
               for file in sorted((ROOT / 'CascadeUnity/Assets/_Cascade' / folder).glob('*.cs'))]
    sources += [ROOT / 'Tools/campaign_uniqueness.cs', ROOT / 'Tools/check_campaign_uniqueness.cs']
    command = [str(runtime / 'NetCoreRuntime/dotnet'), str(runtime / 'DotNetSdkRoslyn/csc.dll'),
               '-nologo', '-langversion:9', '-nostdlib', f'-out:{work / "Check.exe"}']
    subprocess.run(command + [f'-r:{file}' for file in references] + [str(file) for file in sources], check=True)
    shutil.copy2(json_library, work / 'Newtonsoft.Json.dll')
    environment = dict(os.environ, DYLD_LIBRARY_PATH=str(editor / 'Frameworks'))
    arguments = [str(ROOT), str(options.output.resolve())]
    if options.previous:
        arguments.append(str(options.previous.resolve()))
    subprocess.run([str(runtime / 'MonoBleedingEdge/bin/mono'), str(work / 'Check.exe')] + arguments,
                   check=True, env=environment)
