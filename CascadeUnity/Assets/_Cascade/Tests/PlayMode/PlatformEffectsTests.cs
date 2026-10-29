#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Presentation;
using Cascade.Solver;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Cascade.Tests
{
 public sealed partial class AccountReferenceTests
 {
  void PrepareTimberFixture(){WelcomePhone();game.Preferences.Tutorials=false;game.Preferences.EffectQuality=2;game.ReducedMotion=false;Profile.AccountIntroSeen=true;Profile.Missions.NextAfterClears=20000;Profile.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();foreach(var lesson in Profile.SeenMechanics){Profile.SeenCoachPrompts.Add("mechanic-film-v1-"+(int)lesson);Profile.SeenCoachPrompts.Add("triad-guide-v6-"+(int)lesson);}}
  LevelDefinition TimberFixture(int hp=8,bool fragile=false,bool bridge=false,EntryTransform style=EntryTransform.None){var source=game.Catalog.GetPrimary("C03").Definition;var row=new[]{"s:triangle","s:circle","s:square","s:diamond"}.Select(id=>SymbolId.Parse(id)).ToArray();return new LevelDefinition(source.PuzzleId,source.CampaignNodeId,source.SchemaVersion,source.RulesVersion,"timber-presentation",row,row,source.TargetSegments,new[]{new FloorDefinition(hp,new[]{true,true,true},fragile?new[]{0,1,2}:null,entryTransform:style,oneUseBridgeEdge:bridge?(int?)0:null)});}
  [UnityTest] public IEnumerator FragileCrossingCracksAtTheEdgeAndSeatsTheRaisedStop()
  {
   PrepareTimberFixture();game.LoadLevel(TimberFixture(8,true,true));game.ShowGame();yield return FooterWait(()=>game.Interaction.CanPlay,"Timber crossing ready");
   var result=game.Interaction.Execute(PuzzleCommand.Swap(0));Assert.That(result.Accepted,Is.True);Assert.That(result.DurabilityCost,Is.EqualTo(2));var hash=game.Interaction.Attempt.CurrentStateHash;
   yield return FooterWait(()=>game.EventPlayer.CurrentEvent==PuzzleEventKind.DurabilityResolved,"Crossing reaches its actual crack cue");yield return new WaitForSecondsRealtime(.14f);
   var board=game.Machine.GetComponentsInChildren<MeshFilter>().Single(f=>f.name=="Wooden suspended board");var paint=new MaterialPropertyBlock();board.GetComponent<MeshRenderer>().GetPropertyBlock(paint);
   Assert.That(paint.GetVector("_BaseMap_ST"),Is.EqualTo(GameplayReferenceArt.BoardUv(-1,2)),"A weak-edge crossing strengthens the authored whole-board damage state");Assert.That(game.Machine.GetComponentsInChildren<MeshRenderer>().Any(m=>m.name=="Timber grain cracks"),Is.False,"No rejected crack overlay remains");
   game.Interaction.SetPaused(true);var held=board.sharedMesh.vertices;yield return new WaitForSecondsRealtime(.15f);CollectionAssert.AreEqual(held,board.sharedMesh.vertices);game.Interaction.SetPaused(false);
   yield return FooterWait(()=>!game.Interaction.IsPresenting,"Crossing settles");yield return new WaitForSecondsRealtime(.4f);
   var stop=game.Machine.GetComponentsInChildren<MeshFilter>().Single(f=>f.name=="Bridge hinged timber stop");var foot=stop.transform.TransformPoint(stop.sharedMesh.bounds.center-Vector3.up*stop.sharedMesh.bounds.extents.y);var surface=game.Machine.TimberSurfacePoint(0,.045f+.91f/4);foot.z=surface.z=0;
   Assert.That(Vector3.Distance(foot,surface),Is.LessThan(.05f),"Raised timber must remain seated in the bent plank");Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.Interaction.Attempt.RouteLength,Is.EqualTo(1));
   var badge=game.GetComponentsInChildren<PlatformLegend>().Single();var rect=(RectTransform)badge.transform;var corners=new Vector3[4];rect.GetWorldCorners(corners);var bottom=(corners[0]+corners[3])*.5f;float x=(float)typeof(PlatformLegend).GetField("labelX",Private).GetValue(badge);var wood=game.Machine.TimberLowerEdgePoint(0,x);bottom.z=wood.z=0;Assert.That(Vector3.Distance(bottom,wood),Is.LessThan(.002f),"The strength plaque's bottom edge must meet the painted bottom of the timber");
  }
  static Vector3 PaintedBoardPin(UnityEngine.Mesh mesh,Vector3[] vertices,float u)
  {
   var uv=mesh.uv;
   for(int i=0;i<vertices.Length-3;i+=2){float span=uv[i+2].x-uv[i].x;if(span<=0||u<uv[i].x||u>uv[i+2].x)continue;
    return Vector3.Lerp((vertices[i]+vertices[i+1])*.5f,(vertices[i+2]+vertices[i+3])*.5f,(u-uv[i].x)/span);}
   Assert.Fail("Painted pin must be inside the board UV surface");return Vector3.zero;
  }
  [UnityTest] public IEnumerator FractureCoversTheDeckAndBothHalvesRemainPinnedToRopes()
  {
   PrepareTimberFixture();foreach(var style in new[]{EntryTransform.None,EntryTransform.MirrorNext,EntryTransform.ShiftLeftOne,EntryTransform.ShiftRightOne}){
    game.LoadLevel(TimberFixture(style:style));game.ShowGame();yield return FooterWait(()=>game.Interaction.CanPlay,"Styled plank ready");var hash=game.Interaction.Attempt.CurrentStateHash;
    var faceRenderer=game.Machine.GetComponentsInChildren<MeshRenderer>().Single(f=>f.name=="Wooden suspended board");var paint=new MaterialPropertyBlock();int index=style==EntryTransform.None?-1:style==EntryTransform.MirrorNext?0:1;
    foreach(var step in new[]{new Vector2(.08f,1),new Vector2(.20f,2),new Vector2(.38f,3)}){game.Machine.FractureFloor(0,step.x);faceRenderer.GetPropertyBlock(paint);Assert.That(paint.GetVector("_BaseMap_ST"),Is.EqualTo(GameplayReferenceArt.BoardUv(index,(int)step.y)),"The authored states progress from fine cracks to deep cracks to the split board");}
    var face=game.Machine.GetComponentsInChildren<MeshFilter>().Single(f=>f.name=="Wooden suspended board");var before=face.sharedMesh.vertices;game.Machine.FractureFloor(0,.68f);var broken=face.sharedMesh.vertices;
    Assert.That(broken[16].y,Is.LessThan(before[16].y-.1f));Assert.That(broken[18].y,Is.LessThan(before[18].y-.1f));foreach(float u in new[]{.067f,.93f}){var shift=PaintedBoardPin(face.sharedMesh,broken,u)-PaintedBoardPin(face.sharedMesh,before,u);Assert.That(Mathf.Abs(shift.x),Is.LessThan(.002f));Assert.That(Mathf.Abs(shift.y),Is.LessThan(.03f),"The actual painted pins remain attached; timber beyond them can rotate");}
    Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));game.Machine.FractureFloor(0,0);
   }
  }
  [UnityTest] public IEnumerator TimberSplintersFallPauseFadeAndLeaveNoStrandedBodies()
  {
   PrepareTimberFixture();game.LoadLevel(TimberFixture());game.ShowGame();yield return FooterWait(()=>game.Interaction.CanPlay,"Debris fixture ready");string hash=game.Interaction.Attempt.CurrentStateHash;
   game.Feedback.ClearTails();var origin=game.Machine.FloorPosition(0)+Vector3.down*.7f;game.Feedback.WoodChips(origin,8,false,2);
   var bodies=UnityEngine.Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None).Where(b=>b.name.StartsWith("Pooled workshop dust")&&!b.isKinematic).ToArray();Assert.That(bodies.Length,Is.EqualTo(8));Assert.That(bodies[0].gameObject.scene.GetPhysicsScene(),Is.Not.EqualTo(Physics.defaultPhysicsScene));
   Assert.That(bodies.Select(b=>b.angularVelocity).Distinct().Count(),Is.GreaterThan(3));Assert.That(bodies[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.z,Is.GreaterThan(.5f),"Splinters retain volume during their tumble");
   var start=bodies.Select(b=>b.position).ToArray();yield return new WaitForSecondsRealtime(.55f);Assert.That(bodies.Select((b,i)=>b.position.y-start[i].y).Average(),Is.LessThan(-.25f));
   game.Interaction.SetPaused(true);var paused=bodies.Select(b=>b.position).ToArray();yield return new WaitForSecondsRealtime(.12f);for(int i=0;i<bodies.Length;i++)Assert.That(bodies[i].position,Is.EqualTo(paused[i]));game.Interaction.SetPaused(false);
   // Simulate a chip caught on an in-camera ledge. Lifetime must still retire it.
   foreach(var trapped in bodies){trapped.isKinematic=true;trapped.position=origin;}var block=new MaterialPropertyBlock();yield return new WaitForSecondsRealtime(1.85f);bool fading=false;foreach(var trapped in bodies.Where(b=>b.gameObject.activeSelf)){trapped.GetComponent<MeshRenderer>().GetPropertyBlock(block);float alpha=block.GetColor("_BaseColor").a;fading|=alpha>.01f&&alpha<.95f;}Assert.That(fading,Is.True,"Chips stranded in view visibly fade before retirement");
   yield return new WaitForSecondsRealtime(1.4f);Assert.That(bodies.Any(b=>b.gameObject.activeSelf),Is.False);
   game.Feedback.WoodChips(origin,1,false);var offscreen=UnityEngine.Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None).Single(b=>b.name.StartsWith("Pooled workshop dust")&&!b.isKinematic);offscreen.position=camera.ViewportToWorldPoint(new Vector3(.5f,-.3f,10));yield return new WaitForSecondsRealtime(.3f);Assert.That(offscreen.gameObject.activeSelf,Is.False);
   game.ReducedMotion=true;game.Feedback.WoodChips(origin,8,true);yield return null;Assert.That(game.Feedback.ActiveParticleCount,Is.LessThanOrEqualTo(2));game.Feedback.ClearTails();Assert.That(game.Feedback.ActiveParticleCount,Is.Zero);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
  }
  [UnityTest] public IEnumerator TimberWeightAndContactMomentumRemainPresentationOnly()
  {
   PrepareTimberFixture();game.LoadLevel(TimberFixture(4));game.ShowGame();yield return FooterWait(()=>game.Interaction.CanPlay,"Loaded board is playable without introductory teaching");yield return new WaitForSecondsRealtime(.6f);
   float fresh=game.Machine.LoadedSag;Assert.That(fresh,Is.GreaterThan(.08f));string hash=game.Interaction.Attempt.CurrentStateHash;
   var ropes=game.Machine.GetComponentsInChildren<LineRenderer>().Where(l=>l.name.Contains("braided rope")).ToArray();var before=ropes.Select(l=>l.GetPosition(12)).ToArray();float peak=fresh,travel=0;
   game.Machine.ContactImpulse(0,0,.9f,1);float until=Time.realtimeSinceStartup+.45f;while(Time.realtimeSinceStartup<until){peak=Mathf.Max(peak,game.Machine.LoadedSag);for(int i=0;i<ropes.Length;i++)travel=Mathf.Max(travel,Vector3.Distance(before[i],ropes[i].GetPosition(12)));yield return null;}
   Assert.That(peak,Is.GreaterThan(fresh+.003f));Assert.That(travel,Is.GreaterThan(.003f));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
   for(int i=0;i<3;i++){game.Interaction.Execute(PuzzleCommand.Swap(0));yield return FooterWait(()=>!game.Interaction.IsPresenting,"Wear command settles");}yield return new WaitForSecondsRealtime(.5f);Assert.That(game.Machine.LoadedSag,Is.GreaterThan(fresh+.07f));
   game.ReducedMotion=true;yield return null;float sag=game.Machine.LoadedSag;hash=game.Interaction.Attempt.CurrentStateHash;game.Machine.ContactImpulse(0,0,1,1);yield return new WaitForSecondsRealtime(.1f);Assert.That(game.Machine.LoadedSag,Is.EqualTo(sag).Within(.001f));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
  }
  [UnityTest] public IEnumerator ReceiverSilhouettesStayCentredAtPhoneSizesInAllLanguages()
  {
   PrepareTimberFixture();game.ReducedMotion=true;foreach(string locale in new[]{"en","ru","uk"})foreach(int width in new[]{320,390}){
    camera.aspect=width/(float)(width==320?693:844);camera.ResetProjectionMatrix();WelcomePhone(width,width==320?693:844);game.SetUiLocale(locale);game.StartDescentLevel(TimberFixture());game.ShowGame();yield return FooterWait(()=>game.Interaction.CanPlay,"Receiver level ready");Assert.That(((UnityEngine.UI.Button)Field("undo")).GetComponentInChildren<TMPro.TextMeshProUGUI>().GetComponentInParent<RoundHudBadge>(),Is.Not.Null,"The stock counter returns to its badge after a checkpoint");foreach(var command in PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver).Path){game.Interaction.Execute(command);yield return FooterWait(()=>!game.Interaction.IsPresenting,"Stage command settles");}
    yield return FooterWait(()=>game.CurrentScreen==GameScreen.Checkpoint,"Stage receiver settles");Canvas.ForceUpdateCanvases();
    var sockets=game.Machine.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name=="Target socket").OrderBy(r=>r.bounds.center.x).ToArray();float lo=100,hi=-100;
    for(int i=0;i<sockets.Length;i++){
     var body=game.Machine.GetComponentsInChildren<MeshRenderer>().Single(r=>r.name=="Target "+i);var delta=camera.WorldToScreenPoint(body.bounds.center)-camera.WorldToScreenPoint(sockets[i].bounds.center);Assert.That(Mathf.Abs(delta.x),Is.LessThan(.1f));Assert.That(Mathf.Abs(delta.y),Is.LessThan(.1f),"Shapes must use the exact socket centre");lo=Mathf.Min(lo,delta.y);hi=Mathf.Max(hi,delta.y);Assert.That(body.bounds.size.y,Is.LessThan(sockets[i].bounds.size.y));
    }
    Assert.That(hi-lo,Is.LessThan(.12f),"Every shape uses its visible centre, including the offset triangle pivot");
    var tray=game.Machine.GetComponentsInChildren<SpriteRenderer>().Single(r=>r.name=="Cosmetic receiver rim");
    float creamCentre=tray.transform.position.y+tray.bounds.size.y*(.5f-240f/499f),socketCentre=(sockets.Min(r=>r.bounds.min.y)+sockets.Max(r=>r.bounds.max.y))*.5f;
    Assert.That(Mathf.Abs(camera.WorldToScreenPoint(Vector3.up*creamCentre).y-camera.WorldToScreenPoint(Vector3.up*socketCentre).y),Is.LessThan(.15f),"The whole goal row centres in the authored cream field");
   }
  }
  [UnityTest] public IEnumerator RoundGameplayBadgesKeepReadableCentredTypeAfterLayoutAndLocaleChanges()
  {
   PrepareTimberFixture();game.ReducedMotion=true;game.StartDescentLevel(TimberFixture());game.ShowGame();yield return FooterWait(()=>game.Interaction.CanPlay,"Badge scene is playable");
   foreach(var size in new[]{new Vector2Int(320,568),new Vector2Int(390,844),new Vector2Int(588,1280)})foreach(string locale in new[]{"en","ru","uk"})foreach(bool large in new[]{false,true}){
    camera.aspect=size.x/(float)size.y;camera.ResetProjectionMatrix();WelcomePhone(size.x,size.y);game.SetUiLocale(locale);game.Preferences.LargeText=large;
    typeof(CascadeGame).GetMethod("ApplyPreferences",Private).Invoke(game,null);game.ShowGame();yield return null;yield return null;
    yield return FooterWait(()=>((UnityEngine.UI.Button)Field("drop")).GetComponentInChildren<TMPro.TMP_Text>().text==(locale=="en"?"Descend":locale=="ru"?"Вниз":"Униз"),"Gameplay locale settles before badge capture");Canvas.ForceUpdateCanvases();
    var badges=game.GetComponentsInChildren<RoundHudBadge>();Assert.That(badges.Length,Is.EqualTo(2));
    foreach(var badge in badges){
     var label=badge.GetComponentInChildren<TMPro.TextMeshProUGUI>();Assert.That(label,Is.Not.Null);label.ForceMeshUpdate();var ink=CenteredContentRow.GlyphBounds(label);var area=(RectTransform)badge.transform;
     var low=(Vector2)area.InverseTransformPoint(label.transform.TransformPoint(ink.min));var high=(Vector2)area.InverseTransformPoint(label.transform.TransformPoint(ink.max));var box=Rect.MinMaxRect(low.x,low.y,high.x,high.y);float diameter=Mathf.Min(area.rect.width,area.rect.height),unit=CenteredContentRow.Unit(area);
     Assert.That(box.height/diameter,Is.InRange(.45f,.85f),badge.name+" readable symbol height");Assert.That(Vector2.Distance(box.center,area.rect.center),Is.LessThan(.8f*unit),badge.name+" visible centre");
     Assert.That(box.xMin,Is.GreaterThanOrEqualTo(area.rect.xMin+2*unit-.5f));Assert.That(box.xMax,Is.LessThanOrEqualTo(area.rect.xMax-2*unit+.5f));
     Assert.That(box.yMin,Is.GreaterThanOrEqualTo(area.rect.yMin+2*unit-.5f));Assert.That(box.yMax,Is.LessThanOrEqualTo(area.rect.yMax-2*unit+.5f));
    }
    if(!large)FooterShot("hud-badges-"+locale+"-"+size.x);
   }
  }
 }
}
#endif
