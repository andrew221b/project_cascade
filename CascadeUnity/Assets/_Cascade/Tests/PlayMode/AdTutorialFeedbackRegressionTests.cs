#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Cascade.Tests
{
    public sealed partial class AccountReferenceTests
    {
        const string AdsLessonsReport="AdsLessonsOct27";
        void AdsLessonsShot(string file)
        {
            var folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+AdsLessonsReport));Directory.CreateDirectory(folder);
            Canvas.ForceUpdateCanvases();camera.Render();var old=RenderTexture.active;RenderTexture.active=portrait;
            var pixels=new Texture2D(portrait.width,portrait.height,TextureFormat.RGB24,false);
            pixels.ReadPixels(new Rect(0,0,portrait.width,portrait.height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,file+".png"),pixels.EncodeToPNG());
            RenderTexture.active=old;UnityEngine.Object.Destroy(pixels);
        }
        [UnityTest] public IEnumerator UnavailableVideoRibbonIsLocalizedAndContainedOnPhones()
        {
            game.ShowHome();game.Preferences.Tutorials=false;Profile.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();
            game.ShowGame();yield return FooterWait(()=>game.Interaction.CanPlay,"A normal round opens");
            var banner=(GameReactionBanner)Field("reactionBanner");
            foreach(var locale in new[]{"en","ru","uk"})foreach(var size in new[]{new Vector2Int(320,693),new Vector2Int(390,844)}){
                banner.Clear();game.SetGameLanguage(locale);
                while((bool)typeof(CascadeGame).GetField("applyingUiLanguage",Private).GetValue(game))yield return null;
                WelcomePhone(size.x,size.y);game.Preferences.LargeText=true;
                banner.Show("Video unavailable. Try again later.","");yield return null;game.RefreshLayout();Canvas.ForceUpdateCanvases();
                var label=banner.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Reaction title");label.ForceMeshUpdate();
                string expected=locale=="ru"?"Видео недоступно. Попробуй позже.":locale=="uk"?"Відео недоступне. Спробуй пізніше.":"Video unavailable. Try again later.";
                Assert.That(label.text,Is.EqualTo(expected));Assert.That(label.isTextOverflowing||label.isTextTruncated,Is.False,locale+" "+size.x);
                var bounds=CenteredContentRow.GlyphBounds(label);var field=label.rectTransform.rect;
                Assert.That(bounds.min.x,Is.GreaterThanOrEqualTo(field.xMin-1));Assert.That(bounds.max.x,Is.LessThanOrEqualTo(field.xMax+1));
                Assert.That(bounds.min.y,Is.GreaterThanOrEqualTo(field.yMin-1));Assert.That(bounds.max.y,Is.LessThanOrEqualTo(field.yMax+1));
                AdsLessonsShot("video-unavailable-"+locale+"-"+size.x);
            }
        }
        [UnityTest] public IEnumerator ShapeAndMixMapCirclesKeepTheSameRoundGeometry()
        {
            game.ShowHome();typeof(CascadeGame).GetMethod("SetScreen",Private).Invoke(game,new object[]{GameScreen.JourneyMap});yield return null;WelcomePhone(390,844);game.RefreshLayout();yield return null;
            var glyphs=game.GetComponentsInChildren<MapModeGlyphs>();Assert.That(glyphs.Length,Is.EqualTo(2));
            foreach(var glyph in glyphs){
                using var vertices=new VertexHelper();typeof(MapModeGlyphs).GetMethod("OnPopulateMesh",BindingFlags.Instance|BindingFlags.NonPublic,null,new[]{typeof(VertexHelper)},null).Invoke(glyph,new object[]{vertices});
                var min=Vector2.one*float.PositiveInfinity;var max=Vector2.one*float.NegativeInfinity;
                for(int i=0;i<25;i++){var vertex=new UIVertex();vertices.PopulateUIVertex(ref vertex,i);min=Vector2.Min(min,vertex.position);max=Vector2.Max(max,vertex.position);}
                Assert.That((max-min).x/(max-min).y,Is.EqualTo(1).Within(.015f),glyph.name+": the circle must not stretch into an oval");
            }
            float shapeHeight=glyphs[0].rectTransform.rect.height*.86f*glyphs[0].transform.lossyScale.y;
            foreach(var letter in game.GetComponentsInChildren<TextMeshProUGUI>().Where(x=>x.name=="Reference shop map mode symbols 1"||x.name=="Reference shop map mode mixed letter")){
                letter.ForceMeshUpdate();float height=CenteredContentRow.GlyphBounds(letter).size.y*letter.transform.lossyScale.y;
                Assert.That(height/shapeHeight,Is.InRange(.85f,1.15f),letter.name+": letters match the painted shape height");
            }
            foreach(string locale in new[]{"en","ru","uk"}){
                game.SetGameLanguage(locale);while((bool)typeof(CascadeGame).GetField("applyingUiLanguage",Private).GetValue(game))yield return null;
                typeof(CascadeGame).GetMethod("SetScreen",Private).Invoke(game,new object[]{GameScreen.JourneyMap});yield return null;game.RefreshLayout();yield return null;
                foreach(var label in game.GetComponentsInChildren<TextMeshProUGUI>().Where(x=>x.name.Contains("map mode"))){label.ForceMeshUpdate();Assert.That(label.isTextOverflowing||label.isTextTruncated,Is.False,label.text);}
                AdsLessonsShot("map-modes-"+locale+"-390");
            }
        }
    }
}
#endif
