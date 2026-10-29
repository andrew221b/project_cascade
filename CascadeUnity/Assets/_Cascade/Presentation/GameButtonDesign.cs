using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    // Layout owners read DesiredSize. This view owns the shared skin and content,
    // not navigation callbacks, inventory cards, counters or gameplay state.
    [DefaultExecutionOrder(1000)]
    public sealed class GameButtonDesign:MonoBehaviour
    {
        public GameButtonPreset Preset;
        public bool PreserveSlot;
        // Optional painted-field offset inside an enlarged transparent hit target.
        public Vector2 ContentOffset;
        RectTransform safe,root,body,field;Button button;Image face;CascadeGame owner;
        readonly List<TextMeshProUGUI> labels=new List<TextMeshProUGUI>();
        readonly List<RectTransform> icons=new List<RectTransform>();
        readonly List<RectTransform> normalIcons=new List<RectTransform>();
        RectTransform mapIcon;GameButtonRole normalRole=GameButtonRole.PrimaryGreen;
        readonly List<Vector2> iconSizes=new List<Vector2>();
        readonly List<CenteredContentRow> rows=new List<CenteredContentRow>();
        bool bound,explicitRole;int createdFrame;float lastUnit;Vector2 lastSize,lastContentOffset;bool map;
        public bool Bound=>bound;
        public void Initialize(RectTransform safeArea,GameButtonRole? role=null)
        {
            if(button!=null){safe=safeArea;if(role.HasValue)SetRole(role.Value);return;}
            createdFrame=Time.frameCount;safe=safeArea;owner=safeArea.GetComponentInParent<CascadeGame>();button=GetComponent<Button>();root=(RectTransform)transform;
            if(role.HasValue){Preset=GameButtonPreset.Load(role.Value);explicitRole=true;}
        }
        public void SetRole(GameButtonRole role){if(Preset!=null&&Preset.Role==role&&explicitRole)return;Preset=GameButtonPreset.Load(role);explicitRole=true;if(bound)return;}
        public void MapAction()
        {
            if(bound){if(labels.Count>0)labels[0].SetCopy("Map");Apply(true);return;}
            SetRole(GameButtonRole.SecondaryYellow);map=true;
        }
        public static UiCopy CanonicalCaption(UiCopy copy)=>MapCaption(copy.English)?(UiCopy)"Map":copy.English=="Find in shop"?(UiCopy)"Shop":copy;
        static bool MapCaption(string value)=>value=="Map"||value=="Back to map"||value=="Back to the map"||value=="To the map"||value=="To map";
        void Start(){if(button==null){var canvas=GetComponentInParent<Canvas>();if(canvas!=null)Initialize((RectTransform)canvas.rootCanvas.transform,Preset==null?(GameButtonRole?)null:Preset.Role);}}
        void OnEnable()=>Canvas.willRenderCanvases+=Bind;
        void OnDisable()=>Canvas.willRenderCanvases-=Bind;
        public float DesignUnit
        {
            get{
                if(safe==null||root==null)return 1;
                float scale=Mathf.Min(safe.rect.width/720f,safe.rect.height/1280f);
                return root.parent.InverseTransformVector(safe.TransformVector(Vector3.right*scale)).magnitude;
            }
        }
        public Vector2 DesiredSize(Vector2 authored)=>Preset==null||PreserveSlot?authored:Vector2.Max(Preset.Size*DesignUnit,Vector2.one*(44*CenteredContentRow.Unit(root)/Mathf.Min(1,root.GetComponentInParent<Canvas>().rootCanvas.scaleFactor)));
        public void Refresh(){if(bound)Apply(false);else Bind();}
        static bool Decoration(Graphic g)=>g.name.IndexOf("flower",StringComparison.OrdinalIgnoreCase)>=0||g.name.IndexOf("sprig",StringComparison.OrdinalIgnoreCase)>=0||g.name.IndexOf("foliage",StringComparison.OrdinalIgnoreCase)>=0||g.GetComponentInParent<RoundHudBadge>()!=null||g.GetComponentInParent<TMP_Text>()!=null;
        static bool Known(Sprite sprite,out GameButtonRole role)
        {
            role=GameButtonRole.PrimaryGreen;if(sprite==null)return false;
            switch(sprite.name){
                case "GreenAction":case "TealAction":case "RedAction":case "GrayAction":case "Account Green":role=GameButtonRole.PrimaryGreen;return true;
                case "AmberAction":case "Account Gold":case "Settings Restore":role=GameButtonRole.SecondaryYellow;return true;
                case "InventoryStatCard":role=GameButtonRole.IconPrimary;return true;
                case "SquareButton":role=GameButtonRole.IconSecondary;return true;
                default:return false;
            }
        }
        void Bind()
        {
            if(bound){Apply(false);return;}
            if(!isActiveAndEnabled||button==null||safe==null||Time.frameCount<=createdFrame)return;
            // A level/card owns its nested actions; those are not global actions.
            if(root.parent.GetComponentInParent<Button>()!=null||root.name.StartsWith("Purchase ",StringComparison.Ordinal)){enabled=false;Destroy(this);return;}
            body=transform.Find("Reference shop button body") as RectTransform??root;
            foreach(var text in body.GetComponentsInChildren<TextMeshProUGUI>(true))if(text.GetComponentInParent<RoundHudBadge>()==null)labels.Add(text);
            if(Preset!=null&&(Preset.Role==GameButtonRole.IconPrimary||Preset.Role==GameButtonRole.IconSecondary))labels.RemoveAll(label=>string.IsNullOrEmpty(label.text)||int.TryParse(label.text,out _));
            foreach(var text in labels){string id=text.GetComponent<LocalizedCaption>()?.SourceIdentity;
                if(MapCaption(id))map=true;
                if(id=="Find in shop"){text.SetCopy("Shop");SetRole(GameButtonRole.PrimaryGreen);}
            }
            if(root.name=="Back"&&Preset==null){Preset=GameButtonPreset.Load(GameButtonRole.IconSecondary);explicitRole=true;
                var obj=new GameObject("Shared back arrow",typeof(RectTransform),typeof(ChevronGraphic));obj.transform.SetParent(body,false);((RectTransform)obj.transform).sizeDelta=Vector2.one*48;var arrow=obj.GetComponent<ChevronGraphic>();arrow.PointsRight=false;arrow.Weight=1;arrow.color=new Color(.19f,.055f,.014f);arrow.raycastTarget=false;
            }
            if(map){Preset=GameButtonPreset.Load(GameButtonRole.SecondaryYellow);explicitRole=true;}
            foreach(var image in body.GetComponentsInChildren<Image>(true)){
                if(image.GetComponentInParent<Button>()!=button||Decoration(image))continue;
                var r=image.rectTransform;
                if(r.rect.width<body.rect.width*.65f||r.rect.height<body.rect.height*.60f)continue;
                if(Known(image.sprite,out var role)){
                    if(!explicitRole){if((role==GameButtonRole.IconPrimary||role==GameButtonRole.IconSecondary)&&root.rect.width/root.rect.height>1.6f)continue;Preset=GameButtonPreset.Load(role);}
                    face=image;break;
                }
            }
            if(Preset==null){labels.Clear();enabled=false;Destroy(this);return;}
            normalRole=map?GameButtonRole.PrimaryGreen:Preset.Role;
            if(face==null){
                float largest=0;foreach(var image in body.GetComponentsInChildren<Image>(true))if(image.GetComponentInParent<Button>()==button&&!Decoration(image)&&Known(image.sprite,out _)){float area=image.rectTransform.rect.width*image.rectTransform.rect.height;if(area>largest){largest=area;face=image;}}
                if(face==null)face=button.targetGraphic as Image??button.GetComponent<Image>();
            }
            if(face==null){labels.Clear();return;}
            foreach(var aspect in body.GetComponentsInChildren<AspectRatioFitter>(true))if(aspect.transform==face.transform)aspect.enabled=false;
            if(!PreserveSlot&&Preset.Role!=GameButtonRole.TertiaryLabel){
                var skin=face.GetComponent<PrimaryButtonSkin>()??face.gameObject.AddComponent<PrimaryButtonSkin>();skin.Configure(Preset.Skin);
                if(face.transform!=root){face.rectTransform.anchorMin=Vector2.zero;face.rectTransform.anchorMax=Vector2.one;face.rectTransform.offsetMin=face.rectTransform.offsetMax=Vector2.zero;}
            }
            // The preset is the complete control skin. Per-screen blossoms would
            // otherwise make identical controls look different and obscure captions.
            foreach(var plant in body.GetComponentsInChildren<AttachedPlants>(true)){plant.enabled=false;foreach(var image in plant.GetComponentsInChildren<Image>(true))if(image!=face)image.gameObject.SetActive(false);}
            foreach(var graphic in body.GetComponentsInChildren<Graphic>(true))if(graphic.GetComponentInParent<RoundHudBadge>()==null&&(graphic.name.IndexOf("flower",StringComparison.OrdinalIgnoreCase)>=0||graphic.name.IndexOf("sprig",StringComparison.OrdinalIgnoreCase)>=0))graphic.gameObject.SetActive(false);
            bool square=Preset.Role==GameButtonRole.IconPrimary||Preset.Role==GameButtonRole.IconSecondary;
            foreach(var graphic in body.GetComponentsInChildren<Graphic>(true)){
                if(graphic==face||graphic is TextMeshProUGUI||Decoration(graphic)||graphic.GetComponentInParent<Button>()!=button)continue;
                if(graphic is Image im&&(im.sprite==null||Known(im.sprite,out _)))continue;
                var rect=(RectTransform)graphic.transform;if(!(graphic is Image)||rect.rect.width<body.rect.width*.8f&&rect.rect.height<body.rect.height*.95f)icons.Add(rect);
            }
            normalIcons.AddRange(icons);
            if(map){
                foreach(var icon in icons)icon.gameObject.SetActive(false);icons.Clear();
                var iconObject=new GameObject("Shared map scroll icon",typeof(RectTransform),typeof(Image));iconObject.transform.SetParent(body,false);
                var image=iconObject.GetComponent<Image>();image.sprite=FailureReferenceArt.Get(8);image.preserveAspect=true;image.raycastTarget=false;mapIcon=image.rectTransform;icons.Add(mapIcon);
                if(labels.Count>0){labels[0].gameObject.SetActive(true);labels[0].SetCopy("Map");}
            }
            if(!square&&icons.Count==0&&labels.Count==1&&labels[0].GetComponent<LocalizedCaption>()?.SourceIdentity=="Shop"){
                var obj=new GameObject("Shared shop icon",typeof(RectTransform),typeof(Image));obj.transform.SetParent(body,false);var image=obj.GetComponent<Image>();image.sprite=Resources.Load<Sprite>("CascadeArt/ItemsReferenceOct14/Storefront")??Resources.Load<Sprite>("CascadeArt/HomeOct13/ShopIcon");image.preserveAspect=true;image.raycastTarget=false;icons.Add(image.rectTransform);
            }
            if(!map){normalIcons.Clear();normalIcons.AddRange(icons);}
            foreach(var row in body.GetComponentsInChildren<CenteredContentRow>(true))rows.Add(row);
            // Existing action rows keep their elements but receive one writing field.
            foreach(var row in rows){row.enabled=false;Destroy(row);}
            var fieldObject=new GameObject("Design button writing field",typeof(RectTransform));fieldObject.transform.SetParent(body,false);field=fieldObject.GetComponent<RectTransform>();
            var content=new List<RectTransform>();
            foreach(var icon in icons){icon.SetParent(field,false);icon.anchorMin=icon.anchorMax=icon.pivot=Vector2.one*.5f;icon.sizeDelta=Vector2.one*Preset.IconSize*DesignUnit;content.Add(icon);}
            foreach(var label in labels){
                if(square&&label.text!="?"&&icons.Count>0){label.gameObject.SetActive(false);continue;}
                label.gameObject.SetActive(true);label.rectTransform.SetParent(field,false);content.Add(label.rectTransform);
            }
            // Preserve composite price order (caption, coin, amount) while ordinary
            // actions retain their icon-first group.
            foreach(var row in rows)if(row.Elements!=null&&row.Elements.Length==content.Count){bool same=true;foreach(var item in row.Elements)if(!content.Contains(item)){same=false;break;}if(same){content.Clear();content.AddRange(row.Elements);break;}}
            var layout=fieldObject.AddComponent<CenteredContentRow>();
            if(!square&&labels.Count>1&&icons.Count==0)layout.InitializeVertical(field,content.ToArray());else layout.Initialize(field,content.ToArray());
            layout.Padding=Preset.Inset;layout.FixedIconSize=true;rows.Clear();rows.Add(layout);bound=true;Apply(true);
        }
        bool SynchroniseCaption()
        {
            if(!bound||labels.Count==0||Preset.Role==GameButtonRole.IconPrimary||Preset.Role==GameButtonRole.IconSecondary)return false;
            string identity=labels[0].GetComponent<LocalizedCaption>()?.SourceIdentity??labels[0].text;bool changed=false;
            if(identity=="Find in shop"){labels[0].SetCopy("Shop");identity="Shop";changed=true;}
            bool wantsMap=MapCaption(identity);
            if(wantsMap!=map){
                map=wantsMap;Preset=GameButtonPreset.Load(map?GameButtonRole.SecondaryYellow:normalRole);explicitRole=true;
                foreach(var icon in normalIcons)if(icon!=null)icon.gameObject.SetActive(!map);
                icons.Clear();
                if(map){
                    if(mapIcon==null){var go=new GameObject("Shared map scroll icon",typeof(RectTransform),typeof(Image));go.transform.SetParent(field,false);var image=go.GetComponent<Image>();image.sprite=FailureReferenceArt.Get(8);image.preserveAspect=true;image.raycastTarget=false;mapIcon=image.rectTransform;}
                    mapIcon.gameObject.SetActive(true);icons.Add(mapIcon);
                }else{if(mapIcon!=null)mapIcon.gameObject.SetActive(false);icons.AddRange(normalIcons);}
                changed=true;
            }
            if(map){
                if(Preset.Role!=GameButtonRole.SecondaryYellow){Preset=GameButtonPreset.Load(GameButtonRole.SecondaryYellow);changed=true;}
                if(identity!="Map"){labels[0].SetCopy("Map");changed=true;}
            }else if(identity=="Shop"&&icons.Count==0){
                var go=new GameObject("Shared shop icon",typeof(RectTransform),typeof(Image));go.transform.SetParent(field,false);var image=go.GetComponent<Image>();image.sprite=Resources.Load<Sprite>("CascadeArt/ItemsReferenceOct14/Storefront")??Resources.Load<Sprite>("CascadeArt/HomeOct13/ShopIcon");image.preserveAspect=true;image.raycastTarget=false;icons.Add(image.rectTransform);normalIcons.Add(image.rectTransform);changed=true;
            }
            if(changed){var content=new List<RectTransform>(icons);foreach(var label in labels)if(label.gameObject.activeSelf)content.Add(label.rectTransform);rows[0].Initialize(field,content.ToArray());}
            return changed;
        }
        public void UseGlyph(Graphic graphic)
        {
            if(!bound||graphic==null||icons.Count==1&&icons[0]==graphic.rectTransform)return;
            icons.Clear();icons.Add(graphic.rectTransform);graphic.rectTransform.SetParent(field,false);
            graphic.rectTransform.anchorMin=graphic.rectTransform.anchorMax=graphic.rectTransform.pivot=Vector2.one*.5f;graphic.rectTransform.sizeDelta=Vector2.one*Preset.IconSize*DesignUnit;
            rows[0].Initialize(field,graphic.rectTransform);Apply(true);
        }
        void LateUpdate(){if(!bound)Bind();if(bound)Apply(false);}
        void Apply(bool force)
        {
            force|=SynchroniseCaption();
            float unit=DesignUnit;float captionSize=Preset.FontSize*unit*(owner!=null&&owner.Preferences.LargeText?1.1f:1);var size=DesiredSize(root.rect.size);
            if(!PreserveSlot&&root.pivot!=Vector2.one*.5f){var center=root.TransformPoint(root.rect.center);root.pivot=Vector2.one*.5f;root.position=center;}
            if(!PreserveSlot&&Vector2.Distance(root.rect.size,size)>.01f){root.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,size.x);root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,size.y);}
            if(body!=root){body.sizeDelta=size;body.anchoredPosition=Vector2.zero;}
            bool dirty=force||lastSize!=size||lastContentOffset!=ContentOffset||Mathf.Abs(lastUnit-unit)>.0001f;lastSize=size;lastUnit=unit;lastContentOffset=ContentOffset;
            foreach(var label in labels){float max=(Preset.Role==GameButtonRole.IconSecondary||Preset.Role==GameButtonRole.IconPrimary?Preset.IconSize*unit:captionSize);if(Mathf.Abs(label.fontSizeMax-max)>.01f||label.transform.parent!=field||label.color!=Preset.Ink||label.font!=Preset.Font)dirty=true;}
            for(int i=0;i<icons.Count;i++){var icon=icons[i];if(icon.anchorMin!=Vector2.one*.5f||icon.anchorMax!=Vector2.one*.5f||icon.pivot!=Vector2.one*.5f||i>=iconSizes.Count||Vector2.Distance(icon.sizeDelta,iconSizes[i])>.01f)dirty=true;}
            if(face!=null){var expected=CenteredContentRow.ReadingRect(face.rectTransform);if(Vector2.Distance(field.rect.size,expected.size-Vector2.one*(2*Preset.Inset*unit))>.01f)dirty=true;}
            if(!dirty)return;
            if(face!=null&&face.sprite!=Preset.Skin&&Preset.Skin!=null){var skin=face.GetComponent<PrimaryButtonSkin>();if(skin!=null)skin.Configure(Preset.Skin);else face.sprite=Preset.Skin;}
            var writing=face==null?body.rect:CenteredContentRow.ReadingRect(face.rectTransform);var origin=body.InverseTransformPoint(face==null?body.TransformPoint(writing.center):face.transform.TransformPoint(writing.center));
            field.anchorMin=field.anchorMax=field.pivot=Vector2.one*.5f;field.anchoredPosition=(Vector2)origin-body.rect.center+ContentOffset*unit;field.sizeDelta=writing.size-Vector2.one*(2*Preset.Inset*unit);
            foreach(var foreign in body.GetComponentsInChildren<CenteredContentRow>(true))if(foreign.transform!=field){foreign.enabled=false;Destroy(foreign);}
            foreach(var label in labels)if(label.transform.parent!=field)label.transform.SetParent(field,false);
            foreach(var icon in icons){icon.anchorMin=icon.anchorMax=icon.pivot=Vector2.one*.5f;icon.sizeDelta=Vector2.one*Preset.IconSize*unit;}
            foreach(var label in labels){label.font=Preset.Font;label.fontSizeMax=captionSize;label.color=Preset.Ink;label.fontStyle=FontStyles.Bold;}
            if(Preset.Role==GameButtonRole.IconPrimary||Preset.Role==GameButtonRole.IconSecondary)foreach(var label in labels)label.fontSizeMax=Preset.IconSize*unit;
            foreach(var row in rows){row.Initialize(field,row.Elements);row.Padding=Preset.Inset;row.Refresh();}
            iconSizes.Clear();foreach(var icon in icons)iconSizes.Add(icon.sizeDelta);
        }
    }
}
