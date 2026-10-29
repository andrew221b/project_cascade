#if UNITY_EDITOR
using System;using System.Collections;using System.IO;using System.Linq;using System.Reflection;
using Cascade.Content;using Cascade.Domain;using Cascade.Presentation;using NUnit.Framework;
using UnityEngine;using UnityEngine.SceneManagement;using UnityEngine.TestTools;
namespace Cascade.Tests
{
    public sealed class MobilePerformanceTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;string directory;
        object Preferred()=>typeof(CascadeGame).GetMethod("PreferredLevels",Private).Invoke(game,null);
        [UnitySetUp]public IEnumerator Setup(){
            directory=Path.Combine(Path.GetTempPath(),"CascadePerformanceSafety",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);yield return null;
        }
        [UnityTearDown]public IEnumerator Cleanup(){if(game!=null)UnityEngine.Object.Destroy(game.gameObject);yield return null;CascadeGame.EditorProfileDirectory=null;if(Directory.Exists(directory))Directory.Delete(directory,true);}
        [UnityTest]public IEnumerator CachedSequenceTracksAllModesAndLocalesWithoutCachingProgress(){
            foreach(var mode in new[]{PuzzleMode.Shapes,PuzzleMode.Words,PuzzleMode.Mixed})foreach(string locale in new[]{"en","ru","uk"}){
                typeof(CascadeGame).GetProperty("PreferredMode").SetValue(game,mode);game.Preferences.PuzzleLocale=locale;
                var actual=(System.Collections.Generic.IReadOnlyList<LevelContent>)Preferred();Assert.That(Preferred(),Is.SameAs(actual));
                var expected=mode==PuzzleMode.Shapes?game.Catalog.PrimaryLevels.Where(c=>c.Mode==mode).ToArray():mode==PuzzleMode.Mixed?
                    MixedJourney.Sequence(game.Catalog,locale):
                    game.Catalog.LocalizedLevels.Where(c=>c.Locale==locale&&c.Mode==mode).ToArray();
                CollectionAssert.AreEqual(expected,actual);yield return null;
            }
        }
        [UnityTest]public IEnumerator SkiesLoadOnlyWhenTheirWorldIsVisited(){
            var world=game.Machine.LivingWorld;var skies=(Texture2D[])typeof(LayeredWorld).GetField("backgrounds",Private).GetValue(world);
            Assert.That(skies.Count(t=>t!=null),Is.EqualTo(1));
            world.SetZone(6);Assert.That(skies[6],Is.Not.Null);Assert.That(skies.Count(t=>t!=null),Is.EqualTo(2));
            world.SetZone(0);Assert.That(skies.Count(t=>t!=null),Is.EqualTo(2));yield return null;
        }
        [Test]public void MobileSoftShadowsUseSmallExistingAtlasAndLowCostFilter(){
            var asset=UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Settings/Mobile_RPAsset.asset");
            var serialized=new UnityEditor.SerializedObject(asset);
            Assert.That(serialized.FindProperty("m_SoftShadowsSupported").boolValue,Is.True);
            Assert.That(serialized.FindProperty("m_MainLightShadowmapResolution").intValue,Is.EqualTo(1024));
            Assert.That(serialized.FindProperty("m_ShadowCascadeCount").intValue,Is.EqualTo(1));
            Assert.That(serialized.FindProperty("m_ShadowDistance").floatValue,Is.EqualTo(24));
            Assert.That(serialized.FindProperty("m_SoftShadowQuality").intValue,Is.EqualTo(1));
            foreach(string name in new[]{"TimberSurface","HempRope"}){
                var shader=Resources.Load<Shader>("CascadeArt/"+name);Assert.That(shader,Is.Not.Null);Assert.That(shader.isSupported,Is.True);
                Assert.That(UnityEditor.ShaderUtil.GetShaderMessages(shader).Any(m=>m.severity.ToString()=="Error"),Is.False);
            }
        }
    }
}
#endif
