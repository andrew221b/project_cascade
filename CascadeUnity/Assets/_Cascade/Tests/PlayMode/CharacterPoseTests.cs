#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Cascade.Tests
{
    public sealed partial class AccountReferenceTests
    {
        const string CharacterReport="CharacterArmAttachmentOct16";
        string CharacterFolder=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports",CharacterReport));
        sealed class ActorArt{public string Name;public Sprite Sprite;public bool Cutout;public ActorArt(string name,Sprite sprite,bool cutout=false){Name=name;Sprite=sprite;Cutout=cutout;}}
        IEnumerable<ActorArt> ActorSprites(bool costumes=true,bool residents=true)
        {
            Sprite From(string method,params object[] args)=>(Sprite)typeof(CascadeGame).GetMethod(method,Private).Invoke(game,args);
            yield return new ActorArt("home",AccountReferenceArt.Get("HomeMika"));
            yield return new ActorArt("inventory",Resources.Load<Sprite>("CascadeArt/ShopReferenceOct13/InventoryMika"));
            yield return new ActorArt("apple",Resources.Load<Sprite>("CascadeArt/PauseReferenceOct14/RestingMika"));
            yield return new ActorArt("sad",FailureReferenceArt.Hero(),true);
            yield return new ActorArt("victory",From("VictoryMika"));
            yield return new ActorArt("hopeful",Resources.Load<Sprite>("CascadeArt/Journey/Mika"));
            yield return new ActorArt("thinking",Resources.Load<Sprite>("CascadeArt/Journey/MikaThinking"));
            yield return new ActorArt("collection",Resources.Load<Sprite>("CascadeArt/HomeOct13/Mika"));
            if(residents)foreach(string name in new[]{"Mika","Pip","Rue","Aster","Bram","Sora","Neri","Toma","Lumi","Gran"})yield return new ActorArt("speaker-"+name,From("ConversationPortrait",name));
            if(costumes)foreach(string id in new[]{"sky","moon","rain_badge","sky_pilot","moth_mask","garden_festival","cloud_gardener","explorer_hat","frog_suit","star_knight","fox_mask","mushroom_cap","bee_suit","winter_hood","pirate_hat","jester_suit","constellation_mage"}){
                yield return new ActorArt(id+"-hopeful",From("MikaCostume",id,false,false));yield return new ActorArt(id+"-thinking",From("MikaCostume",id,true,false));
            }
        }
        [Test] public void CharacterAllArtKeepsPositiveSkinAndVisibleSecondaryMotion()
        {
            var rows=new List<string>{"pose,minimum_triangle_area_ratio,tail_arc_source_px,lantern_arc_degrees"};var folds=new List<string>();
            foreach(var art in ActorSprites()){
                Assert.That(art.Sprite,Is.Not.Null,art.Name);var p=CharacterPoseProfile.For(art.Sprite);Assert.That(p,Is.Not.Null,art.Name);
                const int columns=36,lines=54;var points=new CharacterPoseMotion.Binding[(columns+1)*(lines+1)];var posed=new Vector2[points.Length];
                for(int y=0;y<=lines;y++)for(int x=0;x<=columns;x++)points[y*(columns+1)+x]=new CharacterPoseMotion.Binding(new Vector2(x/(float)columns,y/(float)lines),p);
                var motion=new CharacterPoseMotion(p,197);float worst=1,tailMin=100,tailMax=-100,lampMin=100,lampMax=-100;string worstDetail="";
                for(int frame=0;frame<42*30;frame++){
                    motion.Advance(1f/30,WorkshopWind.Sample(frame/30f),Mathf.Max(0,Mathf.Sin(frame*.47f))*.7f);
                    tailMin=Mathf.Min(tailMin,motion.TailAngle);tailMax=Mathf.Max(tailMax,motion.TailAngle);lampMin=Mathf.Min(lampMin,motion.LanternAngle);lampMax=Mathf.Max(lampMax,motion.LanternAngle);
                    if(frame%6!=0)continue;
                    for(int i=0;i<points.Length;i++)posed[i]=motion.Deform(points[i]);
                    for(int y=0;y<lines;y++)for(int x=0;x<columns;x++){
                        int a=y*(columns+1)+x,b=a+columns+1;float area=1f/(columns*lines);
                        float first=Cross(posed[a+1]-posed[a],posed[b]-posed[a])/area,second=Cross(posed[b]-posed[b+1],posed[a+1]-posed[b+1])/area;
                        if(first<worst||second<worst){worst=Mathf.Min(first,second);var point=points[a];worstDetail=art.Name+" @ "+motion.Age+" source "+point.Source+"; head "+point.Head+", cloth "+point.Scarf+", lamp "+point.Lantern+", bag "+point.Bag;}
                    }
                }
                rows.Add(art.Name+","+worst.ToString("F5",System.Globalization.CultureInfo.InvariantCulture)+","+((tailMax-tailMin)*p.Cell.height*Mathf.Deg2Rad*.2f).ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+","+(lampMax-lampMin).ToString("F3",System.Globalization.CultureInfo.InvariantCulture));
                Directory.CreateDirectory(CharacterFolder);File.WriteAllLines(Path.Combine(CharacterFolder,"geometry.csv"),rows);
                if(worst<=.15f)folds.Add(worst.ToString("F4")+": "+worstDetail);
                if(p.Tail.z>0)Assert.That(tailMax-tailMin,Is.GreaterThan(9),art.Name+" tail must remain noticeably alive");
            }
            Assert.That(folds,Is.Empty,"Skin must not fold or pinch: "+string.Join("; ",folds));
        }
        static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        [Test] public void CharacterDetailsVaryNaturallyAndResetWithoutExtraRenderParts()
        {
            var rows=new List<string>{"pose,ear_span_degrees,accessory_span_degrees,tail_tip_span_degrees,expression_span,sniff_peak"};
            foreach(var art in ActorSprites()){
                var profile=CharacterPoseProfile.For(art.Sprite);var motion=new CharacterPoseMotion(profile,197);
                float earMin=100,earMax=-100,accentMin=100,accentMax=-100,tailMin=100,tailMax=-100,expressionMin=100,expressionMax=-100,sniffPeak=0,maxStep=0;
                Vector4 previous=Vector4.zero;
                for(int i=0;i<30*120;i++){
                    motion.Advance(1f/120,WorkshopWind.Sample(i/120f));
                    var values=new Vector4(motion.NearEarAngle,motion.AccentAngle,motion.Expression,motion.Sniff);
                    maxStep=Mathf.Max(maxStep,(values-previous).magnitude);previous=values;
                    earMin=Mathf.Min(earMin,motion.NearEarAngle);earMax=Mathf.Max(earMax,motion.NearEarAngle);
                    accentMin=Mathf.Min(accentMin,motion.AccentAngle);accentMax=Mathf.Max(accentMax,motion.AccentAngle);
                    tailMin=Mathf.Min(tailMin,motion.TailTipAngle);tailMax=Mathf.Max(tailMax,motion.TailTipAngle);
                    expressionMin=Mathf.Min(expressionMin,motion.Expression);expressionMax=Mathf.Max(expressionMax,motion.Expression);sniffPeak=Mathf.Max(sniffPeak,motion.Sniff);
                }
                Assert.That(maxStep,Is.LessThan(.32f),art.Name+" micro gestures must enter and leave smoothly");
                Assert.That(expressionMax-expressionMin,Is.GreaterThan(.05f),art.Name+" expression must change with its pose");
                Assert.That(sniffPeak,Is.GreaterThan(.05f),art.Name+" muzzle/beak has a quiet independent breath");
                if(profile.NearEar.z>0)Assert.That(earMax-earMin,Is.GreaterThan(1),art.Name+" ear must have a visible independent arc");
                if(profile.Accent.z>0)Assert.That(accentMax-accentMin,Is.GreaterThan(.5f),art.Name+" accessory must respond to real wind");
                motion.Reset();Assert.That(motion.Expression,Is.Zero);Assert.That(motion.Sniff,Is.Zero);
                Assert.That(motion.Deform(new Vector2(profile.Mouth.x,profile.Mouth.y)),Is.EqualTo(new Vector2(profile.Mouth.x,profile.Mouth.y)));
                rows.Add(art.Name+","+(earMax-earMin)+","+(accentMax-accentMin)+","+(tailMax-tailMin)+","+(expressionMax-expressionMin)+","+sniffPeak);
            }
            Directory.CreateDirectory(CharacterFolder);File.WriteAllLines(Path.Combine(CharacterFolder,"detail-motion.csv"),rows);
        }
        [Test] public void CharacterEyeglassesStayRigidDuringExpressionsAndSniffing()
        {
            foreach(var art in ActorSprites(false).Where(x=>x.Name=="speaker-Aster"||x.Name=="speaker-Toma"||x.Name=="speaker-Gran")){
                var profile=CharacterPoseProfile.For(art.Sprite);var motion=new CharacterPoseMotion(profile,197);
                var a=new Vector2(profile.NearEye.x,profile.NearEye.y);var b=new Vector2(profile.FarEye.x,profile.FarEye.y);
                float distance=Vector2.Scale(a-b,profile.Cell.size).magnitude;
                for(int i=0;i<30*60;i++){
                    motion.Advance(1f/60,WorkshopWind.Sample(i/60f),CharacterPoseView.SpeechShape("We can follow the path together.",i/60f));
                    Assert.That(Vector2.Scale(motion.Deform(a)-motion.Deform(b),profile.Cell.size).magnitude,Is.EqualTo(distance).Within(.02f),art.Name+" spectacles cannot stretch with the brows");
                }
            }
        }
        [Test] public void CharacterAppleUsesCompleteIntactAuthoredFrames()
        {
            var atlas=Resources.Load<Texture2D>("CascadeArt/CharacterMotionOct16/AppleBiteFrames");
            Assert.That(atlas,Is.Not.Null);Assert.That(atlas.width/4,Is.EqualTo(atlas.height/3),"Exactly twelve square full-character states");
            Assert.That(atlas.filterMode,Is.EqualTo(FilterMode.Bilinear));
            foreach(int fps in new[]{30,60,120}){
                float previous=0,largest=0;
                for(int i=0;i<48*fps;i++){
                    float frame=MikaAppleFrames.Frame(i/(float)fps);Assert.That(frame,Is.InRange(previous,11f));largest=Mathf.Max(largest,frame-previous);previous=frame;
                }
                Assert.That(largest,Is.LessThanOrEqualTo(10.01f/fps),"Authored whole poses use a consistent ten-frame-per-second cadence");
                Assert.That(previous,Is.EqualTo(11),"Never loop back to an unbitten apple");
            }
        }
        [Test] public void CharacterSpeechRestsAtPunctuationAndFeetRemainPlanted()
        {
            Assert.That(CharacterPoseView.SpeechShape("...",.25f),Is.Zero);Assert.That(CharacterPoseView.SpeechShape("Hi",10,0),Is.Zero);Assert.That(CharacterPoseView.SpeechShape("Hello",0),Is.Zero);
            Assert.That(CharacterPoseView.SpeechShape("Hello",3.6f,5),Is.Zero,"Each repeated phrase retains a closed-mouth pause");
            float volume=Enumerable.Range(0,60).Select(i=>CharacterPoseView.SpeechShape("Hello there",i/60f)).Max();Assert.That(volume,Is.GreaterThan(.4f));
            foreach(var art in ActorSprites(false,false)){
                var p=CharacterPoseProfile.For(art.Sprite);if(p.Kind==CharacterPoseKind.Victory)continue;
                var motion=new CharacterPoseMotion(p);var foot=new Vector2(.6f,1);
                for(int i=0;i<1200;i++){motion.Advance(1f/30,Mathf.Sin(i*.04f));Assert.That(Vector2.Distance(motion.Deform(foot),foot),Is.LessThan(.0001f),art.Name);}
            }
        }
        [Test] public void CharacterSecondaryInertiaIsContinuousAtThirtyAndOneHundredTwentyHertz()
        {
            float worst=0;
            foreach(var art in ActorSprites()){
                var p=CharacterPoseProfile.For(art.Sprite);var slow=new CharacterPoseMotion(p,73);var fast=new CharacterPoseMotion(p,73);
                var points=new[]{p.Neck,new Vector2(p.Tail.x,p.Tail.y),new Vector2(p.Lantern.x,p.Lantern.y),new Vector2(p.Mouth.x,p.Mouth.y),new Vector2(.5f,.8f)};
                for(int i=0;i<12*30;i++){
                    float gust=WorkshopWind.Sample(i/30f),speech=CharacterPoseView.SpeechShape("The lantern is still warm. Let's follow the path.",i/30f);
                    slow.Advance(1f/30,gust,speech);for(int n=0;n<4;n++)fast.Advance(1f/120,gust,speech);
                    foreach(var v in points)worst=Mathf.Max(worst,Vector2.Distance(slow.Deform(v),fast.Deform(v)));
                }
            }
            Assert.That(worst,Is.LessThan(.001f),"Substeps must keep face, cloth and props consistent across phone frame rates");
        }
        [Test] public void CharacterVictoryLanternStaysRigidAndPinnedToTheRaisedPaw()
        {
            var sprite=(Sprite)typeof(CascadeGame).GetMethod("VictoryMika",Private).Invoke(game,null);var profile=CharacterPoseProfile.For(sprite);var motion=new CharacterPoseMotion(profile);
            Vector2 a=new Vector2(.80f,.39f),b=new Vector2(.94f,.63f);float Metric(Vector2 p)=>Vector2.Scale(p,profile.Cell.size).magnitude;float length=Metric(a-b);
            for(int i=0;i<42*60;i++){
                motion.Advance(1f/60,WorkshopWind.Sample(i/60f));Assert.That(Metric(motion.DeformLantern(profile.LanternHook)-motion.Deform(profile.LanternHook)),Is.LessThan(.005f));
                Assert.That(Metric(motion.DeformLantern(a)-motion.DeformLantern(b)),Is.EqualTo(length).Within(.01f),"Metal and glass cannot stretch during the hops");
            }
        }
        [UnityTest] public IEnumerator CharacterActualScreensUseTheirOwnRigsAndRespectSuspension()
        {
            game.ReducedMotion=false;game.Preferences.EffectQuality=2;game.ShowHome();yield return null;
            var home=game.GetComponentsInChildren<CharacterPoseView>().Single(v=>v.Profile!=null);Assert.That(home.Profile.Kind,Is.EqualTo(CharacterPoseKind.Lantern));
            float age=home.AnimationAge;yield return new WaitForSecondsRealtime(.15f);Assert.That(home.AnimationAge,Is.GreaterThan(age));
            game.SendMessage("OnApplicationPause",true);age=home.AnimationAge;yield return new WaitForSecondsRealtime(.12f);Assert.That(home.AnimationAge,Is.EqualTo(age));game.SendMessage("OnApplicationPause",false);
            game.ReducedMotion=true;age=home.AnimationAge;yield return new WaitForSecondsRealtime(.12f);Assert.That(home.AnimationAge,Is.EqualTo(age));game.ReducedMotion=false;
            game.LoadNode("C02");game.ShowGame();game.SendMessage("TogglePause");yield return null;
            var apple=game.GetComponentsInChildren<CharacterPoseView>().Single(v=>v.Profile!=null);Assert.That(apple.Profile.Kind,Is.EqualTo(CharacterPoseKind.Apple));
            Assert.That(apple.GetComponentsInChildren<CharacterArtPart>(),Is.Empty,"The apple uses complete drawn frames; no detachable limb layer can be rendered");
            string hash=game.Interaction.Attempt.CurrentStateHash;age=apple.AnimationAge;yield return new WaitForSecondsRealtime(.15f);Assert.That(apple.AnimationAge,Is.GreaterThan(age));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator CharacterPreservesExistingWorldColorsAndWarmLanternHighlights()
        {
            game.ReducedMotion=false;game.Preferences.EffectQuality=2;game.ShowHome();yield return null;
            var hero=game.GetComponentsInChildren<CharacterPoseView>().Single(v=>v.Profile!=null);hero.ResetReview();
            var source=Resources.Load<Shader>(MenuWorldArt.Path+"WorldSurface");var reference=new Material(source);var animated=hero.Hero.material;Canvas.ForceUpdateCanvases();
            try{
                foreach(int world in new[]{0,1,5,8}){
                    var tint=MenuWorldArt.Light(world,22,false);reference.SetFloat("_World",world);reference.SetColor("_LightTint",tint);animated.SetFloat("_World",world);animated.SetColor("_LightTint",tint);
                    hero.Hero.material=reference;var before=WelcomePixels();hero.Hero.material=animated;var after=WelcomePixels();
                    foreach(var point in new[]{new Vector2(.58f,.55f),new Vector2(.58f,.20f),new Vector2(.865f,.72f)}){
                        var uv=hero.Profile.Uv(hero.SourceSprite,point);var bounds=hero.Mesh;var scale=new Vector2((bounds.High.x-bounds.Low.x)/(bounds.UvHigh.x-bounds.UvLow.x),(bounds.High.y-bounds.Low.y)/(bounds.UvHigh.y-bounds.UvLow.y));var local=bounds.Low+Vector2.Scale(uv-bounds.UvLow,scale);var screen=RectTransformUtility.WorldToScreenPoint(camera,hero.Hero.rectTransform.TransformPoint(local));int index=Mathf.Clamp(Mathf.RoundToInt(screen.y),0,portrait.height-1)*portrait.width+Mathf.Clamp(Mathf.RoundToInt(screen.x),0,portrait.width-1);var a=before[index];var b=after[index];Assert.That(Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b),Is.LessThanOrEqualTo(8),"Original fur/denim/lamp lighting must survive the animation material, world "+world);
                    }
                }
            }finally{hero.Hero.material=animated;UnityEngine.Object.DestroyImmediate(reference);}
        }
        [UnityTest] public IEnumerator CharacterCaptureEveryPoseWithMeasuredFaceLandmarks()
        {
            game.ReducedMotion=false;game.Preferences.EffectQuality=2;game.ShowHome();yield return null;
            var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(c=>c.enabled).ToArray();
            var oldTarget=camera.targetTexture;var oldColor=camera.backgroundColor;var oldClear=camera.clearFlags;var target=new RenderTexture(240,320,24);var readback=new Texture2D(240,320,TextureFormat.RGB24,false);target.Create();
            var host=new GameObject("Native character motion review",typeof(RectTransform),typeof(Canvas));host.layer=5;var canvas=host.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            string output=Path.Combine(CharacterFolder,"poses");Directory.CreateDirectory(output);var metadata=new List<string>{"id,kind,texture,sprite"};
            try{
                foreach(var c in canvases)c.enabled=false;camera.targetTexture=target;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.69f,.79f,.81f);
                foreach(var art in ActorSprites()){
                    var obj=new GameObject(art.Name,typeof(RectTransform));obj.layer=5;obj.transform.SetParent(host.transform,false);var image=art.Cutout?(Image)obj.AddComponent<ReferenceCutoutImage>():obj.AddComponent<Image>();image.sprite=art.Sprite;image.preserveAspect=true;image.raycastTarget=false;image.rectTransform.anchorMin=image.rectTransform.anchorMax=new Vector2(.5f,.5f);image.rectTransform.sizeDelta=new Vector2(220,294);
                    if(art.Cutout){image.rectTransform.sizeDelta=new Vector2(220,220*art.Sprite.rect.height/art.Sprite.rect.width);((ReferenceCutoutImage)image).SetContour(FailureReferenceArt.HeroContour);}
                    var view=CharacterPoseView.Attach(game,image);view.ResetReview();yield return null;
                    metadata.Add(art.Name+","+view.Profile.Kind+","+art.Sprite.texture.name+",\""+art.Sprite.rect+"\"");
                    void Shot(string file){Canvas.ForceUpdateCanvases();camera.Render();var active=RenderTexture.active;RenderTexture.active=target;readback.ReadPixels(new Rect(0,0,240,320),0,0);readback.Apply();RenderTexture.active=active;File.WriteAllBytes(Path.Combine(output,file+".png"),readback.EncodeToPNG());}
                    Shot(art.Name+"-neutral");view.SetReviewFace(1,.8f);Shot(art.Name+"-blink-speak");
                    for(int i=0;i<120;i++)view.AdvanceReview(1f/30,.8f,.65f);
                    Shot(art.Name+"-gesture");
                    UnityEngine.Object.DestroyImmediate(obj);
                }
                File.WriteAllLines(Path.Combine(CharacterFolder,"poses.csv"),metadata);
            }finally{camera.targetTexture=oldTarget;camera.backgroundColor=oldColor;camera.clearFlags=oldClear;foreach(var c in canvases)if(c!=null)c.enabled=true;UnityEngine.Object.DestroyImmediate(host);target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(readback);}
        }
        [UnityTest] public IEnumerator CharacterCaptureArmAttachmentCloseup()
        {
            game.ReducedMotion=false;game.Preferences.EffectQuality=2;game.ShowHome();yield return null;
            var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(c=>c.enabled).ToArray();
            var oldTarget=camera.targetTexture;var target=new RenderTexture(480,640,24);target.Create();var pixels=new Texture2D(480,640,TextureFormat.RGB24,false);
            var host=new GameObject("Native shoulder attachment review",typeof(RectTransform),typeof(Canvas));host.layer=5;
            var canvas=host.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            var actor=new GameObject("Apple pose at phone resolution",typeof(RectTransform),typeof(Image));actor.layer=5;actor.transform.SetParent(host.transform,false);
            var image=actor.GetComponent<Image>();image.sprite=Resources.Load<Sprite>("CascadeArt/PauseReferenceOct14/RestingMika");image.preserveAspect=true;
            image.rectTransform.anchorMin=image.rectTransform.anchorMax=Vector2.one*.5f;image.rectTransform.sizeDelta=new Vector2(450,600);
            var view=CharacterPoseView.Attach(game,image);view.ResetReview();
            string output=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/CharacterArmAttachmentOct16/closeup"));Directory.CreateDirectory(output);
            try{
                foreach(var c in canvases)c.enabled=false;camera.targetTexture=target;yield return null;
                foreach(int quality in new[]{2,1}){
                    game.Preferences.EffectQuality=quality;view.ResetReview();
                    for(int frame=0;frame<360;frame++){
                        view.AdvanceReview(1f/30,WorkshopWind.Sample(frame/30f));Canvas.ForceUpdateCanvases();camera.Render();
                        Assert.That(view.GetComponentsInChildren<CharacterArtPart>(),Is.Empty,"No separately moving arm on either quality setting");
                        Assert.That(image.material.GetFloat("_UseAppleFrames"),Is.EqualTo(1));
                        Assert.That(image.material.GetFloat("_AppleFrame"),Is.EqualTo(MikaAppleFrames.Frame(view.AnimationAge)).Within(.0001f));
                        if(quality==2||frame==0||frame==90||frame==117||frame==165){
                            var previous=RenderTexture.active;RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,480,640),0,0);pixels.Apply();RenderTexture.active=previous;
                            File.WriteAllBytes(Path.Combine(output,(quality==1?"low-":"")+frame.ToString("D4")+".jpg"),pixels.EncodeToJPG(94));
                        }
                        if(frame%30==0)yield return null;
                    }
                }
            }finally{camera.targetTexture=oldTarget;foreach(var c in canvases)if(c!=null)c.enabled=true;UnityEngine.Object.DestroyImmediate(host);target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);}
        }
        [UnityTest] public IEnumerator CharacterCaptureContinuousNativeMotion()
        {
            game.ReducedMotion=false;game.Preferences.EffectQuality=2;game.ShowHome();yield return null;
            var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(c=>c.enabled).ToArray();var oldTarget=camera.targetTexture;
            var target=new RenderTexture(240,320,24);target.Create();var readback=new Texture2D(240,320,TextureFormat.RGB24,false);
            var host=new GameObject("Continuous native character review",typeof(RectTransform),typeof(Canvas));host.layer=5;var canvas=host.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            var measures=new List<string>{"pose,frames,fps,minimum_visible_triangle_area"};
            try{
                foreach(var c in canvases)c.enabled=false;camera.targetTexture=target;
                foreach(var art in ActorSprites()){
                    bool skin=art.Name.EndsWith("-hopeful",StringComparison.Ordinal)||art.Name.EndsWith("-thinking",StringComparison.Ordinal);bool speaking=art.Name.StartsWith("speaker-",StringComparison.Ordinal);
                    int fps=skin?12:24,seconds=skin||speaking?6:12;string output=Path.Combine(CharacterFolder,"motion",art.Name);Directory.CreateDirectory(output);
                    var obj=new GameObject(art.Name,typeof(RectTransform));obj.layer=5;obj.transform.SetParent(host.transform,false);var image=art.Cutout?(Image)obj.AddComponent<ReferenceCutoutImage>():obj.AddComponent<Image>();image.sprite=art.Sprite;image.preserveAspect=true;image.raycastTarget=false;image.rectTransform.anchorMin=image.rectTransform.anchorMax=new Vector2(.5f,.5f);image.rectTransform.sizeDelta=new Vector2(220,294);if(art.Cutout)((ReferenceCutoutImage)image).SetContour(FailureReferenceArt.HeroContour);
                    if(art.Cutout)image.rectTransform.sizeDelta=new Vector2(220,220*art.Sprite.rect.height/art.Sprite.rect.width);
                    var view=CharacterPoseView.Attach(game,image);view.ResetReview();yield return null;float smallest=100;
                    for(int frame=0;frame<fps*seconds;frame++){
                        float t=frame/(float)fps;float speech=speaking?CharacterPoseView.SpeechShape("The lantern is still warm. Let's follow the path together.",t):0;
                        view.AdvanceReview(1f/fps,WorkshopWind.Sample(60+t),speech);Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;readback.ReadPixels(new Rect(0,0,240,320),0,0);readback.Apply();RenderTexture.active=previous;
                        File.WriteAllBytes(Path.Combine(output,frame.ToString("D4")+".jpg"),readback.EncodeToJPG(92));
                        var mesh=image.canvasRenderer.GetMesh();var v=mesh.vertices;var indices=mesh.triangles;
                        for(int i=0;i<indices.Length;i+=3){float area=-Vector3.Cross(v[indices[i+1]]-v[indices[i]],v[indices[i+2]]-v[indices[i]]).z;smallest=Mathf.Min(smallest,area);}
                        Assert.That(smallest,Is.GreaterThan(.02f),art.Name+" rendered skin folds at native frame "+frame);
                        foreach(var part in view.GetComponentsInChildren<CharacterArtPart>()){
                            var partMesh=part.canvasRenderer.GetMesh();var vertices=partMesh.vertices;var uv=partMesh.uv;var ts=partMesh.triangles;
                            for(int i=0;i<ts.Length;i+=3){var source=view.Profile.Point(art.Sprite,(uv[ts[i]]+uv[ts[i+1]]+uv[ts[i+2]])/3);float area=-Vector3.Cross(vertices[ts[i+1]]-vertices[ts[i]],vertices[ts[i+2]]-vertices[ts[i]]).z;Assert.That(area,Is.GreaterThan(.01f),art.Name+" visible articulated part folds at frame "+frame+", source "+source);}
                        }
                        if(frame%24==0)yield return null;
                    }
                    measures.Add(art.Name+","+(fps*seconds)+","+fps+","+smallest.ToString("F5",System.Globalization.CultureInfo.InvariantCulture));UnityEngine.Object.DestroyImmediate(obj);
                }
                File.WriteAllLines(Path.Combine(CharacterFolder,"native-motion.csv"),measures);
            }finally{camera.targetTexture=oldTarget;foreach(var c in canvases)if(c!=null)c.enabled=true;UnityEngine.Object.DestroyImmediate(host);target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(readback);}
        }
        [UnityTest] public IEnumerator CharacterCaptureReviewPagesAtPhoneScale()
        {
            game.ReducedMotion=false;game.Preferences.EffectQuality=2;game.ShowHome();yield return null;
            var oldTarget=camera.targetTexture;var oldClear=camera.clearFlags;var oldColor=camera.backgroundColor;
            var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(c=>c.enabled).ToArray();
            var target=new RenderTexture(960,1280,24);target.Create();var pixels=new Texture2D(960,1280,TextureFormat.RGB24,false);
            var host=new GameObject("Native 52-pose review pages",typeof(RectTransform),typeof(Canvas));host.layer=5;
            var canvas=host.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            var art=ActorSprites().ToArray();string output=Path.Combine(CharacterFolder,"review-pages");Directory.CreateDirectory(output);
            try{
                foreach(var c in canvases)c.enabled=false;camera.targetTexture=target;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.55f,.70f,.76f);
                for(int page=0;page*16<art.Length;page++){
                    var views=new List<CharacterPoseView>();
                    for(int i=page*16;i<Mathf.Min(art.Length,(page+1)*16);i++){
                        int cell=i%16;var obj=new GameObject(art[i].Name,typeof(RectTransform));obj.layer=5;obj.transform.SetParent(host.transform,false);
                        var image=art[i].Cutout?(Image)obj.AddComponent<ReferenceCutoutImage>():obj.AddComponent<Image>();image.sprite=art[i].Sprite;image.preserveAspect=true;image.raycastTarget=false;
                        image.rectTransform.anchorMin=image.rectTransform.anchorMax=Vector2.one*.5f;image.rectTransform.anchoredPosition=new Vector2(-360+cell%4*240,480-cell/4*320);image.rectTransform.sizeDelta=new Vector2(220,286);
                        if(art[i].Cutout){image.rectTransform.sizeDelta=new Vector2(220,220*art[i].Sprite.rect.height/art[i].Sprite.rect.width);((ReferenceCutoutImage)image).SetContour(FailureReferenceArt.HeroContour);}
                        var view=CharacterPoseView.Attach(game,image);view.ResetReview();views.Add(view);
                    }
                    yield return null;
                    foreach(float age in new[]{0f,3.10f,8f}){
                        foreach(var view in views){view.ResetReview();for(int i=0;i<Mathf.RoundToInt(age*30);i++)view.AdvanceReview(1f/30,WorkshopWind.Sample(i/30f),view.Profile.Kind==CharacterPoseKind.Speaker?.6f:0);}
                        Canvas.ForceUpdateCanvases();camera.Render();var active=RenderTexture.active;RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,960,1280),0,0);pixels.Apply();RenderTexture.active=active;
                        File.WriteAllBytes(Path.Combine(output,page+"-"+age.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+".jpg"),pixels.EncodeToJPG(94));
                    }
                    foreach(var view in views)UnityEngine.Object.DestroyImmediate(view.gameObject);
                }
            }finally{camera.targetTexture=oldTarget;camera.clearFlags=oldClear;camera.backgroundColor=oldColor;foreach(var c in canvases)if(c!=null)c.enabled=true;UnityEngine.Object.DestroyImmediate(host);target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);}
        }
        [UnityTest] public IEnumerator CharacterMotionAndMeshesStayAllocationFree()
        {
            game.ReducedMotion=false;game.Preferences.EffectQuality=2;game.ShowHome();yield return null;
            var output=new List<string>{"pose,quality,body_vertices,median_cpu_ms,managed_bytes_for_600_frames"};
            foreach(var art in ActorSprites(false).Where(x=>x.Name=="home"||x.Name=="apple"||x.Name=="victory"||x.Name=="speaker-Aster")){
                var obj=new GameObject("Character benchmark",typeof(RectTransform),typeof(Image));obj.transform.SetParent(game.transform,false);var image=obj.GetComponent<Image>();image.sprite=art.Sprite;
                var view=CharacterPoseView.Attach(game,image);view.ResetReview();var source=art.Sprite.rect;var texture=art.Sprite.texture;var quad=new UIVertex[4];
                for(int i=0;i<4;i++){quad[i]=UIVertex.simpleVert;bool right=i>=2,top=i==1||i==2;quad[i].position=new Vector3(right?110:-110,top?147:-147);quad[i].uv0=new Vector2((right?source.xMax:source.xMin)/texture.width,(top?source.yMax:source.yMin)/texture.height);}
                var times=new double[5];
                using(var body=new VertexHelper())using(var part=new VertexHelper()){
                    void Frame(){view.Motion.Advance(1f/60,.7f,.5f);body.Clear();for(int i=0;i<4;i++)body.AddVert(quad[i]);body.AddTriangle(0,1,2);body.AddTriangle(0,2,3);view.Mesh.ModifyMesh(body);}
                    foreach(int quality in new[]{2,1}){
                        game.Preferences.EffectQuality=quality;for(int i=0;i<30;i++)Frame();long allocation=GC.GetAllocatedBytesForCurrentThread();
                        for(int sample=0;sample<5;sample++){long start=System.Diagnostics.Stopwatch.GetTimestamp();for(int i=0;i<120;i++)Frame();times[sample]=(System.Diagnostics.Stopwatch.GetTimestamp()-start)*1000.0/System.Diagnostics.Stopwatch.Frequency/120;}
                        allocation=GC.GetAllocatedBytesForCurrentThread()-allocation;Assert.That(allocation,Is.Zero,art.Name+" must not allocate in its warmed simulation/mesh loop");Array.Sort(times);output.Add(art.Name+","+quality+","+body.currentVertCount+","+times[2].ToString("F6",System.Globalization.CultureInfo.InvariantCulture)+","+allocation);
                    }
                }
                UnityEngine.Object.DestroyImmediate(obj);
            }
            Directory.CreateDirectory(CharacterFolder);File.WriteAllLines(Path.Combine(CharacterFolder,"cpu.csv"),output);
        }
        [UnityTest] public IEnumerator CharacterCaptureActualPhoneScreensAndSpeakingResidents()
        {
            game.ReducedMotion=false;game.Preferences.EffectQuality=2;game.Preferences.Tutorials=false;game.Coach.Enabled=false;
            Profile.AccountIntroSeen=true;Profile.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();
            var output=Path.Combine(CharacterFolder,"screens");Directory.CreateDirectory(output);
            void Route(GameScreen screen)=>typeof(CascadeGame).GetMethod("SetScreen",Private).Invoke(game,new object[]{screen});
            void Shot(string name){Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;RenderTexture.active=portrait;var t=new Texture2D(portrait.width,portrait.height,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,portrait.width,portrait.height),0,0);t.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),t.EncodeToPNG());RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(t);}
            foreach(var size in new[]{new Vector2Int(390,844),new Vector2Int(320,693)}){
                WelcomePhone(size.x,size.y);game.SetEditorSafeInsets(new Vector4(.018f,.045f,.018f,.04f));game.SetUiLocale(size.x==390?"ru":"uk");
                foreach(var screen in new[]{GameScreen.Home,GameScreen.TravelBag,GameScreen.Pause,GameScreen.Fail,GameScreen.Win,GameScreen.JourneyEnd}){
                    if(screen==GameScreen.Win)Profile.Completion=new LevelCompletion{AttemptId=game.Interaction.Attempt.AttemptId.ToString("D"),NodeId=game.Interaction.Attempt.Level.CampaignNodeId,RatingVersion=3,Stars=3,Score=1860,Stages=3,Coins=40,Lights=1};
                    Route(screen);yield return new WaitForSecondsRealtime(screen==GameScreen.Win?2.5f:.5f);game.RefreshLayout();
                    var views=game.GetComponentsInChildren<CharacterPoseView>().Where(v=>v.Profile!=null).ToArray();Assert.That(views.Length,Is.GreaterThan(0),screen.ToString());
                    string hash=game.Interaction.Attempt.CurrentStateHash;
                    foreach(var view in views){view.ResetReview();float age=view.Profile.Kind==CharacterPoseKind.Apple?3.95f:view.Profile.Kind==CharacterPoseKind.Victory?1.20f:3.5f;for(int i=0;i<Mathf.RoundToInt(age*120);i++)view.AdvanceReview(1f/120,.6f);}
                    Shot(size.x+"-"+screen);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash),"Character animation never changes the puzzle");
                }
            }
            WelcomePhone();game.SetUiLocale("en");game.ShowJourney();yield return null;
            foreach(string name in new[]{"Mika","Pip","Rue","Aster","Bram","Sora","Neri","Toma","Lumi","Gran"}){
                typeof(CascadeGame).GetMethod("CancelConversation",Private).Invoke(game,null);
                var conversation=new Cascade.Content.StoryConversation(-1,new Cascade.Content.StoryLine(name,"We can rest here. The lantern will keep the path in sight."));
                typeof(CascadeGame).GetMethod("OpenConversation",Private).Invoke(game,new object[]{conversation,(UiCopy)"Back to the map",(Action)(()=>{}),null});yield return new WaitForSecondsRealtime(.3f);
                var body=(TMPro.TextMeshProUGUI)Field("conversationBody");body.maxVisibleCharacters=int.MaxValue;
                var portrait=(Image)Field("conversationPortrait");var view=portrait.GetComponent<CharacterPoseView>();Assert.That(view.Profile,Is.Not.Null,name);view.ResetReview();for(int i=0;i<120;i++)view.AdvanceReview(1f/120,.2f,CharacterPoseView.SpeechShape(body.text,i/120f));
                Shot("dialogue-"+name);
            }
        }
    }
}
#endif
