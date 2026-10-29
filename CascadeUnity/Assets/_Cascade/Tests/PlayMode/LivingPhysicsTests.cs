#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Domain;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Cascade.Tests {
 public sealed partial class AccountReferenceTests {
  string LivingReviewFolder=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/LivingPhysicsOct18/Captures"));
  Rigidbody[] LivingSplinters()=>UnityEngine.Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None).Where(b=>{var mesh=b.GetComponent<MeshFilter>();return mesh!=null&&mesh.sharedMesh!=null&&mesh.sharedMesh.name=="Pooled timber splinter"&&b.gameObject.activeInHierarchy;}).ToArray();
  void LivingShot(string name){
   Directory.CreateDirectory(LivingReviewFolder);Canvas.ForceUpdateCanvases();camera.Render();var active=RenderTexture.active;RenderTexture.active=portrait;
   var pixels=new Texture2D(portrait.width,portrait.height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,portrait.width,portrait.height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(LivingReviewFolder,name+".png"),pixels.EncodeToPNG());RenderTexture.active=active;UnityEngine.Object.Destroy(pixels);
  }
  string SplinterReviewFolder=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/PlatformSplintersOct18/Captures"));
  void SplinterShot(string name){
   Directory.CreateDirectory(SplinterReviewFolder);Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;RenderTexture.active=portrait;
   var pixels=new Texture2D(portrait.width,portrait.height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,portrait.width,portrait.height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(SplinterReviewFolder,name+".png"),pixels.EncodeToPNG());RenderTexture.active=previous;UnityEngine.Object.Destroy(pixels);
  }
  [UnityTest] public IEnumerator LivingPlatformFragmentsFallWithTheRowAndKeepThePuzzle(){
   PrepareTimberFixture();game.LoadLevel(TimberFixture(1));game.ShowGame();yield return FooterWait(()=>game.Interaction.CanPlay,"Break fixture ready");yield return new WaitForSecondsRealtime(.4f);SplinterShot("break-before");
   var result=game.Interaction.Execute(PuzzleCommand.Swap(0));Assert.That(result.Accepted,Is.True);string hash=game.Interaction.Attempt.CurrentStateHash;
   yield return FooterWait(()=>LivingSplinters().Length==16,"The board must become sixteen large, separate splinters");var bodies=LivingSplinters();var start=bodies.Select(b=>b.position).ToArray();var paint=new MaterialPropertyBlock();
   Assert.That(bodies.All(b=>!b.isKinematic&&b.useGravity),Is.True);Assert.That(bodies[0].gameObject.scene.GetPhysicsScene(),Is.Not.EqualTo(Physics.defaultPhysicsScene));
   Assert.That(bodies.Select(b=>b.GetComponent<MeshFilter>().sharedMesh).Distinct().Count(),Is.EqualTo(16));
   foreach(var body in bodies){var mesh=body.GetComponent<MeshFilter>().sharedMesh;Assert.That(mesh.vertexCount,Is.GreaterThan(24));Assert.That(mesh.bounds.size.x,Is.LessThan(game.Machine.FloorSpan(0)*.32f),"No half-platform-sized piece remains");Assert.That(mesh.bounds.size.z,Is.GreaterThan(.08f));body.GetComponent<MeshRenderer>().GetPropertyBlock(paint);Assert.That(paint.GetColor("_BaseColor").a,Is.EqualTo(1));Assert.That(paint.GetVector("_BaseMap_ST").x,Is.GreaterThan(0));}
   SplinterShot("break-release");game.Interaction.SetPaused(true);var held=bodies.Select(b=>b.position).ToArray();yield return new WaitForSecondsRealtime(.13f);for(int i=0;i<bodies.Length;i++)Assert.That(bodies[i].position,Is.EqualTo(held[i]));game.Interaction.SetPaused(false);
   for(int i=0;i<10;i++){yield return new WaitForSecondsRealtime(.07f);SplinterShot("break-fall-"+i.ToString("D2"));if(i==2){Assert.That(bodies.Select((b,n)=>b.position.y-start[n].y).Average(),Is.LessThan(-.12f));foreach(var body in bodies.Where(b=>b.gameObject.activeSelf)){body.GetComponent<MeshRenderer>().GetPropertyBlock(paint);Assert.That(paint.GetColor("_BaseColor").a,Is.EqualTo(1),"Visible falling timber stays opaque");}}}
   Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
   yield return new WaitForSecondsRealtime(7.6f);Assert.That(LivingSplinters(),Is.Empty,"No stranded cosmetic bodies remain");
  }
  [UnityTest] public IEnumerator TimberSplintersBounceOnLowerDecksThenLeaveTheCamera(){
   PrepareTimberFixture();var source=TimberFixture();var level=new LevelDefinition(source.PuzzleId,source.CampaignNodeId,source.SchemaVersion,source.RulesVersion,"splinter-lower-decks",source.InitialSymbolIds,source.TargetSymbolIds,source.TargetSegments,new[]{new FloorDefinition(8,new[]{true,true,true}),new FloorDefinition(8,new[]{true,true,true}),new FloorDefinition(8,new[]{true,true,true})});
   game.LoadLevel(level);game.ShowGame();yield return FooterWait(()=>game.Interaction.CanPlay,"Lower decks ready");yield return new WaitForSecondsRealtime(.4f);string hash=game.Interaction.Attempt.CurrentStateHash;
   game.Machine.FractureFloor(0,.60f);game.Machine.ReleaseFloorTimber(0);var bodies=LivingSplinters();Assert.That(bodies.Length,Is.EqualTo(16));
   Assert.That(game.Machine.DebrisSupport(0,.5f,out _,out _,out _),Is.False,"An invisible released deck cannot catch its own splinters");
   var downward=new bool[bodies.Length];var bounced=new bool[bodies.Length];var positions=bodies.Select(b=>b.position).ToArray();var trace=new List<string>{"time,active,bounced,minY,maxY,minZ,maxZ"};float started=Time.realtimeSinceStartup;int shot=0;
   while(Time.realtimeSinceStartup-started<7.5f){
    float elapsed=Time.realtimeSinceStartup-started;int active=0;float min=100,max=-100,minZ=100,maxZ=-100;
    for(int i=0;i<bodies.Length;i++)if(bodies[i].gameObject.activeSelf){
     active++;var b=bodies[i];min=Mathf.Min(min,b.position.y);max=Mathf.Max(max,b.position.y);minZ=Mathf.Min(minZ,b.position.z);maxZ=Mathf.Max(maxZ,b.position.z);downward[i]|=b.linearVelocity.y<-1;
     if(downward[i]&&b.linearVelocity.y>.15f&&Enumerable.Range(1,2).Any(f=>Mathf.Abs(b.position.y-game.Machine.TimberSurfacePoint(f,.5f).y)<.85f))bounced[i]=true;
     Assert.That(float.IsNaN(b.position.x)||float.IsInfinity(b.position.y),Is.False);
    }
    trace.Add(elapsed.ToString("F3")+","+active+","+bounced.Count(v=>v)+","+min+","+max+","+minZ+","+maxZ);
    if(elapsed>=shot*.12f&&shot<25){SplinterShot("lower-decks-"+shot.ToString("D2"));shot++;}
    if(active==0)break;yield return null;
   }
   Directory.CreateDirectory(SplinterReviewFolder);File.WriteAllLines(Path.Combine(SplinterReviewFolder,"lower-deck-trajectory.csv"),trace);
   Assert.That(bounced.Count(v=>v),Is.GreaterThanOrEqualTo(2),"Real gravity/contact must produce readable rebounds on lower platforms");
   Assert.That(bodies.Any(b=>b.gameObject.activeSelf),Is.False,"Splinters leave the camera before their eight-second fallback, instead of resting until a fade");
   Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
  }
  [UnityTest] public IEnumerator FallingTimberSurvivesDustReuseAndRespectsReducedMotion(){
   PrepareTimberFixture();game.LoadLevel(TimberFixture());game.ShowGame();yield return FooterWait(()=>game.Interaction.CanPlay,"Pool fixture ready");var hash=game.Interaction.Attempt.CurrentStateHash;
   var face=game.Machine.GetComponentsInChildren<MeshFilter>().Single(m=>m.name=="Wooden suspended board");
   game.Machine.ReleaseFloorTimber(0);var bodies=LivingSplinters();Assert.That(bodies.Length,Is.EqualTo(16));
   for(int i=0;i<3;i++)game.Feedback.Burst(game.Machine.FloorPosition(0),Color.white,32,false);
   Assert.That(LivingSplinters().Length,Is.EqualTo(16),"Landing sparks and dust must not overwrite visible falling timber");
   game.Feedback.ClearTails();game.Feedback.Quality=1;game.Feedback.ReleaseTimber(face,Vector3.zero,0);Assert.That(LivingSplinters().Length,Is.EqualTo(10));
   game.Feedback.ClearTails();game.ReducedMotion=true;game.Feedback.ReleaseTimber(face,Vector3.zero,0);Assert.That(LivingSplinters(),Is.Empty);game.Feedback.WoodChips(game.Machine.FloorPosition(0),8,true);yield return null;
   Assert.That(game.Feedback.ActiveParticleCount,Is.LessThanOrEqualTo(2));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));game.Feedback.ClearTails();
  }
  [UnityTest] public IEnumerator LivingPlankLoadRespondsToMassAndLift(){
   PrepareTimberFixture();float[] sag=new float[2];
   for(int variant=0;variant<2;variant++){
    var source=TimberFixture();var word=game.Catalog.AllLevels.First(l=>l.Definition.TargetSymbolIds.Count==4&&l.Definition.TargetSymbolIds.All(id=>id.Kind==SymbolKind.Glyph&&id.Locale=="en")).Definition;
    var row=variant==0?new[]{"s:triangle","s:diamond","s:triangle","s:diamond"}.Select(id=>SymbolId.Parse(id)).ToArray():word.TargetSymbolIds.ToArray();
    var segments=variant==0?new[]{new TargetSegment(SegmentKind.Shapes,0,4)}:word.TargetSegments.ToArray();
    var level=new LevelDefinition(source.PuzzleId,source.CampaignNodeId,source.SchemaVersion,source.RulesVersion,"living-load-"+variant,row,row,segments,source.Floors);
    game.LoadLevel(level);game.ShowGame();yield return FooterWait(()=>game.Interaction.CanPlay,"Weight comparison ready");yield return new WaitForSecondsRealtime(1.5f);sag[variant]=game.Machine.LoadedSag;LivingShot(variant==0?"load-shapes":"load-letters");
   }
   string hash=game.Interaction.Attempt.CurrentStateHash;Assert.That(sag[1],Is.GreaterThan(sag[0]*1.12f),"Heavier letter blocks must produce more static deflection, without normalization cancelling their mass");
   game.Machine.SetHeldWeight(1);yield return new WaitForSecondsRealtime(.9f);float lifted=game.Machine.LoadedSag;Assert.That(lifted,Is.LessThan(sag[1]*.93f));LivingShot("load-one-held");game.Machine.SetHeldWeight(-1);yield return new WaitForSecondsRealtime(1.2f);Assert.That(game.Machine.LoadedSag,Is.EqualTo(sag[1]).Within(.018f));
   foreach(var part in game.Machine.GetComponentsInChildren<MeshFilter>().Where(m=>m.name.StartsWith("Persistent token "))){
    var foot=part.transform.TransformPoint(part.sharedMesh.bounds.center-Vector3.up*part.sharedMesh.bounds.extents.y);float x=(foot.x-game.Machine.FloorPosition(0).x)/game.Machine.FloorSpan(0)+.5f;var contact=game.Machine.TimberSurfacePoint(0,x);Assert.That(Mathf.Abs(foot.y-contact.y),Is.LessThan(.055f),"Each block remains seated on the painted timber, not an obsolete rectangle");
   }
   Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
  }
  [UnityTest] public IEnumerator LivingWildlifePerchesUseRealSupportsAndContinuousPoses(){
   PrepareTimberFixture();game.LoadLevel(TimberFixture(8));game.ShowGame();yield return FooterWait(()=>game.Interaction.CanPlay,"Quiet perch fixture ready");game.Machine.SetAmbientPaused(true);
   var flags=BindingFlags.Instance|BindingFlags.NonPublic;var life=game.Machine.GetComponent<WorldLifeView>();var perch=typeof(WorldLifeView).GetField("perch",flags).GetValue(life);
   void Set(object owner,string name,object value)=>owner.GetType().GetField(name,flags).SetValue(owner,value);
   Set(game.Machine,"nextAmbientGust",100000f);Set(game.Machine,"gustStarted",-100f);Set(game.Machine,"gustDirection",0f);Set(game.Machine,"windTime",100f);Set(game.Machine,"wildlifeLastInput",80f);
   Assert.That(game.Machine.TryChooseWildlifePerch(4,.35f,true,out var target),Is.True);Assert.That(target.Kind,Is.Not.EqualTo(WildlifePerchKind.Frame));
   Assert.That(game.Machine.TryWildlifeSupport(new WildlifePerchTarget(WildlifePerchKind.Frame,0,.4f,game.Machine.WildlifeRevision),out _,out _),Is.False,"A vertical screen post has no horizontal ledge for feet");
   if(target.Kind==WildlifePerchKind.EmptyPlatform||target.Kind==WildlifePerchKind.Board){Assert.That(game.Machine.TryWildlifeSupport(target,out var contact,out _),Is.True);Assert.That(contact.y,Is.EqualTo(game.Machine.TimberSurfacePoint(target.Index,target.X).y).Within(.001f));}
   perch.GetType().GetMethod("Cancel").Invoke(perch,new object[]{100f});Set(perch,"nextVisit",0f);Set(perch,"revision",game.Machine.WildlifeRevision);Set(perch,"visit",1);
   string hash=game.Interaction.Attempt.CurrentStateHash;var record=new List<string>();var previous=Vector3.zero;float largestStep=0;
   for(int frame=0;frame<150;frame++){
    float phase=100+frame/30f;Set(game.Machine,"windTime",phase);if(frame==105)game.Machine.NotifyWildlifeInput();game.Machine.LivingWorld.SetQuietFlight(false);game.Machine.LivingWorld.Paint(phase);
    if(life.BirdState!=WildlifePerchState.None&&frame>0)largestStep=Mathf.Max(largestStep,Vector3.Distance(previous,life.BirdFeet));previous=life.BirdFeet;
    if(frame==60||frame==75){Assert.That(life.BirdState,Is.EqualTo(WildlifePerchState.Resting));Assert.That(life.BirdSupportKind,Is.Not.EqualTo(WildlifePerchKind.Frame));}
    if(frame%10==0){LivingShot("perch-"+frame.ToString("D3"));record.Add(phase+" "+life.BirdState+" "+life.BirdFeet);}
    if(frame%15==0)yield return null;
   }
   Directory.CreateDirectory(LivingReviewFolder);File.WriteAllLines(Path.Combine(LivingReviewFolder,"perch-motion.txt"),record);Assert.That(largestStep,Is.LessThan(.5f),"Perch transitions must not teleport the feet");Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
  }
 }
}
#endif
