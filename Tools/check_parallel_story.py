#!/usr/bin/env python3
"""Run parallel-story memory, independent journey and localized story tests; no player build."""
from pathlib import Path
import json
import shutil
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[1]
RUNTIME = Path('/Applications/Unity/Hub/Editor/6000.3.8f1/Unity.app/Contents/Resources/Scripting')
OUTPUT = ROOT / 'Reports/ParallelStory-Oct09'
OUTPUT.mkdir(parents=True, exist_ok=True)
with tempfile.TemporaryDirectory(prefix='cascade-parcel-tests-') as directory:
    work = Path(directory)
    cache = ROOT / 'CascadeUnity/Library/PackageCache'
    libraries = [next(cache.glob(pattern)) for pattern in (
        'com.unity.nuget.newtonsoft-json@*/Runtime/Newtonsoft.Json.dll',
        'com.unity.ext.nunit@*/net40/unity-custom/nunit.framework.dll')]
    references = [RUNTIME / 'MonoBleedingEdge/lib/mono/4.5' / name
                  for name in ('mscorlib.dll', 'System.dll', 'System.Core.dll', 'Facades/netstandard.dll')] + libraries
    assets = ROOT / 'CascadeUnity/Assets/_Cascade'
    sources = [file for folder in ('Domain', 'Solver', 'Application', 'Content', 'Persistence')
               for file in sorted((assets / folder).glob('*.cs'))]
    fixture = (assets / 'Tests/EditMode/ReferenceFixtures.cs').read_text()
    (work / 'ReferenceFixtures.cs').write_text(fixture.replace('using UnityEngine;', '').replace(
        'Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName', json.dumps(str(ROOT))))
    sources += [work / 'ReferenceFixtures.cs', ROOT / 'Reports/ReleaseAudit-2026-10-09/pure_runner.cs']
    sources += [assets / 'Tests/EditMode' / (name + '.cs') for name in (
        'ParallelStoryTests', 'ModeJourneyTests', 'WorldPrologueTests', 'StoryConversationTests', 'LevelDefinitionTests')]
    content = (assets / 'Tests/EditMode/ContentTests.cs').read_text()
    helper = "using System.IO;using Cascade.Content;namespace Cascade.Tests {internal static class ContentTests {\n"
    helper += "\n".join(line for line in content.splitlines() if 'static string Read(' in line or 'static ContentCatalog LoadCatalog(' in line)
    (work / 'ContentHelper.cs').write_text(helper + "\n}}")
    sources.append(work / 'ContentHelper.cs')
    subprocess.run([str(RUNTIME / 'NetCoreRuntime/dotnet'), str(RUNTIME / 'DotNetSdkRoslyn/csc.dll'),
                    '-nologo', '-nostdlib', '-langversion:9', f'-out:{work / "Tests.exe"}']
                   + [f'-r:{file}' for file in references] + [str(file) for file in sources], check=True)
    for library in libraries:
        shutil.copy2(library, work / library.name)
    with (OUTPUT / 'output.txt').open('w') as log:
        result = subprocess.run([str(RUNTIME / 'MonoBleedingEdge/bin/mono'), str(work / 'Tests.exe'), str(OUTPUT)],
                                stdout=log, stderr=subprocess.STDOUT, timeout=160)
    print((OUTPUT / 'output.txt').read_text())
    print((OUTPUT / 'pure-suite-summary.json').read_text())
    raise SystemExit(result.returncode)
