#!/usr/bin/env python3
"""Run focused effect-ablation proofs plus full catalog generation on production C#."""
from pathlib import Path
import subprocess
import tempfile
import shutil
import sys

ROOT = Path(__file__).resolve().parents[1]
UNITY = Path('/Applications/Unity/Hub/Editor/6000.3.8f1/Unity.app/Contents/Resources/Scripting')
with tempfile.TemporaryDirectory(prefix='cascade-effect-budget-') as directory:
    work = Path(directory)
    libraries = UNITY / 'MonoBleedingEdge/lib/mono/4.5'
    json_library = next((ROOT / 'CascadeUnity/Library/PackageCache').glob('com.unity.nuget.newtonsoft-json@*/Runtime/Newtonsoft.Json.dll'))
    nunit_library = next((ROOT / 'CascadeUnity/Library/PackageCache').glob('com.unity.ext.nunit@*/net40/unity-custom/nunit.framework.dll'))
    references = [libraries / name for name in ('mscorlib.dll', 'System.dll', 'System.Core.dll', 'Facades/netstandard.dll')] + [json_library, nunit_library]
    sources = [file for folder in ('Domain', 'Solver', 'Application', 'Content', 'Persistence') for file in sorted((ROOT / 'CascadeUnity/Assets/_Cascade' / folder).glob('*.cs'))]
    command = [str(UNITY / 'NetCoreRuntime/dotnet'), str(UNITY / 'DotNetSdkRoslyn/csc.dll'), '-nologo', '-langversion:9', '-nostdlib', f'-out:{work / "Audit.exe"}']
    command += [f'-r:{file}' for file in references] + [str(file) for file in sources] + [str(ROOT / 'Tools/check_effect_budgets.cs')] + [str(ROOT / 'CascadeUnity/Assets/_Cascade/Tests/EditMode' / name) for name in ('EffectBudgetTests.cs', 'PersonalMissionTests.cs', 'CampaignTriadTests.cs')]
    subprocess.run(command, check=True)
    shutil.copy2(json_library, work / 'Newtonsoft.Json.dll')
    shutil.copy2(nunit_library, work / 'nunit.framework.dll')
    subprocess.run([str(UNITY / 'MonoBleedingEdge/bin/mono'), str(work / 'Audit.exe'), str(ROOT)] + sys.argv[1:], check=True)
