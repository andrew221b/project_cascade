#!/usr/bin/env python3
"""Run real half-star rating, persistence, reward and merge tests; no player build."""
from pathlib import Path
import json
import shutil
import subprocess
import tempfile
import sys

ROOT = Path(__file__).resolve().parents[1]
RUNTIME = Path('/Applications/Unity/Hub/Editor/6000.3.8f1/Unity.app/Contents/Resources/Scripting')
TARGETED='--targeted' in sys.argv
OUTPUT = ROOT / ('Reports/HalfStar-Oct12/targeted' if TARGETED else 'Reports/HalfStar-Oct12')
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
    runner=(ROOT / 'Reports/ReleaseAudit-2026-10-09/pure_runner.cs').read_text().replace('t.Name==args[1]',"args[1].Split(',').Contains(t.Name)")
    (work / 'Runner.cs').write_text(runner)
    sources += [work / 'ReferenceFixtures.cs', work / 'Runner.cs']
    sources += [assets / 'Tests/EditMode' / (name + '.cs') for name in (
        'HalfStarResultsTests', 'FiveStarResultsTests', 'CompletionProgressTests', 'RewardTests', 'CloudReconcilerTests', 'ModeJourneyTests', 'LevelDefinitionTests')]
    content = (assets / 'Tests/EditMode/ContentTests.cs').read_text()
    helper = "using System.IO;using Cascade.Content;namespace Cascade.Tests {internal static class ContentTests {\n"
    helper += "\n".join(line for line in content.splitlines() if 'static string Read(' in line or 'static ContentCatalog LoadCatalog(' in line)
    (work / 'ContentHelper.cs').write_text(helper + "\n}}")
    sources.append(work / 'ContentHelper.cs')
    # Compile the unchanged account replay-verifier source too, with only its JSON
    # utility supplied locally; this does not simulate a Cloud Code transport/deploy.
    verifier = (ROOT / 'CascadeUnity/CascadeAccountCCM/CascadeAccount/PublishedCatalog.cs').read_text()
    (work / 'PublishedCatalog.cs').write_text('using System; using System.IO; using System.Linq; using System.Collections.Generic;\n' + verifier.replace('namespace CascadeAccountModule;', 'namespace CascadeAccountModule {') + '\n}')
    (work / 'AccountJson.cs').write_text('using Newtonsoft.Json; namespace CascadeAccountModule { public static class AccountJson { public static T Decode<T>(string value)=>JsonConvert.DeserializeObject<T>(value); public static string Encode(object value)=>JsonConvert.SerializeObject(value); } }')
    bridge = ROOT / 'Tools/half_star_server_checks.cs'
    sources += [work / 'PublishedCatalog.cs', work / 'AccountJson.cs', bridge]
    resources = [f'-resource:{ROOT / "Data" / filename},CascadeAccount.Published.{name}.json' for name, filename in (
        ('recipes', 'production_triad_recipes.json'), ('recipes_legacy', 'production_triad_recipes_legacy_oct09.json'),
        ('levels', 'levels.json'), ('localized_levels', 'localized_levels.json'), ('words_en', 'words_en.json'),
        ('words_ru', 'words_ru.json'), ('words_uk', 'words_uk.json'), ('campaign_extension', 'campaign_extension.json'))]
    subprocess.run([str(RUNTIME / 'NetCoreRuntime/dotnet'), str(RUNTIME / 'DotNetSdkRoslyn/csc.dll'),
                    '-nologo', '-nostdlib', '-langversion:9', f'-out:{work / "Tests.exe"}']
                   + resources + [f'-r:{file}' for file in references] + [str(file) for file in sources], check=True)
    for library in libraries:
        shutil.copy2(library, work / library.name)
    with (OUTPUT / 'output.txt').open('w') as log:
        result = subprocess.run([str(RUNTIME / 'MonoBleedingEdge/bin/mono'), str(work / 'Tests.exe'), str(OUTPUT)] + (['HalfStarResultsTests,HalfStarServerChecks'] if TARGETED else []),
                                stdout=log, stderr=subprocess.STDOUT, timeout=160)
    print((OUTPUT / 'output.txt').read_text())
    print((OUTPUT / 'pure-suite-summary.json').read_text())
    raise SystemExit(result.returncode)
