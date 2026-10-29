#!/usr/bin/env python3
"""Execute the actual pure score-selection method without claiming native audio playback."""
from pathlib import Path
import re
import argparse
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[1]
UNITY = next(p for p in (Path('/Volumes/MY DISK/unity_installs/6000.3.8f1/Unity.app/Contents/Resources/Scripting'),Path('/Applications/Unity/Hub/Editor/6000.3.8f1/Unity.app/Contents/Resources/Scripting')) if p.exists())
parser=argparse.ArgumentParser();parser.add_argument('--output',type=Path,default=ROOT/'Reports/MusicOct17/score-routing.txt');args=parser.parse_args()
presentation = ROOT / 'CascadeUnity/Assets/_Cascade/Presentation'
screens = re.search(r'public enum GameScreen \{[^}]+\}', (presentation/'CascadeGame.Flow.cs').read_text()).group()
with tempfile.TemporaryDirectory(prefix='cascade-score-routing-') as directory:
    work = Path(directory)
    harness = work/'Review.cs'
    harness.write_text('''using System;
namespace UnityEngine { public class ScriptableObject {} public class AudioClip {} }
namespace Cascade.Presentation { '''+screens+''' }
class Review {
 static void Main() {
  var library=new Cascade.Presentation.ToyAudioLibrary { menuMusic=new UnityEngine.AudioClip(), music=new UnityEngine.AudioClip(), worldMusic=new UnityEngine.AudioClip[9] };
  for(int z=0;z<9;z++){library.worldMusic[z]=new UnityEngine.AudioClip();}
  int cases=0;
  foreach(Cascade.Presentation.GameScreen screen in Enum.GetValues(typeof(Cascade.Presentation.GameScreen)))for(int zone=-1;zone<=9;zone++){
   bool attempt=screen==Cascade.Presentation.GameScreen.Playing||screen==Cascade.Presentation.GameScreen.Pause||screen==Cascade.Presentation.GameScreen.Win||screen==Cascade.Presentation.GameScreen.Fail||screen==Cascade.Presentation.GameScreen.Checkpoint||screen==Cascade.Presentation.GameScreen.WorldComplete;
   bool browse=screen==Cascade.Presentation.GameScreen.JourneyMap||screen==Cascade.Presentation.GameScreen.Dialogue||screen==Cascade.Presentation.GameScreen.Prologue||screen==Cascade.Presentation.GameScreen.JourneyEnd;
   var expected=zone>=0&&zone<9?(attempt||browse?library.worldMusic[zone]:library.menuMusic):library.menuMusic;
   if(!Object.ReferenceEquals(library.ScoreFor(screen,zone),expected))throw new Exception(screen+"/"+zone+" incorrect theme");cases++;
  }
  library.worldMusic[4]=null;
  if(!Object.ReferenceEquals(library.ScoreFor(Cascade.Presentation.GameScreen.Playing,4),library.menuMusic))throw new Exception("Missing world fallback");
  if(!Object.ReferenceEquals(library.ScoreFor(Cascade.Presentation.GameScreen.JourneyMap,4),library.menuMusic))throw new Exception("Missing map world fallback");
  library.worldMusic=null;
  if(!Object.ReferenceEquals(library.ScoreFor(Cascade.Presentation.GameScreen.Prologue,4),library.menuMusic))throw new Exception("Missing world array fallback");
  library.menuMusic=null;
  if(!Object.ReferenceEquals(library.ScoreFor(Cascade.Presentation.GameScreen.Home,0),library.music))throw new Exception("Legacy fallback");
  Console.WriteLine("PASS: "+cases+" screen/zone selections plus missing-world and legacy-home fallbacks. Actual ToyAudioLibrary.ScoreFor; engine playback NOT tested.");
 }
}''')
    libraries=UNITY/'MonoBleedingEdge/lib/mono/4.5'
    command=[str(UNITY/'NetCoreRuntime/dotnet'),str(UNITY/'DotNetSdkRoslyn/csc.dll'),'-nologo','-nostdlib',f'-out:{work/"Review.exe"}']
    command += [f'-r:{libraries/name}' for name in ('mscorlib.dll','System.dll','System.Core.dll')]
    subprocess.run(command+[str(harness),str(presentation/'ToyAudioLibrary.cs')],check=True)
    result=subprocess.run([str(UNITY/'MonoBleedingEdge/bin/mono'),str(work/'Review.exe')],check=True,capture_output=True,text=True).stdout
    report=args.output;report.parent.mkdir(parents=True,exist_ok=True);report.write_text(result)
    print(result.strip())
