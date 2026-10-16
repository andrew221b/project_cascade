using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // UI counterpart of the shared painted rig: one visitor, physical frame contacts,
    // an externally owned clock and no raycasts, gameplay commands or new physics world.
    public sealed class WorldFrameResident:MonoBehaviour
    {
        RectTransform frame,actor,body,near,far,shade;Image bodyArt,nearArt,farArt;
        WildlifeContactShade contactShade;
        int world,previousVisit=-1;float firstAge=-1,width,landing=1.8f,rest=8,departure=1.4f;
        WildlifeSpecies species;bool crawling,clinging,rightSide;Vector2 contact;float facing,supportAngle;
        WorldFrameFit frameFit;Vector2 originalContact;
        WildlifeAudioEmitter sound;
        public bool Resting{get;private set;}
        public WildlifeSpecies Species=>species;
        public Vector3 FeetPosition=>actor==null?Vector3.zero:actor.position;
        public float SurfaceAngle=>supportAngle;
        public void Initialize(int biome)
        {
            world=biome;frame=(RectTransform)transform;frameFit=frame.parent.GetComponent<WorldFrameFit>();
            shade=NewRect("Frame foot contact shade",frame);contactShade=shade.gameObject.AddComponent<WildlifeContactShade>();contactShade.raycastTarget=false;
            actor=NewRect("Pooled frame visitor",frame);
            far=Picture("Far wing",out farArt);body=Picture("Painted body",out bodyArt);near=Picture("Near wing",out nearArt);
            sound=gameObject.AddComponent<WildlifeAudioEmitter>();sound.Bind(GetComponentInParent<CascadeGame>(),bodyArt,WildlifeSpecies.Swallow);
            actor.gameObject.SetActive(false);
        }
        static RectTransform NewRect(string name,Transform parent)
        {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var r=(RectTransform)go.transform;r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;return r;
        }
        RectTransform Picture(string name,out Image image)
        {
            var r=NewRect(name,actor);image=r.gameObject.AddComponent<Image>();image.raycastTarget=false;return r;
        }
        public void Pose(float age,int quality)
        {
            if(frame==null)return;Resting=false;
            if(quality<=0||float.IsNaN(age)||float.IsInfinity(age)){actor.gameObject.SetActive(false);shade.gameObject.SetActive(false);return;}
            if(firstAge<0||age<firstAge)firstAge=age;
            float clock=age-firstAge,period=quality==1?48:38;int visit=Mathf.FloorToInt(clock/period);
            float seed=WildlifeMotion.Hash(world*197+visit*311+43),t=Mathf.Repeat(clock,period)-(2.5f+seed*2);
            if(visit!=previousVisit||!Mathf.Approximately(width,frame.rect.width)){previousVisit=visit;Configure(visit,seed);}
            contact=frameFit==null?originalContact:frameFit.RemapNormalizedContact(originalContact);
            float end=landing+rest+departure;if(t<0||t>=end){actor.gameObject.SetActive(false);shade.gameObject.SetActive(false);return;}
            actor.gameObject.SetActive(true);
            Vector2 point;float fold,alpha=1,tilt=0;
            var outside=new Vector2(facing>0?-.10f:1.10f,Mathf.Min(.84f,contact.y+.14f));
            var normal=new Vector2(rightSide?-1:1,0);
            if(clinging)outside=new Vector2(rightSide?.72f:.28f,-.12f);
            if(crawling)outside=contact-Vector2.up*.12f;
            if(t<landing){
                float p=WildlifePerchMotion.LandingProgress(t/landing*WildlifePerchMotion.ApproachTime);
                if(crawling)point=Vector2.Lerp(outside,contact,p)+Vector2.up*(Mathf.Pow(Mathf.Sin(p*Mathf.PI*3),2)*.006f);
                else if(clinging)point=WildlifeMotion.Bezier(outside,outside+Vector2.up*.26f+normal*.06f,contact+normal*.045f-Vector2.up*.055f,contact,p);
                else point=WildlifeMotion.Bezier(outside,outside+new Vector2(facing*.16f,-.03f),contact+new Vector2(-facing*.035f,.055f),contact,p);
                fold=0;alpha=Mathf.SmoothStep(0,1,t/.25f);tilt=crawling?supportAngle:Mathf.LerpAngle(-facing*10,supportAngle,p);
            }else if(t<landing+rest){
                Resting=true;float at=t-landing;point=contact;fold=WildlifePerchMotion.Fold(at);
                tilt=supportAngle+(clinging?0:Mathf.Sin(at*(species==WildlifeSpecies.BarnOwl?.6f:1.2f)+seed*8)*1.2f);
            }else{
                float at=t-landing-rest,p=WildlifePerchMotion.LeavingProgress(at/departure*WildlifePerchMotion.DepartureTime);
                var exit=crawling?contact-Vector2.up*.13f:new Vector2(facing>0?1.14f:-.14f,contact.y+.21f);
                var lift=clinging?normal*.10f+Vector2.up*.06f:new Vector2(facing*.025f,.085f);
                point=crawling?Vector2.Lerp(contact,exit,p)+Vector2.up*(Mathf.Pow(Mathf.Sin(p*Mathf.PI*2),2)*.007f):WildlifeMotion.Bezier(contact,contact+lift,exit+new Vector2(-facing*.16f,-.02f),exit,p);
                fold=1-WildlifePerchMotion.Fold(at*2);alpha=1-Mathf.SmoothStep(0,1,(p-.78f)/.22f);tilt=crawling?supportAngle:Mathf.LerpAngle(supportAngle,-facing*12*(1-p),Mathf.SmoothStep(0,1,p*2));
            }
            float pressure=t<landing?Mathf.SmoothStep(0,1,(t/landing-.72f)/.28f):t<landing+rest?1:1-Mathf.SmoothStep(0,1,(t-landing-rest)/.20f);
            shade.gameObject.SetActive(pressure>.001f);shade.anchoredPosition=Vector2.Scale(contact-Vector2.one*.5f,frame.rect.size);
            shade.localRotation=Quaternion.Euler(0,0,supportAngle);contactShade.color=new Color(.12f,.065f,.025f,alpha*pressure*.24f);
            actor.anchoredPosition=Vector2.Scale(point-Vector2.one*.5f,frame.rect.size);
            actor.localRotation=Quaternion.Euler(0,0,tilt);actor.localScale=new Vector3(facing,1,1);
            float rate=species==WildlifeSpecies.BarnOwl?1.5f:species==WildlifeSpecies.Bumblebee?10.3f:4.8f;
            var pose=WildlifeMotion.Pose(WildlifeMotion.Style(species),t,rate,seed,effort:t<landing||t>landing+rest?1:0);
            bodyArt.color=new Color(1,1,1,alpha);body.localScale=new Vector3(1,crawling&&Resting?1+Mathf.Sin(t*19)*Mathf.Max(0,Mathf.Sin(t*1.3f))*.008f:1,1);
            sound.Resting=Resting;
            PaintWing(near,nearArt,pose.NearWing,pose.WingFold,fold,alpha);PaintWing(far,farArt,pose.FarWing,-pose.WingFold*.7f,fold,alpha*.82f);
        }
        void Configure(int visit,float seed)
        {
            species=WorldFauna.Perched(world,visit,true);crawling=species==WildlifeSpecies.FieldCricket;clinging=WildlifePerchMotion.Clings(species);
            sound.SetSpecies(species);
            rightSide=seed>=.5f;facing=rightSide?-1:1;if(clinging)facing=-facing;
            supportAngle=clinging?WildlifePerchMotion.FrameAngle(rightSide):0;
            originalContact=WorldFrameArt.FrameContact(rightSide,seed,frame.rect.width/Mathf.Max(1,frame.rect.height));contact=originalContact;
            landing=crawling?2.6f:species==WildlifeSpecies.Bumblebee?2.35f:species==WildlifeSpecies.BarnOwl?2.4f:1.8f;departure=crawling?1.7f:species==WildlifeSpecies.BarnOwl?1.8f:1.4f;rest=(crawling?10:species==WildlifeSpecies.BarnOwl?11:6)+seed*5;
            bodyArt.sprite=RegionalWildlifeArt.Part(species);nearArt.sprite=RegionalWildlifeArt.Part(species,1);farArt.sprite=RegionalWildlifeArt.Part(species,2);
            width=frame.rect.width;float size=Mathf.Min(frame.rect.width,frame.rect.height)*(species==WildlifeSpecies.BarnOwl?.066f:species==WildlifeSpecies.Bumblebee?.014f:crawling?.019f:.060f);
            shade.sizeDelta=new Vector2(size*.30f,size*.095f);
            var sprite=bodyArt.sprite;float ratio=sprite.rect.height/sprite.rect.width;
            body.pivot=new Vector2(species==WildlifeSpecies.Bullfinch?.64f:species==WildlifeSpecies.BarnOwl?.66f:crawling?.48f:.56f,.015f);body.sizeDelta=new Vector2(size,size*ratio);body.anchoredPosition=Vector2.zero;
            Vector2 shoulder=new Vector2((.56f-body.pivot.x)*size,size*ratio*.58f);
            ConfigureWing(near,nearArt,shoulder,size*1.20f);ConfigureWing(far,farArt,shoulder,size*1.10f);
        }
        static void ConfigureWing(RectTransform rect,Image image,Vector2 shoulder,float span)
        {
            rect.pivot=new Vector2(.96f,.48f);rect.anchoredPosition=shoulder;rect.sizeDelta=new Vector2(span,span*image.sprite.rect.height/image.sprite.rect.width);
        }
        void PaintWing(RectTransform rect,Image image,float angle,float bend,float folded,float alpha)
        {
            bool bird=WildlifeMotion.IsBird(species);float opacity=crawling?0:bird?1-Mathf.SmoothStep(0,1,(folded-.50f)/.40f):1;
            image.color=new Color(1,1,1,alpha*opacity);image.enabled=opacity>.001f;
            rect.localScale=new Vector3(Mathf.Lerp(1,species==WildlifeSpecies.Bumblebee?.32f:.40f,folded),Mathf.Lerp(1,.54f,folded),1);
            rect.localRotation=Quaternion.Euler(0,bend*18,Mathf.Lerp(angle,19,folded));
        }
        void OnDisable(){if(actor!=null)actor.gameObject.SetActive(false);if(shade!=null)shade.gameObject.SetActive(false);Resting=false;}
    }
    public sealed class WildlifeContactShade:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();var r=rectTransform.rect;mesh.AddVert(r.center,color,Vector2.zero);
            const int count=20;var edge=color;edge.a=0;
            for(int i=0;i<count;i++){float a=i*Mathf.PI*2/count;mesh.AddVert(r.center+new Vector2(Mathf.Cos(a)*r.width*.5f,Mathf.Sin(a)*r.height*.5f),edge,Vector2.zero);}
            for(int i=0;i<count;i++)mesh.AddTriangle(0,i+1,(i+1)%count+1);
        }
    }
}
