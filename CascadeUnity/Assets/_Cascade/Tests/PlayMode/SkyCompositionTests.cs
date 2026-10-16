#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Cascade.Tests
{
    public sealed class SkyCompositionTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;Camera view;RenderTexture target,oldTarget;string directory;bool background,audio;
        static string Output=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/FlightPerspectivePauseOct15"));
        static Type EditorType(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable."+name)).First(t=>t!=null);
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;audio=AudioListener.pause;AudioListener.pause=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeNavigationReviewSky",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            foreach(string locale in new[]{"en","ru","uk"})yield return UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();view=Camera.main;oldTarget=view.targetTexture;
            Resize(390,844);game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.SetEditorSafeInsets(new Vector4(.01f,.04f,.01f,.035f));
            game.Preferences.Tutorials=false;game.Preferences.EffectQuality=2;game.Coach.Enabled=false;game.ReducedMotion=false;
            var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(game);profile.AccountIntroSeen=true;profile.Missions.NextAfterClears=20000;
            ((HashSet<int>)typeof(CascadeGame).GetField("heardStory",Private).GetValue(game)).UnionWith(Enumerable.Range(0,10000));game.ShowHome();yield return null;
        }
        void Resize(int width,int height)
        {
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}
            target=new RenderTexture(width,height,24);view.targetTexture=target;scaler.enabled=true;game.RefreshLayout();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(view!=null)view.targetTexture=oldTarget;if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}CascadeGame.EditorProfileDirectory=null;AudioListener.pause=audio;UnityEngine.Application.runInBackground=background;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        IEnumerator Shot(string name,Action<CascadeGame> pose=null)
        {
            yield return new WaitForSecondsRealtime(.5f);
            yield return (IEnumerator)EditorType("PlayableCapture").GetMethod("CaptureSettled").Invoke(null,new object[]{"FlightPerspectivePauseOct15",name+".png",target.width,target.height,pose});
        }
        IEnumerator Pause()
        {
            game.LoadNode("C02");game.ShowGame();for(int i=0;i<180&&!game.Interaction.CanPlay;i++)yield return null;Assert.That(game.Interaction.CanPlay,Is.True);
            game.SendMessage("TogglePause");yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Pause));
        }
        Color32[] Pixels(string name=null)
        {
            var previous=RenderTexture.active;var copy=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);
            try{Canvas.ForceUpdateCanvases();view.Render();RenderTexture.active=target;copy.ReadPixels(new Rect(0,0,target.width,target.height),0,0);copy.Apply();if(name!=null){Directory.CreateDirectory(Output);File.WriteAllBytes(Path.Combine(Output,name+".png"),copy.EncodeToPNG());}return copy.GetPixels32();}
            finally{RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(copy);}
        }
        void World(int world)=>EditorType("WorldMenusReview").GetMethod("ConfigureWorld").Invoke(null,new object[]{game,world});
        [UnityTest] public IEnumerator PausePavingReachesBothPostsOnPhoneAndTablet()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;
            foreach(var size in new[]{new Vector2Int(320,693),new Vector2Int(390,844),new Vector2Int(720,1280),new Vector2Int(768,1024)})foreach(int world in new[]{0,5,8}){
                Resize(size.x,size.y);World(world);yield return Pause();hash=game.Interaction.Attempt.CurrentStateHash;
                var frame=game.GetComponentsInChildren<Image>().Single(i=>i.name=="World-specific architectural frame");var mat=frame.material;
                Assert.That(mat.GetFloat("_ContinuousLanding"),Is.EqualTo(1));
                var floor=game.GetComponentsInChildren<RawImage>().Single(i=>i.name=="Continuous menu floor");var c=new Vector3[4];floor.rectTransform.GetWorldCorners(c);
                Assert.That(view.WorldToViewportPoint(c[0]).x,Is.LessThan(.001f));Assert.That(view.WorldToViewportPoint(c[2]).x,Is.GreaterThan(.999f));Assert.That(view.WorldToViewportPoint(c[0]).y,Is.LessThan(.001f));
                var continuous=Pixels();mat.SetFloat("_ContinuousLanding",0);var oldStrips=Pixels();mat.SetFloat("_ContinuousLanding",1);int corrected=0;
                for(int y=0;y<size.y/4;y++)for(int x=0;x<size.x;x++){int p=y*size.x+x;if(Math.Abs(continuous[p].r-oldStrips[p].r)+Math.Abs(continuous[p].g-oldStrips[p].g)+Math.Abs(continuous[p].b-oldStrips[p].b)>18)corrected++;}
                Assert.That(corrected,Is.GreaterThan(100),"Removing the legacy frame's paving must change actual pixels at the side seams");
                yield return Shot("after-pause-"+world+"-"+size.x);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
                game.GetComponentsInChildren<Button>().Single(b=>b.name=="Resume").onClick.Invoke();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing));
            }
        }
        [UnityTest] public IEnumerator MenusShareTwoSeparatedAircraftAndNoStationaryDuplicate()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;int maximum=0;float nearest=float.PositiveInfinity;
            for(int world=0;world<9;world++){
                World(world);game.ShowHome();yield return null;var env=game.GetComponentInChildren<MenuWorldEnvironment>();float selected=22,best=-1;
                var aircraft=game.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Distant sky aircraft",StringComparison.Ordinal)||i.name=="Reference home umbrella airship").ToArray();
                for(float t=0;t<280;t+=.5f){
                    env.SetReviewAge(t);var all=aircraft.Where(i=>i.enabled&&i.color.a>.05f).ToArray();Assert.That(all.Length,Is.LessThanOrEqualTo(2));
                    var visible=all.Where(i=>{var p=view.WorldToViewportPoint(i.transform.position);return p.x>0&&p.x<1&&p.y>0&&p.y<1;}).ToArray();
                    maximum=Mathf.Max(maximum,visible.Length);float score=visible.Sum(i=>{float x=view.WorldToViewportPoint(i.transform.position).x;return i.color.a*(x>.10f&&x<.25f||x>.76f&&x<.90f?3:.1f);});if(score>best){best=score;selected=t;}
                    if(visible.Length==2){float gap=Mathf.Abs(view.WorldToViewportPoint(visible[0].transform.position).y-view.WorldToViewportPoint(visible[1].transform.position).y);nearest=Mathf.Min(nearest,gap);Assert.That(gap,Is.GreaterThan(.06f),"Two ships must occupy separate heights, not a cluster");}
                }
                float pose=selected;yield return Shot("after-home-world-"+world,g=>env.SetReviewAge(pose));
                env.SetReviewAge(pose);var on=aircraft.Where(i=>i.enabled).ToArray();var with=Pixels();foreach(var ship in on)ship.enabled=false;var without=Pixels();foreach(var ship in on)ship.enabled=true;int changed=0;
                for(int p=0;p<with.Length;p++)if(Math.Abs(with[p].r-without[p].r)+Math.Abs(with[p].g-without[p].g)+Math.Abs(with[p].b-without[p].b)>18)changed++;
                Assert.That(changed,Is.GreaterThan(20),"World "+world+": scheduled ships must contribute actual visible pixels beside Mika, not be hidden behind controls");
                if(world==0){var home=game.GetComponentInChildren<HomeSkyLife>();home.SetReviewSequence(197);home.SetReviewAge(0);for(int i=0;i<420;i++)home.SetReviewAge(i/30f);yield return Shot("after-bird-insect-perspective",g=>{env.SetReviewAge(22);home.SetReviewAge(12);});}
                game.Preferences.EffectQuality=1;env.SetReviewAge(30);Assert.That(env.VisibleAircraft,Is.LessThanOrEqualTo(1));game.Preferences.EffectQuality=2;
                game.ReducedMotion=true;env.SetReviewAge(30);Assert.That(env.VisibleAircraft,Is.Zero);game.ReducedMotion=false;
            }
            Assert.That(maximum,Is.EqualTo(2));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Directory.CreateDirectory(Output);File.WriteAllText(Path.Combine(Output,"traffic-final.txt"),"Maximum visible aircraft="+maximum+"; minimum vertical viewport gap="+nearest+"\n");
        }
        [UnityTest] public IEnumerator GameplayTrafficUsesTheSameBudgetAndStaysBetweenFrameAndSky()
        {
            game.LoadNode("C02");game.ShowGame();for(int i=0;i<180&&!game.Interaction.CanPlay;i++)yield return null;game.Machine.SetAmbientPaused(true);string hash=game.Interaction.Attempt.CurrentStateHash;
            var world=game.Machine.LivingWorld;int maximum=0;
            for(int zone=0;zone<9;zone++){
                world.SetZone(zone);float selected=0;int best=0;
                for(int i=0;i<360;i++){
                    float t=i*.75f;world.Paint(t);var ships=world.GetComponentsInChildren<MeshRenderer>().Where(r=>r.enabled&&(r.name=="Travelling glider"||r.name=="Glass Lagoon sky skiff"||r.name.StartsWith("Distant travelling aircraft",StringComparison.Ordinal))).ToArray();
                    Assert.That(ships.Length,Is.LessThanOrEqualTo(2));maximum=Mathf.Max(maximum,ships.Length);if(ships.Length>best){selected=t;best=ships.Length;}
                    foreach(var ship in ships){Assert.That(ship.transform.position.z,Is.InRange(2.95f,3.16f));Assert.That(ship.sharedMaterial.GetFloat("_Haze"),Is.InRange(.17f,.47f));}
                }
                float pose=selected;int biome=zone;yield return Shot("after-game-world-"+zone,g=>{world.SetZone(biome);world.Paint(pose);});
            }
            Assert.That(maximum,Is.EqualTo(2));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator RenderedBeeAndBirdKeepTheirPhysicalScaleAtTheSameDepth()
        {
            var asm=typeof(CascadeGame).Assembly;var libraryType=asm.GetType("Cascade.Presentation.WildlifeRigLibrary");var rigType=asm.GetType("Cascade.Presentation.WildlifeRig");
            var library=Activator.CreateInstance(libraryType,new[]{typeof(CascadeGame).GetField("art",Private).GetValue(game)});
            var owner=new GameObject("Temporary physical scale review");owner.layer=31;var camera=new GameObject("Temporary scale camera",typeof(Camera)).GetComponent<Camera>();camera.orthographic=true;camera.orthographicSize=2;camera.transform.position=new Vector3(0,0,-10);camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.12f,.35f,.52f);
            var capture=new RenderTexture(390,844,24);camera.targetTexture=capture;var oldView=view;var oldRender=target;var rigs=new object[2];
            try{
                view=camera;target=capture;
                for(int i=0;i<2;i++){rigs[i]=Activator.CreateInstance(rigType,new object[]{owner.transform,"Measured species "+i,library,null,camera});rigType.GetMethod("Select").Invoke(rigs[i],new object[]{i==0?WildlifeSpecies.Swallow:WildlifeSpecies.Bumblebee});}
                foreach(float depth in new[]{.15f,.5f,.85f}){
                    float[] widths=new float[2];
                    for(int i=0;i<2;i++){
                        var species=i==0?WildlifeSpecies.Swallow:WildlifeSpecies.Bumblebee;float size=WildlifePerspective.BodySize(species,4*camera.aspect,depth);
                        rigType.GetMethod("Fly").Invoke(rigs[i],new object[]{camera,new Vector3(-.8f,0),new Vector3(-.2f,0),new Vector3(.2f,0),new Vector3(.8f,0),1.7f,18f,i==0?4.7f:10.8f,.07f,0f,0,-1f,size,10f,Color.white,.85f,Vector2.zero,-1f});
                        var root=(Transform)rigType.GetField("Root").GetValue(rigs[i]);root.position=new Vector3(i==0?-.4f:.4f,0,0);
                        var renderers=root.GetComponentsInChildren<MeshRenderer>().Where(r=>r.enabled).ToArray();var box=renderers[0].bounds;foreach(var renderer in renderers.Skip(1))box.Encapsulate(renderer.bounds);widths[i]=box.size.x;
                    }
                    Assert.That(widths[1],Is.LessThan(widths[0]*.35f),"The complete painted bee, including wings, must remain smaller than the bird on the same plane");Pixels("same-depth-"+Mathf.RoundToInt(depth*100));
                }
            }finally{
                view=oldView;target=oldRender;foreach(var rig in rigs)if(rig!=null)rigType.GetMethod("Dispose").Invoke(rig,null);libraryType.GetMethod("Dispose").Invoke(library,null);
                UnityEngine.Object.DestroyImmediate(owner);UnityEngine.Object.DestroyImmediate(camera.gameObject);capture.Release();UnityEngine.Object.DestroyImmediate(capture);
            }
            yield return null;
        }
    }
}
#endif
