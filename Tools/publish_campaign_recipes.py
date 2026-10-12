#!/usr/bin/env python3
"""Bake campaign recipes with the production resolver, or check the current bake."""
from pathlib import Path
import os
import shutil
import subprocess
import sys
import tempfile

ROOT = Path(__file__).resolve().parents[1]
EDITOR = Path(os.environ.get('CASCADE_UNITY_EDITOR', '/Applications/Unity/Hub/Editor/6000.3.8f1/Unity.app/Contents'))
RUNTIME = EDITOR / 'Resources/Scripting'
if any(arg not in ('--check', '--refresh-opening', '--deduplicate', '--refresh-pacing') for arg in sys.argv[1:]):
    raise SystemExit('Usage: publish_campaign_recipes.py [--check | --refresh-opening | --deduplicate | --refresh-pacing]')
with tempfile.TemporaryDirectory(prefix='cascade-publish-recipes-') as directory:
    work = Path(directory)
    libraries = RUNTIME / 'MonoBleedingEdge/lib/mono/4.5'
    json_library = next((ROOT / 'CascadeUnity/Library/PackageCache').glob('com.unity.nuget.newtonsoft-json@*/Runtime/Newtonsoft.Json.dll'))
    references = [libraries / name for name in ('mscorlib.dll', 'System.dll', 'System.Core.dll', 'Facades/netstandard.dll')] + [json_library]
    sources = [file for folder in ('Domain', 'Solver', 'Application', 'Content', 'Persistence')
               for file in sorted((ROOT / 'CascadeUnity/Assets/_Cascade' / folder).glob('*.cs'))]
    command = [str(RUNTIME / 'NetCoreRuntime/dotnet'), str(RUNTIME / 'DotNetSdkRoslyn/csc.dll'), '-nologo', '-langversion:9', '-nostdlib', f'-out:{work / "Publish.exe"}']
    command += [f'-r:{file}' for file in references] + [str(file) for file in sources] + [str(ROOT / 'Tools/publish_campaign_recipes.cs')]
    subprocess.run(command, check=True)
    shutil.copy2(json_library, work / 'Newtonsoft.Json.dll')
    environment = dict(os.environ, DYLD_LIBRARY_PATH=str(EDITOR / 'Frameworks'))
    subprocess.run([str(RUNTIME / 'MonoBleedingEdge/bin/mono'), str(work / 'Publish.exe'), str(ROOT)] + sys.argv[1:], check=True, env=environment)
