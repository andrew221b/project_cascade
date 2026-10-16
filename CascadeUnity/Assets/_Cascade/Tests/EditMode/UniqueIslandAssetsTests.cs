using System.Collections.Generic;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
namespace Cascade.Tests {
public sealed class UniqueIslandAssetsTests {
[Test] public void EveryWorldHasSixDistinctCompleteAssetsWithoutFirstWorldFallback(){var sprites=new HashSet<Sprite>();var textures=new HashSet<Texture2D>();for(int world=0;world<9;world++)for(int i=0;i<6;i++){var s=WorldIslandArt.Get(world,i);var t=WorldIslandArt.Texture(world,i);Assert.That(s.name,Does.StartWith(WorldIslandArt.Theme(world)));Assert.That(t,Is.Not.Null);Assert.That(s.rect.width,Is.EqualTo(t.width));Assert.That(s.rect.height,Is.EqualTo(t.height));Assert.That(t.wrapMode,Is.EqualTo(TextureWrapMode.Clamp));Assert.That(t.mipmapCount,Is.GreaterThan(1));Assert.That(sprites.Add(s),Is.True,"Repeated sprite across worlds");Assert.That(textures.Add(t),Is.True,"Repeated source texture across worlds");}Assert.That(sprites.Count,Is.EqualTo(54));}
[Test] public void BridgeDockPointsStayInsideNaturalFittedArtwork(){for(int world=0;world<9;world++)for(int i=0;i<5;i++){Vector2 box=i==0?new Vector2(302,260):i==1?new Vector2(264,271):i==2?new Vector2(234,220):i==3?new Vector2(216,270):new Vector2(220,211);foreach(var normalized in new[]{new Vector2(.16f,.65f),new Vector2(.85f,.54f)}){var p=WorldIslandArt.Point(world,i,box,normalized);Assert.That(p.x,Is.InRange(0,box.x));Assert.That(p.y,Is.InRange(0,box.y));}}}
}
}
