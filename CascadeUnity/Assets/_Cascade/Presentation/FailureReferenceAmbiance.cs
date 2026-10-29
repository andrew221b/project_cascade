using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    public sealed class FailureReferenceAmbiance:MonoBehaviour
    {
        CascadeGame game;int world;float age=16;Material scene,landscape,far;Image heading;
        readonly List<(RectTransform Rect,Vector2 Position,float Angle,int Index)> decorations=new List<(RectTransform,Vector2,float,int)>();
        readonly List<Material> owned=new List<Material>();
        public void Initialize(CascadeGame owner,RectTransform stage,Image hero,Image bench,Image title)
        {
            game=owner;world=game.EnvironmentWorld;heading=title;
            scene=Make("Failure world-lit Mika");landscape=Make("Failure world-lit wood");landscape.SetFloat("_Landscape",1);
            scene.SetFloat("_CutSky",1);
            hero.material=scene;bench.material=landscape;
            var headingMaterial=new Material(Resources.Load<Shader>(WorldFrameArt.Path+"FrameSurface")){name="Failure original wood heading"};headingMaterial.SetFloat("_Header",1);headingMaterial.SetFloat("_World",world);owned.Add(headingMaterial);heading.material=headingMaterial;
            far=Make("Failure distant world islands");far.SetFloat("_AuthoredIsland",1);far.SetFloat("_Haze",.42f);far.SetTexture("_SkyTex",MenuWorldArt.Sky(world));
            for(int i=0;i<2;i++){var picture=Picture("Failure distant island "+i,stage,WorldIslandArt.Get(world,i),i==0?24:574,i==0?733:398,112,154);picture.material=far;picture.rectTransform.SetAsFirstSibling();decorations.Add((picture.rectTransform,picture.rectTransform.anchoredPosition,0,i));}
            var weather=new GameObject("Failure thematic weather",typeof(RectTransform));weather.transform.SetParent(stage,false);weather.transform.SetAsFirstSibling();var rect=weather.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;weather.AddComponent<MenuWorldParticles>().Initialize(world);
            Paint(false);
        }
        Material Make(string name){var material=new Material(Resources.Load<Shader>(MenuWorldArt.Path+"WorldSurface")){name=name};material.SetFloat("_World",world);owned.Add(material);return material;}
        Image Picture(string name,Transform parent,Sprite sprite,float x,float y,float width,float height){var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(0,1);rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(x,-y);rect.sizeDelta=new Vector2(width,height);var image=go.GetComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;return image;}
        void LateUpdate(){if(game==null||!game.CelebrationMotionAllowed)return;bool moving=!game.ReducedMotion&&game.Preferences.EffectQuality>0;if(moving)age+=Mathf.Min(Time.unscaledDeltaTime,.05f);Paint(moving);}
        void Paint(bool moving){var light=MenuWorldArt.Light(world,age,moving);foreach(var material in owned){material.SetColor("_LightTint",light);material.SetFloat("_Age",age);if(material.HasProperty("_Wind"))material.SetFloat("_Wind",moving?Cascade.Application.WorkshopWind.Sample(age):0);}foreach(var item in decorations){float sway=moving?Mathf.Sin(age*.64f+item.Index*.73f):0;item.Rect.anchoredPosition=item.Position+new Vector2(item.Index<2?sway*2:0,item.Index<2?Mathf.Sin(age*.83f+item.Index)*3:0);item.Rect.localRotation=Quaternion.Euler(0,0,item.Angle+(world==6||item.Index<2?0:sway*.9f));}}
        void OnDestroy(){foreach(var material in owned)if(material!=null)Destroy(material);}
    }
    public sealed class ReferenceHeaderTheme:MonoBehaviour
    {
        CascadeGame game;Material material;float age=16;
        public void Initialize(CascadeGame owner){game=owner;material=new Material(Resources.Load<Shader>(WorldFrameArt.Path+"FrameSurface")){name="Authored header theme"};material.SetFloat("_Header",1);GetComponent<Image>().material=material;Paint();}
        void LateUpdate(){if(game==null||!game.CelebrationMotionAllowed)return;if(!game.ReducedMotion&&game.Preferences.EffectQuality>0)age+=Mathf.Min(Time.unscaledDeltaTime,.05f);Paint();}
        void Paint(){material.SetFloat("_World",game.EnvironmentWorld);material.SetFloat("_Age",age);material.SetFloat("_Wind",!game.ReducedMotion&&game.Preferences.EffectQuality>0?Cascade.Application.WorkshopWind.Sample(age):0);material.SetColor("_LightTint",MenuWorldArt.Light(game.EnvironmentWorld,age,!game.ReducedMotion&&game.Preferences.EffectQuality>0));}
        void OnDestroy(){if(material!=null)Destroy(material);}
    }
}
