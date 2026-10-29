#if UNITY_EDITOR
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
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Cascade.Tests
{
    public sealed class DescentTransitionTests
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;Camera camera;RenderTexture target,oldTarget;string directory;bool background,oldAudio;
        T Field<T>(string key)=>(T)typeof(CascadeGame).GetField(key,Flags).GetValue(game);
        string Output=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/DescentMotionOct16"));
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;oldAudio=AudioListener.pause;AudioListener.pause=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeDescentMotion",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();camera=Camera.main;oldTarget=camera.targetTexture;
            Resize(390,844);game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Preferences.Tutorials=false;game.Coach.Enabled=false;game.ReducedMotion=true;
            var profile=Field<PlayerProfile>("profile");profile.AccountIntroSeen=true;profile.Missions.NextAfterClears=20000;
            profile.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();
            Field<HashSet<int>>("heardStory").UnionWith(Enumerable.Range(0,9500));
            game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;
            yield return Checkpoint("C03");
        }
        void Resize(int width,int height)
        {
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;
            if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}
            target=new RenderTexture(width,height,24);camera.targetTexture=target;scaler.enabled=true;game.RefreshLayout();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            foreach(var ghost in UnityEngine.Object.FindObjectsByType<DescentGhost>(FindObjectsSortMode.None))UnityEngine.Object.Destroy(ghost.gameObject);
            if(camera!=null)camera.targetTexture=oldTarget;
            if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}CascadeGame.EditorProfileDirectory=null;
            UnityEngine.Application.runInBackground=background;AudioListener.pause=oldAudio;if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        static IEnumerator Await(Func<bool> ready,string message)
        {
            double start=Time.realtimeSinceStartupAsDouble;
            while(!ready()&&Time.realtimeSinceStartupAsDouble-start<30)yield return null;
            Assert.That(ready(),Is.True,message);
        }
        IEnumerator Ready()=>Await(()=>!game.IsPlanningDescent&&!game.IsDescending&&game.PreparationReady&&!game.Interaction.IsPresenting,"Descent must settle");
        IEnumerator Checkpoint(string node)
        {
            game.ReducedMotion=true;game.StartDescentLevel(game.Catalog.GetPrimary(node).Definition);game.ShowGame();yield return Ready();
            yield return SolveStage();
        }
        IEnumerator SolveStage()
        {
            yield return Ready();
            foreach(string encoded in game.Descent.Stages[game.Descent.StageIndex].Witness){
                var command=encoded=="D"?PuzzleCommand.Drop:encoded.StartsWith("R")?PuzzleCommand.Reach(int.Parse(encoded.Substring(1).Split(':')[0]),int.Parse(encoded.Split(':')[1])):PuzzleCommand.Swap(int.Parse(encoded));
                Assert.That(game.Interaction.Execute(command)?.Accepted,Is.True,"Witness "+encoded);
                yield return Await(()=>!game.Interaction.IsPresenting,"Command must finish");yield return null;
            }
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Checkpoint));
        }
        DescentTransition Begin()
        {
            game.ReducedMotion=false;game.GoLower();Assert.That(game.IsDescending,Is.True,"Actual GoLower must start travel");
            return game.GetComponent<DescentTransition>();
        }
        DescentGhost Ghost(string name)=>UnityEngine.Object.FindObjectsByType<DescentGhost>(FindObjectsSortMode.None).Single(g=>g.name==name);
        void CommittedStageAndDurableHistory(int stage)
        {
            var saved=new ProfileStore(game.ProfilePath).Load().Profile;
            // Runtime deliberately keeps unfinished boards in memory only (October28 policy).
            // The transition still commits its checkpoint before travel and durably consumes its words/shapes.
            var live=Field<PlayerProfile>("profile");Assert.That(live.Descent.StageIndex,Is.EqualTo(stage));
            Assert.That(saved.Descent,Is.Null);Assert.That(saved.Attempt,Is.Null);Assert.That(saved.Clears,Is.Empty);Assert.That(saved.Rewards.Records,Is.Empty);
            Assert.That(saved.Variety.Seed,Is.EqualTo(live.Variety.Seed));Assert.That(saved.Variety.Draws,Is.EquivalentTo(live.Variety.Draws));Assert.That(saved.Variety.UsedWords,Is.EquivalentTo(live.Variety.UsedWords));
            Assert.That(live.RestoreAttempt(game.Catalog,variety:Field<StageVarietyStore>("stageVariety")).CurrentStateHash,Is.EqualTo(game.Interaction.Attempt.CurrentStateHash));
        }
        [UnityTest] public IEnumerator SnapshotsKeepBothBraidedRopesVisiblePropertiesAndDisabledParts()
        {
            var source=new GameObject("snapshot probe",typeof(MeshFilter),typeof(MeshRenderer));
            source.GetComponent<MeshFilter>().sharedMesh=game.Machine.GetComponentsInChildren<MeshFilter>().First(f=>f.sharedMesh!=null).sharedMesh;
            var material=new Material(Shader.Find("UI/Default"));var renderer=source.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;
            var properties=new MaterialPropertyBlock();properties.SetFloat("_DescentSnapshotProbe",.37f);renderer.SetPropertyBlock(properties);
            var probe=new GameObject("Snapshot probe owner").AddComponent<DescentGhost>();probe.Capture(renderer,camera);
            var copy=probe.GetComponentInChildren<MeshRenderer>();var copied=new MaterialPropertyBlock();copy.GetPropertyBlock(copied);
            Assert.That(copied.GetFloat("_DescentSnapshotProbe"),Is.EqualTo(.37f));Assert.That(copy.sharedMaterial,Is.Not.SameAs(material));
            renderer.enabled=false;probe.Capture(renderer,camera);Assert.That(probe.GetComponentsInChildren<MeshRenderer>().Length,Is.EqualTo(1));
            UnityEngine.Object.Destroy(source);UnityEngine.Object.Destroy(material);yield return null;Assert.That(copy.sharedMaterial,Is.Not.Null);
            UnityEngine.Object.Destroy(probe.gameObject);
            var motion=Begin();motion.SetPreview(.5f);yield return null;
            foreach(var ghost in new[]{Ghost("Departing wooden rig"),Ghost("Arriving wooden rig")}){
                var ropes=ghost.GetComponentsInChildren<MeshFilter>().Where(f=>f.name.EndsWith("braided rope")).ToArray();
                Assert.That(ropes.Length,Is.EqualTo(2));Assert.That(ropes.All(r=>r.sharedMesh.vertexCount>20),Is.True);
                Assert.That(ghost.GetComponentsInChildren<SpriteRenderer>().Any(r=>r.name=="Cosmetic receiver rim"),Is.True,"The sliced receiver frame cannot pop in after travel");
                Assert.That(ghost.GetComponentsInChildren<MeshRenderer>().Any(r=>r.sharedMaterial.shader.name=="TextMeshPro/Sprite"),Is.True,"Authored numbers retain their sprite submesh");
                if(ghost.name.StartsWith("Arriving"))Assert.That(ghost.GetComponentsInChildren<MeshFilter>().Any(f=>f.name.StartsWith("Score digit")),Is.True,"The wood strength number travels with its plaque");
            }
            var bridges=Ghost("Departing wooden rig").GetComponentsInChildren<LineRenderer>();Assert.That(bridges.Length,Is.EqualTo(2));
            Assert.That(Ghost("Departing wooden rig").GetComponentsInChildren<MeshFilter>().Any(f=>f.name.ToLowerInvariant().Contains("ribbon")),Is.True,"Checkpoint cloth travels with its rig");
            CommittedStageAndDurableHistory(1);motion.Settle();
        }
        [UnityTest] public IEnumerator ContinuousTravelHoldsLivePoseAndMatchesItExactlyAtArrival()
        {
            var motion=Begin();motion.SetPreview(0);yield return null;string hash=game.Interaction.Attempt.CurrentStateHash;
            var old=Ghost("Departing wooden rig");var next=Ghost("Arriving wooden rig");float height=camera.orthographicSize*2,previous=-1;
            var live=game.Machine.transform.Find("Toy machine");Assert.That(live.gameObject.activeSelf,Is.False);
            var livePart=live.GetComponentsInChildren<MeshFilter>(true).First(f=>f.sharedMesh!=null&&f.GetComponent<MeshRenderer>().enabled);
            Vector3 rest=livePart.transform.position;
            foreach(float p in new[]{0f,.02f,.1f,.25f,.5f,.75f,.9f,.98f,1f}){
                motion.SetPreview(p);yield return null;
                float travel=old.transform.position.y;
                Assert.That(travel,Is.GreaterThanOrEqualTo(previous));previous=travel;
                Assert.That(travel-next.transform.position.y,Is.EqualTo(height).Within(.0001f));
                Assert.That(game.Interaction.CanControl,Is.False);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
                Assert.That(Vector3.Distance(livePart.transform.position,rest),Is.LessThan(.0001f),"Hidden spring pose cannot drift under the snapshot");
            }
            Assert.That(old.transform.position.y,Is.EqualTo(height).Within(.0001f));Assert.That(next.transform.position,Is.EqualTo(Vector3.zero));
            motion.Settle();Assert.That(live.gameObject.activeSelf,Is.True);Assert.That(Vector3.Distance(livePart.transform.position,rest),Is.LessThan(.0001f));
            Assert.That(game.Machine.LivingWorld.DescentTravelling,Is.False);CommittedStageAndDurableHistory(1);yield return Ready();
        }
        Color32[] ReadPixels(Rect area)
        {
            Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;
            var image=new Texture2D((int)area.width,(int)area.height,TextureFormat.RGB24,false);image.ReadPixels(area,0,0);image.Apply();
            var result=image.GetPixels32();RenderTexture.active=previous;UnityEngine.Object.Destroy(image);return result;
        }
        [UnityTest] public IEnumerator StrengthDigitsStayPaintedAboveTheirPlaqueAtTheLiveHandoff()
        {
            var motion=Begin();motion.SetPreview(1);yield return null;
            var digit=Field<RectTransform>("floorArea").GetComponentsInChildren<Image>(true).First(i=>i.name.StartsWith("Score digit")&&i.gameObject.activeSelf&&i.sprite!=null);
            var corners=new Vector3[4];digit.rectTransform.GetWorldCorners(corners);
            var low=camera.WorldToScreenPoint(corners[0]);var high=camera.WorldToScreenPoint(corners[2]);
            var area=new Rect(Mathf.Floor(low.x),Mathf.Floor(low.y),Mathf.Ceil(high.x)-Mathf.Floor(low.x),Mathf.Ceil(high.y)-Mathf.Floor(low.y));
            Assert.That(area.width*area.height,Is.GreaterThan(30));
            File.WriteAllLines(Path.Combine(Output,"digit-geometry.txt"),Ghost("Arriving wooden rig").GetComponentsInChildren<MeshFilter>().Where(f=>f.name.StartsWith("Score digit")||f.name.Contains("plaque")).Select(f=>{
                var m=f.sharedMesh;var r=f.GetComponent<Renderer>();var mat=r.sharedMaterial;
                return f.name+" world="+f.transform.position+" scale="+f.transform.lossyScale+" bounds="+m.bounds+" vertices="+m.vertexCount+" opacity="+m.colors32.Min(c=>c.a)+".."+m.colors32.Max(c=>c.a)+" tex="+mat.mainTexture.name+" uv="+string.Join(";",m.uv.Select(v=>v.ToString()));
            }));
            var before=ReadPixels(area);Capture(Path.Combine(Output,"digit-snapshot.png"));motion.Settle();game.Machine.HoldDescentPose(true);
            yield return null;var after=ReadPixels(area);Capture(Path.Combine(Output,"digit-live.png"));
            double error=0;for(int i=0;i<before.Length;i++)error+=Math.Abs(before[i].r-after[i].r)+Math.Abs(before[i].g-after[i].g)+Math.Abs(before[i].b-after[i].b);
            error/=before.Length*3;File.WriteAllText(Path.Combine(Output,"digit-handoff-error.txt"),error.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Assert.That(error,Is.LessThan(16),"The authored strength digit cannot vanish behind the plaque in the moving snapshot");game.Machine.HoldDescentPose(false);
        }
        [UnityTest] public IEnumerator RealTimeDescentFinishesOnceAndReleasesInputWithoutChangingTheRow()
        {
            var motion=Begin();double start=Time.realtimeSinceStartupAsDouble;string hash=game.Interaction.Attempt.CurrentStateHash;
            game.GoLower();Assert.That(game.Descent.StageIndex,Is.EqualTo(1));yield return Ready();
            yield return Await(()=>game.Interaction.CanPlay,"Input must be released after travel");
            Assert.That(Time.realtimeSinceStartupAsDouble-start,Is.InRange(.95,2.8));Assert.That(motion.Progress,Is.EqualTo(1));
            Assert.That(game.Interaction.CanPlay,Is.True);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));CommittedStageAndDurableHistory(1);
            yield return null;Assert.That(UnityEngine.Object.FindObjectsByType<DescentGhost>(FindObjectsSortMode.None),Is.Empty);
        }
        [UnityTest] public IEnumerator FocusPauseReducedMotionAndNavigationAlwaysRestoreTheCommittedStage()
        {
            for(int interruption=0;interruption<4;interruption++){
                if(interruption>0)yield return Checkpoint("C03");
                var motion=Begin();motion.SetPreview(.43f);yield return null;
                if(interruption==0)game.SendMessage("OnApplicationFocus",false);
                else if(interruption==1)game.SendMessage("OnApplicationPause",true);
                else if(interruption==2)game.ReducedMotion=true;
                else game.ShowHome();
                yield return null;Assert.That(game.IsDescending,Is.False);Assert.That(game.Machine.LivingWorld.DescentTravelling,Is.False);CommittedStageAndDurableHistory(1);
                yield return null;Assert.That(UnityEngine.Object.FindObjectsByType<DescentGhost>(FindObjectsSortMode.None),Is.Empty);
                game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.ShowGame();yield return Ready();
                Assert.That(game.Interaction.CanPlay,Is.True);Assert.That(Field<RectTransform>("header").GetComponent<CanvasGroup>().alpha,Is.EqualTo(1));
            }
        }
        [UnityTest] public IEnumerator NewStageSnapshotUsesTheSelectedLanguageBeforeItsAsyncCaptionCallback()
        {
            foreach(string locale in new[]{"ru","uk"}){
                game.SetUiLocale(locale);
                yield return UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));
                yield return null;
                if(game.Descent.StageIndex>0)yield return Checkpoint("C03");
                var motion=Begin();motion.SetPreview(0);
                Assert.That(Field<TMPro.TextMeshProUGUI>("receiverLabel").text,Is.EqualTo(new UiCopy("Make this row").ToString()),"Snapshots must not freeze the temporary English caption");
                motion.Settle();yield return Ready();
            }
        }
        void Capture(string name)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(name));Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;
            var pixels=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,target.width,target.height),0,0);pixels.Apply();
            File.WriteAllBytes(name,name.EndsWith(".png")?pixels.EncodeToPNG():pixels.EncodeToJPG(88));RenderTexture.active=previous;UnityEngine.Object.Destroy(pixels);
        }
        [UnityTest,Timeout(300000)] public IEnumerator CaptureBothStageTransitionsAndLocalizedCompactPhone()
        {
            int frame=0;game.SetUiLocale("ru");yield return null;yield return null;
            for(int stage=1;stage<=2;stage++){
                Capture(Path.Combine(Output,"stage-"+stage+"-checkpoint.png"));
                var motion=Begin();motion.SetPreview(0);yield return null;
                for(int i=0;i<=36;i++){
                    motion.SetPreview(i/36f);yield return null;Capture(Path.Combine(Output,"frames",(frame++).ToString("D4")+".jpg"));
                    if(i%9==0)Capture(Path.Combine(Output,"stage-"+stage+"-travel-"+i+".png"));
                }
                motion.Settle();yield return Ready();yield return Await(()=>game.Interaction.CanPlay,"Arriving controls must finish preparing");Capture(Path.Combine(Output,"stage-"+stage+"-arrived.png"));
                for(int i=0;i<18;i++){yield return null;Capture(Path.Combine(Output,"frames",(frame++).ToString("D4")+".jpg"));}
                if(stage==1){game.ReducedMotion=true;yield return SolveStage();}
            }
            foreach(string locale in new[]{"en","ru","uk"}){
                Resize(320,693);game.SetUiLocale(locale);yield return Checkpoint("C483");
                var motion=Begin();motion.SetPreview(.82f);yield return null;Capture(Path.Combine(Output,"compact-stars-"+locale+".png"));motion.Settle();yield return Ready();
            }
        }
    }
}
#endif
