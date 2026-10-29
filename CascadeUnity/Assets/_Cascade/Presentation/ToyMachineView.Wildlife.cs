using Cascade.Domain;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cascade.Presentation
{
    public enum WildlifePerchKind { EmptyPlatform, Receiver, Shape, Board, Frame }
    public readonly struct WildlifePerchTarget
    {
        public readonly WildlifePerchKind Kind;
        public readonly int Index,Revision;
        public readonly float X;
        public WildlifePerchTarget(WildlifePerchKind kind,int index,float x,int revision)
        {Kind=kind;Index=index;X=x;Revision=revision;}
    }

    public sealed partial class ToyMachineView
    {
        float wildlifeLastInput;
        int wildlifeActivity,wildlifeRevision;
        CascadeGame wildlifeGame;
        readonly Vector2[] wildlifeObstacles=new Vector2[8];
        public float WildlifeIdleSeconds=>Mathf.Max(0,windTime-wildlifeLastInput);
        public int WildlifeActivity=>wildlifeActivity;
        public int WildlifeRevision=>wildlifeRevision;
        public bool WildlifeSceneVisible=>gameplayView&&machine!=null&&machine.gameObject.activeInHierarchy
            &&Frame!=null&&Frame.Status==PuzzleStatus.Playing
            &&(wildlifeGame==null||wildlifeGame.CurrentScreen==GameScreen.Playing);
        public bool WildlifeCanLand=>WildlifeSceneVisible&&interaction!=null&&interaction.CanPlay
            &&!interaction.IsPreparing&&!interaction.IsPresenting&&heldWeight<0
            &&(wildlifeGame==null||!wildlifeGame.MechanicPopupOpen&&!wildlifeGame.PendingLesson.HasValue)
            &&(windTime-gustStarted>2.7f||Mathf.Abs(gustDirection)<.15f)&&Mathf.Abs(windAnticipation)<.1f;

        public void NotifyWildlifeInput(){wildlifeLastInput=windTime;wildlifeActivity++;}
        void TickWildlifeInput()
        {
            if(Mouse.current?.leftButton.isPressed==true||Touchscreen.current?.primaryTouch.press.isPressed==true
                ||Keyboard.current?.anyKey.isPressed==true||heldWeight>=0)NotifyWildlifeInput();
        }
        // Flight corridors use rendered wood/pieces, rather than oversized invisible UI cards.
        public bool TryWildlifeFlightBand(out Vector2 band)
        {
            band=Vector2.zero;if(!WildlifeSceneVisible||viewCamera==null||floors.Count==0)return false;
            float depth=Vector3.Dot(transform.position-viewCamera.transform.position,viewCamera.transform.forward);
            float bottom=viewCamera.ViewportToWorldPoint(new Vector3(0,.24f,depth)).y;
            float top=viewCamera.ViewportToWorldPoint(new Vector3(0,.82f,depth)).y;
            if(lowerAnchor!=null){lowerAnchor.GetWorldCorners(corners);bottom=Mathf.Max(bottom,corners[2].y+.20f);}
            int count=0;
            for(int f=0;f<floors.Count;f++){
                if(!Frame.FloorVisible(f)||floors[f].fracture>.2f)continue;
                float center=SurfacePoint(f,.5f,.30f).y,height=FloorFaceHeight(f);
                var floor=interaction.Attempt.Level.Floors[f];
                bool device=floor.Bomb!=null||floor.Key!=null||floor.Repair!=null||floor.OptionalStamp!=null||floor.Powerup!=null;
                float low=center-height*(device?4.2f:.9f),high=center+height*.9f;
                if(f==Frame.ActiveFloor)foreach(var piece in tokens)if(piece.renderer.enabled)high=Mathf.Max(high,piece.renderer.bounds.max.y+height*.7f);
                if(high<bottom||low>top)continue;
                wildlifeObstacles[count++]=new Vector2(low,high);
            }
            for(int i=1;i<count;i++){var item=wildlifeObstacles[i];int j=i-1;
                while(j>=0&&wildlifeObstacles[j].x>item.x){wildlifeObstacles[j+1]=wildlifeObstacles[j];j--;}wildlifeObstacles[j+1]=item;}
            float cursor=bottom,largest=0;
            for(int i=0;i<=count;i++){
                float edge=i<count?Mathf.Min(top,wildlifeObstacles[i].x):top;
                if(edge-cursor>largest){largest=edge-cursor;band=new Vector2(cursor,edge);}
                if(i<count)cursor=Mathf.Max(cursor,wildlifeObstacles[i].y);
            }
            return largest>.35f;
        }
        public bool TryChooseWildlifePerch(int preferred,float seed,bool occupied,out WildlifePerchTarget target,WildlifePerchTarget? avoid=null)
        {
            target=default;if(Frame==null||tokens.Count==0)return false;
            seed=Mathf.Clamp01(seed);int salt=Mathf.FloorToInt(seed*1000000);
            for(int i=0;i<5;i++){
                var kind=(WildlifePerchKind)((preferred+i)%5);
                if(!occupied&&kind!=WildlifePerchKind.EmptyPlatform&&kind!=WildlifePerchKind.Receiver&&kind!=WildlifePerchKind.Frame)continue;
                if(kind==WildlifePerchKind.Frame||kind==WildlifePerchKind.Shape)continue;
                if(kind==WildlifePerchKind.EmptyPlatform){
                    int count=floors.Count-Frame.ActiveFloor-1;
                    int offset=Mathf.FloorToInt(seed*count);
                    for(int n=0;n<count;n++){
                        int f=Frame.ActiveFloor+1+(offset+n)%count;
                        target=new WildlifePerchTarget(kind,f,Mathf.Lerp(.18f,.82f,WildlifeMotion.Hash(salt+n*47+13)),wildlifeRevision);
                        if(Available(target))return true;
                    }
                }else if(kind==WildlifePerchKind.Receiver){
                    for(int n=0;n<2;n++){
                        float x=Mathf.Lerp(.035f,.11f,WildlifeMotion.Hash(salt+83));
                        if((seed<.5f)==(n==0))x=1-x;
                        target=new WildlifePerchTarget(kind,0,x,wildlifeRevision);
                        if(Available(target))return true;
                    }
                }else if(kind==WildlifePerchKind.Shape){
                    int offset=Mathf.FloorToInt(seed*tokens.Count);
                    for(int t=0;t<tokens.Count;t++){
                        int slot=(offset+t)%tokens.Count;
                        if(Frame.Row[slot].Kind!=SymbolKind.Shape)continue;
                        target=new WildlifePerchTarget(kind,slot,.5f,wildlifeRevision);
                        if(Available(target))return true;
                    }
                }else{
                    for(int spot=0;spot<26;spot++){
                        float x=spot<24?Mathf.Lerp(.14f,.86f,WildlifeMotion.Hash(salt+spot*97+163))
                            :spot==24?Mathf.Lerp(.14f,.18f,seed):Mathf.Lerp(.82f,.86f,seed);
                        target=new WildlifePerchTarget(kind,Frame.ActiveFloor,x,wildlifeRevision);
                        if(Available(target))return true;
                    }
                }
            }
            target=default;return false;
            bool Available(WildlifePerchTarget candidate){
                if(avoid.HasValue&&candidate.Kind==avoid.Value.Kind&&candidate.Index==avoid.Value.Index
                    &&Mathf.Abs(candidate.X-avoid.Value.X)<.16f)return false;
                return TryWildlifeSupport(candidate,out _,out _);
            }
        }
        public bool TryWildlifeSupport(WildlifePerchTarget target,out Vector3 point,out float angle)
        {
            point=Vector3.zero;angle=0;
            if(!WildlifeSceneVisible||target.Revision!=wildlifeRevision)return false;
            if(target.Kind==WildlifePerchKind.Frame){
                return false;
            }else if(target.Kind==WildlifePerchKind.Receiver){
                if(lowerAnchor==null||!lowerAnchor.gameObject.activeInHierarchy)return false;
                lowerAnchor.GetWorldCorners(corners);
                if(receiverSkin!=null&&receiverSkin.enabled){
                    // The supporting wood is twelve pixels below the 499-pixel tray top.
                    // Follow its actual sliced transform, including sway, heave and roll.
                    point=receiverSkin.transform.TransformPoint(new Vector3((target.X-.5f)*receiverSkin.size.x/1.10f,(.5f-12f/499f)*receiverSkin.size.y,0));
                }else{
                    point=new Vector3(Mathf.Lerp(corners[0].x,corners[2].x,target.X),Mathf.Lerp(corners[0].y,corners[2].y,.94f),-.76f);
                    point+=ReceiverPointOffset(point);
                }
                angle=ReceiverAngle;point.z=-.76f;
            }else if(target.Kind==WildlifePerchKind.Shape){
                if(target.Index<0||target.Index>=tokens.Count||heldWeight==target.Index||!tokens[target.Index].transform.gameObject.activeInHierarchy)return false;
                var piece=tokens[target.Index];
                if(piecePoses[target.Index]!=PiecePose.Resting&&piecePoses[target.Index]!=PiecePose.Settling)return false;
                point=piece.transform.TransformPoint(new Vector3(piece.mesh.bounds.center.x,piece.mesh.bounds.max.y,0));
                point.z=-.76f;angle=piece.transform.eulerAngles.z;
            }else{
                int f=target.Index;
                if(f<0||f>=floors.Count||!Frame.FloorVisible(f)||floors[f].fracture>.03f)return false;
                if(target.Kind==WildlifePerchKind.EmptyPlatform&&f<=Frame.ActiveFloor)return false;
                if(target.Kind==WildlifePerchKind.Board&&f!=Frame.ActiveFloor)return false;
                point=TimberSurfacePoint(f,target.X);point.z=-.76f;angle=SurfaceAngle(f,target.X);
                if(target.Kind==WildlifePerchKind.Board){
                    float clearance=FloorSpan(f)*.032f;
                    foreach(var piece in tokens){
                        var bounds=piece.renderer.bounds;
                        if(point.x>bounds.min.x-clearance&&point.x<bounds.max.x+clearance)return false;
                    }
                }
            }
            var viewport=viewCamera.WorldToViewportPoint(point);
            return target.Kind==WildlifePerchKind.Frame?viewport.y>.20f&&viewport.y<.83f:viewport.x>.035f&&viewport.x<.965f&&viewport.y>.16f&&viewport.y<.84f;
        }
    }
}
