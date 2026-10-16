"""Run existing pure C#/NUnit economic regressions without a second Unity Editor."""
from pathlib import Path
import json
import os
import shutil
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[1]
UNITY = Path('/Volumes/MY DISK/unity_installs/6000.3.8f1/Unity.app/Contents/Resources/Scripting')
REPORT = ROOT / 'Reports/ResumeEconomyOct15'
FIXTURES = [
    'AdServiceTests', 'StageHelpEconomyTests', 'UnlimitedHelpTests', 'ShopTests',
    'PreviewMonetizationTests', 'HalfStarResultsTests', 'CoinEconomyTests',
    'TrophyEconomyTests', 'ParcelLootTests', 'RewardTests',
    'CloudJournalTests', 'CloudReconcilerTests', 'CloudReplyReconcilerTests',
    'ReleasePersistenceTests',
]

def main():
    REPORT.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(prefix='cascade-resume-economy-') as directory:
        work = Path(directory)
        lib = UNITY / 'MonoBleedingEdge/lib/mono/4.5'
        cache = ROOT / 'CascadeUnity/Library/PackageCache'
        libraries = [next(cache.glob(pattern)) for pattern in (
            'com.unity.nuget.newtonsoft-json@*/Runtime/Newtonsoft.Json.dll',
            'com.unity.ext.nunit@*/net40/unity-custom/nunit.framework.dll',
        )]
        references = [lib / name for name in (
            'mscorlib.dll', 'System.dll', 'System.Core.dll', 'Facades/netstandard.dll',
        )] + libraries
        base = ROOT / 'CascadeUnity/Assets/_Cascade'
        sources = [p for folder in ('Domain', 'Solver', 'Application', 'Content', 'Persistence')
                   for p in sorted((base / folder).glob('*.cs'))]
        for p in sorted((base / 'Tests/EditMode').glob('*.cs')):
            source = p.read_text()
            if p.name == 'ReferenceFixtures.cs':
                source = source.replace('using UnityEngine;', '').replace(
                    'Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName',
                    json.dumps(str(ROOT)))
                adapted = work / p.name
                adapted.write_text(source)
                sources.append(adapted)
            elif not any(namespace in source for namespace in (
                    'UnityEngine', 'UnityEditor', 'Cascade.Presentation')):
                sources.append(p)
        helper = work / 'ContentHelper.cs'
        helper.write_text('using System.IO;using Cascade.Content;namespace Cascade.Tests{'
                          'internal static class ContentTests{public static ContentCatalog LoadCatalog(){'
                          'string R(string n)=>File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",n));'
                          'return ContentImporter.Import(R("levels.json"),R("localized_levels.json"),'
                          'R("words_en.json"),R("words_ru.json"),R("words_uk.json"));}}}')
        runner = work / 'PureRunner.cs'
        runner.write_text((ROOT / 'Reports/ReleaseAudit-2026-10-09/pure_runner.cs').read_text()
                          .replace('"ReleaseAudit-2026-10-09"', '"ResumeEconomyOct15"'))
        sources += [helper, runner]
        executable = work / 'Suite.exe'
        subprocess.run([str(UNITY / 'NetCoreRuntime/dotnet'),
                        str(UNITY / 'DotNetSdkRoslyn/csc.dll'), '-nologo', '-nostdlib',
                        '-langversion:9', f'-out:{executable}']
                       + [f'-r:{p}' for p in references] + [str(p) for p in sources], check=True)
        for p in libraries:
            shutil.copy2(p, work / p.name)
        results = []
        runtime_env = os.environ.copy()
        runtime_env['DYLD_LIBRARY_PATH'] = str(UNITY.parents[1] / 'PlaybackEngines/MacStandaloneSupport/Variations/macos_arm64_player_nondevelopment_mono/UnityPlayer.app/Contents/Frameworks')
        for fixture in FIXTURES:
            output = REPORT / 'pure-fixtures' / fixture
            output.mkdir(parents=True, exist_ok=True)
            with (output / 'output.txt').open('w') as log:
                subprocess.run([str(UNITY / 'MonoBleedingEdge/bin/mono'), str(executable),
                                str(output), fixture], stdout=log, stderr=subprocess.STDOUT,
                               timeout=90, check=False, env=runtime_env)
            cases = json.loads((output / 'pure-suite.json').read_text())
            if not cases:
                raise RuntimeError(f'No tests executed for {fixture}')
            results += cases
            print(f'{fixture}: {sum(x["status"] == "Passed" for x in cases)} passed, '
                  f'{sum(x["status"] == "Failed" for x in cases)} failed', flush=True)
        (REPORT / 'pure-suite.json').write_text(json.dumps(results, indent=2))
        summary = {status: sum(x['status'] == status for x in results)
                   for status in ('Passed', 'Failed', 'Skipped')}
        summary['method'] = 'Current pure production C# with actual NUnit cases; not native Unity PlayMode.'
        (REPORT / 'pure-suite-summary.json').write_text(json.dumps(summary, indent=2))
        print(json.dumps(summary), flush=True)
        if summary['Failed']:
            raise SystemExit(1)

if __name__ == '__main__':
    main()
