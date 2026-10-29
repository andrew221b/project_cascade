using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;
using TMPro;

namespace Cascade.Presentation
{
    // Rehearses an already committed result. No completion callback can alter an attempt.
    public sealed class OutcomeCeremony : MonoBehaviour
    {
        CascadeGame game;
        RectTransform root,portrait,ribbon;Transform ledge;
        Image hero;
        TextMeshProUGUI title;
        OutcomeLightGraphic light;
        CanvasGroup group;
        Sprite hopeful,thinking;
        Vector2 heroRest;
        readonly Vector3[] portraitCorners=new Vector3[4],receiverCorners=new Vector3[4];
        public bool Running {get;private set;}
        public bool Success {get;private set;}
        public float Progress {get;private set;}=1;
        bool checkpoint;
        internal RectTransform DescentRibbon=>checkpoint&&ribbon!=null&&ribbon.gameObject.activeInHierarchy?ribbon:null;
        public void Initialize(CascadeGame owner,RectTransform story,Image friend,TextMeshProUGUI heading,OutcomeLightGraphic glow)
        {
            game=owner;root=story;ribbon=heading.transform.parent as RectTransform;hero=friend;portrait=friend.rectTransform;heroRest=portrait.anchoredPosition;title=heading;light=glow;
            ledge=root.Find("Mika's outcome ledge");
            group=root.gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
            hopeful=Resources.Load<Sprite>("CascadeArt/Journey/Mika");thinking=Resources.Load<Sprite>("CascadeArt/Journey/MikaThinking");
            root.gameObject.SetActive(false);ribbon.gameObject.SetActive(false);
        }
        public IEnumerator Play(bool success,bool isCheckpoint)
        {
            Success=success;checkpoint=isCheckpoint;
            // Failure goes directly to its stable result screen; never expose the old "Try again" ceremony.
            if(!success){Running=false;Progress=1;root.gameObject.SetActive(false);ribbon.gameObject.SetActive(false);yield break;}
            game.BeginOutcomeReceiver();
            // The receiver has already landed. Final victory goes straight to its star screen.
            if(success&&!checkpoint){
                root.gameObject.SetActive(false);ribbon.gameObject.SetActive(false);
                Running=true;Progress=0;game.Feedback.Haptic();
                // Let the exact row read as landed before the result dissolves in.
                // Do not relocate the receiver or restart its landing spring here.
                float hold=0,holdDuration=game.ReducedMotion?0:.24f;
                try{
                    while(hold<holdDuration){
                        if(!game.IntroductionAnimationAllowed){yield return null;continue;}
                        Progress=Mathf.Clamp01(hold/holdDuration);
                        game.Machine.SetReceiverLight(Mathf.SmoothStep(0,.65f,Progress));
                        hold+=Mathf.Min(.05f,Time.unscaledDeltaTime);yield return null;
                    }
                }finally{Running=false;Progress=1;}
                yield break;
            }
            Running=true;Progress=0;
            root.gameObject.SetActive(true);hero.gameObject.SetActive(!checkpoint);hero.sprite=success?hopeful:thinking??hopeful;
            if(ledge!=null)ledge.gameObject.SetActive(!checkpoint);
            ribbon.gameObject.SetActive(true);PlaceRibbon();AlignLedge();
            title.SetCopy(checkpoint?new UiCopy("Stage {0}/3",game.Descent.StageIndex+1):"Try again");
            if(success&&!checkpoint)game.Feedback.Haptic();
            // Give a checkpoint landing a readable beat before the stable checkpoint
            // screen takes over. The ribbon remains visible there until the player
            // chooses to go lower.
            float duration=game.ReducedMotion?.14f:checkpoint?1.65f:success?1.35f:1.05f;
            float elapsed=0;int lit=-1;bool seeds=false;
            try {
                while(elapsed<duration){
                    Progress=Mathf.Clamp01(elapsed/duration);Paint(Progress);
                    if(success){
                        int step=Mathf.Min(game.Machine.TokenCount-1,(int)(Mathf.Clamp01((Progress-.10f)/.48f)*game.Machine.TokenCount));
                        if(step>lit){lit=step;game.Feedback.Burst(game.Machine.TokenPosition(step),new Color(1,.83f,.46f,.8f),checkpoint?2:3,game.ReducedMotion);}
                        if(!seeds&&Progress>.54f&&!checkpoint){seeds=true;game.Feedback.LanternSeeds(game.Machine.TokenPosition(game.Machine.TokenCount/2),game.ReducedMotion);}
                    }
                    elapsed+=Time.unscaledDeltaTime;yield return null;
                }
            } finally {Settle();}
        }
        void Paint(float t)
        {
            game.PlaceOutcomeReceiver(t,Success);
            float reveal=Mathf.SmoothStep(0,1,Mathf.Clamp01((t-.24f)/.36f));
            group.alpha=game.ReducedMotion?1:reveal;
            if(ribbon!=null){float ease=game.ReducedMotion?1:1-Mathf.Pow(1-Mathf.Clamp01((t-.18f)/.48f),3);ribbon.localScale=new Vector3(Mathf.Lerp(.86f,1,ease),Mathf.Lerp(.68f,1,ease),1);}
            float bow=game.ReducedMotion?0:Mathf.Sin(Mathf.Clamp01((t-.25f)/.75f)*Mathf.PI);
            portrait.anchoredPosition=heroRest+Vector2.up*(Success?8*bow:-3*bow);
            portrait.localRotation=Quaternion.Euler(0,0,Success?-1.5f*bow:2*bow);
            portrait.localScale=Vector3.one*(1+(Success?.026f:-.016f)*bow);
            light.SetLight(Success?reveal*(.75f+.25f*bow):0,t);
            game.Machine.SetReceiverLight(Success?reveal:.04f);
            for(int i=0;i<game.Machine.TokenCount;i++){
                float local=Mathf.Clamp01((t-i*.036f)/.58f);
                float wave=game.ReducedMotion?0:Mathf.Sin(local*Mathf.PI*2)*Mathf.Exp(-local*4);
                float lift=Success?Mathf.Max(0,wave)*.15f:Mathf.Max(0,wave)*.055f;
                game.OffsetPresentedToken(i,Vector3.up*lift,new Vector2(1+wave*.025f,1-wave*.04f));
                game.Machine.SetPiecePose(i,PiecePose.Delivered,Success?wave*(i%2==0?2:-2):wave*(i%2==0?8:-8));
            }
        }
        public void Settle()
        {
            if(game==null)return;
            if(Running&&(checkpoint||!Success))game.PlaceOutcomeReceiver(1,Success);
            Running=false;Progress=1;if(ribbon!=null)ribbon.localScale=Vector3.one;
            portrait.anchoredPosition=heroRest;portrait.localScale=Vector3.one;portrait.localRotation=Quaternion.identity;group.alpha=1;
            light.SetLight(Success&&!checkpoint?.75f:0,1);
            for(int i=0;i<game.Machine.TokenCount;i++){game.OffsetPresentedToken(i,Vector3.zero,Vector2.one);game.Machine.SetPiecePose(i,PiecePose.Delivered);}
            game.Machine.SetReceiverLight(Success?.65f:0);
        }
        public void HideReferenceFailure(){if(root!=null)root.gameObject.SetActive(false);if(ribbon!=null)ribbon.gameObject.SetActive(false);}
        public void ShowResult(GameScreen screen)
        {
            bool terminal=screen==GameScreen.Checkpoint;
            if(Running&&screen==GameScreen.Playing)return;
            root.gameObject.SetActive(terminal);ribbon.gameObject.SetActive(terminal);if(!terminal){game.Machine.SetReceiverLight(0);return;}
            checkpoint=screen==GameScreen.Checkpoint;Success=checkpoint;hero.gameObject.SetActive(!checkpoint);hero.sprite=Success?hopeful:thinking??hopeful;
            if(ledge!=null)ledge.gameObject.SetActive(!checkpoint);
            PlaceRibbon();
            title.SetCopy(checkpoint?new UiCopy("Stage {0}/3",game.Descent.StageIndex+1):"Try again");Settle();
        }
        public void Layout(float unit,RectTransform receiver)
        {
            root.anchoredPosition=Vector2.zero;
            portrait.GetWorldCorners(portraitCorners);receiver.GetWorldCorners(receiverCorners);
            float foot=root.parent.InverseTransformPoint(portraitCorners[0]).y;
            float targetTop=root.parent.InverseTransformPoint(receiverCorners[2]).y;
            root.anchoredPosition=Vector2.up*Mathf.Max(0,targetTop+28*unit-foot);
            AlignLedge();
            var image=ribbon.GetComponent<Image>();var bounds=ribbon.rect;float aspect=image.sprite.rect.width/image.sprite.rect.height;
            float width=Mathf.Min(bounds.width,bounds.height*aspect),height=Mathf.Min(bounds.height,bounds.width/aspect);
            if(!image.preserveAspect){width=bounds.width;height=bounds.height;}
            title.rectTransform.anchorMin=title.rectTransform.anchorMax=new Vector2(.5f,.5f);
            title.rectTransform.sizeDelta=new Vector2(width*.72f,height*.60f);title.rectTransform.anchoredPosition=Vector2.up*height*.025f;
            title.margin=new Vector4(3*unit,2*unit,3*unit,2*unit);title.fontSizeMin=17*unit;title.fontSizeMax=24*unit*(game.Preferences.LargeText?1.12f:1);
        }
        void PlaceRibbon()
        {
            ribbon.anchorMin=new Vector2(.045f,checkpoint?.66f:.79f);
            ribbon.anchorMax=new Vector2(.955f,checkpoint?.80f:.93f);
            ribbon.offsetMin=ribbon.offsetMax=Vector2.zero;
        }
        void AlignLedge()
        {
            if(!(ledge is RectTransform board)||hero.sprite==null)return;
            var bounds=portrait.rect;var sprite=hero.sprite;float aspect=sprite.rect.width/sprite.rect.height;
            float height=hero.preserveAspect?Mathf.Min(bounds.height,bounds.width/aspect):bounds.height;
            // Respect the sprite's transparent bottom padding and Image's aspect-fit letterbox.
            float foot=portrait.localPosition.y+bounds.yMin+(bounds.height-height)*.5f
                +DataUtility.GetPadding(sprite).y/sprite.rect.height*height;
            float thickness=root.rect.height*.017f;
            board.anchorMin=board.anchorMax=new Vector2(.5f,.5f);
            board.sizeDelta=new Vector2(root.rect.width*.46f,thickness);
            // The timber artwork occupies the middle of its thin graphic, rather than its top edge.
            board.anchoredPosition=new Vector2(0,foot-root.rect.center.y+thickness*.25f);
        }
        void OnDisable(){if(Running)Settle();}
#if UNITY_EDITOR
        public void SetPreview(float progress){Paint(Mathf.Clamp01(progress));}
#endif
    }

    // Warm light blooms from Mika's lantern; soft edges, one finite ceremony.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class OutcomeLightGraphic : MaskableGraphic
    {
        float strength,phase;
        public void SetLight(float value,float time){strength=value;phase=time;raycastTarget=false;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper h)
        {
            h.Clear();if(strength<=0)return;var r=rectTransform.rect;
            var origin=r.min+Vector2.Scale(r.size,new Vector2(.70f,.38f));
            const int rays=12;
            for(int i=0;i<rays;i++){
                float angle=(i/(float)rays)*Mathf.PI*2+.05f*phase;
                float half=.035f+(i%3)*.012f;
                float length=Mathf.Min(r.width,r.height)*(.24f+(i%4)*.035f);
                var tint=new Color(1,.79f,.39f,strength*.20f);int start=h.currentVertCount;
                h.AddVert(origin,tint,Vector2.zero);tint.a=0;
                h.AddVert(origin+new Vector2(Mathf.Cos(angle-half),Mathf.Sin(angle-half))*length,tint,Vector2.zero);
                h.AddVert(origin+new Vector2(Mathf.Cos(angle+half),Mathf.Sin(angle+half))*length,tint,Vector2.zero);h.AddTriangle(start,start+1,start+2);
            }
        }
    }
}
