#!/usr/bin/env python3
"""Run actual production FitWidth against tight deck gaps; not Unity rendering."""
from pathlib import Path
import re,subprocess,tempfile
ROOT=Path(__file__).resolve().parents[1]
RUNTIME=Path('/Applications/Unity/Hub/Editor/6000.3.8f1/Unity.app/Contents/Resources/Scripting')
source=(ROOT/'CascadeUnity/Assets/_Cascade/Presentation/ToolSuspensionRope.cs').read_text()
method=re.search(r'public static float FitWidth\([^}]+}',source).group()
code='''using System;
static class Mathf {public static float Sqrt(float x)=>(float)Math.Sqrt(x); public static float Min(float x,float y)=>Math.Min(x,y); public static float Max(float x,float y)=>Math.Max(x,y);}
class Check {'''+method+'''
static void Main(){
 int checks=0;
 // The previous 88x55 prop + child timer is already 90.2 high at rest;
 // it cannot fit a common 86-unit deck gap with 14 units of clearance.
 if(88*(.775+.25)<=86-14)throw new Exception("Baseline no longer reproduces");
 foreach(float unit in new[]{1f,1.25f,1.6f})foreach(float gap in new[]{72f,80f,86f,100f,140f,220f})
 foreach(float timer in new[]{0f,26f})foreach(float scale in new[]{1f,1.1f,1.14f}){
  float width=FitWidth(88,gap*unit,unit,timer*unit,scale);
  if(width<=0||width>88)throw new Exception("Invalid size");
  for(int angle=-55;angle<=55;angle++){
   double a=angle*Math.PI/180,paintedHeight=width*scale*(.83*Math.Abs(Math.Sin(a))+.775*Math.Abs(Math.Cos(a)));
   if(paintedHeight+timer*unit+24*unit>gap*unit+.001)throw new Exception("Deck/timer overlap");checks++;
  }
 }
 Console.WriteLine("PASS: old tight-gap footprint reproduces overflow; "+checks+" fitted angle/gap/scale cases clear timer, cord and deck reserves. Native animation NOT RUN.");
}}
'''
with tempfile.TemporaryDirectory(prefix='cascade-hanging-fit-') as d:
 p=Path(d);(p/'Check.cs').write_text(code)
 libs=RUNTIME/'MonoBleedingEdge/lib/mono/4.5'
 subprocess.run([str(RUNTIME/'NetCoreRuntime/dotnet'),str(RUNTIME/'DotNetSdkRoslyn/csc.dll'),'-nologo','-nostdlib',f'-r:{libs/"mscorlib.dll"}',f'-out:{p/"Check.exe"}',str(p/'Check.cs')],check=True)
 output=subprocess.check_output([str(RUNTIME/'MonoBleedingEdge/bin/mono'),str(p/'Check.exe')],text=True)
 out=ROOT/'Reports/HangingFit-Oct09';out.mkdir(exist_ok=True);(out/'check.txt').write_text(output);print(output.strip())
