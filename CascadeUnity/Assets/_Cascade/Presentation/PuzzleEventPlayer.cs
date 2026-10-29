using System.Collections;
using System.Linq;
using Cascade.Domain;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed class PuzzleEventPlayer : MonoBehaviour
    {
        CascadeGame game;
        MotionProfile profile;
        ToyFeedbackPool feedback;
        Coroutine playback;
        bool interruptibleExchange;
        bool restoringRow;
        int startedFrame;
        bool releaseCaptured,releasedExchange;
        readonly Vector3[] from = new Vector3[8], to = new Vector3[8], oldPositions = new Vector3[8];
        readonly Vector3[] oldScales = new Vector3[8];
        readonly Vector2[] fromStretch = new Vector2[8];
        readonly float[] oldAngles = new float[8], fromAngles = new float[8];
        readonly SymbolId[] oldRow = new SymbolId[8];
        readonly bool[] used = new bool[8];
        readonly int[] sourceSlots=new int[8];
        readonly float[] selection = new float[8], selectionFrom = new float[8], selectionTime = new float[8];
        readonly bool[] selectionTarget = new bool[8];
        public bool IsRunning { get; private set; }
        public int EventIndex { get; private set; } = -1;
        public PuzzleEventKind CurrentEvent { get; private set; }
        public int CompletedPresentations { get; private set; }
        public void Initialize(CascadeGame owner, MotionProfile motion, ToyFeedbackPool pool) { game = owner; profile = motion; feedback = pool; }
        public void Play(TransitionResult result,float leadIn=0)
        {
            if (!result.Accepted) return;
            Skip(); IsRunning = true; EventIndex = -1;releaseCaptured=false;
            startedFrame=Time.frameCount;restoringRow=false;
            interruptibleExchange=result.State.Status==PuzzleStatus.Playing&&result.DurabilityCost<=1;
            foreach(var e in result.Events)
                if(e.Kind!=PuzzleEventKind.CommandAccepted&&e.Kind!=PuzzleEventKind.RowPermuted&&e.Kind!=PuzzleEventKind.DurabilityResolved)
                    interruptibleExchange=false;
            for (int i = 0; i < selection.Length; i++) { selection[i] = selectionFrom[i] = 0; selectionTarget[i] = false; selectionTime[i] = profile.selection; }
            playback = StartCoroutine(Run(result,leadIn));
        }
        public void SettleExchangeForInput()
        {
            // No queued command and no skipped contact/fall. A deliberate new
            // gesture may finish an ordinary, already committed exchange early.
            if(IsRunning&&interruptibleExchange&&Time.frameCount>startedFrame&&!PresentationPaused)Skip();
        }
        public void PlayUndo(float leadIn=0)
        {
            Skip();IsRunning=true;EventIndex=-1;releaseCaptured=true;releasedExchange=false;interruptibleExchange=false;restoringRow=true;
            game.Interaction.BeginRestorePresentation();
            playback=StartCoroutine(Restore(leadIn));
        }
        IEnumerator Restore(float leadIn)
        {
            try{
                yield return ToolLeadIn(leadIn);
                CaptureRow(game.VisibleFrame);
                game.RenderFrame(new PresentationFrame(game.Interaction.Attempt.Current));
                MapRow(game.VisibleFrame);feedback.Sound(ToyCue.Swap);feedback.Haptic(ToyHapticMoment.Swap);
                yield return Travel(.40f,true,false);
            }finally{Finish();}
        }
        public void Skip()
        {
            if (!IsRunning) return;
            if (playback != null) StopCoroutine(playback); playback = null; Finish();
        }
        void OnDisable() { Skip(); }
        void Finish()
        {
            if (!IsRunning) return; IsRunning = false; playback = null; CompletedPresentations++;
            interruptibleExchange=restoringRow=false;
            if(windAir!=null)Destroy(windAir.gameObject);
            feedback?.RowPhysics?.End();
            game?.CompleteTouchExchange();
            if(game!=null&&game.CanPresent){game.Machine.PrepareWind(true,0);game.Machine.ClearReceiverAttraction();game.ReactionBanner.Clear();game.SettleOutcome();}
            if (game != null && game.Interaction != null) { game.Interaction.FinishPresentation(); if(game.CanPresent)game.SyncPresentation(); }
        }
        bool windTravel;
        WindWarningGraphic windAir;
        void BeginWindAir(bool right){
            if(game.ReducedMotion)return;
            var go=new GameObject("Gust air trails",typeof(RectTransform),typeof(CanvasRenderer),typeof(WindWarningGraphic));
            go.transform.SetParent(game.SlotViews[0].GetComponentInParent<UnityEngine.Canvas>().transform,false);
            var rect=(RectTransform)go.transform;rect.anchorMin=new Vector2(.06f,.30f);rect.anchorMax=new Vector2(.94f,.70f);rect.offsetMin=rect.offsetMax=Vector2.zero;
            windAir=go.GetComponent<WindWarningGraphic>();windAir.Right=right;windAir.DrawTips=false;windAir.raycastTarget=false;
        }
        IEnumerator ToolLeadIn(float duration)
        {
            float elapsed=0;
            while(elapsed<duration){if(!PresentationPaused)elapsed+=Mathf.Min(.05f,Time.unscaledDeltaTime);yield return null;}
        }
        IEnumerator Run(TransitionResult result,float leadIn)
        {
            var frame = game.VisibleFrame; var level = game.Interaction.Attempt.Level;
            try {
                yield return ToolLeadIn(leadIn);
                for (int index = 0; index < result.Events.Count; index++) {
                    while(PresentationPaused)yield return null;
                    var e = result.Events[index]; EventIndex = index; CurrentEvent = e.Kind;
                    if (e.Kind == PuzzleEventKind.CommandAccepted) {frame.Read(e,level);continue;}
                    bool reduced = game.ReducedMotion;
                    if (e.Kind == PuzzleEventKind.RowPermuted || e.Kind == PuzzleEventKind.EntryPermuted || e.Kind==PuzzleEventKind.WindPermuted) {
                        windTravel=e.Kind==PuzzleEventKind.WindPermuted;
                        if(windTravel){
                            bool right=e.Value==(int)EntryTransform.ShiftRightOne;
                            yield return WarnWind(right);BeginWindAir(right);game.Machine.CueWind(right);game.CueDeviceWind(right);
                            feedback.Haptic(ToyHapticMoment.Gust);
                            game.SetPresentationFeedback("The wind moved the row");
                        }
                        if(e.Kind==PuzzleEventKind.EntryPermuted&&game.IsWindFloor(e.Floor)) {
                            game.Machine.CueWind(level.Floors[e.Floor].EntryTransform==EntryTransform.ShiftRightOne);
                            game.CueDeviceWind(level.Floors[e.Floor].EntryTransform==EntryTransform.ShiftRightOne);
                            feedback.Haptic(ToyHapticMoment.Gust);
                            game.SetPresentationFeedback("The wind moved the row");
                            game.AnnounceReaction("The wind moved the row","");
                        }
                        CaptureRow(frame); frame.Read(e, level); game.RenderFrame(frame); MapRow(frame);
                        feedback.Sound(ToyCue.Swap);
                        if(!windTravel&&!(e.Kind==PuzzleEventKind.EntryPermuted&&game.IsWindFloor(e.Floor)))feedback.Haptic(ToyHapticMoment.Swap);
                        yield return Travel(windTravel?.95f:profile.swap, !reduced, false);windTravel=false;if(windAir!=null)Destroy(windAir.gameObject);
                    } else if (e.Kind == PuzzleEventKind.FloorExited) {
                        int next=e.Floor+1;if(next<level.Floors.Count&&game.IsWindFloor(next))yield return WarnWind(level.Floors[next].EntryTransform==EntryTransform.ShiftRightOne);
                        if(frame.Durability<=0){
                            feedback.Sound(ToyCue.Fragile);yield return BreakWood(e.Floor);
                        }else yield return Recoil(e.Floor, profile.fracture);
                        frame.Read(e, level); game.RenderFrame(frame);
                    } else if (e.Kind == PuzzleEventKind.FloorEntered || e.Kind == PuzzleEventKind.ReceiverEvaluated) {
                        CaptureRow(frame); frame.Read(e, level); bool arrival = e.Kind == PuzzleEventKind.ReceiverEvaluated;
                        game.RenderFrame(frame, arrival); MapRow(frame);
                        yield return Travel(profile.fall, false, true);feedback.Sound(ToyCue.Land);feedback.Haptic(ToyHapticMoment.Landing);
                        if(arrival&&e.Value!=1)yield return RejectDelivery();else yield return Land(arrival);
                        if (arrival) {
                            feedback.Sound(e.Value == 1 ? ToyCue.Success : ToyCue.Reject);
                            yield return game.PlayOutcome(e.Value==1);
                            game.CompleteDeliveryPose();
                        }
                    } else {
                        Vector3 origin = EventOrigin(e, level); frame.Read(e, level); game.RenderFrame(frame);
                        game.CueDevice(e);
                        switch (e.Kind) {
                            case PuzzleEventKind.BombTicked:
                                game.SetPresentationFeedback(new UiCopy("Bomb: {0} moves left",e.Value)); yield return Hold(.07f); break;
                            case PuzzleEventKind.BombExploded:
                                game.SetPresentationFeedback(new UiCopy("Blast: −{0} durability",e.Value)); feedback.Sound(ToyCue.Bomb); feedback.Haptic(ToyHapticMoment.Bomb);
                                game.AnnounceReaction(new UiCopy("Blast: −{0} durability",e.Value),"Bomb");
                                yield return Recoil(e.Floor, Mathf.Max(.30f,profile.bomb));yield return HoldReaction(); break;
                            case PuzzleEventKind.BombDefused:
                                game.SetPresentationFeedback("Bomb defused"); feedback.Sound(ToyCue.Defuse);
                                game.AnnounceReaction("Bomb defused","BombSafe");yield return HoldReaction();break;
                            case PuzzleEventKind.KeyOpened:
                                game.SetPresentationFeedback(new UiCopy("Branch {0} cleared",e.Value+1)); feedback.Sound(ToyCue.Unlock); feedback.Haptic();
                                game.AnnounceReaction(new UiCopy("Branch {0} cleared",e.Value+1),"PruningSaw");yield return Recoil(e.Floor,profile.contact);yield return HoldReaction(); break;
                            case PuzzleEventKind.Repaired:
                                game.SetPresentationFeedback(new UiCopy("Repair +{0}",e.Value)); feedback.Sound(ToyCue.Repair);
                                game.AnnounceReaction(new UiCopy("+{0} durability",e.Value),"RepairKit");yield return Recoil(e.Floor,profile.contact);yield return HoldReaction(); break;
                            case PuzzleEventKind.RibbonCollected:
                            case PuzzleEventKind.HourglassCollected:
                                game.SetPresentationFeedback(e.Kind==PuzzleEventKind.RibbonCollected?"One far swap ready":"Bomb paused for two swaps");feedback.Sound(ToyCue.Stamp);feedback.Haptic();
                                bool ribbon=e.Kind==PuzzleEventKind.RibbonCollected;int tool=ribbon?9:10;
                                game.AnnounceReaction(ribbon?"One far swap ready":"Bomb paused for two swaps",ribbon?"RibbonSpool":"FrostFlask");
                                yield return HoldReaction();break;
                            case PuzzleEventKind.BombFrozen:
                                game.SetPresentationFeedback("The bomb waits. The wood still wears down.");yield return Hold(.12f);break;
                            case PuzzleEventKind.StampCollected:
                                game.SetPresentationFeedback("Stamp collected"); feedback.Sound(ToyCue.Stamp);
                                game.AnnounceReaction("Stamp: +100 points","Stamp");yield return HoldReaction();break;
                            case PuzzleEventKind.BridgeConsumed:
                                game.SetPresentationFeedback("One-use bridge closed");game.AnnounceReaction("One-use bridge closed","");feedback.Sound(ToyCue.Bridge);yield return Hold(profile.contact);break;
                            case PuzzleEventKind.DurabilityResolved:
                                if (result.DurabilityCost > 1) {
                                    foreach(var rowEvent in result.Events)if(rowEvent.Kind==PuzzleEventKind.RowPermuted)
                                        foreach(int edge in rowEvent.AffectedEdges)if(level.Floors[e.Floor].FragileEdges.Contains(edge)){
                                            game.Machine.CrackFloor(e.Floor,edge);
                                            float u=.045f+.91f*(edge+1f)/game.Machine.TokenCount;
                                            feedback.WoodChips(game.Machine.TimberSurfacePoint(e.Floor,u),3,game.ReducedMotion,game.Machine.FloorSpan(e.Floor)*.12f);
                                        }
                                    feedback.Sound(ToyCue.Fragile);feedback.Haptic(ToyHapticMoment.WoodCrack);yield return Recoil(e.Floor,Mathf.Max(.28f,profile.fracture));
                                }
                                break;
                        }
                    }
                }
            }
            finally { Finish(); }
        }
        void CaptureRow(PresentationFrame frame)
        {
            releasedExchange=!releaseCaptured&&game.ReleasedDrag;
            for (int i = 0; i < frame.Row.Count; i++) { oldRow[i] = frame.Row[i]; oldPositions[i] = game.Machine.TokenVisualPosition(i); oldScales[i] = game.Machine.TokenVisualScale(i); oldAngles[i] = Mathf.DeltaAngle(0,game.Machine.TokenVisualAngle(i)); used[i] = false; }
            if(!releaseCaptured&&game.ReleasedDrag&&game.ReleasedPartnerSlot>=0)oldPositions[game.ReleasedPartnerSlot]=game.ReleasedPartnerPosition;
            releaseCaptured=true;
        }
        void MapRow(PresentationFrame frame)
        {
            // Reserve unchanged occurrences first: a held duplicate must not be replaced
            // by an identical body that stayed in another slot.
            for(int i=0;i<frame.Row.Count;i++)used[i]=oldRow[i]==frame.Row[i];
            for (int destination = 0; destination < frame.Row.Count; destination++) {
                int source = destination;
                if(oldRow[destination]!=frame.Row[destination]) {
                    source=0;while(source<frame.Row.Count && (used[source] || oldRow[source]!=frame.Row[destination]))source++;
                }
                if (source == frame.Row.Count) source = destination; // Defensive view fallback; never invent a gameplay permutation.
                sourceSlots[destination]=source;
                used[source] = true; from[destination] = oldPositions[source]; to[destination] = game.Machine.TokenPosition(destination)+game.Machine.TokenSupportOffset(destination);
                var basis=game.Machine.TokenBaseScale(destination);
                fromStretch[destination]=new Vector2(oldScales[source].x/basis.x,oldScales[source].y/basis.y);fromAngles[destination]=oldAngles[source];
                game.Machine.SetPiecePose(destination,PiecePose.Exchanging,game.ReducedMotion?0:fromAngles[destination]);
                game.OffsetPresentedToken(destination, game.ReducedMotion ? Vector3.zero : from[destination]-game.Machine.TokenPosition(destination), game.ReducedMotion?Vector2.one:fromStretch[destination]);
            }
        }
        IEnumerator Travel(float duration, bool arcs, bool falling)
        {
            if (game.ReducedMotion) { yield return Hold(profile.reducedStep); yield break; }
            if(falling){yield return GuidedFall();yield break;}
            duration=Mathf.Max(duration,releasedExchange?.36f:.32f);
            float time = 0;int count=game.VisibleFrame.Row.Count;
            if(falling){float drop=0;for(int i=0;i<count;i++)drop=Mathf.Max(drop,from[i].y-to[i].y);duration=Mathf.Max(duration,Mathf.Sqrt(2*drop/9.81f)+.07f);}
            else if(arcs)for(int i=0;i<count;i++)if(Mathf.Abs(from[i].x-to[i].x)>.01f)game.Machine.ContactImpulse(game.VisibleFrame.ActiveFloor,i,-.16f,Mathf.Sign(to[i].x-from[i].x));
            for(int i=0;i<count;i++)game.Machine.SetPiecePose(i,falling?PiecePose.Falling:PiecePose.Exchanging);
            while (time < duration) {
                if(PresentationPaused){yield return null;continue;}
                if(windAir!=null)windAir.Paint(time*1.8f,false);
                float t = Mathf.Clamp01(time / duration), eased = 1-Mathf.Pow(1-t,3);
                for (int i = 0; i < game.VisibleFrame.Row.Count; i++) {
                    float local=falling?Mathf.Clamp01((t-(i%3)*.035f)/(1-(i%3)*.035f)):t;
                    Vector3 position = Vector3.Lerp(from[i], to[i], eased);
                    if(falling){
                        float flight=Mathf.Max(0,time-(i%3)*.025f);
                        position.y=Mathf.Max(to[i].y,from[i].y-4.905f*flight*flight);
                        position.x+=game.Machine.WindAt(Mathf.Min(game.VisibleFrame.ActiveFloor,game.Interaction.Attempt.Level.Floors.Count-1))*.055f*Mathf.Sin(local*Mathf.PI);
                    }
                    if (arcs && Mathf.Abs(from[i].x-to[i].x) > .01f){
                        bool placed=releasedExchange&&sourceSlots[i]==game.ReleasedTokenSlot;
                        float route=placed?t:Mathf.Clamp01((t-.08f)/.92f);
                        float smooth=route*route*(3-2*route);
                        position=Vector3.Lerp(from[i],to[i],smooth);
                        if(!placed){
                            float arc=restoringRow?(to[i].x>from[i].x?Mathf.Max(.55f,game.Machine.TokenBodySize(i).y*1.08f):.04f)
                                :Mathf.Max(profile.swapArc,windTravel?.8f:.32f);
                            position.y+=Mathf.Sin(route*Mathf.PI)*arc;
                        }
                        position.z=Mathf.Lerp(from[i].z,placed?-.52f:-.8f,Mathf.Sin(t*Mathf.PI));
                    }
                    float angle=Mathf.Sin(local*Mathf.PI)*(i%2==0?1:-1)*(falling?13:windTravel?24:9);
                    if(restoringRow&&sourceSlots[i]==i)angle=0;
                    if(falling)angle*=Mathf.SmoothStep(0,1,Mathf.Clamp01((position.y-to[i].y)/.3f));
                    game.OffsetPresentedToken(i, position-game.Machine.TokenPosition(i), Vector2.Lerp(fromStretch[i],Vector2.one,eased));
                    game.Machine.SetPiecePose(i,falling?PiecePose.Falling:PiecePose.Exchanging,Mathf.Lerp(fromAngles[i],0,eased)+angle);
                }
                time += Time.unscaledDeltaTime; yield return null;
            }
            while(PresentationPaused)yield return null;
            for (int i = 0; i < game.VisibleFrame.Row.Count; i++){game.OffsetPresentedToken(i, Vector3.zero, Vector2.one);game.Machine.SetPiecePose(i,falling?PiecePose.Settling:PiecePose.Resting);}
            for(int i=0;i<count;i++)if(falling||arcs&&Mathf.Abs(from[i].x-to[i].x)>.01f)
                game.Machine.ContactImpulse(game.VisibleFrame.ActiveFloor,i,falling?Mathf.Clamp(duration*.7f,.3f,.8f)/count:.42f,falling?0:Mathf.Sign(to[i].x-from[i].x)*.45f);
        }
        IEnumerator GuidedFall()
        {
            int count=game.VisibleFrame.Row.Count;bool arrival=game.VisibleFrame.Status!=PuzzleStatus.Playing;
            bool accepted=game.VisibleFrame.Status==PuzzleStatus.Won;
            float drop=0;
            for(int i=0;i<count;i++){
                var seat=arrival?game.Machine.ReceiverLandingPosition(i):to[i];
                drop=Mathf.Max(drop,from[i].y-seat.y);game.Machine.SetPiecePose(i,PiecePose.Falling);
            }
            float duration=Mathf.Max(.30f,Mathf.Sqrt(2*Mathf.Max(0,drop)/9.81f)+(arrival?.23f:.10f)),time=0;
            try{
                while(time<duration){
                    if(PresentationPaused){yield return null;continue;}
                    if(game.ReducedMotion)break;
                    float t=Mathf.Clamp01(time/duration);
                    for(int i=0;i<count;i++){
                        var seat=arrival?game.Machine.ReceiverLandingPosition(i):game.Machine.TokenPosition(i)+game.Machine.TokenSupportOffset(i);
                        var position=DeliveryFlight.Fall(from[i],seat,t,arrival);
                        // Small in-place turn settles before contact; no lateral launch or magnetic return.
                        float angle=Mathf.LerpAngle(fromAngles[i],0,t)+Mathf.Sin(t*Mathf.PI)*(i%2==0?1:-1)*4;
                        game.Machine.SetPiecePose(i,PiecePose.Falling,angle);
                        game.OffsetPresentedToken(i,position-game.Machine.TokenPosition(i),Vector2.Lerp(fromStretch[i],Vector2.one,t*t*(3-2*t)));
                    }
                    if(arrival){float light=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.38f,.83f,t));game.Machine.PaintReceiverAttraction(light,time,accepted);game.Machine.SetReceiverLight(light*.65f);}
                    time+=Mathf.Min(.05f,Time.unscaledDeltaTime);yield return null;
                }
                for(int i=0;i<count;i++){
                    var seat=arrival?game.Machine.ReceiverLandingPosition(i):game.Machine.TokenPosition(i)+game.Machine.TokenSupportOffset(i);
                    game.Machine.SetPiecePose(i,PiecePose.Falling);
                    game.OffsetPresentedToken(i,seat-game.Machine.TokenPosition(i),Vector2.one);
                    if(!arrival)game.Machine.ContactImpulse(game.VisibleFrame.ActiveFloor,i,.55f/count,0);
                }
                if(arrival&&accepted){
                    float fade=0;
                    while(fade<.22f){if(PresentationPaused){yield return null;continue;}game.Machine.PaintReceiverAttraction(1-fade/.22f,time+fade,true);fade+=Mathf.Min(.05f,Time.unscaledDeltaTime);yield return null;}
                }
            }finally{game.Machine.ClearReceiverAttraction();}
        }
        IEnumerator RejectDelivery()
        {
            int count=game.VisibleFrame.Row.Count;for(int i=0;i<count;i++)from[i]=game.Machine.TokenVisualPosition(i);
            float time=0,duration=game.ReducedMotion?profile.reducedStep:.72f;
            while(time<duration){
                if(PresentationPaused){yield return null;continue;}
                float t=Mathf.Clamp01(time/duration);
                for(int i=0;i<count;i++){
                    var seat=game.Machine.TokenPosition(i)+game.Machine.ReceiverPointOffset(game.Machine.TokenPosition(i));
                    var position=game.ReducedMotion?seat:DeliveryFlight.Reject(from[i],seat,t,i,count);
                    game.Machine.SetPiecePose(i,PiecePose.Falling,game.ReducedMotion?0:Mathf.Sin(t*Mathf.PI)*(i-(count-1)*.5f)*12);
                    game.OffsetPresentedToken(i,position-game.Machine.TokenPosition(i),Vector2.one);
                }
                game.Machine.PaintReceiverAttraction(game.ReducedMotion?0:Mathf.Max(0,1-t*2.5f),time,false);
                time+=Mathf.Min(.05f,Time.unscaledDeltaTime);yield return null;
            }
            game.Machine.ClearReceiverAttraction();
            for(int i=0;i<count;i++){game.Machine.SetPiecePose(i,PiecePose.Delivered);game.OffsetPresentedToken(i,Vector3.zero,Vector2.one);}
        }
        IEnumerator BreakWood(int floor)
        {
            game.AnnounceReaction("Wood gives way!","");
            if(game.ReducedMotion){feedback.WoodSnap();feedback.Haptic(.65f);feedback.WoodChips(game.Machine.FloorPosition(floor),2,true,game.Machine.FloorSpan(floor));yield return Hold(profile.reducedStep);yield break;}
            feedback.Haptic(ToyHapticMoment.WoodBreak);
            float time=0;const float duration=.58f;bool splintered=false,tail=false;
            while(true){
                if(PresentationPaused){yield return null;continue;}
                float t=Mathf.Clamp01(time/duration);
                float fracture=t<.32f?.24f*Mathf.SmoothStep(0,1,t/.32f):Mathf.Lerp(.24f,1,Mathf.SmoothStep(0,1,(t-.32f)/.68f));
                game.Machine.FractureFloor(floor,fracture);
                if(!splintered&&t>=.40f){splintered=true;feedback.WoodSnap();game.Machine.ContactImpulse(floor,game.Machine.TokenCount/2,.65f,0);feedback.WoodChips(game.Machine.FloorPosition(floor),14,false,game.Machine.FloorSpan(floor)*.96f);}
                if(!tail&&t>=.64f){tail=true;feedback.WoodChips(game.Machine.FloorPosition(floor),8,false,game.Machine.FloorSpan(floor)*.92f);}
                if(t>=.76f){game.Machine.ReleaseFloorTimber(floor);yield break;}
                time+=Time.unscaledDeltaTime;yield return null;
            }
        }
        IEnumerator Recoil(int floor, float duration)
        {
            if (game.ReducedMotion) { yield return Hold(profile.reducedStep); yield break; }
            float time = 0;
            while (time < duration) {
                if(PresentationPaused){yield return null;continue;}
                game.Machine.RecoilFloor(floor, -Mathf.Sin(time/duration*Mathf.PI)*profile.platformRecoil);
                time += Time.unscaledDeltaTime; yield return null;
            }
            while(PresentationPaused)yield return null;
            game.Machine.RecoilFloor(floor, 0);
        }
        IEnumerator Land(bool arrival)
        {
            float duration = game.ReducedMotion ? profile.reducedStep : Mathf.Max(.19f,profile.landing), time = 0;
            while (time < duration) {
                if(PresentationPaused){yield return null;continue;}
                for (int i = 0; i < game.VisibleFrame.Row.Count; i++){
                    float t=Mathf.Clamp01(time/duration),pulse=game.ReducedMotion?0:Mathf.Cos(t*Mathf.PI*2)*Mathf.Exp(-t*7);
                    float squash=pulse*profile.landingSquash;
                    game.OffsetPresentedToken(i,Vector3.up*(game.ReducedMotion?0:Mathf.Sin(t*Mathf.PI)*Mathf.Exp(-t*5)*(arrival?.025f:.035f)),new Vector2(1+squash*.5f,1-squash));
                    game.Machine.SetPiecePose(i,PiecePose.Settling,pulse*(i%2==0?1:-1)*5);
                }
                time += Time.unscaledDeltaTime; yield return null;
            }
            while(PresentationPaused)yield return null;
            game.RenderFrame(game.VisibleFrame, arrival);
        }
        IEnumerator WarnWind(bool right)
        {
            game.ReactionBanner.ShowWind(right);
            while(game.ReactionBanner.IsShowing){
                if(!PresentationPaused)game.Machine.PrepareWind(right,Mathf.SmoothStep(0,1,game.ReactionBanner.Elapsed/.85f));
                yield return null;
            }
            while(PresentationPaused)yield return null;
        }
        IEnumerator HoldReaction()
        {
            while(game.ReactionBanner!=null&&game.ReactionBanner.IsShowing)yield return null;
        }
        IEnumerator Hold(float duration)
        {
            if (game.ReducedMotion) duration = profile.reducedStep;
            float time = 0; while (time < duration) { if(!PresentationPaused)time += Time.unscaledDeltaTime; yield return null; }
            while(PresentationPaused)yield return null;
        }
        bool PresentationPaused => game.Interaction==null||game.Interaction.IsPaused||game.Interaction.IsSuspended;
        Vector3 EventOrigin(PuzzleEvent e, LevelDefinition level)
        {
            if (e.Floor < 0 || e.Floor >= level.Floors.Count) return game.Machine.TokenPosition(0);
            var floor = level.Floors[e.Floor]; ContactDefinition contact = null;
            if (e.Kind == PuzzleEventKind.BombDefused || e.Kind == PuzzleEventKind.BombExploded || e.Kind == PuzzleEventKind.BombTicked) contact = floor.Bomb;
            else if (e.Kind == PuzzleEventKind.KeyOpened) contact = floor.Key;
            else if (e.Kind == PuzzleEventKind.Repaired) contact = floor.Repair;
            else if (e.Kind == PuzzleEventKind.StampCollected) contact = floor.OptionalStamp;
            else if(e.Kind==PuzzleEventKind.RibbonCollected||e.Kind==PuzzleEventKind.HourglassCollected)contact=floor.Powerup;
            else if(e.Kind==PuzzleEventKind.BombFrozen)contact=floor.Bomb;
            return contact == null ? game.Machine.FloorPosition(e.Floor) : game.Machine.ContactPosition(e.Floor, contact.Slot);
        }
        void LateUpdate()
        {
            if (IsRunning || game == null || game.Interaction == null || !game.enabled || game.Interaction.IsSuspended || game.Interaction.IsPaused) return;
            game.AdvanceTokenTouch(Time.unscaledDeltaTime);
            for (int i = 0; i < game.SlotViews.Count; i++) {
                if(game.OwnsTokenPose(i)) { selection[i]=selectionFrom[i]=game.Interaction.SelectedSlot==i?1:0;selectionTarget[i]=game.Interaction.SelectedSlot==i;selectionTime[i]=profile.selection;continue; }
                bool target = game.Interaction.SelectedSlot == i;
                if (target != selectionTarget[i]) {
                    selectionFrom[i] = selection[i]; selectionTarget[i] = target; selectionTime[i] = 0;
                    if (target) feedback.Sound(ToyCue.Select);
                }
                selectionTime[i] = game.ReducedMotion ? profile.selection : Mathf.Min(profile.selection, selectionTime[i] + Time.unscaledDeltaTime);
                float t = selectionTime[i] / profile.selection, goal = target ? 1 : 0;
                selection[i] = Mathf.Lerp(selectionFrom[i], goal, 1-Mathf.Pow(1-t,3));
                float ratio = 1 + .04f * selection[i];
                game.OffsetPresentedToken(i, Vector3.up * (selection[i]*game.Machine.SelectionLiftWorld(i)), Vector2.one*ratio);
            }
        }
    }
}
