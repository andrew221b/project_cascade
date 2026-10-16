using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Cascade.Tests
{
    public sealed class AircraftFrameDepthTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;Camera camera;RenderTexture target,oldTarget;string directory;bool background,audio;
        [Serializable] sealed class MenuDepth
        {
            public string screen;public int world;public bool aircraft_behind_frame,aircraft_have_haze,floor_behind_frame;public int opaque_frame_aircraft_leak_pixels,unframed_aircraft_control_pixels;
        }
        [Serializable] sealed class DepthReport{public List<MenuDepth> measurements=new List<MenuDepth>();}
        string Phase=>Environment.GetEnvironmentVariable("CASCADE_DEPTH_REVIEW_PHASE")??"AircraftFrameDepthOct15";
        string Folder=>Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Phase);
        Type EditorType(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable."+name)).First(t=>t!=null);
        T Field<T>(string name)=>(T)typeof(CascadeGame).GetField(name,Private).GetValue(game);
        void Route(GameScreen screen)=>typeof(CascadeGame).GetMethod("SetScreen",Private).Invoke(game,new object[]{screen});
        void World(int world)=>EditorType("WorldMenusReview").GetMethod("ConfigureWorld").Invoke(null,new object[]{game,world});
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;audio=AudioListener.pause;AudioListener.pause=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeAircraftDepth",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            void Preload(string m,string s,LogType t){if(t==LogType.Error&&m.StartsWith("OperationException : Failed to initialize localization, could not preload string tables.",StringComparison.Ordinal))LogAssert.Expect(t,new System.Text.RegularExpressions.Regex("^OperationException : Failed to initialize localization, could not preload string tables\\."));}
            UnityEngine.Application.logMessageReceived+=Preload;
            try{yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;foreach(string locale in new[]{"en","ru","uk"}){var table=UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));yield return table;Assert.That(table.Status,Is.EqualTo(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded));}}
            finally{UnityEngine.Application.logMessageReceived-=Preload;}
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();camera=Camera.main;oldTarget=camera.targetTexture;target=new RenderTexture(390,844,24,RenderTextureFormat.ARGB32);
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;camera.targetTexture=target;scaler.enabled=true;
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Preferences.Tutorials=false;game.Coach.Enabled=false;game.Preferences.EffectQuality=2;game.ReducedMotion=false;game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;
            var p=Field<PlayerProfile>("profile");p.AccountIntroSeen=true;p.Missions.NextAfterClears=20000;p.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();Field<HashSet<int>>("heardStory").UnionWith(Enumerable.Range(0,10000));
            game.SetEditorSafeInsets(new Vector4(.02f,.055f,.02f,.05f));game.SetUiLocale("ru");game.Machine.SetAmbientPaused(true);yield return null;game.RefreshLayout();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(camera!=null)camera.targetTexture=oldTarget;if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}CascadeGame.EditorProfileDirectory=null;AudioListener.pause=audio;UnityEngine.Application.runInBackground=background;if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        static bool Before(Transform a,Transform b)
        {
            var left=new List<Transform>();var right=new List<Transform>();for(var t=a;t!=null;t=t.parent)left.Add(t);for(var t=b;t!=null;t=t.parent)right.Add(t);
            left.Reverse();right.Reverse();int i=0;while(i<Math.Min(left.Count,right.Count)&&left[i]==right[i])i++;
            return i<left.Count&&i<right.Count&&left[i].GetSiblingIndex()<right[i].GetSiblingIndex();
        }
        Color32[] Pixels()
        {
            var render=camera.targetTexture;var previous=RenderTexture.active;var pixels=new Texture2D(render.width,render.height,TextureFormat.RGBA32,false);
            try{Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=render;pixels.ReadPixels(new Rect(0,0,render.width,render.height),0,0);pixels.Apply();return pixels.GetPixels32();}
            finally{RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(pixels);}
        }
        int FrameLeak(Image frame,Image aircraft,out int control)
        {
            var graphics=game.GetComponentsInChildren<Graphic>().Where(g=>g.isActiveAndEnabled&&g!=frame).ToArray();var renderers=game.GetComponentsInChildren<Renderer>().Where(r=>r.enabled).ToArray();
            var masks=aircraft.GetComponentsInParent<RectMask2D>().Where(m=>m.enabled).ToArray();
            var groups=frame.GetComponentsInParent<CanvasGroup>();var alphas=groups.Select(g=>g.alpha).ToArray();foreach(var group in groups)group.alpha=1;
            var oldFlags=camera.clearFlags;var oldColour=camera.backgroundColor;
            Color32[] opaque;
            try{foreach(var graphic in graphics)graphic.enabled=false;foreach(var renderer in renderers)renderer.enabled=false;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;opaque=Pixels();}
            finally{foreach(var graphic in graphics)graphic.enabled=true;foreach(var renderer in renderers)renderer.enabled=true;camera.clearFlags=oldFlags;camera.backgroundColor=oldColour;}
            var rect=aircraft.rectTransform;var position=rect.position;var size=rect.sizeDelta;var scale=rect.localScale;var colour=aircraft.color;bool enabled=aircraft.enabled,maskable=aircraft.maskable;
            try{
                foreach(var mask in masks)mask.enabled=false;
                aircraft.maskable=false;
                rect.position=frame.rectTransform.TransformPoint(frame.rectTransform.rect.min+Vector2.Scale(frame.rectTransform.rect.size,new Vector2(.07f,.97f)));
                rect.sizeDelta=Vector2.one*frame.rectTransform.rect.width*.22f;rect.localScale=Vector3.one;aircraft.enabled=true;aircraft.color=Color.white;
                var with=Pixels();aircraft.enabled=false;var without=Pixels();frame.enabled=false;aircraft.enabled=true;var freeWith=Pixels();aircraft.enabled=false;var freeWithout=Pixels();frame.enabled=true;int leak=0;control=0;
                int width=camera.targetTexture.width,height=camera.targetTexture.height;
                for(int y=1;y<height-1;y++)for(int x=1;x<width-1;x++){
                    int i=y*width+x;bool solid=true;
                    for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)solid&=opaque[i+dy*width+dx].a>=250;
                    // Alpha edges legitimately reveal the sky. Probe the fully
                    // opaque interior, with a one-pixel margin for antialiasing.
                    if(!solid)continue;
                    if(Math.Abs(with[i].r-without[i].r)+Math.Abs(with[i].g-without[i].g)+Math.Abs(with[i].b-without[i].b)>18){
                        var a=((Color)with[i]).linear;var b=((Color)without[i]).linear;
                        // The UI uses SrcAlpha blending for alpha as well as RGB.
                        // Readback alpha is opacity squared. A 254/255 dark leaf
                        // can reveal a bright background, especially in sRGB.
                        float opacity=Mathf.Sqrt(opaque[i].a/255f);
                        if(Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b)>3*(1-opacity)+.01f)leak++;
                    }
                    if(Math.Abs(freeWith[i].r-freeWithout[i].r)+Math.Abs(freeWith[i].g-freeWithout[i].g)+Math.Abs(freeWith[i].b-freeWithout[i].b)>18)control++;
                }
                return leak;
            }finally{frame.enabled=true;rect.position=position;rect.sizeDelta=size;rect.localScale=scale;aircraft.enabled=enabled;aircraft.color=colour;aircraft.maskable=maskable;foreach(var mask in masks)mask.enabled=true;for(int i=0;i<groups.Length;i++)groups[i].alpha=alphas[i];}
        }
        MenuDepth Measure(bool verify)
        {
            var frame=game.GetComponentsInChildren<Image>().Single(i=>i.name=="World-specific architectural frame");var environment=game.GetComponentInChildren<MenuWorldEnvironment>();environment.SetReviewAge(22);
            var aircraft=game.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Distant sky aircraft",StringComparison.Ordinal)||i.name=="Reference home umbrella airship").ToArray();
            bool behind=aircraft.All(a=>Before(a.transform,frame.transform));bool fog=aircraft.All(a=>a.material.HasProperty("_Haze")&&a.material.GetFloat("_Haze")>=.25f);
            var floor=game.GetComponentsInChildren<RawImage>().FirstOrDefault(r=>r.name=="Continuous menu floor");bool floorBehind=floor==null||Before(floor.transform,frame.transform);
            var homeGround=game.GetComponentsInChildren<RawImage>().FirstOrDefault(r=>r.name=="Home continuous reference ground");
            if(verify&&homeGround!=null){
                Assert.That(Before(frame.transform,homeGround.transform),Is.True,"The complete home ground must cover the older side strips");
                Assert.That(homeGround.uvRect,Is.EqualTo(new Rect(0,0,1,1)),"Do not zoom or crop the floor horizontally");
                using(var mesh=new VertexHelper()){
                    homeGround.GetComponent<HomeGroundMesh>().ModifyMesh(mesh);Assert.That(mesh.currentVertCount,Is.EqualTo(12));
                    var lower=new UIVertex();var upper=new UIVertex();mesh.PopulateUIVertex(ref lower,0);mesh.PopulateUIVertex(ref upper,1);
                    Assert.That(upper.position.y-lower.position.y,Is.EqualTo(180*homeGround.rectTransform.rect.width/homeGround.texture.width).Within(.01f),"Paving must retain the frame's source proportions");
                }
                var hero=game.GetComponentsInChildren<Image>().FirstOrDefault(i=>i.name=="Mika welcomes you"||i.name=="Mika waves hello");
                if(hero!=null)Assert.That(Before(homeGround.transform,hero.transform),Is.True,"Mika must stand in front of the continuous platform");
            }
            var probe=aircraft.First(a=>a.name=="Distant sky aircraft 0");int leak=FrameLeak(frame,probe,out int control);
            if(verify){Assert.That(behind,Is.True,"Every menu aircraft must render behind the painted frame");Assert.That(fog,Is.True,"Aircraft must share the far islands' sky haze");Assert.That(floorBehind,Is.True,"Opaque floor must not cut off frame foliage");Assert.That(control,Is.GreaterThan(20),"The forced aircraft must cross actual painted frame pixels");Assert.That(leak,Is.LessThan(6),"Aircraft changes pixels on opaque frame decoration");}
            return new MenuDepth {screen=game.CurrentScreen.ToString(),world=environment.World,aircraft_behind_frame=behind,aircraft_have_haze=fog,floor_behind_frame=floorBehind,opaque_frame_aircraft_leak_pixels=leak,unframed_aircraft_control_pixels=control};
        }
        IEnumerator Shot(string name,Action<CascadeGame> pose=null,int width=390,int height=844)
        {
            yield return new WaitForSecondsRealtime(.45f);
            yield return (IEnumerator)EditorType("PlayableCapture").GetMethod("CaptureSettled").Invoke(null,new object[]{Phase,name+".png",width,height,pose});
        }
        [UnityTest] public IEnumerator CaptureAircraftAndLandingEdgesInEveryWorld()
        {
            var metrics=new DepthReport();string hash=game.Interaction.Attempt.CurrentStateHash;
            for(int world=0;world<9;world++){
                World(world);Route(GameScreen.JourneyMap);yield return Shot("world-"+world+"-map",g=>metrics.measurements.Add(Measure(false)));
                game.ShowHome();yield return Shot("world-"+world+"-home",g=>metrics.measurements.Add(Measure(false)));
                game.ShowShop();yield return Shot("world-"+world+"-shop",g=>metrics.measurements.Add(Measure(false)));
            }
            World(0);game.ShowAccountGate();yield return Shot("welcome",g=>metrics.measurements.Add(Measure(false)));
            Directory.CreateDirectory(Folder);File.WriteAllText(Path.Combine(Folder,"menu-depth.json"),JsonUtility.ToJson(metrics,true)+"\n");Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator AircraftAndGroundCannotPaintOverFramesOrChangeTheAttempt()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;
            for(int world=0;world<9;world++){
                World(world);
                foreach(var screen in new[]{GameScreen.JourneyMap,GameScreen.Home,GameScreen.Shop}){Route(screen);yield return null;Measure(true);}
            }
            World(0);game.ShowAccountGate();yield return null;Measure(true);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator GameplayAircraftStayBehindTheFrameAtIslandScaleAndHaze()
        {
            game.LoadNode("C02");game.ShowGame();for(int i=0;i<180&&!game.Interaction.CanPlay;i++)yield return null;Assert.That(game.Interaction.CanPlay,Is.True);
            string hash=game.Interaction.Attempt.CurrentStateHash;var world=game.Machine.LivingWorld;
            for(int zone=0;zone<9;zone++){
                int biome=zone;
                Action<CascadeGame> pose=g=>{
                    world.SetZone(biome);g.RenderFrame(g.VisibleFrame);world.Paint(22);
                    var frame=world.GetComponentsInChildren<MeshRenderer>(true).Single(r=>r.name=="Thematic level framing");float width=camera.orthographicSize*2*camera.aspect;
                    for(int i=0;i<8;i++){
                        world.Paint(i*15.3f);
                        foreach(var craft in world.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name=="Travelling glider"||r.name=="Glass Lagoon sky skiff"||r.name.StartsWith("Distant travelling aircraft",StringComparison.Ordinal))){
                            Assert.That(craft.bounds.min.z,Is.GreaterThan(frame.transform.position.z+.05f),craft.name+" world "+biome);
                            Assert.That(craft.sharedMaterial.shader.name,Is.EqualTo("Cascade/WorldIslandSurface"));
                            Assert.That(craft.sharedMaterial.GetFloat("_Haze"),Is.InRange(biome==1?.25f:.17f,biome==1?.46f:.40f));
                            Assert.That(craft.bounds.size.x,Is.LessThan(width*.14f),craft.name+" dominates distant islands");
                        }
                    }
                    world.Paint(22);
                };
                yield return Shot("world-"+zone+"-gameplay",pose);
            }
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator RootedFlowersKeepTheirStemsOnPaintedFrameLeaves()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;
            foreach(int world in new[]{0,5,8}){
                World(world);
                foreach(var screen in new[]{GameScreen.Home,GameScreen.JourneyMap,GameScreen.Shop}){
                    Route(screen);yield return null;var environment=game.GetComponentInChildren<MenuWorldEnvironment>();environment.SetReviewAge(10);
                    var frame=game.GetComponentsInChildren<Image>().Single(i=>i.name=="World-specific architectural frame");
                    var plants=game.GetComponentInChildren<MenuSeasonalFlecks>();Assert.That(plants.transform.IsChildOf(frame.transform),Is.True);
                    var flowers=plants.GetComponentsInChildren<Image>().Where(i=>i.enabled).ToArray();Assert.That(flowers.Length,Is.EqualTo(4));
                    var stems=flowers.Select(f=>f.rectTransform.position).ToArray();var turns=flowers.Select(f=>f.rectTransform.localRotation).ToArray();
                    var graphics=game.GetComponentsInChildren<Graphic>().Where(g=>g.isActiveAndEnabled&&g!=frame).ToArray();var renderers=game.GetComponentsInChildren<Renderer>().Where(r=>r.enabled).ToArray();
                    var flags=camera.clearFlags;var colour=camera.backgroundColor;var groups=frame.GetComponentsInParent<CanvasGroup>();var alpha=groups.Select(g=>g.alpha).ToArray();Color32[] mask;
                    try{foreach(var g in graphics)g.enabled=false;foreach(var r in renderers)r.enabled=false;foreach(var g in groups)g.alpha=1;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;mask=Pixels();}
                    finally{foreach(var g in graphics)g.enabled=true;foreach(var r in renderers)r.enabled=true;for(int i=0;i<groups.Length;i++)groups[i].alpha=alpha[i];camera.clearFlags=flags;camera.backgroundColor=colour;}
                    environment.SetReviewAge(29.13f);
                    for(int i=0;i<flowers.Length;i++){
                        Assert.That(Vector3.Distance(stems[i],flowers[i].rectTransform.position),Is.LessThan(.001f),"A swaying flower cannot drift away from its stem");
                        Assert.That(Quaternion.Angle(turns[i],flowers[i].rectTransform.localRotation),Is.GreaterThan(.01f));
                        var p=RectTransformUtility.WorldToScreenPoint(camera,stems[i]);int x=Mathf.Clamp(Mathf.RoundToInt(p.x),0,camera.targetTexture.width-1),y=Mathf.Clamp(Mathf.RoundToInt(p.y),0,camera.targetTexture.height-1);
                        Assert.That(mask[y*camera.targetTexture.width+x].a,Is.GreaterThan(180),"The stem must touch actual painted frame pixels, not sky");
                        Assert.That(flowers[i].raycastTarget,Is.False);
                    }
                    yield return Shot("rooted-"+world+"-"+screen.ToString().ToLowerInvariant());
                }
            }
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            game.LoadNode("C02");game.ShowGame();yield return null;
            var scenery=game.Machine.LivingWorld;var weather=scenery.GetComponentInChildren<WorldSeasonalWeather>();
            foreach(int world in new[]{0,5,8}){
                scenery.SetZone(world);scenery.Paint(10);var mesh=weather.GetComponent<MeshFilter>().sharedMesh;var first=mesh.vertices;
                var pivot=WorldSeasonalArt.Stem(world);int start=WorldSeasonalArt.Procedural(world);scenery.Paint(29.13f);var second=mesh.vertices;
                for(int i=start;i<start+4;i++){
                    Vector3 Stem(Vector3[] vertices)=>vertices[i*4]+(vertices[i*4+1]-vertices[i*4])*pivot.x+(vertices[i*4+3]-vertices[i*4])*pivot.y;
                    Assert.That(Vector3.Distance(Stem(first),Stem(second)),Is.LessThan(.0001f),"Gameplay flowers must rotate about a fixed painted support");
                }
            }
        }
        [Test] public void AllAircraftStylesTravelSmoothlyAndFaceTheirDirection()
        {
            float largestStep=0,largestTurn=0,largestScaleStep=0;int visibleSamples=0;
            for(int world=0;world<9;world++)for(int kind=0;kind<6;kind++)for(float age=0;age<360;age+=.125f){
                var a=SkyFlightMotion.Sample(age,kind,world);var b=SkyFlightMotion.Sample(age+.016f,kind,world);
                if(a.Alpha<.05f||b.Alpha<.05f)continue;visibleSamples++;
                largestStep=Mathf.Max(largestStep,Vector2.Distance(a.Position,b.Position));largestTurn=Mathf.Max(largestTurn,Mathf.Abs(a.Roll-b.Roll));largestScaleStep=Mathf.Max(largestScaleStep,Mathf.Abs(a.Scale-b.Scale));
                Assert.That(Mathf.Sign(b.Position.x-a.Position.x),Is.EqualTo(a.Facing));
            }
            Assert.That(visibleSamples,Is.GreaterThan(100000));Assert.That(largestStep,Is.LessThan(.001f));Assert.That(largestTurn,Is.LessThan(.05f));Assert.That(largestScaleStep,Is.LessThan(.001f));
        }
        [UnityTest] public IEnumerator PublishedRecipeStagesFitThePhoneAndReplayToTheirTargets()
        {
            var store=new PublishedTriadStore(Resources.Load<TextAsset>("CascadeContent/production_triad_recipes").text,Resources.Load<TextAsset>("CascadeContent/production_triad_manifest").text);
            foreach(string node in new[]{"C197","C198","C209","C219","C235","C288","C309","C424","C512"}){
                var content=game.Catalog.GetPrimary(node);var source=content.Definition;var run=store.Bind(source,content.Locale,game.Catalog.ContentHash);
                for(int stage=0;stage<3;stage++){
                    var attempt=new AttemptService(run.Level(source,stage),content.Locale);
                    typeof(CascadeGame).GetMethod("MountAttempt",Private).Invoke(game,new object[]{attempt,run});game.ShowGame();
                    for(int i=0;i<180&&!game.Interaction.CanPlay;i++)yield return null;Assert.That(game.Interaction.CanPlay,Is.True,node);
                    int currentStage=stage;
                    yield return Shot("recipe-"+node+"-stage-"+(stage+1),g=>{
                        string number=(string)typeof(CascadeGame).GetMethod("DisplayLevelNumber",Private).Invoke(g,new object[]{node});
                        var label=Field<TextMeshProUGUI>("levelLabel");
                        Assert.That(label.GetComponent<LocalizedCaption>().SourceIdentity,Is.EqualTo(new UiCopy("Level {0}, stage {1}/3",number,currentStage+1).English),"The captured chapter must identify its actual level and stage");
                        Assert.That(label.text,Is.EqualTo(new UiCopy("Level {0}, stage {1}/3",number,currentStage+1).ToString()),"Localized chapter text must update with its arguments");
                    });
                    foreach(string text in run.Stages[stage].Witness){var command=text=="D"?PuzzleCommand.Drop:text.StartsWith("R",StringComparison.Ordinal)?PuzzleCommand.Reach(int.Parse(text.Substring(1).Split(':')[0]),int.Parse(text.Split(':')[1])):PuzzleCommand.Swap(int.Parse(text));Assert.That(attempt.Execute(command).Accepted,Is.True);}
                    Assert.That(attempt.Current.Status,Is.EqualTo(PuzzleStatus.Won));Assert.That(attempt.Current.Row.SequenceEqual(source.TargetSymbolIds),Is.True);
                    if(stage<2)run.AcceptCheckpoint(source,attempt);
                }
            }
        }
        [UnityTest] public IEnumerator CaptureContinuousFlightAndStemMotionOnThePhone()
        {
            game.LoadNode("C02");game.ShowGame();yield return new WaitForSecondsRealtime(.6f);game.Machine.LivingWorld.SetZone(0);game.Machine.SetAmbientPaused(true);
            string hash=game.Interaction.Attempt.CurrentStateHash;
            Directory.CreateDirectory(Folder);var pixels=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
            void Capture(string name){
                var active=RenderTexture.active;
                try{Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,target.width,target.height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(Folder,name+".png"),pixels.EncodeToPNG());}
                finally{RenderTexture.active=active;}
            }
            try{
                for(int frame=0;frame<72;frame++){yield return null;game.Machine.LivingWorld.Paint(18+frame/12f);Capture("motion-game-"+frame.ToString("D3"));}
                World(8);game.ShowHome();yield return new WaitForSecondsRealtime(.6f);game.Machine.SetAmbientPaused(true);var environment=game.GetComponentInChildren<MenuWorldEnvironment>();
                var buttons=game.GetComponentsInChildren<Button>().Select(b=>(RectTransform)b.transform).ToArray();var positions=buttons.Select(b=>b.position).ToArray();
                for(int frame=0;frame<72;frame++){
                    yield return null;float age=18+frame/12f;environment.SetReviewAge(age);typeof(ToyMachineView).GetField("windTime",Private).SetValue(game.Machine,age);
                    foreach(var motion in game.GetComponentsInChildren<WorldMotion>())motion.SendMessage("LateUpdate");Capture("motion-menu-"+frame.ToString("D3"));
                    for(int i=0;i<buttons.Length;i++)Assert.That(Vector3.Distance(buttons[i].position,positions[i]),Is.LessThan(.001f),"Aircraft motion cannot move menu controls");
                }
            }finally{UnityEngine.Object.Destroy(pixels);}
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
    }
}
