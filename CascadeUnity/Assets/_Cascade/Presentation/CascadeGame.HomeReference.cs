using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        const string HomeArtPath="CascadeArt/HomeOct13/";
        Sprite HomeArt(string name)=>name=="PlayButton"?CleanMenuArt.Get("GreenAction"):Resources.Load<Sprite>(HomeArtPath+name);
        RectTransform HomeBounds(string name,Transform parent,float x,float y,float width,float height)
        {
            bool fromBottom=y>=367;
            float anchor=fromBottom?0:1;
            var rect=Rect(name,parent,new Vector2(.5f,anchor),new Vector2(.5f,anchor));
            rect.pivot=new Vector2(.5f,fromBottom?0:1);rect.sizeDelta=new Vector2(width,height);
            rect.anchoredPosition=new Vector2(x+width*.5f-360,fromBottom?1280-y-height:-y);
            return rect;
        }
        Image HomePicture(string name,Transform parent,string asset,float x,float y,float width,float height)
        {
            var image=HomeBounds(name,parent,x,y,width,height).gameObject.AddComponent<Image>();
            image.sprite=HomeArt(asset);image.preserveAspect=true;image.raycastTarget=false;image.color=Color.white;
            return image;
        }
        void DrawReferenceHome()
        {
            var stage=HomeReferenceSurface(false,out var layout);
            float shortcutInset=HomeReferenceLayout.ShortcutInset/Mathf.Max(.001f,stage.localScale.x);
            var logo=HomePicture("Flying Ladders illustrated logo",stage,"Logo",136,118,455,256);
            layout.BindTopArt(logo.rectTransform,null);
            var hero=HomeBounds("Mika welcomes you",stage,159,367,383,546).gameObject.AddComponent<Image>();
            hero.sprite=AccountReferenceArt.Get("HomeMika");hero.preserveAspect=true;hero.raycastTarget=false;
            AttachMikaCosmetic(hero,null);
            HomeReferenceButton(stage,layout,"Mythings","My things","BagButton",109.5f,908,165,157,ShowTravelBag);
            HomeReferenceButton(stage,layout,"Shop","Shop","ShopButton",277.5f,908,165,157,ShowShop);
            HomeReferenceButton(stage,layout,"Settings","Settings","SettingsButton",445.5f,908,165,157,ShowSettings);
            HomeReferenceButton(stage,layout,"Startgame","Play","PlayButton",113.5f,1082,493,128,ShowPlayChoice,true);
            homeAccountShortcut=null;
            if(AudienceCanUseLinkedAccount){
                homeAccountShortcut=Button(UiCopy.Raw(""),stage,Vector2.zero,Vector2.one,HomeAccountAction,sharedDesign:false);
                SetHomeButtonBounds(homeAccountShortcut,66+shortcutInset,HomeReferenceLayout.ShortcutTop,94,94);
                homeAccountShortcut.GetComponent<Image>().color=Color.white;homeAccountShortcut.GetComponent<Image>().preserveAspect=true;
                homeAccountShortcut.GetComponentInChildren<TextMeshProUGUI>().gameObject.SetActive(false);
                layout.BindAccountShortcut((RectTransform)homeAccountShortcut.transform);RefreshHomeAccountShortcut();
                layout.BindTouch((RectTransform)homeAccountShortcut.transform,new Vector2(94,94));
            }
            if(purchaseStore?.Owned!=true){
                var noAds=Button(UiCopy.Raw(""),stage,Vector2.zero,Vector2.one,ShowNoAds,sharedDesign:false);noAds.name="Remove ads shortcut";
                SetHomeButtonBounds(noAds,579-shortcutInset,HomeReferenceLayout.ShortcutTop,92,94);
                layout.BindTopArt(logo.rectTransform,(RectTransform)noAds.transform);
                layout.BindTouch((RectTransform)noAds.transform,new Vector2(92,94));
                noAds.GetComponent<Image>().sprite=HomeArt("NoAds");noAds.GetComponent<Image>().type=Image.Type.Simple;
                noAds.GetComponent<Image>().preserveAspect=true;
                noAds.GetComponent<Image>().color=Color.white;
                noAds.GetComponentInChildren<TextMeshProUGUI>().gameObject.SetActive(false);
            }
            DrawAccountReferenceDialog(stage,layout);layout.Fit();
        }
        RectTransform HomeReferenceSurface(bool welcome,out HomeReferenceLayout layout)
        {
            LoadNavigationArt();machine.SetZone(EnvironmentWorld);
            // Reference positions and sizes remain fixed; the adaptive viewport changes
            // only the outer frame and the small amount of extra sky.
            var extensions=Rect("Home frame extensions",menuRoot,Vector2.zero,Vector2.one);
            var stage=Rect("Home reference layout",menuRoot,new Vector2(.5f,.5f),new Vector2(.5f,.5f));
            stage.sizeDelta=new Vector2(720,1280);
            layout=stage.gameObject.AddComponent<HomeReferenceLayout>();layout.Initialize(this,menuRoot,extensions,HomeArt("Backdrop").texture);
            var environment=Picture("Reference wooden frame and platform",stage,Vector2.zero,Vector2.one,HomeArt("Backdrop"),false);
            environment.type=Image.Type.Sliced;environment.color=Color.clear;
            var landscape=Picture("Account and home castle landscape",extensions,Vector2.zero,Vector2.one,AccountReferenceArt.Get("Scene"),false);
            landscape.gameObject.AddComponent<AspectRatioFitter>().aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;
            landscape.GetComponent<AspectRatioFitter>().aspectRatio=942/1670f;
            var aircraft=HomeBounds("Reference home umbrella airship",stage,548,welcome?534.5f:432.5f,80,73).gameObject.AddComponent<Image>();
            aircraft.sprite=EnvironmentWorld==0?AccountReferenceArt.Get("Airship"):Resources.Load<Sprite>(MenuWorldArt.Path+"Aircraft"+(EnvironmentWorld%6));
            aircraft.preserveAspect=true;aircraft.raycastTarget=false;
            aircraft.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(8,5),.8f,1.2f,true);
            DrawHomeSky(stage);
            layout.BindGround(HomeArt("GroundForeground").texture);
            var frame=Picture("Timber and foliage in front of distant islands",stage,Vector2.zero,Vector2.one,HomeArt("FrameForeground"),false);
            frame.type=Image.Type.Sliced;frame.color=Color.white;
            return stage;
        }
        void DrawHomeSky(RectTransform stage)
        {
            var sky=Rect("Living home sky",stage,new Vector2(53/720f,0),new Vector2(680/720f,1));
            sky.offsetMin=new Vector2(0,382);sky.offsetMax=new Vector2(0,-139);sky.gameObject.AddComponent<RectMask2D>();
            var foliage=Rect("Natural menu foliage",sky,Vector2.zero,Vector2.one);foliage.gameObject.AddComponent<MenuLeafDrift>().Initialize(this,ReferenceShopArt("MenuGreenLeaf"));
            var life=Rect("Home birds and insects",sky,Vector2.zero,Vector2.one);life.gameObject.AddComponent<HomeSkyLife>().Initialize(this);
            var worn=Rect("Home worn trophy atmosphere",sky,Vector2.zero,Vector2.one);CosmeticAmbiance(worn,EquippedGift,false);
            var scenery=Rect("Home scenery trophy atmosphere",sky,Vector2.zero,Vector2.one);CosmeticAmbiance(scenery,EquippedScenery,false);
        }
        void SetHomeButtonBounds(Button button,float x,float y,float width,float height)
        {
            var rect=(RectTransform)button.transform;
            bool fromBottom=y>=367;float anchor=fromBottom?0:1;
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,anchor);
            rect.pivot=new Vector2(.5f,fromBottom?0:1);rect.sizeDelta=new Vector2(width,height);
            rect.anchoredPosition=new Vector2(x+width*.5f-360,fromBottom?1280-y-height:-y);
        }
        void StylePrimaryFace(Image image)
        {
            (image.GetComponent<PrimaryButtonSkin>()??image.gameObject.AddComponent<PrimaryButtonSkin>()).Configure(HomeArt("PlayButton"));
        }
        void HomeReferenceButton(Transform parent,HomeReferenceLayout layout,string name,UiCopy copy,string asset,float x,float y,float width,float height,Action action,bool play=false)
        {
            var button=Button(copy,parent,Vector2.zero,Vector2.one,action,sharedDesign:play);button.name=name;
            SetHomeButtonBounds(button,x,y,width,height);
            layout.BindTouch((RectTransform)button.transform,new Vector2(width,height));
            var image=button.GetComponent<Image>();image.sprite=HomeArt(play?asset:"NavButtonFrame");image.type=Image.Type.Simple;image.color=Color.white;
            if(play)StylePrimaryFace(image);else image.preserveAspect=true;
            if(!play){
                var icon=Picture("Home menu icon",button.transform,new Vector2(.5f,1),new Vector2(.5f,1),HomeArt(asset.Replace("Button","Icon")),false);
                icon.rectTransform.sizeDelta=new Vector2(104,84);icon.rectTransform.anchoredPosition=new Vector2(0,-63);
                icon.preserveAspect=true;icon.color=Color.white;
            }
            var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=Color.white;colors.pressedColor=new Color(.87f,.93f,.84f);button.colors=colors;
            var label=button.GetComponentInChildren<TextMeshProUGUI>();label.name="Home reference caption";
            label.font=Resources.Load<TMP_FontAsset>("CascadeArt/Fonts/InterfaceTitle")??art.titleFont;
            label.fontStyle=FontStyles.Normal;label.alignment=TextAlignmentOptions.Midline;
            label.enableAutoSizing=true;label.fontSizeMin=play?40:18;label.fontSizeMax=play?54:22;label.fontSize=label.fontSizeMax;
            baseTextSizes[label]=label.fontSizeMax;
            label.textWrappingMode=TextWrappingModes.NoWrap;label.overflowMode=TextOverflowModes.Truncate;
            label.margin=Vector4.zero;label.color=MenuPaper;navigationLightLabels.Add(label);
            label.rectTransform.anchorMin=play?new Vector2(.14f,.20f):new Vector2(.08f,.11f);
            label.rectTransform.anchorMax=play?new Vector2(.86f,.78f):new Vector2(.92f,.35f);
            label.rectTransform.offsetMin=label.rectTransform.offsetMax=Vector2.zero;
            var material=new Material(label.font.material){name="Home ivory letters with shaped dark outline"};
            material.SetColor(ShaderUtilities.ID_OutlineColor,new Color(.08f,.16f,.08f));
            material.SetFloat(ShaderUtilities.ID_OutlineWidth,.12f);label.fontSharedMaterial=material;label.UpdateMeshPadding();layout.Own(material);
            if(!play)CentreStack((RectTransform)button.transform,button.transform.Find("Home menu icon") as RectTransform,label.rectTransform);
            else CentreContent((RectTransform)button.transform,label.rectTransform);
            var visual=Rect("Home button illustrated surface",button.transform,Vector2.zero,Vector2.one);visual.SetAsFirstSibling();var painted=visual.gameObject.AddComponent<Image>();painted.sprite=image.sprite;painted.type=image.type;painted.preserveAspect=image.preserveAspect;painted.raycastTarget=false;
            if(play)painted.gameObject.AddComponent<PrimaryButtonSkin>().Configure(painted.sprite);image.color=Color.clear;button.targetGraphic=painted;
            if(play)RegisterButtonDesign(button,GameButtonRole.PrimaryGreen);
        }
    }
}
